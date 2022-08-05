using Newtonsoft.Json;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.UserData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlaylistSaver.ProgramData
{
    public static class AppConfiguration
    {
        public static void Configure()
        {
            // Retrieve clientID and clientSecret from Json file
            OAuthSystem.LoadSecretData();

            OAuthSystem.LogInAsync();

            SetDefaultJsonSettings();
            CreateFileStructure();
            Settings.settingsInstance.Read();
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
            Directories.MainDirectory = Directory.CreateDirectory(Path.Combine(roamingPath, "MooshsPlaylistSaver"));
            Directories.ChannelsDirectory = Directories.MainDirectory.CreateSubdirectory("channels");
            Directories.PlaylistsDirectory = Directories.MainDirectory.CreateSubdirectory("playlists");
            Directories.UserDataDirectory = Directories.MainDirectory.CreateSubdirectory("userData");
        }

    }
}
