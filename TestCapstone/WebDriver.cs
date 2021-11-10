using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCapstone
{
    static internal class WebDriver
    {

        public static IWebDriver CreateBrowser()
        {
            return new ChromeDriver();
        }
    }
}
