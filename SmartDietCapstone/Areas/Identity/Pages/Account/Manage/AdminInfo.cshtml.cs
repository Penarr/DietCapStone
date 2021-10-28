using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartDietCapstone.Areas.Identity.Data;
using SmartDietCapstone.Models;

namespace SmartDietCapstone.Areas.Identity.Pages.Account.Manage
{
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
        public string[] categoryLabels;
        public int[] linkedCategoryCount;

        public async Task OnGetAsync()
        {
            await GetUserMacros();
            await GetFoodCategories();
        }


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
                                categories.Add(food.category);
                        }
                            

                        categories.Sort();
                        categoryLabels = categories.Distinct().ToArray();
                        linkedCategoryCount = new int[categories.Distinct().Count()];

                        for (int i = 0; i < categories.Count; i++)
                        {
                            for (int j = 0; j < categoryLabels.Length; j++)
                            {
                                if (categoryLabels[j] == categories[i])
                                    linkedCategoryCount[j]++;
                            }
                        }
                    }

                }
                catch (Exception e) { }


            }
        }

    }
}

