using Newtonsoft.Json;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.Windows.ViewModels;
using PlaylistSaver.Windows.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlaylistSaver
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Chrome

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(8);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        #endregion


        public MainWindow()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            //OAuthLogin.LogIn();

            //CreateFileStructure();
            //Settings.settingsInstance.Read();

            //Task.Run(async () => PlaylistData.RetrieveAndSavePlaylistData(new List<string>() { "PLTvYM5xUeYq52fHEmFFbNUZ6UNremMknh" }));

            // Audio
            //DownloadPlaylist("PLTvYM5xUeYq5Y5UskzFN9u25B3kuMJNkF");

            //// Muzyka
            //DownloadPlaylist("PLTvYM5xUeYq6O-Xglkv1RJF8iTKLCy1hr");

            //Sad
            //DownloadPlaylist("PLTvYM5xUeYq52fHEmFFbNUZ6UNremMknh");


            // YTTest
            //DownloadPlaylist("PLTvYM5xUeYq4jlMByeD8U67mzBMzHEm1G");

            // YTTestMoosh
            //DownloadPlaylist("PLUZK4y109BtAbkKhS3hY6rX-MS6CAsMSI");

            InitializeComponent();
            StateChanged += MainWindowStateChangeRaised;
        }

        /// <summary>
        /// Creates folders used by the program in appdata/roaming.
        /// </summary>
        private static void CreateFileStructure()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            GlobalItems.mainDirectory = Directory.CreateDirectory(Path.Combine(roamingPath, "MooshsPlaylistSaver"));
            GlobalItems.channelsDirectory = GlobalItems.mainDirectory.CreateSubdirectory("channels");
            GlobalItems.playlistsDirectory = GlobalItems.mainDirectory.CreateSubdirectory("playlists");
        }

        /// <summary>
        /// Downloads the information about items in the playlist (and also channels associated with the items) 
        /// with the given Id - that includes downloading it from youtube and saving it.
        /// </summary>
        /// <param name="playlistId">The Id of the playlist to download.</param>
        public static void DownloadPlaylist(string playlistId)
        {
            // Download and parse the data about playlist items
            List<PlaylistItem> playlistItems = Task.Run(async () => PlaylistItemsData.Retrieve(playlistId).Result).Result;

            // Download and save data about channels associated with items in the playlist
            Task.Run(async () => ChannelsData.RetrieveAndSaveChannelsData(playlistItems));

            // Save the playlist data locally
            SavePlaylistItems.Save(playlistItems, playlistId);

            PlaylistItemsView.playlistItemsList = playlistItems;
        }
    }
}
