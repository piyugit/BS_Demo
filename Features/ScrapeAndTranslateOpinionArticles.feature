Feature: ScrapeAndTranslateOpinionArticles
   @browser:chrome_desktop
  Scenario: Scrape and Translate Opinion Articles
    Given I visit the El País homepage
    Then I verify the page is displayed in Spanish
    When I navigate to the Opinion section
    And I fetch the first 1 articles
    Then I print the Spanish titles and content of each article
    And I download the cover image of each article if available
    When I translate each article title to English
    Then I print the translated headers
    Then I print any words repeated more than twice across all translated headers


  Scenario Outline: Scrape and Translate Opinion Articles on <Browsers>
    Given I visit the El País homepage
    #Given I visit the El País homepage on <Browsers>
    Then I verify the page is displayed in Spanish
    When I navigate to the Opinion section
    And I fetch the first 1 articles
    Then I print the Spanish titles and content of each article
    And I download the cover image of each article if available
    When I translate each article title to English
    Then I print the translated headers
    Then I print any words repeated more than twice across all translated headers

    Examples:
      | Browsers        |
      #| chrome           |
      #| firefox          |
      #| edge            |
      #| chrome-headless  |
      #| firefox-headless |
      #| edge-headless   |
      | chrome_desktop  |
      | firefox_desktop |
      | ipad            |
      | android         |
      | iphone          |
