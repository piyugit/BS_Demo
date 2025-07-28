using BS_TechAssement_WebScrapping.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;

namespace BS_TechAssement_WebScrapping.Hooks
{
    [Binding]
    public class TestHooks
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver _driver;

        public TestHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Console.WriteLine("==>=== Test Run Started ===");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Console.WriteLine("==>=== Test Run Completed ===");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {

            string browser = "chrome"; // default

            // Option 1: From tags like @browser:chrome
            var tag = _scenarioContext.ScenarioInfo.Tags.FirstOrDefault(t => t.StartsWith("browser:"));
            if (tag != null)
            {
                browser = tag.Split(':')[1];
            }

            // Option 2: From example table if you’re using Scenario Outline
            if (_scenarioContext.ScenarioInfo.Arguments.Contains("Browsers"))
            {
                browser = _scenarioContext.ScenarioInfo.Arguments["Browsers"]?.ToString();
            }
            _driver = WebDriverFactory.CreateWebDriver(browser);
            _scenarioContext["WebDriver"] = _driver;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            Console.WriteLine($">>> Finished Scenario: {_scenarioContext.ScenarioInfo.Title}");

            if (_scenarioContext.TestError != null)
            {
                Console.WriteLine("==>Test Failed:");
                Console.WriteLine(_scenarioContext.TestError.Message);
            }

            // Quit driver
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                Console.WriteLine("==>WebDriver session ended.");
            }
        }

    }
}
