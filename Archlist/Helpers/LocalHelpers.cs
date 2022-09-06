using Helpers;
using Newtonsoft.Json;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Archlist.Helpers
{
    public static class LocalHelpers
    {
        public static string SEnding(int value)
        {
            return value == 1 ? "" : "s";
        }

        /// <summary>
        /// Returns a Bitmap image from the given path inside of the project/Resources/Images.
        /// </summary>
        /// <param name="imagePath">The image path, starting from Resources/Images. Ex: "@"Symbols/White/add_64px.png"</param>
        public static BitmapImage GetResourcesBitmapImage(string imagePath)
        {
            string path = @"Resources/Images/" + imagePath;
            return DirectoryExtensions.GetProjectBitmapImage(path);
        }

        public static T Deserialize<T>(this FileInfo file)
        {
            return JsonConvert.DeserializeObject<T>(file.ReadAllText());
        }

        /// <summary>
        /// Serializes the given object and writes it to the given file.
        /// </summary>
        /// <param name="file">The file to write to.</param>
        /// <param name="serializedObject">The object to serialize.</param>
        public static void Serialize<T>(this FileInfo file, T serializedObject)
        {
            string jsonString = JsonConvert.SerializeObject(serializedObject);
            File.WriteAllText(file.FullName, jsonString);
        }

        /// <summary>
        /// Synchronously downloads the image to the given directory with the given name.
        /// </summary>
        /// <param name="thumbnailUrl">The directory where the image will be downloaded to.</param>
        /// <param name="thumbnailPath">The path where the thumbnail will be saved.</param>
        public static void DownloadImage(string thumbnailUrl, string thumbnailPath)
        {
            // Image is first downloaded to a memory, and only then when it has been fully downloaded
            // it is saved to a file.
            var temp = GlobalItems.HttpClient.DownloadImageAsync(thumbnailUrl, thumbnailPath);
        }

        /// <summary>
        /// Asynchronously downloads the image to the given directory with the given name.
        /// </summary>
        /// <param name="thumbnailUrl">The directory where the image will be downloaded to.</param>
        /// <param name="thumbnailPath">The path where the thumbnail will be saved.</param>
        public static async Task DownloadImageAsync(string thumbnailUrl, string thumbnailPath)
        {
            // Image is first downloaded to a memory, and only then when it has been fully downloaded
            // it is saved to a file.
            await GlobalItems.HttpClient.DownloadImageAsync(thumbnailUrl, thumbnailPath);
        }

        /// <summary>
        /// Asynchronously downloads the image to the given directory with the given name.
        /// </summary>
        /// <param name="downloadDirectory">The directory where the image will be downloaded to.</param>
        /// <param name="thumbnailName">The name of the thumbnail to set.</param>
        public static async Task DownloadImageAsync(string thumbnailUrl, DirectoryInfo downloadDirectory, string thumbnailName)
        {
            string thumbnailPath = Path.Combine(downloadDirectory.FullName, thumbnailName);

            // Image is first downloaded to a memory, and only then when it has been fully downloaded
            // it is saved to a file.
            await GlobalItems.HttpClient.DownloadImageAsync(thumbnailUrl, thumbnailPath);
        }
    }
}
