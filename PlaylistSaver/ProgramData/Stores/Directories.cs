using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.ProgramData.Stores
{
    public static class Directories
    {
        public static DirectoryInfo ChannelsDirectory;
        public static DirectoryInfo PlaylistsDirectory;
        public static DirectoryInfo UserDataDirectory;
        public static DirectoryInfo MainDirectory;

        public static string ImagesPath { get; set; } = "Resources/Assets/Images/";

    }
}
