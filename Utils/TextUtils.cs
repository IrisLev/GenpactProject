using System.Text.RegularExpressions;

namespace GenpactProject.Utils;

public static class TextUtils
{
    public static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        var normalized = Regex.Replace(text.ToLowerInvariant(), @"[^a-z\s]", "");
        return Regex.Replace(normalized, @"\s+", " ").Trim();
    }

    public static int CountUniqueWords(string text)
    {
        var normalized = NormalizeText(text);
        if (string.IsNullOrEmpty(normalized))
            return 0;
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new HashSet<string>(words).Count;
    }
}
