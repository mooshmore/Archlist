using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver
{
    internal static class GlobalItems
    {
        internal static HttpClient HttpClient = new();
        /// <remarks>
        /// This creates a new instance every time it is called to allow downloading multiple files at once.
        /// </remarks>
        internal static WebClient WebClient => new();
        internal static readonly string userApiKey = "AIzaSyDS3X0t5HnL0kTvQuiWkQJ9Yxq_PvXBkvE";

        internal static DirectoryInfo channelsDirectory;
        internal static DirectoryInfo playlistsDirectory;
        internal static DirectoryInfo mainDirectory;
        internal static string imagesPath = "Resources/Assets/Images/";
    }
}
