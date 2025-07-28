using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;

namespace BS_TechAssement_WebScrapping.Utilities
{
    public static class WebDriverFactory
    {
        public static IWebDriver CreateWebDriver(string browser = "chrome")
        {
           // bool useBrowserStack = true;
            bool useBrowserStack = Environment.GetEnvironmentVariable("USE_BROWSERSTACK")?.ToLower() == "true";
            //string deviceType = Environment.GetEnvironmentVariable("BROWSERSTACK_DEVICE") ?? browser;
            string deviceType = browser;
            if (!useBrowserStack)
            {
                switch (browser.ToLower())
                {
                    case "chrome_desktop":
                        var chromeOptions = new ChromeOptions();
                        chromeOptions.AddArgument("--start-maximized");
                        return new ChromeDriver(chromeOptions);

                    case "firefox_desktop":
                        var firefoxOptions = new OpenQA.Selenium.Firefox.FirefoxOptions();
                        return new OpenQA.Selenium.Firefox.FirefoxDriver(firefoxOptions);

                    case "edge":
                        var edgeOptions = new EdgeOptions();
                        return new EdgeDriver(edgeOptions);

                    case "edge-headless":
                        var edgeHeadless = new EdgeOptions();
                        edgeHeadless.AddArgument("--headless=new");
                        edgeHeadless.AddArgument("--disable-gpu");
                        return new EdgeDriver(edgeHeadless);

                    case "chrome-headless":
                        var chromeHeadless = new ChromeOptions();
                        chromeHeadless.AddArgument("--headless");
                        chromeHeadless.AddArgument("--disable-gpu");
                        return new ChromeDriver(chromeHeadless);

                    case "firefox-headless":
                        var firefoxHeadless = new OpenQA.Selenium.Firefox.FirefoxOptions();
                        firefoxHeadless.AddArgument("--headless");
                        return new OpenQA.Selenium.Firefox.FirefoxDriver(firefoxHeadless);

                    default:
                        throw new ArgumentException($"Unsupported local browser: {browser}");
                }
            }
            else
            {
                // BrowserStack configuration
                var browserstackOptions = new Dictionary<string, object>
                {
                    //i am not using it now 
                    //["userName"] = Environment.GetEnvironmentVariable("BROWSERSTACK_USERNAME"),
                    //["accessKey"] = Environment.GetEnvironmentVariable("BROWSERSTACK_ACCESS_KEY"),
                    //["buildName"] = "WebScraping Build",
                    ["sessionName"] = $"Test on {deviceType} - {Guid.NewGuid()}"
                };

                DriverOptions options;

                switch (deviceType.ToLower())
                {
                    case "chrome_desktop":
                        var chromeOpts = new ChromeOptions
                        {
                            BrowserVersion = "latest",
                            PlatformName = "Windows 11"
                        };
                        chromeOpts.AddAdditionalOption("bstack:options", browserstackOptions);
                        options = chromeOpts;
                        break;

                    case "firefox_desktop":
                        var firefoxOpts = new OpenQA.Selenium.Firefox.FirefoxOptions
                        {
                            BrowserVersion = "latest",
                            PlatformName = "Windows 10"
                        };
                        firefoxOpts.AddAdditionalOption("bstack:options", browserstackOptions);
                        options = firefoxOpts;
                        break;

                    case "safari_mac":
                        ChromeOptions SafChrOptions = new ChromeOptions();
                        browserstackOptions.Add("os", "OS X");
                        browserstackOptions.Add("osVersion", "Monterey");
                        browserstackOptions.Add("browserVersion", "latest");
                        SafChrOptions.AddAdditionalOption("bstack:options", browserstackOptions);
                        options = SafChrOptions;
                        break;

                    case "ipad":
                        SafariOptions Sfoption = new SafariOptions();
                        browserstackOptions.Add("osVersion", "18");
                        browserstackOptions.Add("deviceName", "iPad 9th");
                        Sfoption.AddAdditionalOption("bstack:options", browserstackOptions);
                        options = Sfoption;
                        break;


                    case "iphone":
                        var iosOptions = new AppiumOptions();
                        browserstackOptions["deviceName"] = "iPhone 15";
                        browserstackOptions["osVersion"] = "16";
                        browserstackOptions["realMobile"] = "true";
                        browserstackOptions["browserName"] = "Safari";
                        iosOptions.AddAdditionalAppiumOption("bstack:options", browserstackOptions);
                        options = iosOptions;
                        break;

                    case "android":
                        var androidOptions = new AppiumOptions();

                        // Browser name should be set to "Chrome" for Android mobile browser testing
                        //Dictionary<string, object> bstackOptions = new Dictionary<string, object>();
                        ChromeOptions capabilities = new ChromeOptions();
                        browserstackOptions.Add("osVersion", "13.0");
                        browserstackOptions.Add("deviceName", "Samsung Galaxy S23");
                        browserstackOptions.Add("consoleLogs", "info");
                        // capabilities.AddAdditionalOption("bstack:options", bstackOptions);

                        androidOptions.AddAdditionalAppiumOption("bstack:options", browserstackOptions);

                        options = androidOptions;
                        break;


                    default:
                        throw new ArgumentException($"Unsupported BrowserStack device type: {deviceType}");
                }
                return new RemoteWebDriver(new Uri("https://hub-cloud.browserstack.com/wd/hub/"), options.ToCapabilities(), TimeSpan.FromSeconds(600));
            }


            
        }

    }
}
