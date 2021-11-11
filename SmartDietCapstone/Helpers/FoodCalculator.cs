using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartDietCapstone.Helpers;
using SmartDietCapstone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using System.Threading.Tasks;

namespace SmartDietCapstone
{/// <summary>
/// FOUNDATION FOODS DOCUMENTATION https://fdc.nal.usda.gov/docs/Foundation_Foods_Documentation_Apr2021.pdf
/// API DOCUMENTATION https://fdc.nal.usda.gov/api-spec/fdc_api.html
/// </summary>
    public class FoodCalculator
    {
        // Nutrient number values in FDC api
        private const int proteinApiNum = 203;
        private const int carbApiNum = 205;
        private const int fatApiNum = 204;
        private const int calApiNum = 208;
   

        private APICaller caller;
        
        private const string dataType = "Foundation,SR%20Legacy";
        // Ideal amount in grams of each meal
        public double calorieCount;
        public double fatCount;
        public double proteinCount;
        public double carbCount;


        
        public FoodCalculator(string gender, int age, double weight, double height, int goal, int activityLevel, bool isKeto, int carbAmount, APICaller apiCaller)
        {
            
            calorieCount = Math.Round(CalculateCalories(gender, age, weight, height, goal, activityLevel));
            fatCount = Math.Round(CalculateFat(calorieCount, carbAmount, isKeto));
            proteinCount = Math.Round(CalculateProtein(calorieCount));
            carbCount = Math.Round(CalculateCarbs(calorieCount, carbAmount, isKeto));
            caller = apiCaller;
        }
        /// <summary>
        /// Implements Mifflin-St Jeor method of calculating calories based on physiology and activity levels.
        /// Assumes measurements are imperial, not metric
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="age"></param>
        /// <param name="weight"></param>
        /// <param name="height"></param>
        /// <param name="goal"></param>
        /// <param name="activityLevel"></param>
        /// <returns></returns>
        public double CalculateCalories(string gender, int age, double weight, double height, int goal, int activityLevel)
        {
            double calories = 2000;
            if (gender == "male")
                calories = 10 * weight + 6.25 * height - 5 * age + 5;


            else if (gender == "female")
                calories =   10 * weight + 6.25 * height - 5 * age - 161;

            switch (activityLevel)
            {
                case 0:
                    calories *= 1.2;
                    break;

                case 1:
                    calories *= 1.375;
                    break;
                case 2:
                    calories *= 1.55;
                    break;
                case 3:
                    calories *= 1.725;
                    break;
                case 4:
                    calories *= 1.9;
                    break;
                case 5:
                    calories *= 2;
                    break;
            }

            switch (goal)
            {
                case 0:
                    calories -= 500;
                    break;

                case 2:
                    calories += 500;
                    break;
            }

            return calories;


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="j"></param>
        /// <param name="caloriesRemaining"></param>
        /// <param name="proteinRemaing"></param>
        /// <param name="fatRemaining"></param>
        /// <param name="carbsRemaining"></param>
        /// <param name="mealNum"></param>
        /// <returns></returns>
        private async Task<Food> CalculateFood(string query, int j, double caloriesRemaining, double proteinRemaing, double fatRemaining, double carbsRemaining, int mealNum)
        {

            
            //get id of protein, carb, fat, kcal and derivation description
            Random rand = new Random();
            var result = await caller.SearchFood(query);
           

            // JObject that will store information in an array from api
            JObject obj;

            Food food = new Food();
            
            try
            {
                bool validFoodChoice = false;
                // Try food again
                while (!validFoodChoice)
                {
                    obj = JObject.Parse(result);
                    int randIndex = rand.Next(0, obj["foods"].Count());
                    
                    food.fdcId = (int)obj["foods"][randIndex]["fdcId"];
                    food.name = obj["foods"][randIndex]["description"].ToString();

                    food.category = obj["foods"][randIndex]["foodCategory"].ToString();
                    var foodNutrients = obj["foods"][randIndex]["foodNutrients"];
                    double calsPerGram = 0;
                    double proteinPerGram = 0;
                    double fatPerGram = 0;
                    double carbsPerGram = 0;
                    // Iterate through  nutrient value, assign value per gram to variable
                    for (int i = 0; i < foodNutrients.Count(); i++)
                    {
                        try
                        {
                            double nutrientNumber = (double)foodNutrients[i]["nutrientNumber"];
                            switch (nutrientNumber)
                            {
                                case calApiNum:
                                    calsPerGram = (double)foodNutrients[i]["value"] / 100;
                                    break;

                                case proteinApiNum:
                                    proteinPerGram = (double)foodNutrients[i]["value"] / 100;
                                    break;
                                case carbApiNum:
                                    carbsPerGram = (double)foodNutrients[i]["value"] / 100;
                                    break;
                                case fatApiNum:
                                    fatPerGram = (double)foodNutrients[i]["value"] / 100;
                                    break;
                            }
                            if (fatPerGram != 0 && proteinPerGram != 0 && carbsPerGram != 0 && calsPerGram != 0)
                                break;
                        }
                        catch(Exception ex){

                        }
                        
                    }
                    if (calsPerGram == 0)
                        calsPerGram = (proteinPerGram * 4 + carbsPerGram * 4 + fatPerGram * 9);
                    // This section will decide serving size of food
                    double calsPerMeal = calorieCount / mealNum;
                    double proteinPerMeal = proteinCount / mealNum;
                    double fatPerMeal = fatCount / mealNum;
                    double carbsPerMeal = carbCount / mealNum;
                    // Calculate serving size of food in meal


                    // Protein
                    if (j == 0)
                    {
                        
                        double servingSize = (proteinPerMeal) / proteinPerGram;
                        double caloriesOfFood = calsPerGram * servingSize;
                        if (caloriesOfFood > calsPerMeal + 200 / mealNum)
                            servingSize = (calsPerMeal + 200 / mealNum) / calsPerGram;
                        food.servingSize = Math.Round(servingSize);
                        food.carbs = Math.Round(carbsPerGram * servingSize);
                        food.protein = Math.Round(proteinPerGram * servingSize);
                        food.fat = Math.Round(fatPerGram * servingSize);
                        food.cals = Math.Round(calsPerGram * servingSize);
                        if (!(food.carbs >= food.protein)! && !(food.fat >= food.protein) || food.cals <= calsPerMeal + calsPerMeal * 0.1 || food.cals < 1)
                            validFoodChoice = true;
                        
                    }
                    // Carb food
                    else if (j == 1)
                    {

                        double servingSize = (carbsRemaining - carbsPerMeal * 0.05) / carbsPerGram;
                        double caloriesOfFood = calsPerGram * servingSize;
                        if (caloriesOfFood > caloriesRemaining + 200 / mealNum)
                            servingSize = caloriesRemaining  / calsPerGram;
                        food.servingSize = Math.Round(servingSize);
                        food.carbs = Math.Round(carbsPerGram * servingSize);
                        food.protein = Math.Round(proteinPerGram * servingSize);
                        food.fat = Math.Round(fatPerGram * servingSize);
                        food.cals = Math.Round(calsPerGram * servingSize);
                        if (!(food.protein >= food.carbs))
                            validFoodChoice = true;
                    }
                    // Fill with vegetable
                    else if (j == 2)
                    {
                        if (caloriesRemaining > 200)
                        {
                            double servingSize = caloriesRemaining / calsPerGram;
                            food.servingSize = Math.Round(servingSize);
                            food.carbs = Math.Round(carbsPerGram * servingSize);
                            food.protein = Math.Round(proteinPerGram * servingSize);
                            food.fat = Math.Round(fatPerGram * servingSize);
                            food.cals = calsPerGram * servingSize;
                            validFoodChoice = true;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                var error = e.Message;

            }
            return food;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calories"></param>
        /// <param name="carbAmount"></param>
        /// <param name="isKeto"></param>
        /// <returns></returns>
        public double CalculateFat(double calories, int carbAmount, bool isKeto)
        {
            double fatPercent = 0.35;

            if (isKeto)
                fatPercent = 0.60;
            else
            {
                switch (carbAmount)
                {
                    case 1:
                        fatPercent = 0.3;
                        break;
                    case 2:
                        fatPercent = 0.2;
                        break;
                    case 3:
                        fatPercent = 0.1;
                        break;

                }
            }

            // Amount of fat in grams
            return calories * fatPercent / 9;

        }


        public double CalculateProtein(double calories)
        {
            // Amount of protein in grams
            return calories * 0.3 / 4;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calories"></param>
        /// <param name="carbAmount"></param>
        /// <param name="isKeto"></param>
        /// <returns></returns>
        public double CalculateCarbs(double calories, int carbAmount, bool isKeto)
        {
            double carbPercent = 0.5;

            if (isKeto)
                carbPercent = 0.1;
            else
            {
                switch (carbAmount)
                {
                    case 1:
                        carbPercent = 0.4;
                        break;
                    case 2:
                        carbPercent = 0.5;
                        break;
                    case 3:
                        carbPercent = 0.6;
                        break;

                }
            }

            // Amount of carbs in grams
            return calories * carbPercent / 4;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mealNum"></param>
        /// <returns></returns>
        public async Task<List<Meal>> GenerateDiet(int mealNum)
        {
            string[] queries = { "Meat chicken turkey fish", "bread rice potato", "vegetable fruit" };
            List<Meal> mealPlan = new List<Meal>();
            for (int i = 0; i < mealNum; i++)
            {
                // Calories for each meal
                double calsRemaining = calorieCount / mealNum;
                double proteinRemaining = proteinCount / mealNum;
                double carbsRemaining = carbCount / mealNum;
                double fatRemaining = fatCount / mealNum;

                Meal meal = new Meal();
                //Individual food generation per meal
                for (int j = 0; j < 3; j++)
                {
                    
                   
                    if(calsRemaining > 200)
                    {
                        Food f = await CalculateFood(queries[j], j, calsRemaining, proteinRemaining, fatRemaining, carbsRemaining, mealNum);
                        meal.AddFood(f);
                        // Calculate remaining macros for meal
                        calsRemaining -= meal.totalCals;
                        proteinRemaining -= meal.totalProtein;
                        carbsRemaining -= meal.totalCarbs;
                        fatRemaining -= meal.totalFat;

                     

                    }
                    
                }
                mealPlan.Add(meal);
            }
            return mealPlan;
        }




    }
}
