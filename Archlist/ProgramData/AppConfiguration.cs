using Newtonsoft.Json;
using Archlist.PlaylistMethods;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Archlist.Helpers.Systems;

namespace Archlist.ProgramData
{
    public static class AppConfiguration
    {
        public static void Configure()
        {
            // Retrieve clientID and clientSecret from Json file
            OAuthSystem.LoadSecretData();
            
            SetDefaultJsonSettings();
            CreateFileStructure();
            Settings.SettingsInstance.Read();
            SetAutoLinkOpening();
        }

        private static void SetAutoLinkOpening()
        {
            // Overrides all uri links to automatically open in a browser, I think.
            EventManager.RegisterClassHandler(
                typeof(System.Windows.Documents.Hyperlink),
                System.Windows.Documents.Hyperlink.RequestNavigateEvent,
                new System.Windows.Navigation.RequestNavigateEventHandler(
                    (sender, en) => Process.Start(new ProcessStartInfo(
                        en.Uri.ToString()
                    )
                    { UseShellExecute = true })
                )
            );
        }

        private static void SetDefaultJsonSettings()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// Creates folders used by the program in appdata/roaming.
        /// </summary>
        public static void CreateFileStructure()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            //  AppData
            //  ├─ Roaming
            //  │  ├─ Archlist_ms
            //  │  │  ├─ channels
            //  │  │  ├─ playlists
            //  │  │  │  ├─ allPlaylists
            //  │  │  │  ├─ unavailablePlaylists
            //  │  │  ├─ userData
            //  │  │  ├─ logs


            Directories.MainDirectory = Directory.CreateDirectory(Path.Combine(roamingPath, "Archlist_ms"));



            Directories.ChannelsDirectory = Directories.MainDirectory.CreateSubdirectory("channels");

            Directories.PlaylistsDirectory = Directories.MainDirectory.CreateSubdirectory("playlists");
            Directories.AllPlaylistsDirectory = Directories.PlaylistsDirectory.CreateSubdirectory("allPlaylists");
            Directories.UnavailablePlaylistsDirectory = Directories.PlaylistsDirectory.CreateSubdirectory("unavailablePlaylists");
            Directories.UsersDataDirectory = Directories.MainDirectory.CreateSubdirectory("userData");
            Directories.MainDirectory.CreateSubdirectory("logs");
        }

    }
}
