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
    public static class GlobalItems
    {
        public static HttpClient HttpClient = new();
        /// <remarks>
        /// This creates a new instance every time it is called to allow downloading multiple files at once.
        /// </remarks>
        public static WebClient WebClient => new();
        public static readonly string userApiKey = "AIzaSyDS3X0t5HnL0kTvQuiWkQJ9Yxq_PvXBkvE";

        public static DirectoryInfo channelsDirectory;
        public static DirectoryInfo playlistsDirectory;
        public static DirectoryInfo mainDirectory;
        public static string imagesPath = "Resources/Assets/Images/";
    }
}
