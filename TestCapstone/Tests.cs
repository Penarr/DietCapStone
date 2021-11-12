

using Microsoft.AspNetCore.Mvc.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SmartDietCapstone;

using SmartDietCapstone.Data;
using SmartDietCapstone.Helpers;
using SmartDietCapstone.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace TestCapstone
{
    public class Tests : IClassFixture<WebApplicationFactory<SmartDietCapstone.Startup>>
    {
         //private readonly WebApplicationFactory<SmartDietCapstone.Startup> _factory;
        private HttpClient client;
        private const string Url = "https://smartdietcapstone.azurewebsites.net/";

        public Tests(WebApplicationFactory<SmartDietCapstone.Startup> factory)
        {
           
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.json");

            client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(async (services) =>
                {
                    var serviceProvider = services.BuildServiceProvider();

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices
                            .GetRequiredService<SmartDietCapstoneContext>();
                        var logger = scopedServices
                            .GetRequiredService<ILogger<Tests>>();

                        try
                        {
                            int result = await DbInitializer.SeedUsersAndRoles(scopedServices);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred seeding " +
                                "the database with test messages. Error: {Message}",
                                ex.Message);
                        }
                    }
                });
            })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

            

        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        [InlineData("/Diet")]
        [InlineData("/EditMeal")]
        [InlineData("/Error")]
        [InlineData("/Identity/Account/Register")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange done in constructor
            

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
            
            string gender = "male";
            int age = 25;
            double weight = 190 / 2.20462;
            double height = 180;
            int activityLevel = 1;
            int goal = 0;
            int carbNumSelect = 1;
            bool isKeto = false;

            var apiClient = new HttpClient();// From online calculator using same values and method to calculate calories
            APICaller caller = new APICaller("https://api.nal.usda.gov/fdc/v1/", "LFvEHThAZuPapYjKemtarLfGUylkrh1SnDwCdmCA", apiClient);
            FoodCalculator calculator = new FoodCalculator(gender, age, weight, height, goal, activityLevel, isKeto, carbNumSelect, caller);
            Assert.InRange(calculator.calorieCount, 2040, 2411); // Range gotten from same inputs at https://www.leighpeele.com/mifflin-st-jeor-calculator


        }

        [Fact]
        public async Task TestDietGeneration()
        {
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


            foreach (Meal m in diet)
            {
                dietCals += m.totalCals;
                dietProtein += m.totalProtein;
                dietCarbs += m.totalCarbs;
                dietFat += m.totalFat;
            }

            Assert.InRange(dietCals, calculator.calorieCount - 250, calculator.calorieCount + 250);


        }

        [Fact]
        public async Task TestRegiser_EmailInUse()
        {
             
            using(var driver = WebDriver.CreateBrowser())
            {
               
                driver.Navigate().GoToUrl("https://smartdietcapstone.azurewebsites.net/Identity/Account/Register");
                driver.FindElement(By.LinkText("Register")).Click();
                driver.FindElement(By.Id("Input_Email")).SendKeys("penarr@dietcapstone.ca");
                driver.FindElement(By.Id("Input_Password")).SendKeys("Pa55word1!");
                driver.FindElement(By.Id("Input_ConfirmPassword")).SendKeys("Pa55word1!");

                driver.FindElement(By.ClassName("btn-primary")).Click();

                
                var returnUrl = driver.Url;
                Assert.Equal("https://smartdietcapstone.azurewebsites.net/Identity/Account/Register", returnUrl);

            }

        }


        public async Task TestDietSave()
        {
            


        }
    }
}
