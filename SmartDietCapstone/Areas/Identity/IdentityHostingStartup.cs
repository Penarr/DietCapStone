using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartDietCapstone.Areas.Identity.Data;
using SmartDietCapstone.Data;

[assembly: HostingStartup(typeof(SmartDietCapstone.Areas.Identity.IdentityHostingStartup))]
namespace SmartDietCapstone.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
               
                services.AddDbContext<SmartDietCapstoneContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("SmartDietCapstoneContextConnection")));

                services.AddIdentity<SmartDietCapstoneUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<SmartDietCapstoneContext>().AddDefaultUI().AddDefaultTokenProviders();
            });
        }
    }
}