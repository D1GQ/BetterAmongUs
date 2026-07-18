using BetterAmongUs.Utilities;
using System.Text;

namespace BetterAmongUs.Structs;

/// <summary>
/// Represents a mutable string builder that concatenates text with a specified separator.
/// </summary>
internal readonly struct SplitStringBuilder(int capacity, char? separator = null)
{
    private readonly char? _separator = separator;
    private readonly StringBuilder _value = new(capacity);

    /// <summary>
    /// Appends a text segment to the current string with automatic separator insertion.
    /// </summary>
    /// <param name="text">The text segment to append.</param>
    /// <returns>The current <see cref="SplitStringBuilder"/> instance for fluent method chaining.</returns>
    internal SplitStringBuilder Append(string text)
    {
        if (Utils.RemoveHtmlText(text) == string.Empty)
            return this;

        if (_value.Length > 0)
        {
            if (_separator != null)
            {
                _value.Append($" {_separator} ");
            }
            else
            {
                _value.Append(' ');
            }
        }

        _value.Append(text);
        return this;
    }

    /// <summary>
    /// Appends a formatted string to the current builder with automatic separator insertion.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="strings">An array of objects to format.</param>
    /// <returns>The current <see cref="SplitStringBuilder"/> instance for fluent method chaining.</returns>
    internal SplitStringBuilder AppendFormat(string format, params string[] strings)
    {
        return Append(string.Format(format, strings));
    }

    /// <summary>
    /// Clears all content from the string builder, resetting it to an empty state.
    /// </summary>
    internal void Clear()
    {
        _value.Clear();
    }

    /// <summary>
    /// Returns the complete concatenated string with all appended segments.
    /// </summary>
    /// <returns>
    /// The full string containing all appended text segments separated by the configured separator.
    /// Returns <see cref="string.Empty"/> if no text has been appended.
    /// </returns>
    public readonly override string ToString()
    {
        return _value.ToString();
    }
}