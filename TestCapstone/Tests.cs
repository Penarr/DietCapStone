
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using SmartDietCapstone;
using SmartDietCapstone.Data;
using SmartDietCapstone.Helpers;
using System;
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
        public void Test()
        {
            var client = _factory.CreateClient();
            
          

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
            APICaller caller = new APICaller("https://api.nal.usda.gov/fdc/v1/", "LFvEHThAZuPapYjKemtarLfGUylkrh1SnDwCdmCA", client);
            FoodCalculator calculator = new FoodCalculator(gender, age, weight, height, goal, activityLevel, isKeto, carbNumSelect, caller);
            
        }
    }
}
