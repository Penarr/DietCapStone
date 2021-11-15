using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft;
using Newtonsoft.Json;
using SmartDietCapstone.Models;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

using SmartDietCapstone.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartDietCapstone.Pages
{

    public class DietModel : PageModel
    {

        public List<Meal> diet;
        public FoodCalculator foodCalculator;
        public double dietCalories;
        public double recommendedCalories;
        public double dietProtein;
        public double recommendedProtein;
        public double dietCarbs;
        public double recommendedCarbs;
        public double dietFat;
        public double recommendedFat;


        [BindProperty]
        [Required(ErrorMessage = "Please enter a name for your diet")]
        public string DietName { get; set; }
        private readonly UserManager<SmartDietCapstoneUser> _userManager;

        public DietModel(UserManager<SmartDietCapstoneUser> userManager)
        {

            _userManager = userManager;
        }

        /// <summary>
        /// Constructs diet based on diet for get
        /// </summary>

        public async Task OnGet()
        {
            await SetDietAndCalculator();
            CalculateDietMacros();
          
        }
        /// <summary>
        /// Calculates information of entire diet
        /// </summary>
        internal void CalculateDietMacros()
        {

            if (diet != null)
            {
                foreach (Meal meal in diet)
                {
                    dietCalories += meal.totalCals;
                    dietProtein += meal.totalProtein;
                    dietCarbs += meal.totalCarbs;
                    dietFat += meal.totalFat;

                }
            }
        }
        /// <summary>
        /// Sets diet and calculator from cookies.
        /// If calculator cookie is set and user is logged in, will set user's new macro
        /// recommendations to the new amount
        /// </summary>
        private async Task SetDietAndCalculator()
        {
           
            var calculator = "";


            if (HttpContext.Session.Keys.Contains("diet"))
            {
                var dietString = HttpContext.Session.GetString("diet");
                diet = JsonConvert.DeserializeObject<List<Meal>>(dietString);
            }



            if (HttpContext.Session.Keys.Contains("calculator")) // Saves most recent recommendations to user table
            {
                calculator = HttpContext.Session.GetString("calculator");
                foodCalculator = JsonConvert.DeserializeObject<FoodCalculator>(calculator);
                if (User.Identity.IsAuthenticated) // Saves updated user information
                {
                    var user = await _userManager.GetUserAsync(User);
                    user.UserCalories = foodCalculator.calorieCount;
                    user.UserProtein = foodCalculator.proteinCount;
                    user.UserCarbs = foodCalculator.carbCount;
                    user.UserFat = foodCalculator.fatCount;
                    await _userManager.UpdateAsync(user);
                }
                recommendedCalories = foodCalculator.calorieCount;
                recommendedProtein = foodCalculator.proteinCount;
                recommendedCarbs = foodCalculator.carbCount;
                recommendedFat = foodCalculator.fatCount;
            }
            if (User.Identity.IsAuthenticated) // Uses current information about user if viewing a diet not generated randomly
            {
                var user = await _userManager.GetUserAsync(User);
                if (user.UserCalories > 0)
                {
                    recommendedCalories = user.UserCalories;
                    recommendedProtein = user.UserProtein;
                    recommendedCarbs = user.UserCarbs;
                    recommendedFat = user.UserFat;
                }

            }

           
        }
        /// <summary>
        /// Saves diet to database as favourite diet if user is logged in
        /// </summary>
        /// <returns>Favourite diets page</returns>
        public async Task<IActionResult> OnPostSaveDiet(string DietName)
        {
            await SetDietAndCalculator();
            
            if (diet.Count > 0 && ModelState.IsValid)
            {
                HttpContext.Session.SetString("favouriteDiet", HttpContext.Session.GetString("diet"));
                TempData["dietName"] = DietName;
                return new RedirectToPageResult("/Account/Manage/FavouriteDiets", "SaveDiet", new { area = "Identity" });
            }

            return new PageResult();

        }
        /// <summary>
        /// Adds an empty meal to the diet, then redirects to the edit page to create said meal.
        /// </summary>
        /// <returns>Edit meal page</returns>

        public async Task<IActionResult> OnPostAddMeal()
        {
            await SetDietAndCalculator();

            diet.Add(new Meal());
            int mealIndex = diet.Count() - 1;
            HttpContext.Session.SetInt32("mealIndex", mealIndex);
            HttpContext.Session.SetString("meal",JsonConvert.SerializeObject(diet[mealIndex]));
            return new RedirectToPageResult("/EditMeal");



        }
        /// <summary>
        /// Opens the edit meal page to edit meal
        /// </summary>
        /// <param name="mealIndex">Index of the meal in the diet list</param>
        /// <returns>Edit meal page result</returns>

        public async Task<IActionResult> OnPostGoToEditMeal(int mealIndex)
        {
            await SetDietAndCalculator();
            if (diet.Count > mealIndex)
            {
                Meal meal = diet[mealIndex];

                string jsonMeal = JsonConvert.SerializeObject(meal);
                HttpContext.Session.SetInt32("mealIndex", mealIndex);
                HttpContext.Session.SetString("meal", jsonMeal);
                return new RedirectToPageResult("EditMeal");

            }
            CalculateDietMacros();
            return new PageResult();
        }
        /// <summary>
        /// Updates the diet cookie for any meal deleted
        /// </summary>
        /// <param name="dietIndex">Index of meal to be deleted</param>
        /// <returns>Diet page with updated diet</returns>
        public async Task<IActionResult> OnPostDeleteMeal(int deleteIndex)
        {
            await SetDietAndCalculator();
            if (diet.Count > deleteIndex)
            {
                diet.RemoveAt(deleteIndex);

                HttpContext.Session.SetString("diet", JsonConvert.SerializeObject(diet));
                return new RedirectToPageResult("Diet");

            }

            CalculateDietMacros();
            return new PageResult();
        }

        /// <summary>
        /// Makes changes to meal being edited, and adds them to the diet
        /// </summary>
        public async Task OnGetEditDiet()
        {
            await SetDietAndCalculator();

            if (HttpContext.Session.Keys.Contains("mealIndex"))
            {
                int mealIndex = (int)HttpContext.Session.GetInt32("mealIndex");
                if (mealIndex == diet.Count)
                    diet.Add(new Meal());
                   
                
                if (HttpContext.Session.Keys.Contains("meal"))
                {
                    diet[mealIndex] = JsonConvert.DeserializeObject<Meal>(HttpContext.Session.GetString("meal"));
                    HttpContext.Session.SetString("diet", JsonConvert.SerializeObject(diet));
                    HttpContext.Session.Remove("meal");
                   
                }
               

            }
            CalculateDietMacros();
        }
    }
}
