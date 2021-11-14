using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using SmartDietCapstone.Helpers;
using SmartDietCapstone.Data;

namespace SmartDietCapstone.Pages
{
    public class IndexModel : PageModel
    {
        [Required(ErrorMessage = "Age field required")]
        [BindProperty]
        [Range(1, 115,ErrorMessage ="Age must be between 1 to 115")]
        
        
        public int age { get; set; }


        [Required(ErrorMessage = "Weight field required")]
        [BindProperty]
        [Range(1, 1000, ErrorMessage = "Weight must be between 1 to 1000")]
        public double weight { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private readonly HttpClient _client;
        private IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, HttpClient client, IConfiguration configuration)
        {
            _logger = logger;
            _client = client;
            _configuration = configuration;
            
        }
        /// <summary>
        /// Generates diet based on user's inputs.
        /// </summary>
        /// <param name="genderSelect"></param>
        /// <param name="age"></param>
        /// <param name="weight"></param>
        /// <param name="feetSelect"></param>
        /// <param name="inchSelect"></param>
        /// <param name="activitySelect"></param>
        /// <param name="goalSelect"></param>
        /// <param name="isKeto"></param>
        /// <param name="carbNumSelect"></param>
        /// <param name="mealNumSelect"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync(string genderSelect, int age, double weight, int feetSelect, int inchSelect, int activitySelect, int goalSelect, bool isKeto, int carbNumSelect, int mealNumSelect)
        {
            double centimetres = feetSelect * 30.48 + (inchSelect * 2.54);
            double kilograms = weight / 2.20462;

            string apiKey = _configuration["Secrets:FDCApiKey"];
            string apiUrl = _configuration["Secrets:FDCApi"];
            double height = inchSelect + feetSelect * 12;
            APICaller caller = new APICaller(apiUrl, apiKey, _client);

            if (!ModelState.IsValid)
            {
                //var errors = ModelState.Values.SelectMany(v => v.Errors);
                //errors = errors.ToList();

                return new PageResult();
            }
            else
            {

                
                FoodCalculator foodCalculator = new FoodCalculator(genderSelect, age, weight, height, goalSelect, activitySelect, isKeto, carbNumSelect, caller);

                var diet = await foodCalculator.GenerateDiet(mealNumSelect);
               
                var jsonDiet = JsonConvert.SerializeObject(diet);
                var jsonCalculator = JsonConvert.SerializeObject(foodCalculator);
                HttpContext.Session.SetString("diet", jsonDiet);
                HttpContext.Session.SetString("calculator", jsonCalculator);




                return new RedirectToPageResult("Diet");
            }

        }
        public JsonResult OnGetTest()
        {
            return new JsonResult("test");
        }


    }
}
