using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Helpers
{
    /// <summary>
    /// A class that holds various type of data type converters and other data converters.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Parses the object to a string and then to an int.
        /// </summary>
        /// <returns>0 if the value was null or empty; Otherwise the parsed value.</returns>
        public static int ToInt(this object value) => value == null || value.ToString() == "" ? 0 : int.Parse(value.ToString());

        /// <summary>
        /// Parses the char to an int.
        /// </summary>
        /// <returns>Null if the char was "\0"; Otherwise the parsed value.</returns>
        public static int? ToInt(this char text) => text.ToString() == "\0" ? null : (int?)int.Parse(text.ToString());

        /// <summary>
        /// Converts a given object to a int?. If the object.ToString() == "" then a null is returned.
        /// </summary>
        /// <returns>Null if the object was null; Otherwise the parsed value.</returns>
        public static int? ToInt_null(this object obj) => obj.ToString() == "" ? null : (int?)obj.ToInt();

        /// <summary>
        /// Converts the given text to a char. If the string is empty it returns a empty char (\0).
        /// </summary>
        /// <returns>A first character of the string or a empty char(\0) if the string is empty.</returns>
        public static char ToChar(this string text) => text.Length == 0 ? (char)0 : text[0];

        /// <summary>
        /// Converts the given text to a char?. If the text == "" it returns null.
        /// </summary>
        /// <returns>A first character of the string or a null if the string is empty.</returns>
        public static char? ToChar_null(this string text) => text.Length == 0 ? null : (char?)text[0];

        /// <summary>
        /// Returns a string that represents the current object. If the object is null then null is returned - without throwing an error.
        /// </summary>
        /// <returns>Null if the object was null; Otherwise a string that represents the current object</returns>
        public static string ToString_null(this object obj) => obj?.ToString();

        /// <summary>
        /// Converts the given object to a boolean.
        /// </summary>
        public static bool ToBoolean(this object obj) => bool.Parse(obj.ToString());

        /// <summary>
        /// Parses the given object to a TimeSpan.
        /// </summary>
        public static TimeSpan ToTimeSpan(this object obj) => TimeSpan.Parse(obj.ToString());

        /// <summary>
        /// Parses the given object to a TimeSpan?. If the object is null it returns null.
        /// </summary>
        public static TimeSpan? ToTimeSpan_null(this object obj) => obj.IsNullOrEmpty() ? null : (TimeSpan?)TimeSpan.Parse(obj.ToString());

        /// <summary>
        /// Creates and returns SQL date format ( YYYY-MM-DD ) string from the given date.
        /// </summary>
        /// <param name="date">The date from which the string will be created from.</param>
        /// <returns>A SQL date format string created from the given date.</returns>
        public static string ToSQLFormat(this DateTime date) => date.ToString("yyyy-MM-dd");

        /// <summary>
        /// Creates and returns SQL date format ( YYYY-MM-DD ) string from the given date.
        /// </summary>
        /// <param name="date">The date from which the string will be created from.</param>
        /// <returns>A SQL date format string created from the given date.</returns>
        public static string ToFullSQLFormat(this DateTime date) => date.ToString("yyyy-MM-dd HH:mm:ss.fff");

        /// <summary>
        /// Returns the date in a day format = day/month/year.
        /// </summary>
        /// <param name="date">The date to be converted.</param>
        /// <param name="interlude">The interlude. "/" by default.</param>
        /// <returns>A string created from the date in a day/month/year format.</returns>
        public static string DayFormat(this DateTime date, string interlude = "/") => $"{date.Day}{interlude}{date.Month}{interlude}{date.Year}";

        public static List<T> ToList<T>(this DataTable dataTable)
        {
            return dataTable.AsEnumerable()
                           .Select(r => r.Field<T>(dataTable.Columns[0].ColumnName))
                           .ToList();
        }

        public static List<T> ToList<T>(this DataTable dataTable, string columnName)
        {
            return dataTable.AsEnumerable()
                           .Select(r => r.Field<T>(columnName))
                           .ToList();
        }

        public static List<string> CreateNewList(this string item)
        {
            var list = new List<string> { item };
            return list;
        }
    }
}
