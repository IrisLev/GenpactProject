using Microsoft.Playwright;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GenpactProject.Api;

public class WikipediaApi
{
    public async Task<string> ExtractDebuggingFeaturesSectionAsync(IAPIRequestContext request)
    {
        // Use MediaWiki Parse API to get rendered HTML and then extract the specific section
        var apiUrl = "https://en.wikipedia.org/w/api.php";
        var parameters = new Dictionary<string, object>
        {
            { "action", "parse" },
            { "page", "Playwright_(software)" },
            { "prop", "sections|text" },
            { "format", "json" }
        };

        var response = await request.GetAsync(apiUrl, new APIRequestContextOptions { Params = parameters });
        if (response.Status != 200)
            throw new Exception($"Unexpected status code: {response.Status}");

        var json = await response.JsonAsync<System.Text.Json.JsonElement>();
        if (!json.TryGetProperty("parse", out var parse))
            throw new Exception("Parse data not found");

        // Identify index of the H2 section "Debugging features" by line or anchor
        int sectionIndex = -1;
        if (parse.TryGetProperty("sections", out var sectionsEl) && sectionsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var section in sectionsEl.EnumerateArray())
            {
                var line = section.GetProperty("line").GetString();
                var indexStr = section.GetProperty("index").GetString();
                var level = section.GetProperty("level").GetString();
                var anchor = section.TryGetProperty("anchor", out var aEl) ? aEl.GetString() : null;
                if (((string.Equals(line, "Debugging features", StringComparison.OrdinalIgnoreCase)) ||
                    (string.Equals(anchor, "Debugging_features", StringComparison.OrdinalIgnoreCase)))
                    && level == "2")
                {
                    if (int.TryParse(indexStr, out var idx)) sectionIndex = idx;
                    break;
                }
            }
        }

        if (sectionIndex == -1)
        {
            // Fallback: parse full HTML and extract section by H2 id
            var fullParams = new Dictionary<string, object>
            {
                { "action", "parse" },
                { "page", "Playwright_(software)" },
                { "prop", "text" },
                { "format", "json" }
            };
            var fullResp = await request.GetAsync(apiUrl, new APIRequestContextOptions { Params = fullParams });
            if (fullResp.Status != 200)
                throw new Exception($"Unexpected status code: {fullResp.Status}");
            var fullJson = await fullResp.JsonAsync<System.Text.Json.JsonElement>();
            var fullHtml = fullJson.GetProperty("parse").GetProperty("text").GetProperty("*").GetString() ?? string.Empty;
            // Capture content after the H2 that contains id="Debugging_features" and before next H2
            var m = Regex.Match(fullHtml, @"id=""Debugging_features""[\s\S]*?</h3>([\s\S]*?)(?=<h3)", RegexOptions.IgnoreCase);
            if (!m.Success) throw new Exception("Debugging features section index not found");
            var sectionHtml = m.Groups[1].Value;
            var sectionText = Regex.Replace(sectionHtml, @"<a\b[^>]*>(.*?)<\/a>", s => s.Groups[1].Value, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            sectionText = Regex.Replace(sectionText, @"<[^>]+>", " ");
            sectionText = Regex.Replace(sectionText, @"\s+", " ").Trim();
            return sectionText;
        }

        // Request just that section's HTML
        var sectionParams = new Dictionary<string, object>
        {
            { "action", "parse" },
            { "page", "Playwright_(software)" },
            { "prop", "text" },
            { "format", "json" },
            { "section", sectionIndex }
        };
        var sectionResp = await request.GetAsync(apiUrl, new APIRequestContextOptions { Params = sectionParams });
        if (sectionResp.Status != 200)
            throw new Exception($"Unexpected status code: {sectionResp.Status}");

        var sectionJson = await sectionResp.JsonAsync<System.Text.Json.JsonElement>();
        var html = sectionJson.GetProperty("parse").GetProperty("text").GetProperty("*").GetString() ?? string.Empty;

        // Strip HTML to text while preserving anchor texts
        // Replace <a> tags with their text content
        var text = Regex.Replace(html, @"<a\b[^>]*>(.*?)<\/a>", m => m.Groups[1].Value, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        // Remove all other tags
        text = Regex.Replace(text, @"<[^>]+>", " ");
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }
}
