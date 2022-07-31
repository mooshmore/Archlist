using static PlaylistSaver.Enums;
using Newtonsoft.Json;
using System.IO;
using Helpers;
using PlaylistSaver.ProgramData.Stores;

namespace PlaylistSaver
{
    /// <summary>
    /// The public settings of the program.
    /// </summary>
    /// <remarks>
    /// This class ins't static so it can be serialized.
    /// </remarks>
    public static class Settings
    {

        public static SettingsClass settingsInstance = new();

        /// <summary>
        /// Defines a quality at which thumbnails will be downloaded.
        /// </summary>
        public static ImageQuality? ImageQuality
        {
            get => settingsInstance.imageQuality;
            set => settingsInstance.imageQuality = value;
        }

        /// <summary>
        /// Defines a quality at which thumbnails will be downloaded.
        /// </summary>
        public static bool FirstAppRun
        {
            get => settingsInstance.firstAppRun;
            set => settingsInstance.firstAppRun = value;
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
                SettingsClass savedSettings = JsonConvert.DeserializeObject<SettingsClass>(settingsFile.ReadAllText());
            
                // If a property doesn't exist set it to a default one
                imageQuality = savedSettings.imageQuality == null ? Enums.ImageQuality.Medium : savedSettings.imageQuality;
            }
            // If file doesn't exist create it and set the default settings
            else
            {
                Directories.MainDirectory.CreateSubfile("settings.json");
                imageQuality = Enums.ImageQuality.Medium;
                firstAppRun = true;
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

        public ImageQuality? imageQuality;
        public bool firstAppRun;
    }
}
