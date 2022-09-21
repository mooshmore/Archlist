using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Helpers
{
    /// <summary>
    /// A class for all types of methods that are too small to have their own class so they are all thrown in here.
    /// </summary>
    public static class MethodsCluster
    {
        /// <summary>
        /// Compares the two values, if the <paramref name="value"/> is bigger than the <paramref name="maxValue"/> then the <paramref name="maxValue"/> is returned;<br/>
        /// Otherwise returns <paramref name="value"/>
        /// </summary>
        public static T GetValueMax<T>(T value, T maxValue) where T : IComparable<T>
        {
            if (value.CompareTo(maxValue) > 0)
                return maxValue;
            else
                return value;
        }

        /// <summary>
        /// Compares the two values, if the <paramref name="value"/> is smaller than the <paramref name="minValue"/> then the <paramref name="minValue"/> is returned;<br/>
        /// Otherwise returns <paramref name="value"/>
        /// </summary>
        public static T GetValueMin<T>(T value, T minValue) where T : IComparable<T>
        {
            if (value.CompareTo(minValue) < 0)
                return minValue;
            else
                return value;
        }

        /// <summary>
        /// Downloads the image asynchrounsly.</br>
        /// The image is first saved to the memory, and only when the image has been fully downloaded it is saved to a file.
        /// </summary>
        /// <param name="uri">The url of the image.</param>
        /// <param name="filePath">The path where the image will be saved to.</param>
        public static async Task DownloadImageAsync(this HttpClient client, string url, string path)
        {
            byte[] fileBytes = await client.GetByteArrayAsync(url);
            File.WriteAllBytes(path, fileBytes);
        }

        /// <summary>
        /// Downloads the file asynchrounsly.</br>
        /// The file is first saved to the memory, and only when the file has been fully downloaded it is saved to a file.
        /// </summary>
        /// <param name="uri">The url of the file.</param>
        /// <param name="filePath">The path where the file will be saved to.</param>
        public static async Task DownloadFileTaskAsync(this HttpClient client, string url, string path)
        {
            string data = await client.DownloadStringTaskAsync(url);
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Downloads the text asynchrounsly.</br>
        /// </summary>
        /// <param name="uri">The url of the file.</param>
        public static async Task<string> DownloadStringTaskAsync(this HttpClient client, string uri)
        {
            string text;
            using (var stream = await client.GetStreamAsync(uri))
            {
                StreamReader reader = new(stream);
                text = reader.ReadToEnd();
            }
            return text;
        }

        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyProperties(this object source, object destination)
        {
            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();
            // Collect all the valid properties to map
            var results = from srcProp in typeSrc.GetProperties()
                          let targetProperty = typeDest.GetProperty(srcProp.Name)
                          where srcProp.CanRead
                          && targetProperty != null
                          && (targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate)
                          && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                          && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                          select new { sourceProperty = srcProp, targetProperty };
            //map the properties
            foreach (var props in results)
            {
                props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
            }
        }

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
        /// Checks if the list is null or empty.
        /// </summary>
        /// <param name="list">The list to be checked.</param>
        /// <returns>True if the value was null or an empty string; False if not.</returns>
        public static bool IsNullOrEmpty<T>(this List<T> list) => list == null || list.Count == 0;

        /// <summary>
        /// If the value is null it returns 0; Otherwise the normal value;
        /// </summary>
        public static int? IsNull_Zero(this int? value) => value == null ? 0 : value;
    }
}