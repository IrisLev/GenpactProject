using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace GenpactProject;

[TestFixture]
public class AlertTests : PageTest
{
    [SetUp]
    public async Task SetUp()
    {
        await Page.GotoAsync("https://the-internet.herokuapp.com/javascript_alerts");
    }

    [Test]
    public async Task TestJsAlert()
    {
        Page.Dialog += async (_, dialog) =>
        {
            Assert.That(dialog.Type, Is.EqualTo("alert"), "Expected an alert dialog");
            Assert.That(dialog.Message, Is.EqualTo("I am a JS Alert"), "Unexpected alert message");
            await dialog.AcceptAsync();
        };

        await Page.GetByText("Click for JS Alert").ClickAsync();

        var result = await Page.Locator("#result").TextContentAsync();
        Assert.That(result, Is.EqualTo("You successfully clicked an alert"));
    }

    [TearDown]
    public async Task TearDown()
    {
        await Page.CloseAsync();
    }
}

