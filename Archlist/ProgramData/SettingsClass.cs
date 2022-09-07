using Newtonsoft.Json;
using System.IO;
using Helpers;
using Archlist.ProgramData.Stores;
using Archlist.Helpers;

namespace Archlist
{
    /// <summary>
    /// The public settings of the program.
    /// </summary>
    /// <remarks>
    /// This class ins't static so it can be serialized.
    /// </remarks>
    public static class Settings
    {
        public static SettingsClass SettingsInstance { get; set; } = new();

        /// <summary>
        /// Defines a quality at which thumbnails will be downloaded.
        /// </summary>
        public static bool WasPreviouslyLoggedIn
        {
            get => SettingsInstance.wasPreviouslyLoggedIn;
            set => SettingsInstance.wasPreviouslyLoggedIn = value;
        }
    }


    public class SettingsClass
    {
        /// <summary>
        /// Reads saved settings from settings.json file. <br/>
        /// If file / settings don't exist then it creates them with default values.
        /// </summary>
        public void Read()
        {
            FileInfo settingsFile = Directories.MainDirectory.SubFile("settings.json");

            // If file exists and has something in it read the saved settings
            if (settingsFile.Exists && !settingsFile.IsEmpty())
            {
                // Read the settings from the file
                var savedSettings = settingsFile.Deserialize<SettingsClass>();

                this.wasPreviouslyLoggedIn = savedSettings.wasPreviouslyLoggedIn;
            }
            // If file doesn't exist create it and set the default settings
            else
            {
                Directories.MainDirectory.CreateSubfile("settings.json");
                wasPreviouslyLoggedIn = false;
            }

            // Save the settings in case that the file was missing some properties
            Save();
        }

        public void Save()
        {
            // Serialize the settings data into a json
            string jsonString = JsonConvert.SerializeObject(this);
            // Create a new channelInfo.json file and write the channel data to it
            File.WriteAllText(Directories.MainDirectory.SubFile("settings.json").FullName, jsonString);
        }

        public bool wasPreviouslyLoggedIn;
    }
}
