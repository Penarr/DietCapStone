using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartDietCapstone.Helpers;
using SmartDietCapstone.Models;

namespace SmartDietCapstone.Pages
{
    [Authorize]
    public class EditMealModel : PageModel
    {
        private APICaller caller;
        public Meal meal;
        public List<Food> searchedFoods;
        public EditMealModel(HttpClient _client, IConfiguration _configuration)
        {
            caller = new APICaller(_configuration["Secrets:FDCApi"], _configuration["Secrets:FDCApiKey"],  _client);
            
        }
        /// <summary>
        /// Converts json string to meal to be edited
        /// </summary>
        public void OnGet()
        {
            if (TempData.ContainsKey("meal"))
            {
                var jsonMeal = TempData["meal"] as string;
                meal = JsonConvert.DeserializeObject<Meal>(jsonMeal);
            }

        }

        /// <summary>
        /// AJAX endpoint that searchs for food using query
        /// </summary>
        /// <param name="query">Query that searches food</param>
        /// <returns></returns>
        public async Task<JsonResult> OnGetFoodSearch(string query)
        {
            searchedFoods = await caller.GetListOfSearchedFoods(query);
            return  new JsonResult(searchedFoods);
        }


        /// <summary>
        /// Validates meal, and saves changes to meal then redirects to diet page
        /// </summary>
        /// <param name="jsonFoods">Json string of foods in diet</param>
        /// <returns></returns>
        public  ActionResult OnPostValidateMeal(string jsonFoods)
        {
            List<Food> foods = JsonConvert.DeserializeObject<List<Food>>(jsonFoods);
            if (foods.Count > 0)
            {
                Meal meal = new Meal();

                foreach (Food food in meal.foods)
                    meal.AddFood(food);

                TempData["meal"] = JsonConvert.SerializeObject(meal);

                return new RedirectToPageResult("Diet", "EditDiet");
            }
            return new PageResult();

        }
    }
}
