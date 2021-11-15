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
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SmartDietCapstone.Pages
{
    [Authorize]
    public class EditMealModel : PageModel
    {
        private APICaller caller;
        public Meal meal;
        public List<Food> searchedFoods;


        [Range(1, int.MaxValue, ErrorMessage = "Meal must have food in it")]
        [BindProperty]
        public int mealLength { get; set; }


        public EditMealModel(HttpClient _client, IConfiguration _configuration)
        {
            caller = new APICaller(_configuration["Secrets:FDCApi"], _configuration["Secrets:FDCApiKey"], _client);

        }
       
        public void OnGet()
        {
            SetMeal();

        }
        /// <summary>
        /// Converts json string to meal to be edited
        /// </summary>
        public void SetMeal()
        {
            if (HttpContext.Session.Keys.Contains("meal"))
            {
                var jsonMeal = HttpContext.Session.GetString("meal");
                meal = JsonConvert.DeserializeObject<Meal>(jsonMeal);
                mealLength = meal.foods.Count;
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
            return new JsonResult(searchedFoods);
        }


        /// <summary>
        /// Validates meal, and saves changes to meal then redirects to diet page
        /// </summary>
        /// <param name="jsonFoods">Json string of foods in diet</param>
        /// <returns></returns>
        public ActionResult OnPostValidateMeal(string jsonFoods, int mealLength)
        {
            SetMeal();
            if (mealLength > 0)
            {
                try
                {

                    List<Food> foods = JsonConvert.DeserializeObject<List<Food>>(jsonFoods);
                    if (foods.Count > 0)
                    {

                        Meal meal = new Meal();

                        foreach (Food food in foods)
                            meal.AddFood(food);

                        HttpContext.Session.SetString("meal", JsonConvert.SerializeObject(meal));

                        return new RedirectToPageResult("Diet", "EditDiet");
                    }

                }
                catch { }
            }
            meal = new Meal();

            return new PageResult();


        }
    }
}
