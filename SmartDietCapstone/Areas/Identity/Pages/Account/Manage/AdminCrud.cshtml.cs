using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SmartDietCapstone.Areas.Identity.Data;

namespace SmartDietCapstone.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles="Admin")]
    public class AdminCrudModel : PageModel
    {


        internal UserManager<SmartDietCapstoneUser> _userManager;
        internal IConfiguration _configuration;

        public List<SmartDietCapstoneUser> users;
        private List<SmartDietCapstoneUser> admins;
        
        public AdminCrudModel(UserManager<SmartDietCapstoneUser> userManager, IConfiguration configuration)
        {
           
            _userManager = userManager;
            _configuration = configuration;
            
            users = _userManager.Users.ToList();
        }


        public async Task GetUsers()
        {
            admins = (List<SmartDietCapstoneUser>)await _userManager.GetUsersInRoleAsync("Admin");
            users = _userManager.Users.Where(user => !admins.Contains(user)).ToList();
        }
        public async Task OnGet()
        {
            await GetUsers();
        }

        public async Task OnPostDeleteUser(string userId)
        {
            var result = _userManager.FindByIdAsync(userId);
            if (result.Result != null)
            {
                var user = result.Result;
                await _userManager.DeleteAsync(user);
            }
            
            await GetUsers();
        }


        
    }
}
