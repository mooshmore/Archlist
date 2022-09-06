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
        public static DirectoryInfo PlaylistsDirectory { get; set; }
        public static DirectoryInfo UserDataDirectory { get; set; }
        public static DirectoryInfo MainDirectory { get; set; }

        public static DirectoryInfo ProjectDirectory => new(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        public static string ImagesPath { get; set; } = ProjectDirectory + "\\Resources\\Images\\";

    }
}
