using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartDietCapstone.Areas.Identity.Data;
using SmartDietCapstone.Models;

namespace SmartDietCapstone.Areas.Identity.Pages.Account.Manage
{
    public class ChartModel : PageModel
    {
        internal IConfiguration _configuration;
        internal UserManager<SmartDietCapstoneUser> _userManager;
        public string[] categoryLabels;
        public int[] linkedCategoryCount;
        

        public ChartModel(IConfiguration configuration, UserManager<SmartDietCapstoneUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }
        public void OnGet()
        {
        }



       
    }
}
