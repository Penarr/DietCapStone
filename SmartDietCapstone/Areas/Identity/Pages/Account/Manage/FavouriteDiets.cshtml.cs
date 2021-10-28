using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartDietCapstone.Areas.Identity.Data;
using SmartDietCapstone.Models;

namespace SmartDietCapstone.Areas.Identity.Pages.Account.Manage
{
    public class FavouriteDietsModel : PageModel
    {
        private readonly UserManager<SmartDietCapstoneUser> _userManager;
        private readonly SignInManager<SmartDietCapstoneUser> _signInManager;
        private readonly IConfiguration _configuration;
        public List<List<Meal>> favouriteDiets;
        public List<double> dietCalories;
        public List<double> dietProtein;
        public List<double> dietCarbs;
        public List<double> dietFat;
        public List<string> dietIds;
        public FavouriteDietsModel(UserManager<SmartDietCapstoneUser> userManager,
            SignInManager<SmartDietCapstoneUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            favouriteDiets = new List<List<Meal>>();
            dietCalories = new List<double>();
            dietProtein = new List<double>();
            dietCarbs = new List<double>();
            dietFat = new List<double>();
            dietIds = new List<string>();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.Keys.Contains("favouriteDiet"))
                await SaveFavouriteDiet();
            await GetFavouriteDiets();
            return new PageResult();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.Keys.Contains("favouriteDiet"))
                await SaveFavouriteDiet();
            await GetFavouriteDiets();
            return new PageResult();
        }

        /// <summary>
        /// Save a diet as a favourite diet
        /// </summary>
        public async Task SaveFavouriteDiet()
        {
            if (HttpContext.Session.Keys.Contains("favouriteDiet"))
            {
                string jsonDiet = HttpContext.Session.GetString("favouriteDiet");

                string connectionString = _configuration.GetConnectionString("SmartDietCapstoneContextConnection");
                var user = await _userManager.GetUserAsync(User);
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string updateQuery = "INSERT INTO Diet (UserId, SerializedDiet) VALUES(@id, @diet);";
                    SqlCommand command = new SqlCommand(updateQuery, conn);

                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@diet", jsonDiet);

                    try
                    {
                        await conn.OpenAsync();
                        var result = await command.ExecuteNonQueryAsync();
                        conn.Close();
                    }


                    catch (Exception e)
                    { var message = e.Message; }
                }
                HttpContext.Session.Remove("favouriteDiet");
            }


        }

        public async Task DeleteFavouriteDiet(string dietId)
        {
            string connectionString = _configuration.GetConnectionString("SmartDietCapstoneContextConnection");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE from Diet where DietId = @id";
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@id", dietId);

                try
                {
                    await conn.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    conn.Close();
                }


                catch (Exception e)
                { var message = e.Message; }


            }
        }
        /// <summary>
        /// Change
        /// </summary>
        /// <param name="dietIndex">Index of favourite diet to edit</param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostViewFavouriteDiet(int dietIndex)
        {
            await GetFavouriteDiets();
            if (dietIndex < favouriteDiets.Count)
            {
                string jsonDiet = JsonConvert.SerializeObject(favouriteDiets[dietIndex]);
                HttpContext.Session.SetString("diet", jsonDiet);
                return new RedirectToPageResult("/Diet");

            }
            return new PageResult();
        }
        /// <summary>
        /// 
        /// </summary>
        public async Task GetFavouriteDiets()
        {
            string connectionString = _configuration.GetConnectionString("SmartDietCapstoneContextConnection");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DietId, SerializedDiet from Diet where UserId = @id;";
                SqlCommand command = new SqlCommand(query, conn);
                var user = await _userManager.GetUserAsync(User);
                command.Parameters.AddWithValue("@id", user.Id);

                try
                {
                    await conn.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        dietIds.Add(reader.GetString(0)); // Saves GUID of diet to string for deletion
                        favouriteDiets.Add(JsonConvert.DeserializeObject<List<Meal>>(reader.GetString(1)));
                    }

                    if (favouriteDiets != null)
                    { // Calculates macros of diet to display in table
                        foreach (List<Meal> diet in favouriteDiets)
                        {
                            double totalCaloriesOfDiet = 0;
                            double totalProteinOfDiet = 0;
                            double totalCarbsOfDiet = 0;
                            double totalFatOfDiet = 0;
                            foreach (Meal meal in diet)
                            {
                                totalCaloriesOfDiet += meal.totalCals;
                                totalProteinOfDiet += meal.totalProtein;
                                totalCarbsOfDiet += meal.totalCarbs;
                                totalFatOfDiet += meal.totalFat;
                            }
                            dietCalories.Add(totalCaloriesOfDiet);
                            dietProtein.Add(totalProteinOfDiet);
                            dietCarbs.Add(totalCarbsOfDiet);
                            dietFat.Add(totalFatOfDiet);
                        }
                    }
                }
                catch (Exception e) 
                { }


            }
        }
    }
}
