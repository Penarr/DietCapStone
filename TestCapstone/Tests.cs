
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using SmartDietCapstone;
using SmartDietCapstone.Data;
using SmartDietCapstone.Helpers;
using SmartDietCapstone.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace TestCapstone
{
    public class Tests : IClassFixture<WebApplicationFactory<SmartDietCapstone.Startup>>
    {
        private readonly WebApplicationFactory<SmartDietCapstone.Startup> _factory;


        public Tests(WebApplicationFactory<SmartDietCapstone.Startup> factory)
        {
            _factory = factory;

        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        [InlineData("/Diet")]
        [InlineData("/EditMeal")]
        [InlineData("/Error")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }


        [Fact]
        public void TestCalorieCalculator()
        {
            var client = _factory.CreateClient();


            string gender = "male";
            int age = 25;
            double weight = 190 / 2.20462;
            double height = 180;
            int activityLevel = 1;
            int goal = 0;
            int carbNumSelect = 1;
            bool isKeto = false;
            //var formContent = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string, string>("gender", gender)

            //}) ;
            //client.PostAsync("/Index", formContent);
            int expectedAmount = 2526; // From online calculator using same values and method to calculate calories
            APICaller caller = new APICaller("https://api.nal.usda.gov/fdc/v1/", "LFvEHThAZuPapYjKemtarLfGUylkrh1SnDwCdmCA", client);
            FoodCalculator calculator = new FoodCalculator(gender, age, weight, height, goal, activityLevel, isKeto, carbNumSelect, caller);
            Assert.InRange(calculator.calorieCount, 2040, 2411); // Range gotten from same inputs at https://www.leighpeele.com/mifflin-st-jeor-calculator


        }

        [Fact]
        public async Task TestDietGeneration()
        {
            var client = _factory.CreateClient();


            string gender = "male";
            int age = 25;
            double weight = 190 / 2.20462;
            double height = 190;
            int activityLevel = 3;
            int goal = 0;
            int carbNumSelect = 1;
            bool isKeto = false;
            var apiClient = new HttpClient();
            // From online calculator using same values and method to calculate calories
            APICaller caller = new APICaller("https://api.nal.usda.gov/fdc/v1/", "LFvEHThAZuPapYjKemtarLfGUylkrh1SnDwCdmCA", apiClient);
            FoodCalculator calculator = new FoodCalculator(gender, age, weight, height, goal, activityLevel, isKeto, carbNumSelect, caller);
            List<Meal> diet = await calculator.GenerateDiet(3);

            double dietCals = 0;
            double dietProtein = 0;
            double dietCarbs = 0;
            double dietFat = 0;


            foreach(Meal m in diet)
            {
                dietCals += m.totalCals;
                dietProtein += m.totalProtein;
                dietCarbs += m.totalCarbs;
                dietFat += m.totalFat;
            }

            Assert.InRange(dietCals, calculator.calorieCount - 250, calculator.calorieCount + 250);


        }
    }
}
