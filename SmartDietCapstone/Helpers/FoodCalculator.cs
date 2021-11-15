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

            if (gender == "male") // male equation
                calories = 10 * weight + 6.25 * height - 5 * age + 5;


            else if (gender == "female") // female equation
                calories =   10 * weight + 6.25 * height - 5 * age - 161;

            switch (activityLevel) // Increases calorie count based on activity level
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

            switch (goal) // goal changes calorie count so users either gain/lose a lb of week. 
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
        /// Generates a diet after calculating calories and macronutrients based
        /// </summary>
        /// <param name="query">Query being sent to api</param>
        /// <param name="j"></param>
        /// <param name="caloriesRemaining"></param>
        /// <param name="proteinRemaining"></param>
        /// <param name="fatRemaining"></param>
        /// <param name="carbsRemaining"></param>
        /// <param name="mealNum"></param>
        /// <returns></returns>
        private async Task<Food> CalculateFood(string query, int j, double caloriesRemaining, double proteinRemaining, double fatRemaining, double carbsRemaining, int mealNum)
        {

            
            //get id of protein, carb, fat, kcal and derivation description
            Random rand = new Random();
            //var result = await caller.SearchFood(query);
            var foodList = await caller.GetListOfSearchedFoods(query);
            int randIndex = rand.Next(0, foodList.Count());

            // JObject that will store information in an array from api
            //JObject obj;


            Food food = foodList[randIndex];
            try
            {
                bool validFoodChoice = false;
                // Try food again
                while (!validFoodChoice)
                {
                double calsPerGram = food.cals / 100;
                double proteinPerGram = food.protein / 100;
                double fatPerGram = food.fat / 100;
                double carbsPerGram = food.carbs / 100;
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
                            servingSize = caloriesRemaining / calsPerGram;
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
        /// Calculates fat percentage based on users information
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

        /// <summary>
        /// Protein is always 30% of calorie count for the purposes of this application
        /// </summary>
        /// <param name="calories"></param>
        /// <returns></returns>
        public double CalculateProtein(double calories)
        {
            // Amount of protein in grams
            return calories * 0.3 / 4;

        }

        /// <summary>
        /// Calcualtes carb count baed on user preference
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
        /// Generates diet based on user's nutrional info.
        /// 
        /// The number of meals is 1 to 4 depending on input, but the max number of foods
        /// per meal is always 3 for the purposes of this. It is a little volatile but works most of the time
        /// within like a 200 or 300 calorie and macro nutrient range.
        /// </summary>
        /// <param name="mealNum"></param>
        /// <returns></returns>
        public async Task<List<Meal>> GenerateDiet(int mealNum)
        {
            string[] queries = { "Meat chicken turkey fish", "bread rice potato", "vegetable fruit" };
            List<Meal> mealPlan = new List<Meal>();
            for (int i = 0; i < mealNum; i++)
            {
                // Amount of each nutrient for each meal
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
