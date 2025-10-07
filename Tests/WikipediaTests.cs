using GenpactProject.Pages;
using GenpactProject.Api;
using GenpactProject.Utils;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace GenpactProject.Tests;

[TestFixture]
public class WikipediaTests : PlaywrightTestBase
{
    private WikipediaPage _wikiPage = null!; // Non-nullable, initialized in SetUp
    private WikipediaApi _wikiApi = null!;   // Non-nullable, initialized in SetUp

    [SetUp]
    public async Task SetUp()
    {
        await SetupBrowser(); // Headless by default, toggle with HEADLESS=false
        _wikiPage = new WikipediaPage(Page);
        _wikiApi = new WikipediaApi();
    }

    [Test]
    public async Task Task1_CompareDebuggingFeaturesWordCount()
    {
        // UI Extraction
            var uiRaw = await _wikiPage.ExtractDebuggingFeaturesSectionAsync();
            var uiUniqueCount = TextUtils.CountUniqueWords(uiRaw);

        // API Extraction
        var apiRequest = await Playwright.APIRequest.NewContextAsync();
            var apiRaw = await _wikiApi.ExtractDebuggingFeaturesSectionAsync(apiRequest);
        await apiRequest.DisposeAsync();
            var apiUniqueCount = TextUtils.CountUniqueWords(apiRaw);

        // Log for debugging
            Console.WriteLine($"UI Text: {uiRaw}");
            Console.WriteLine($"API Text: {apiRaw}");
        Console.WriteLine($"UI Unique Words: {uiUniqueCount}");
        Console.WriteLine($"API Unique Words: {apiUniqueCount}");

        // Assert and report
        bool passed = uiUniqueCount == apiUniqueCount;
        ReportGenerator.AddResult("Task1_CompareDebuggingFeaturesWordCount", passed,
            passed ? "" : $"Counts differ. UI: {uiUniqueCount}, API: {apiUniqueCount}");
        Assert.That(uiUniqueCount, Is.EqualTo(apiUniqueCount),
            $"Unique word counts differ. UI: {uiUniqueCount}, API: {apiUniqueCount}");
    }

    [Test]
    public async Task Task2_ValidateMicrosoftDevToolsLinks()
    {
        var items = await _wikiPage.GetMicrosoftDevToolsAsync();
        bool allLinks = items.All(item => item.IsLink);
        string message = allLinks ? "" : $"Non-link items: {string.Join(", ", items.Where(i => !i.IsLink).Select(i => i.Text))}";

        ReportGenerator.AddResult("Task2_ValidateMicrosoftDevToolsLinks", allLinks, message);
        Assert.That(allLinks, Is.True, message);
    }

    [Test]
    public async Task Task3_ChangeAndVerifyDarkMode()
    {
        var isDark = await _wikiPage.ChangeColorToDarkAsync();

        ReportGenerator.AddResult("Task3_ChangeAndVerifyDarkMode", isDark,
            isDark ? "" : "Failed to switch to Dark mode");
        Assert.That(isDark, Is.True, "Failed to switch to Dark mode");
    }

    [TearDown]
    public async Task TearDown()
    {
        await CleanupBrowser();
    }

    [OneTimeTearDown]
    public void GenerateReport()
    {
        ReportGenerator.GenerateHtmlReport("TestReport.html");
    }
}
 