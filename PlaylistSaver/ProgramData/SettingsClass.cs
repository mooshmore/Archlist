using static PlaylistSaver.Enums;
using Newtonsoft.Json;
using System.IO;
using Helpers;

namespace PlaylistSaver
{
    /// <summary>
    /// The internal settings of the program.
    /// </summary>
    /// <remarks>
    /// This class ins't static so it can be serialized.
    /// </remarks>
    internal static class Settings
    {

        internal static SettingsClass settingsInstance = new();

        /// <summary>
        /// Defines a quality at which thumbnails will be downloaded.
        /// </summary>
        internal static ImageQuality? ImageQuality
        {
            get => settingsInstance.ImageQuality;
            set => settingsInstance.ImageQuality = value;
        }
    }


    internal class SettingsClass
    {
        /// <summary>
        /// Reads saved settings from settings.json file. <br/>
        /// If file / settings don't exist then it creates them with default values.
        /// </summary>
        internal void Read()
        {
            FileInfo settingsFile = GlobalItems.mainDirectory.SubFile("settings.json");

            // If file exists and has something in it read the saved settings
            if (settingsFile.Exists && !settingsFile.IsEmpty())
            {
                // Read the settings from the file
                SettingsClass savedSettings = JsonConvert.DeserializeObject<SettingsClass>(settingsFile.ReadAllText());
            
                // If a property doesn't exist set it to a default one
                ImageQuality = savedSettings.ImageQuality == null ? Enums.ImageQuality.Medium : savedSettings.ImageQuality;
            }
            // If file doesn't exist create it and set the default settings
            else
            {
                GlobalItems.mainDirectory.CreateSubfile("settings.json");
                ImageQuality = Enums.ImageQuality.Medium;
            }

            // Save the settings in case that the file was missing some properties
            Save();
        }

        internal void Save()
        {
            // Serialize the settings data into a json
            string jsonString = JsonConvert.SerializeObject(this);
            // Create a new channelInfo.json file and write the channel data to it
            File.WriteAllText(GlobalItems.mainDirectory.SubFile("settings.json").FullName, jsonString);
        }

        public ImageQuality? ImageQuality;
    }
}
