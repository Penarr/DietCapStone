using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartDietCapstone.Data;
using SmartDietCapstone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;

namespace SmartDietCapstone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();//.Run();
            var configuration = host.Services.GetService<IConfiguration>();
            var hosting = host.Services.GetService<IWebHostEnvironment>();

            if (hosting.IsDevelopment())
            {
                var secrets = configuration.GetSection("Passwords").Get<AppSecrets>();
                DbInitializer.appSecrets = secrets;
            }

            using (var scope = host.Services.CreateScope())
                DbInitializer.SeedUsersAndRoles(scope.ServiceProvider).Wait();


            host.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureAppConfiguration((hostContext, builder) =>
                {
                    // Add other providers for JSON, etc.

                    if (!hostContext.HostingEnvironment.IsDevelopment())
                    {
                        var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
                        builder.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
                        
                    }
                    else
                    {
                        builder.AddUserSecrets<Program>();
                    }
                    
                });


        
    }
}
