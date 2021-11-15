using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartDietCapstone.Areas.Identity.Data;
using SmartDietCapstone.Models;

namespace SmartDietCapstone.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles="Admin")]
    public class AdminInfoModel : PageModel
    {

        internal UserManager<SmartDietCapstoneUser> _userManager;
        internal IConfiguration _configuration;
        public AdminInfoModel(UserManager<SmartDietCapstoneUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        public List<double> usersCalories = new List<double>();
        public List<double> usersProtein = new List<double>();
        public List<double> usersCarbs = new List<double>();
        public List<double> usersFat = new List<double>();

        public List<double> calsPerMeal = new List<double>();
        public List<double> proteinPerMeal = new List<double>();
        public List<double> carbsPerMeal = new List<double>();
        public List<double> fatPerMeal = new List<double>();
        public SortedDictionary<string, int> categoryDictionary = new SortedDictionary<string, int>();

        public async Task OnGetAsync()
        {
            await GetUserMacros();
            await GetFoodCategories();
        }

        /// <summary>
        /// Gets macros of all users
        /// </summary>
        /// <returns></returns>
        public async Task GetUserMacros()
        {
            string connectionString = _configuration.GetConnectionString("SmartDietCapstoneContextConnection");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Query only gets users who have set nutritional information
                string query = "SELECT UserCalories, UserProtein, UserCarbs, UserFat from AspNetUsers where UserCalories > 0 and UserProtein > 0 and UserCarbs > 0 and UserFat > 0;";
                SqlCommand command = new SqlCommand(query, conn);


                try
                {

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        usersCalories.Add((double)reader.GetValue(0));
                        usersProtein.Add((double)reader.GetValue(1));
                        usersCarbs.Add((double)reader.GetValue(2));
                        usersFat.Add((double)reader.GetValue(3));

                    }

                }
                catch (Exception e)
                {
                }
            }
        }
        /// <summary>
        /// Gets information of all saved diets in database
        /// </summary>
        /// <returns></returns>
        public async Task GetFoodCategories()
        {
            string connectionString = _configuration.GetConnectionString("SmartDietCapstoneContextConnection");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT SerializedDiet from Diet;";
                SqlCommand command = new SqlCommand(query, conn);
                var user = await _userManager.GetUserAsync(User);
                command.Parameters.AddWithValue("@id", user.Id);

                try
                {
                    List<Food> diets = new List<Food>();
                    await conn.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        List<string> categories = new List<string>();
                        List<Meal> meals = JsonConvert.DeserializeObject<List<Meal>>(reader.GetString(0));
                        foreach (Meal meal in meals)
                        {
                            calsPerMeal.Add(meal.totalCals);
                            proteinPerMeal.Add(meal.totalProtein);
                            carbsPerMeal.Add(meal.totalCarbs);
                            fatPerMeal.Add(meal.totalFat);
                            foreach (Food food in meal.foods)
                            {
                                if (categoryDictionary.ContainsKey(food.category))
                                    categoryDictionary[food.category] += 1;
                                
                                else
                                    categoryDictionary.Add(food.category, 1);
                            }
                                
                        }
                            

                        

                    }
                   
                }
                catch (Exception e) { }


            }
        }

    }
}

