using System.Text;
using System.Text.RegularExpressions;

namespace BetterAmongUs.Modules;

/// <summary>
/// Provides utilities for handling text files, including filtering, parsing, and formatting.
/// </summary>
internal static class TextFileHandler
{
    /// <summary>
    /// Compares strings against filter patterns in a file using wildcard matching.
    /// </summary>
    /// <param name="filePath">Path to the filter file.</param>
    /// <param name="strings">Array of strings to check against filters.</param>
    /// <returns>True if any string matches a filter pattern, false otherwise.</returns>
    internal static bool CompareStringFilters(string filePath, string[] strings)
    {
        foreach (var content in ReadContents(filePath))
        {
            foreach (var text in strings)
            {
                if (CheckFilterString(content, text))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Compares strings for exact matches with content in a file.
    /// </summary>
    /// <param name="filePath">Path to the file containing match strings.</param>
    /// <param name="strings">Array of strings to check for exact matches.</param>
    /// <returns>True if any string exactly matches content in the file, false otherwise.</returns>
    internal static bool CompareStringMatch(string filePath, string[] strings)
    {
        var stringSet = new HashSet<string>(
            strings.Select(s => s.ToLower().Trim()),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var content in ReadContents(filePath))
        {
            string normalizedContent = content.ToLower().Trim();

            if (stringSet.Contains(normalizedContent))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Reads and parses content from a file, ignoring comments and empty lines.
    /// </summary>
    /// <param name="filePath">Path to the file to read.</param>
    /// <returns>Enumerable collection of trimmed, non-empty content strings.</returns>
    private static IEnumerable<string> ReadContents(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadLines(filePath)
                       .Where(line => !string.IsNullOrWhiteSpace(line) &&
                              !line.StartsWith("//") &&
                              !line.StartsWith("#"))
                       .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(s => s.Trim()));
        }

        return [];
    }

    /// <summary>
    /// Checks if a text matches a filter pattern with wildcard support.
    /// </summary>
    /// <param name="filter">The filter pattern (supports ** wildcards).</param>
    /// <param name="text">The text to check.</param>
    /// <returns>True if the text matches the filter pattern, false otherwise.</returns>
    private static bool CheckFilterString(string filter, string text)
    {
        string pattern = filter switch
        {
            _ when filter.StartsWith("**") && filter.EndsWith("**") => Regex.Escape(filter.Trim('*')), // Contains anywhere
            _ when filter.StartsWith("**") => Regex.Escape(filter.TrimStart('*')) + "$", // Ends with
            _ when filter.EndsWith("**") => "^" + Regex.Escape(filter.TrimEnd('*')), // Starts with
            _ => "^" + Regex.Escape(filter) + "$" // Exact match
        };

        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    /// <summary>
    /// Parses a YAML-like string into a dictionary of key-value pairs.
    /// </summary>
    /// <param name="input">The YAML string to parse.</param>
    /// <returns>Dictionary containing parsed key-value pairs.</returns>
    internal static Dictionary<string, string> ParseYaml(string input)
    {
        var result = new Dictionary<string, string>();
        var lines = input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        string? currentKey = null;
        StringBuilder currentValue = new();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip empty lines
            if (string.IsNullOrEmpty(trimmedLine))
                continue;

            var separatorIndex = line.IndexOf(':');

            // Check if this is a key-value line (has colon and is not in the middle of content)
            if (separatorIndex > -1 && IsKeyValueLine(line, separatorIndex))
            {
                // Save previous key-value pair if it exists
                if (currentKey != null && currentValue.Length > 0)
                {
                    result[currentKey] = currentValue.ToString().Trim();
                    currentValue.Clear();
                }

                var key = line[..separatorIndex].Trim();
                if (key.StartsWith('^'))
                {
                    key = key[1..].Trim();
                }
                var value = line[(separatorIndex + 1)..].Trim();

                currentKey = key;
                if (!string.IsNullOrEmpty(value))
                {
                    currentValue.Append(value);
                }
            }
            else if (currentKey != null)
            {
                // This is a continuation line for the current value
                if (currentValue.Length > 0)
                    currentValue.Append('\n');
                currentValue.Append(line.Trim());
            }
        }

        if (currentKey != null && currentValue.Length > 0)
        {
            result[currentKey] = currentValue.ToString().Trim();
        }

        return result;
    }

    /// <summary>
    /// Determines if a line represents a YAML key-value pair.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <param name="separatorIndex">Index of the colon separator.</param>
    /// <returns>True if the line is a valid key-value pair, false otherwise.</returns>
    private static bool IsKeyValueLine(string line, int separatorIndex)
    {
        string keyPart = line[..separatorIndex].Trim();

        if (!keyPart.StartsWith('^') || keyPart.Contains(' ') || keyPart.Contains('#') ||
            keyPart.Contains('*') || keyPart.Contains('[') || keyPart.Contains('`'))
            return false;

        return true;
    }

    /// <summary>
    /// Converts markdown-like syntax to Unity rich text format.
    /// </summary>
    /// <param name="text">The text to format.</param>
    /// <returns>The text formatted with Unity rich text tags.</returns>
    internal static string FormatToRichText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = ProcessHeaders(text);
        text = ProcessBlockQuotes(text);
        text = ProcessHorizontalRules(text);
        text = ProcessBoldAndItalic(text);
        text = ProcessStrikethrough(text);
        text = ProcessLinks(text);
        text = ProcessInlineCode(text);
        text = ProcessLineBreaks(text);

        return text;
    }

    /// <summary>
    /// Processes markdown headers (# through ######) to Unity size tags.
    /// </summary>
    private static string ProcessHeaders(string text)
    {
        var headerSizes = new Dictionary<int, int>
        {
            {1, 200}, {2, 180}, {3, 160}, {4, 140}, {5, 120}, {6, 110}
        };

        foreach (var header in headerSizes)
        {
            string pattern = @"^(#{1," + header.Key + @"})\s+(.+?)(?=\n|$)";
            string replacement = $"<size={header.Value}%><b>$2</b></size>\n";
            text = Regex.Replace(text, pattern, replacement, RegexOptions.Multiline);
        }

        return text;
    }

    /// <summary>
    /// Processes block quotes (>) to styled text.
    /// </summary>
    private static string ProcessBlockQuotes(string text)
    {
        return Regex.Replace(text, @"^>\s+(.+?)(?=\n|$)", "<color=#888888><i>│ $1</i></color>", RegexOptions.Multiline);
    }

    /// <summary>
    /// Processes horizontal rules (---, ***, ___) to Unicode line characters.
    /// </summary>
    private static string ProcessHorizontalRules(string text)
    {
        return Regex.Replace(text, @"^\s*([-*_]){3,}\s*$", "───────────────────────────", RegexOptions.Multiline);
    }

    /// <summary>
    /// Processes bold (**), italic (*), and bold-italic (***) markdown.
    /// </summary>
    private static string ProcessBoldAndItalic(string text)
    {
        text = Regex.Replace(text, @"(\*\*|__)(?![*\s])(.*?)(?<![*\s])\1", "<b>$2</b>");
        text = Regex.Replace(text, @"(\*|_)(?![*\s])(.*?)(?<![*\s])\1", "<i>$2</i>");
        text = Regex.Replace(text, @"(\*\*\*|___)(?![*\s])(.*?)(?<![*\s])\1", "<b><i>$2</i></b>");

        return text;
    }

    /// <summary>
    /// Processes strikethrough (~~) markdown.
    /// </summary>
    private static string ProcessStrikethrough(string text)
    {
        return Regex.Replace(text, @"~~(.+?)~~", "<s>$1</s>");
    }

    /// <summary>
    /// Processes markdown links ([text](url)) to Unity link tags.
    /// </summary>
    private static string ProcessLinks(string text)
    {
        return Regex.Replace(text, @"\[([^\]]+)\]\(([^)]+)\)",
            match =>
            {
                string name = match.Groups[1].Value;
                string link = match.Groups[2].Value;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(link))
                    return match.Value;

                return $"<link=\"{link}\"> <b>{name}</b></link> ";
            });
    }

    /// <summary>
    /// Processes inline code (`code`) to styled text.
    /// </summary>
    private static string ProcessInlineCode(string text)
    {
        return Regex.Replace(text, @"`([^`]+)`", "<color=#FF8C00><size=85%>$1</size></color>");
    }

    /// <summary>
    /// Normalizes line breaks for better text flow.
    /// </summary>
    private static string ProcessLineBreaks(string text)
    {
        return Regex.Replace(text, @"\n\s*\n", "\n\n");
    }
}