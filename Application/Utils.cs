using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Classes
{
    public class Utils
    {
        public void RemoveAds(string classname, ChromeDriver driver)
        {
            var adsElements = driver.FindElements(By.ClassName(classname));

            foreach (var adElement in adsElements)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display='none'", adElement);
            }
        }

    }
}
