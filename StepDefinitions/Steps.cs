using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;


namespace BS_TechAssement_WebScrapping.Steps
{
    [Binding]
    public class Steps
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly ScenarioContext _context;

        //private List<(string title, string content, string? imageUrl)> articles = new();

        public Steps(ScenarioContext scenarioContext)
        {
            _context = scenarioContext;
            _driver = (IWebDriver)scenarioContext["WebDriver"];
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
        }
        [Given(@"I visit the El País homepage on (.*)")]
        [Given(@"I visit the El País homepage")]
        public void GivenIVisitTheElPaisHomepage()
        {
            _driver.Navigate().GoToUrl("https://elpais.com/");

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(50));

            // Wait until the document is fully loaded
            //wait.Until(driver =>
            //    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").ToString() == "complete"
            //);

            // Optional: wait for main content to be visible (to confirm page is interactive)
            wait.Until(driver =>
            {
                try
                {
                    return driver.FindElement(By.TagName("main")).Displayed;
                }
                catch
                {
                    return false;
                }
            });

            // Now wait for cookie banner only if it appears (may take a few seconds after page load)
            try
            {
                wait.Until(driver =>
                {
                    try
                    {
                        var btn = driver.FindElement(By.Id("didomi-notice-agree-button"));
                        return btn.Displayed && btn.Enabled;
                    }
                    catch (NoSuchElementException)
                    {
                        return false;
                    }
                });

                var acceptButton = _driver.FindElement(By.Id("didomi-notice-agree-button"));
                acceptButton.Click();
                Console.WriteLine("==>Accept button clicked.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("==>Accept button not found within timeout. Likely already accepted.");
            }

        }

       
        //public void GivenIVisitTheElPaisHomepage(string browser)
        //{
        //    _driver.Navigate().GoToUrl("https://elpais.com/");
        //}


        [Then(@"I verify the page is displayed in Spanish")]
        public void ThenIVerifyThePageIsDisplayedInSpanish()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var actions = new Actions(_driver);

            var htmlElement = wait.Until(d => d.FindElement(By.TagName("html")));
            var htmlLang = htmlElement.GetAttribute("lang");

            Assert.IsNotNull(htmlLang, "The 'lang' attribute is missing from the <html> tag.");

            // If not already Spanish, attempt to switch edition
            if (!htmlLang.StartsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("==>Page is not in Spanish. Attempting to switch...");

                try
                {
                    // Hover over edition dropdown
                    var editionHead = wait.Until(d => d.FindElement(By.Id("edition_head")));
                    actions.MoveToElement(editionHead).Perform();

                    // Click on España edition
                    var espanaEditionLi = wait.Until(d => d.FindElement(By.CssSelector("li[data-edition='el-pais']")));
                    espanaEditionLi.Click();

                    // Wait until the lang changes to Spanish
                    wait.Until(d =>
                    {
                        var lang = d.FindElement(By.TagName("html")).GetAttribute("lang");
                        return lang != null && lang.StartsWith("es", StringComparison.OrdinalIgnoreCase);
                    });

                    Console.WriteLine("==>Switched to Spanish edition successfully.");
                }
                catch (Exception ex)
                {
                    Assert.Fail("Failed to switch to Spanish edition: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("==>Page is already in Spanish.");
            }

            // Final validation
            htmlLang = _driver.FindElement(By.TagName("html")).GetAttribute("lang");
            Assert.IsNotNull(htmlLang, "The 'lang' attribute is missing from the <html> tag after switch.");
            StringAssert.StartsWith("es", htmlLang, "The website is not displayed in Spanish.");
        }



        [When(@"I navigate to the Opinion section")]
        public void WhenINavigateToTheOpinionSection()
        {
            var hum = _wait.Until(d => d.FindElement(By.Id("btn_open_hamburger")));
            hum.Click();
            var opinionLink = _driver.FindElement(By.XPath("//a[contains(@href, '/opinion') and text()='Opinión']"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", opinionLink);
            //_driver.Navigate().GoToUrl("https://elpais.com/opinion/");
            //_wait.Until(d => d.Url.Contains("/opinion"));
        }

        [When(@"I fetch the first (.*) articles")]
        public void WhenIFetchTheFirstNArticles(int count)
        {
            // Wait until articles are present on the page and take the first 'count' articles
            var articlesOnPage = _wait.Until(d => d.FindElements(By.CssSelector("article")).Take(count).ToList());
            var articles = new List<(string title, string content, string? imageUrl)>();
            // Loop through each article and extract relevant details
            foreach (var article in articlesOnPage)
            {
                try
                {
                    // Try to extract the article title from either <h2> or <h3> tags
                    string title = article.FindElement(By.CssSelector("h2, h3")).Text.Trim();

                    // Get the full visible text content of the article
                    //string content = article.Text.Trim();
                    // Get the para only visible text content of the article if needed
                    var contentParagraphs = article.FindElements(By.CssSelector("p.c_d"));
                    string content = string.Join("\n", contentParagraphs.Select(p => p.Text.Trim()));

                    // Attempt to get the image URL if any <img> tag is found inside the article
                    string? imgUrl = null;
                    var img = article.FindElements(By.TagName("img")).FirstOrDefault();
                    if (img != null)
                    {
                        imgUrl = img.GetAttribute("src");
                    }

                    // Store the collected information into a list of articles
                    articles.Add((title, content, imgUrl));
                }
                catch
                {
                    Console.WriteLine("==>Entered in catch block of: I fetch the first (.*) articles ");
                }
                
            }
            _context["Articles"] = articles;
        }


        [Then(@"I print the Spanish titles and content of each article")]
        public void ThenIPrintTheSpanishTitlesAndContentOfEachArticle()
        {
            var articles = _context["Articles"] as List<(string title, string content, string? imageUrl)>;
            int index = 1; // Initialize a counter
            // Loop through each article tuple (title, content, image URL) in the 'articles' list
            foreach (var (title, content, _) in articles)
            {
                // Print the article title with a newspaper emoji for readability
                Console.WriteLine($"\n Title {index}: {title}");

                // Print the first 300 characters of the article content (or fewer if content is shorter)
                // This ensures we don't get an exception from Substring if the content is too short
                Console.WriteLine($" Content {index}: {content.Substring(0, Math.Min(content.Length, 300))}...\n");

                // Note: The underscore (_) is used to ignore the third item in the tuple (image URL), since it's not needed here
                index++; // Increment the counter
            }

        }

        [Then(@"I download the cover image of each article if available")]
        /// <summary>
        /// Downloads the cover image of each article (if available) and saves it locally.
        /// </summary>
        public void ThenIDownloadTheCoverImageOfEachArticleIfAvailable()
        {
            var articles = _context["Articles"] as List<(string title, string content, string? imageUrl)>;
            // Define the folder path to save images
            string imgFolder = Path.Combine(Directory.GetCurrentDirectory(), "DownloadedImages");

            // Ensure the directory exists
            Directory.CreateDirectory(imgFolder);

            // Loop through each article and process its image
            foreach (var (title, _, imgUrl) in articles)
            {
                // Skip if there is no image URL
                if (string.IsNullOrEmpty(imgUrl)) continue;

                try
                {
                    // Create a new HTTP client instance to fetch the image
                    using var client = new HttpClient();

                    // Download the image as a byte array
                    var imgData = client.GetByteArrayAsync(imgUrl).Result;

                    // Sanitize the title to create a valid filename
                    var fileName = Regex.Replace(title, @"[^\w\d]", "_") + ".jpg";

                    // Save the image to the specified folder
                    File.WriteAllBytes(Path.Combine(imgFolder, fileName), imgData);

                    Console.WriteLine($"Image saved: {fileName}");
                }
                catch
                {
                    // Handle any download or write failures gracefully
                    Console.WriteLine("==>Failed to download image.");
                }
            }
        }


        [When(@"I translate each article title to English")]
        public async Task WhenITranslateEachArticleTitleToEnglish()
        {
            var articles = _context["Articles"] as List<(string title, string content, string? imageUrl)>;
            // Create a list of translation tasks for each article title (from Spanish "es" to English "en")
            var tasks = articles.Select(article =>
                TranslationHelper.TranslateAsync(article.title, "es", "en")
            ).ToList();

            // Alternative helper (commented out) in case using a different translation service
            // var tasks = articles.Select(article =>
            //     OpenSourceTranslationHelper.TranslateAsync(article.title, "es", "en")
            // ).ToList();

            // Wait for all translation tasks to complete in parallel
            var translatedTitles = await Task.WhenAll(tasks);

            // Print each translated title to the console
            foreach (var title in translatedTitles)
                Console.WriteLine($"Translated: {title}");

            // Store the translated titles in the scenario context for use in later steps
            _context["TranslatedTitles"] = translatedTitles.ToList();
        }



        [Then(@"I print any words repeated more than twice across all translated headers")]
        public void ThenIPrintAnyRepeatedWords()
        {
            // Retrieve translated titles from the context
            var translatedTitles = _context["TranslatedTitles"] as List<string>;

            // Exit if no titles are available
            if (translatedTitles == null || translatedTitles.Count == 0)
            {
                Console.WriteLine("==>No translated titles found to analyze.");
                return;
            }

            // Dictionary to count occurrences of each word (case-insensitive)
            var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var title in translatedTitles)
            {
                // Use regex to extract words (ignores punctuation)
                var words = Regex.Matches(title.ToLower(), @"\b\w+\b").Select(m => m.Value);

                // Count each word
                foreach (var word in words)
                {
                    if (!wordCounts.ContainsKey(word))
                        wordCounts[word] = 0;
                    wordCounts[word]++;
                }
            }

            // Find words that appear more than once
            var repeated = wordCounts.Where(kvp => kvp.Value > 1).ToList();

            // Print results
            Console.WriteLine("==>\nRepeated Words:");
            if (repeated.Count == 0)
            {
                Console.WriteLine("==>No word repeated.");
            }
            else
            {
                foreach (var kvp in repeated)
                {
                    Console.WriteLine($" {kvp.Key}: {kvp.Value} time(s)");
                }
            }
        }

        [Then(@"I print the translated headers")]
        public void ThenIPrintTheTranslatedHeaders()
        {
            Console.WriteLine("==>Printing Translated Headers");

            // Check if the context dictionary contains the key "TranslatedTitles"
            if (_context.ContainsKey("TranslatedTitles"))
            {
                // Try to retrieve and cast the object to a List of strings
                var translatedTitles = _context["TranslatedTitles"] as List<string>;

                // Proceed only if the cast was successful and list is not empty
                if (translatedTitles != null && translatedTitles.Any())
                {
                    Console.WriteLine("==>Translated Headers:");

                    // Loop through each translated title and print it
                    foreach (var title in translatedTitles)
                    {
                        Console.WriteLine(title);
                    }
                }
                else
                {
                    // If list is null or empty, print a message
                    Console.WriteLine("==>Translated titles list is empty or invalid.");
                }
            }
            else
            {
                // If the key was not found in the context
                Console.WriteLine("==>No translated titles found in context.");
            }
        }


        [AfterScenario]
        public void DisposeBrowser()
        {
            _driver?.Quit();
        }
    }
}

