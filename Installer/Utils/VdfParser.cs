namespace Installer.Utils;

/// <summary>
/// Simple parser for Steam's libraryfolders.vdf to extract library paths.
/// </summary>
public static class VdfParser
{
    /// <summary>
    /// Parses a VDF file and extracts all library paths.
    /// </summary>
    public static List<string> ParseLibraryFolders(string vdfContent)
    {
        var paths = new List<string>();

        // VDF format uses "path" keys to specify library locations
        // Example:
        // "0"
        // {
        //     "path"		"C:\\Program Files (x86)\\Steam"
        // }

        var lines = vdfContent.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Look for "path" entries
            if (trimmed.StartsWith("\"path\"", StringComparison.OrdinalIgnoreCase))
            {
                var path = ExtractQuotedValue(trimmed);
                if (!string.IsNullOrEmpty(path))
                {
                    // Unescape backslashes
                    path = path.Replace("\\\\", "\\");
                    paths.Add(path);
                }
            }
        }

        return paths;
    }

    private static string? ExtractQuotedValue(string line)
    {
        // Format: "key"		"value"
        // Find the second quoted string
        var firstQuote = line.IndexOf('"');
        if (firstQuote < 0)
            return null;

        var secondQuote = line.IndexOf('"', firstQuote + 1);
        if (secondQuote < 0)
            return null;

        var thirdQuote = line.IndexOf('"', secondQuote + 1);
        if (thirdQuote < 0)
            return null;

        var fourthQuote = line.IndexOf('"', thirdQuote + 1);
        if (fourthQuote < 0)
            return null;

        return line.Substring(thirdQuote + 1, fourthQuote - thirdQuote - 1);
    }
}
