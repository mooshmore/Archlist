using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// !!! MANY OF THE METHODS HERE HAVEN'T BEEN TESTED. 
    /// As far as they should work properly because I've coded them ( ͡° ͜ʖ ͡°), make sure to check that they work right before you use them. 
    /// 
    /// A class that contains multiple methods that extend existing string methods and add a new ones, like adding 
    /// case sensitive variants to existing methods and adds some now ones, like number of occurences or shorten.
    /// </summary>
    public static class StringFunctionsExtensions
    {
        #region Contains

        /// <summary>
        /// Returns a value indicating whether a specified character occurs within this string with option to ignore case sensitivity.
        /// The search starts at a specified character position.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not..</param>
        /// <returns>true if the value parameter occurs within this string, or if value is the empty string (""); otherwise, false.</returns>
        public static bool Contains(this string processedText, string value, bool caseSensitive) => processedText.Contains(value, 0, processedText.Length, caseSensitive);

        /// <summary>
        /// Returns a value indicating whether a specified character occurs within this string with option to ignore case sensitivity.
        /// The search starts at a specified character position.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>true if the value parameter occurs within this string, or if value is the empty string (""); otherwise, false.</returns>
        public static bool Contains(this string processedText, string value, int startIndex, bool caseSensitive = true) => processedText.Contains(value, startIndex, processedText.Length - startIndex, caseSensitive);

        /// <summary>
        /// Returns a value indicating whether a specified character occurs within this string with option to ignore case sensitivity.
        /// The search starts at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>true if the value parameter occurs within this string, or if value is the empty string (""); otherwise, false.</returns>
        public static bool Contains(this string processedText, string value, int startIndex, int count, bool caseSensitive = true)
        {
            return caseSensitive
                ? processedText.Substring(startIndex, count).Contains(value)
                : processedText.Substring(startIndex, count).IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1;
        }

        #endregion

        #region Index of

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string within this instance
        /// with option to ignore case sensitivity.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in anyOf was found; -1 if no character in anyOf was found.</returns>
        public static int IndexOf(this string processedText, string value, bool caseSensitive) => processedText.IndexOf(value, 0, value.Length, caseSensitive);

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string in this instance
        /// with option to ignore case sensitivity. The search starts at a specified character position.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in anyOf was found; -1 if no character in anyOf was found.</returns>
        public static int IndexOf(this string processedText, string value, int startIndex, bool caseSensitive) => processedText.IndexOf(value, startIndex, value.Length - startIndex, caseSensitive);

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string in this instance
        /// with option to ignore case sensitivity. The search starts at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in anyOf was found; -1 if no character in anyOf was found.</returns>
        public static int IndexOf(this string processedText, string value, int startIndex, int count, bool caseSensitive)
        {
            return caseSensitive
                ? processedText.Substring(startIndex, count).IndexOf(value)
                : processedText.Substring(startIndex, count).IndexOf(value, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Last Index of

        /// <summary>
        /// Reports the zero-based index position of the last occurrence of a specified string within this instance 
        /// with option to ignore case sensitivity.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in anyOf was found; -1 if no character in anyOf was found.</returns>
        public static int LastIndexOf(this string processedText, string value, bool caseSensitive) => processedText.LastIndexOf(value, 0, value.Length, caseSensitive);

        /// <summary>
        /// Reports the zero-based index position of the last occurrence of a specified string within this instance with option 
        /// to ignore case sensitivity. The search starts at a specified character position and proceeds backward toward the beginning of the string.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in anyOf was found; -1 if no character in anyOf was found.</returns>
        public static int LastIndexOf(this string processedText, string value, int startIndex, bool caseSensitive) => processedText.LastIndexOf(value, startIndex, value.Length - startIndex, caseSensitive);

        /// <summary>
        /// Reports the zero-based index position of the last occurrence of a specified string within this instance with option 
        /// to ignore case sensitivity. The search starts at a specified character position and proceeds backward toward the 
        /// beginning of the string for a specified number of character positions.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>The zero-based index position of the first occurrence in this instance where any character in anyOf was found; -1 if no character in anyOf was found.</returns>
        public static int LastIndexOf(this string processedText, string value, int startIndex, int count, bool caseSensitive)
        {
            return caseSensitive
                ? processedText.Substring(startIndex, count).LastIndexOf(value)
                : processedText.Substring(startIndex, count).LastIndexOf(value, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Number of occurrences

        /// <summary>
        /// Reports the zero-based index of the number of occurrences of a specified string within this instance
        /// with option to ignore case sensitivity.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>Number of occurrences of a declared value in a string.</returns>
        public static int NumberOfOccurences(this string processedText, string value, bool caseSensitive = true) => processedText.NumberOfOccurences(value, 0, value.Length, caseSensitive);

        /// <summary>
        /// Reports the zero-based index of the number of occurrences of a specified string within this instance
        /// with option to ignore case sensitivity. 
        /// The search starts at a specified character position.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>Number of occurrences of a declared value in a string.</returns>
        public static int NumberOfOccurences(this string processedText, string value, int startIndex, bool caseSensitive = true) => processedText.NumberOfOccurences(value, startIndex, value.Length - startIndex, caseSensitive);

        /// <summary>
        /// Reports the zero-based index of the number of occurrences of a specified string within this instance 
        /// with option to ignore case sensitivity. 
        /// The search starts at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>Number of occurrences of a declared value in a string.</returns>
        public static int NumberOfOccurences(this string processedText, string value, int startIndex, int count, bool caseSensitive = true)
        {
            return caseSensitive
                ? (processedText.Substring(startIndex, count).Length - processedText.Substring(startIndex, count).Replace(value, "").Length) / value.Length
                : (processedText.Substring(startIndex, count).Length - processedText.Substring(startIndex, count).ToLower().Replace(value.ToLower(), "").Length) / value.Length;
        }

        #endregion

        #region Replace

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string
        /// with option to ignore case sensitivity. 
        /// </summary>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of oldValue.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of oldValue are replaced with newValue. If oldValue is not found in the current instance, the method returns the current instance unchanged.</returns>
        public static string Replace(this string processedText, string oldValue, string newValue, bool caseSensitive) => processedText.Replace(oldValue, newValue, 0, processedText.Length, caseSensitive);

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string
        /// with option to ignore case sensitivity. The replacement starts at a specified character position.
        /// </summary>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of oldValue.</param>
        /// <param name="startIndex">The replacement starting position.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of oldValue are replaced with newValue. If oldValue is not found in the current instance, the method returns the current instance unchanged.</returns>
        public static string Replace(this string processedText, string oldValue, string newValue, int startIndex, bool caseSensitive = true) => processedText.Replace(oldValue, newValue, startIndex, processedText.Length - startIndex, caseSensitive);

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string
        /// with option to ignore case sensitivity. The replacement starts at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of oldValue.</param>
        /// <param name="startIndex">The replacement starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of oldValue are replaced with newValue. If oldValue is not found in the current instance, the method returns the current instance unchanged.</returns>
        public static string Replace(this string processedText, string oldValue, string newValue, int startIndex, int count, bool caseSensitive = true)
        {
            return caseSensitive
                ? processedText.Substring(startIndex, count).Replace(oldValue, newValue)
                : Regex.Replace(processedText.Substring(startIndex, count), oldValue, newValue, RegexOptions.IgnoreCase);
        }

        #endregion

        #region Reverse

        /// <summary>
        /// Reverses the given string.
        /// </summary>
        /// <returns>The reversed string.</returns>
        public static string Reverse(this string text)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        #endregion

        #region All indexes of

        /// <summary>
        /// Reports the zero-based index of the all occurrences of the specified string within this instance with option to ignore case sensitivity.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>List of index positions of all occurrences in this instance where any character in anyOf was found. The list returns empty if no instances have been found.</returns>
        public static List<int> AllIndexesOf(this string processedText, string value, bool caseSensitive = true) => processedText.AllIndexesOf(value, 0, value.Length, caseSensitive);

        /// <summary>
        /// Reports the zero-based index of the all occurrences of the specified string within this instance with option to ignore case sensitivity.
        /// The search starts at a specified character position.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>List of index positions of all occurrences in this instance where any character in anyOf was found. The list returns empty if no instances have been found.</returns>
        public static List<int> AllIndexesOf(this string processedText, string value, int startIndex, bool caseSensitive = true) => processedText.AllIndexesOf(value, startIndex, value.Length - startIndex, caseSensitive);

        /// <summary>
        /// Reports the zero-based index of the all occurrences of the specified string within this instance with option to ignore case sensitivity.
        /// The search starts at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="caseSensitive">The value that decides whether function ignores case sensitivity or not.</param>
        /// <returns>List of index positions of all occurrences in this instance where any character in anyOf was found. The list returns empty if no instances have been found.</returns>
        public static List<int> AllIndexesOf(this string processedText, string value, int startIndex, int count, bool caseSensitive = true)
        {
            var indexesList = new List<int>();
            processedText = processedText.Substring(startIndex, count);
            if (caseSensitive)
            {
                for (int i = processedText.IndexOf(value); i > -1; i = processedText.IndexOf(value, i + 1))
                {
                    indexesList.Add(i);
                }
            }
            else
            {
                for (int i = processedText.IndexOf(value, StringComparison.OrdinalIgnoreCase); i > -1; i = processedText.IndexOf(value, i + 1, StringComparison.OrdinalIgnoreCase))
                {
                    indexesList.Add(i);
                }
            }
            return indexesList;
        }

        #endregion

        #region Shorten

        /// <summary>
        /// Shortens the string by deleting the given amount of characters from the back of the string.
        /// </summary>
        /// <param name="amountOfCharacters">The amount of characters to delete at the back.</param>
        /// <returns>The string shorter by the given amount of characters.</returns>
        public static string Shorten(this string text, int amountOfCharacters) => text.Substring(0, text.Length - amountOfCharacters);

        #endregion

        #region Trim frontBack

        /// <summary>
        /// Removes the given amount of characters from the front and the back.
        /// </summary>
        /// <param name="text">The text to be trimmed.</param>
        /// <param name="amountOfCharacters_front">Amount of characters to be removed from the front.</param>
        /// <param name="amountOfCharacters_back">Amount of characters to be removed from the back.</param>
        /// <returns>The trimmed text.</returns>
        public static string Trim_FrontBack(this string text, int amountOfCharacters_front, int amountOfCharacters_back) => text.Substring(amountOfCharacters_front, text.Length - amountOfCharacters_front - amountOfCharacters_back);

        #endregion

        #region Front

        /// <summary>
        /// Returns a text trimmed to the given amount of characters.
        /// </summary>
        /// <param name="text">The text to be trimmed.</param>
        /// <param name="amountOfCharacters">The amount of characters that will be left from the beginning of the string.</param>
        /// <returns>The trimmed text.</returns>
        public static string Front(this string text, int amountOfCharacters) => text.Substring(0, amountOfCharacters);

        #endregion

        #region Remove/Replace whitespace

        /// <summary>
        /// Returns a string with all whitespaces (including line breaks) removed.
        /// </summary>
        /// <param name="text">The text to be converted.</param>
        /// <returns>A string with all of the whitespaces removed.</returns>
        public static string RemoveWhitespace(this string text) => new Regex(@"\s+").Replace(text, "");

        /// <summary>
        /// Retruns a string with all whitespaces (including line breaks) replaced with the given text.
        /// </summary>
        /// <param name="text">The text to be converted.</param>
        /// <param name="replacement">The string that will replace the whitespaces.</param>
        /// <returns>A string with all whitespaces replaced with the given text.</returns>
        public static string ReplaceWhitespace(this string text, string replacement) => new Regex(@"\s+").Replace(text, replacement);

        #endregion

        #region Trim From/To

        /// <summary>
        /// Trims the text from the start of the text to the last position of the searched value.
        /// </summary>
        /// <param name="text">The text to be trimmed.</param>
        /// <param name="searchedValue">The last value that the text will be trimmed to.</param>
        /// <param name="includeSearchedValue">Whether to include the searched value in the returned text or not. False by default.</param>
        /// <returns>The trimmed text.</returns>
        public static string TrimToLast(this string text, string searchedValue, bool includeSearchedValue = false)
        {
            return includeSearchedValue
                ? text.Substring(0, text.LastIndexOf(searchedValue) + searchedValue.Length)
                : text.Substring(0, text.LastIndexOf(searchedValue));
        }

        /// <summary>
        /// Trims the text from the last position of the searched value to the end of the string.
        /// </summary>
        /// <param name="text">The text to be trimmed.</param>
        /// <param name="searchedValue">The last value that the text will be trimmed to.</param>
        /// <param name="includeSearchedValue">Whether to include the searched value in the returned text or not. True by default.</param>
        /// <returns>The trimmed text.</returns>
        public static string TrimFromLast(this string text, string searchedValue, bool includeSearchedValue = false)
        {
            return includeSearchedValue
                ? text[text.LastIndexOf(searchedValue)..]
                : text[(text.LastIndexOf(searchedValue) + searchedValue.Length)..];
        }

        /// <summary>
        /// Trims the text from the start of the text to the first position of the searched value.
        /// </summary>
        /// <param name="text">The text to be trimmed.</param>
        /// <param name="searchedValue">The first value that the text will be trimmed to.</param>
        /// <param name="includeSearchedValue">Whether to include the searched value in the returned text or not. False by default.</param>
        /// <returns>The trimmed text.</returns>
        public static string TrimTo(this string text, string searchedValue, bool includeSearchedValue = false)
        {
            return includeSearchedValue
                ? text.Substring(0, text.IndexOf(searchedValue) + searchedValue.Length)
                : text.Substring(0, text.IndexOf(searchedValue));
        }

        /// <summary>
        /// Trims the text from the first position of the searched value to the end of the string.
        /// </summary>
        /// <param name="text">The text to be trimmed.</param>
        /// <param name="searchedValue">The first value that the text will be trimmed to.</param>
        /// <param name="includeSearchedValue">Whether to include the searched value in the returned text or not. True by default.</param>
        /// <returns>The trimmed text.</returns>
        public static string TrimFrom(this string text, string searchedValue, bool includeSearchedValue = false) => includeSearchedValue ? text[text.IndexOf(searchedValue)..] : text[(text.IndexOf(searchedValue) + searchedValue.Length)..];

        #endregion

        #region Trim FromTo

        public static string TrimFromTo(this string text, string from, string to)
        {
            text = text.TrimFrom(from);
            text = text.TrimTo(to);
            return text;
        }

        #endregion

        #region Replace if empty /+ null

        /// <summary>
        /// Replaces the text with replacement string if the text is equal to null.
        /// </summary>
        /// <param name="replacement">The string that will replace the text if the conditions are met.</param>
        /// <returns>The modified string.</returns>
        public static string ReplaceIfNull(this object text, string replacement) => text == null ? (string)text : replacement;

        /// <summary>
        /// Replaces the text with replacement string if the text is equal to null or is an empty string.
        /// </summary>
        /// <param name="replacement">The string that will replace the text if the conditions are met.</param>
        /// <returns>The modified string.</returns>
        public static string ReplaceIfNullOrEmpty(this object text, string replacement) => text == null || text.ToString() == "" ? replacement : (string)text;

        #endregion

        #region Capitalize

        /// <summary>
        /// Capitalizes the first letter.
        /// </summary>
        public static string CapitalizeFirst(this string text)
        {
            return char.ToUpper(text[0]) + text[1..];
        }

        #endregion

        #region Line count

        /// <summary>
        /// Returns number of lines by counting \n characters.
        /// </summary>
        public static int LineCount(this string text)
        {
            return text.Split('\n').Length;
        }

        #endregion

        #region Split

        /// <summary>
        /// Returns number of lines by counting \n characters.
        /// </summary>
        public static List<string> SplitBy(this string text, Delimiter delimiter)
        {
            return delimiter switch
            {
                Delimiter.Linebreak => text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList(),
                _ => throw new NotImplementedException(),
            };
        }

        public enum Delimiter
        {
            Linebreak
        }

        #endregion

        #region Replace

        /// <summary>
        /// Replaces the given array of old values with the new values.
        /// </summary>
        public static string Replace(this string text, string[] oldValue, string newValue)
        {
            return string.Join(newValue, text.Split(oldValue, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Replaces multiple value and replacement combinations in the text at once.
        /// </summary>
        public static string Replace(this string text, params (string oldValue, string newValue)[] replacements)
        {
            foreach (var replacement in replacements)
            {
                text = text.Replace(replacement.oldValue, replacement.newValue);
            }
            return text;
        }

        #endregion

        #region Index of digit

        /// <summary>
        /// Returns the index of the first digit in the string.
        /// </summary>
        public static int FirstIndexOfDigit(this string text)
        {
            return text.IndexOfAny("0123456789".ToCharArray());
        }

        /// <summary>
        /// Returns the last index of the first digit in the string.
        /// </summary>
        public static int LastIndexOfDigit(this string text)
        {
            return text.LastIndexOfAny("0123456789".ToCharArray());
        }

        #endregion
    }
}