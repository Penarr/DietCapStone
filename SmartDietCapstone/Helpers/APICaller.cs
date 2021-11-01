using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartDietCapstone.Helpers
{
    public class APICaller
    {
        // Nutrient number values in FDC api
        private const int proteinApiNum = 203;
        private const int carbApiNum = 205;
        private const int fatApiNum = 204;
        private const int calApiNum = 208;
        // Values to access api
        private string apiKey;
        private string apiUrl;

        private HttpClient _client;
        public APICaller(string apiUrl, string apiKey, HttpClient client)
        {
            this.apiKey = apiKey;
            this.apiUrl = apiUrl;
            _client = client;
        }
        public async Task<string> SearchFood(string query)
        {
            var response = await _client.GetAsync($"{apiUrl}search?query={query}&dataType=Foundation&pageSize=140&api_key={apiKey}"); // search
            
            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query">query being entered into fdc api search</param>
        /// <returns>List of searched foods</returns>
        public async Task<List<Food>> GetListOfSearchedFoods(string query)
        {
            List<Food> searchedFoods = new List<Food>();
            var response = await _client.GetAsync($"{apiUrl}search?query={query}&dataType=Foundation&pageSize=140&api_key={apiKey}"); // search
            var data = await response.Content.ReadAsStringAsync();


            JObject jFoodList = JObject.Parse(data);

            for (int i = 0; i < jFoodList["foods"].Count(); i++)
            {
                Food food = new Food();
                food.name = jFoodList["foods"][i]["description"].ToString();
                food.fdcId = (int)jFoodList["foods"][i]["fdcId"];
                food.servingSize = 100;
                food.category = jFoodList["foods"][i]["foodCategory"].ToString();
                var foodNutrients = jFoodList["foods"][i]["foodNutrients"];
                for (int j = 0; j < foodNutrients.Count(); j++)
                {
                    try
                    {
                        double nutrientNumber = (double)foodNutrients[j]["nutrientNumber"];
                        switch (nutrientNumber)
                        {
                            case calApiNum:
                                food.cals = (double)foodNutrients[j]["value"];
                                break;

                            case proteinApiNum:
                                food.protein = (double)foodNutrients[j]["value"];
                                break;
                            case carbApiNum:
                                food.carbs = (double)foodNutrients[j]["value"];
                                break;
                            case fatApiNum:
                                food.fat = (double)foodNutrients[j]["value"];
                                break;
                        }
                        if (food.cals != 0 && food.protein != 0 && food.carbs != 0 && food.fat != 0)
                            break;


                    }
                    catch (Exception e) { }

                }
                if (food.cals == 0)
                    food.cals = Math.Round(food.protein * 4 + food.carbs * 4 + food.fat * 9);
                searchedFoods.Add(food);
            }
            return searchedFoods;
        }




    }
}

