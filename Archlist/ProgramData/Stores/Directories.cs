using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archlist.ProgramData.Stores
{
    public static class Directories
    {
        public static DirectoryInfo ChannelsDirectory { get; set; }
        public static DirectoryInfo AllPlaylistsDirectory { get; set; }
        public static DirectoryInfo UsersDataDirectory { get; set; }
        public static DirectoryInfo MainDirectory { get; set; }

        public static DirectoryInfo ProjectDirectory => new(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        public static string ImagesPath { get; set; } = ProjectDirectory + "\\Resources\\Images\\";
        public static DirectoryInfo PlaylistsDirectory { get; internal set; }
        public static DirectoryInfo UnavailablePlaylistsDirectory { get; internal set; }
    }
}
