using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Helpers
{
    /// <summary>
    /// A class for all types of methods that are too small to have their own class so they are all thrown in here.
    /// </summary>
    public static class MethodsCluster
    {
        /// <summary>
        /// Merges two JArrays with each other, by adding <paramref name="mergedITem"/> to <paramref name="baseItem"/>.
        /// </summary>
        /// <param name="baseItem">The JObject to wchich arrays will be merged to.</param>
        /// <param name="mergedITem">The JOjbect from which items will be copied.</param>
        /// <param name="baseItemToken">The token to merge the arrays by.</param>
        /// <param name="mergedItemToken">The token from the <paramref name="mergedITem"/> to merge the arrays by. If not supplied will be the same as <paramref name="baseItemToken"/>.</param>
        /// <param name="mergeType">The type of the merge to be used.</param>
        public static void MergeJArray(this JObject baseItem, JObject mergedITem, string baseItemToken, string mergedItemToken = "", MergeArrayHandling mergeType = MergeArrayHandling.Union)
        {
            if (mergedItemToken == "")
                mergedItemToken = baseItemToken;
            var mergeSettings = new JsonMergeSettings
            {
                MergeArrayHandling = mergeType
            };

            (baseItem.SelectToken(baseItemToken) as JArray).Merge(mergedITem.SelectToken(mergedItemToken), mergeSettings);
        }

        /// <summary>
        /// Checks if the value is null or empty.
        /// </summary>
        /// <param name="obj">The value to be checked.</param>
        /// <returns>True if the value was null or an empty string; False if not.</returns>
        public static bool IsNullOrEmpty(this object obj) => obj == null || obj.ToString() == "";
        /// <summary>
        /// If the value is null it returns 0; Otherwise the normal value;
        /// </summary>
        public static int? IsNull_Zero(this int? value) => value == null ? 0 : value;

        /// <summary>
        /// Checks if the given decimal string matches the regex format.
        /// Examples: 2:35 - true, 06:52 - true 15:9 - false, 20:60 - false
        /// </summary>
        /// <param name="value">The value string.</param>
        /// <returns>A true if the string matches the format, false if it doesn't.</returns>
        public static bool Check_DecimalFormat(string value) => new Regex("^[0-9]+(.[0-9]{1,2})?$").IsMatch(value);

        public static bool ControlIsPressed => System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control;
    }
}