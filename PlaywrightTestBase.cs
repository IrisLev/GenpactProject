using Microsoft.Playwright.NUnit;
using System.Threading.Tasks;

namespace GenpactProject;

public class PlaywrightTestBase : PageTest
{
    public Task SetupBrowser()
    {
        // Page, Context and Browser are managed by PageTest fixtures.
        return Task.CompletedTask;
    }

    public Task CleanupBrowser()
    {
        // Cleanup is handled by PageTest. Nothing to do here.
        return Task.CompletedTask;
    }
}
