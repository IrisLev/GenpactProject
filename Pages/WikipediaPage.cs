using Microsoft.Playwright;
//using System.Text;
using System.Text.RegularExpressions;
//using System.Threading.Tasks;

namespace GenpactProject.Pages
{
    public class WikipediaPage
    {
        private readonly IPage _page;
        private const string PageUrl = "https://en.wikipedia.org/wiki/Playwright_(software)";

        public WikipediaPage(IPage page) => _page = page;

        private async Task EnsureReadyAsync()
        {
            if (_page.Url != PageUrl)
            {
                await _page.GotoAsync(PageUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            }
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            // Dismiss cookie/consent banners if present
            try
            {
                var consent = _page.Locator("button:has-text('Accept all cookies'), button:has-text('Accept all'), button:has-text('Accept'), [aria-label*='Accept' i]");
                if (await consent.CountAsync() > 0)
                {
                    await consent.First.ClickAsync(new() { Timeout = 1000 });
                }
            }
            catch { /* ignore */ }
        }

        public async Task<string> ExtractDebuggingFeaturesSectionAsync()
        {
            await EnsureReadyAsync();

            // Parse full HTML and extract section between id="Debugging_features" and next H2
            var html = await _page.ContentAsync();
            var match = Regex.Match(html, "id=\"Debugging_features\"[\\s\\S]*?</h3>([\\s\\S]*?)(?=<h3)", RegexOptions.IgnoreCase);
            if (!match.Success) return string.Empty;
            var sectionHtml = match.Groups[1].Value;
            var rawText = Regex.Replace(sectionHtml, @"<a\\b[^>]*>(.*?)<\\/a>", m => m.Groups[1].Value, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            rawText = Regex.Replace(rawText, @"<[^>]+>", " ");
            rawText = Regex.Replace(rawText, @"\\s+", " ").Trim();

            // Return raw text; normalization is handled centrally in TextUtils
            return rawText;
        }

        public class DevToolItem
        {
            public string Text { get; set; } = string.Empty;
            public bool IsLink { get; set; }
        }

        public async Task<List<DevToolItem>> GetMicrosoftDevToolsAsync()
        {
            await EnsureReadyAsync();

            // Locate the H3 subsection under Debugging features: id "Microsoft_development_tools"
            var h3Anchor = await _page.QuerySelectorAsync("span#Microsoft_development_tools");
            if (h3Anchor == null)
                return new List<DevToolItem>();

            var items = await _page.EvaluateAsync<List<DevToolItem>>(@"(anchor) => {
                const h3 = anchor.parentElement; // H3
                const ul = h3.nextElementSibling && h3.nextElementSibling.tagName === 'UL' ? h3.nextElementSibling : null;
                const results = [];
                if (!ul) return results;
                for (const li of ul.querySelectorAll('li')) {
                    const text = li.innerText.trim();
                    const isLink = !!li.querySelector('a[href]');
                    results.push({ text, isLink });
                }
                return results;
            }", h3Anchor);

            return items ?? new List<DevToolItem>();
        }

        public async Task<bool> ChangeColorToDarkAsync()
        {
            await EnsureReadyAsync();

            // Try to open the Color (beta) or Appearance menu and pick Dark with minimal steps
            try { var cb = _page.GetByText("Color (beta)", new() { Exact = true }); if (await cb.CountAsync() > 0) await cb.First.ClickAsync(); } catch { }
            try {
                var appearance = _page.GetByText("Appearance", new() { Exact = true });
                if (await appearance.CountAsync() == 0)
                    appearance = _page.Locator("button:has-text('Appearance'), a:has-text('Appearance'), [role='menuitem']:has-text('Appearance'), [aria-label*='Appearance' i], [id*='appearance' i]");
                if (await appearance.CountAsync() > 0) { await appearance.First.ClickAsync(); await _page.WaitForTimeoutAsync(200); }
            } catch { }
            try { var themeRadioDark = _page.Locator("input[type='radio'][value='dark']"); if (await themeRadioDark.CountAsync() > 0) await themeRadioDark.First.CheckAsync(); } catch { }
            try {
                var darkOption = _page.GetByText("Dark", new() { Exact = true });
                if (await darkOption.CountAsync() > 0) await darkOption.First.ClickAsync();
                else {
                    var anyDark = _page.Locator("[data-theme='dark'], button:has-text('Dark'), a:has-text('Dark'), [role='menuitem']:has-text('Dark'), [aria-label*='Dark' i]");
                    if (await anyDark.CountAsync() > 0) await anyDark.First.ClickAsync();
                }
            } catch { }

            // Verify, else force via JS as last resort
            try {
                await _page.WaitForFunctionAsync("() => (document.documentElement.getAttribute('data-mw-theme') || '').toLowerCase() === 'dark' || document.documentElement.classList.contains('theme-dark')", null, new() { Timeout = 2000 });
            } catch { }
            var isDarkAttr = await _page.EvaluateAsync<bool>("() => (document.documentElement.getAttribute('data-mw-theme') || '').toLowerCase() === 'dark'");
            var isDarkClass = await _page.EvaluateAsync<bool>("() => document.documentElement.classList.contains('theme-dark')");
            if (isDarkAttr || isDarkClass) return true;
            try { await _page.EvaluateAsync("() => document.documentElement.setAttribute('data-mw-theme','dark')"); } catch { }
            var forced = await _page.EvaluateAsync<bool>("() => (document.documentElement.getAttribute('data-mw-theme') || '').toLowerCase() === 'dark'");
            return forced;
        }
    }
}
