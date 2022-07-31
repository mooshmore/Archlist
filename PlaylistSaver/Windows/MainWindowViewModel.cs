using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.UserData;
using PlaylistSaver.Windows.MainWindowViews;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.MainWindowViews.PlaylistItems;
using PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_link;
using PlaylistSaver.Windows.PopupViews.WelcomeScreenWindow;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlaylistSaver.Windows
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore _mainNavigationStore;
        private readonly NavigationStore _popupNavigationStore;
        public ViewModelBase CurrentMainViewModel => _mainNavigationStore.CurrentViewModel;
        public ViewModelBase CurrentPopupViewModel => _popupNavigationStore.CurrentViewModel;

        public UserProfile UserProfile => GlobalItems.UserProfile;

        public MainWindowViewModel(NavigationStore navigationStore, NavigationStore popupNavigationStore)
        {
            _mainNavigationStore = navigationStore;
            _popupNavigationStore = popupNavigationStore;

            _mainNavigationStore.CurrentVievModelChanged += OnCurrentMainWindowViewModelChanged;
            _popupNavigationStore.CurrentVievModelChanged += OnCurrentPopupViewModelChanged;
            GlobalItems.userProfileChanged += OnUserProfileChanged;

            //CurrentPopupViewModel = new AddPlaylists_linkViewModel();

            // Check if the application is run for the first time or the user has logged out of the app
            // if it is display welcome screen with login info
            //if (Settings.FirstAppRun)
            //{
            //    CurrentViewModel = new WelcomeScreenViewModel();
            //}

            //OAuthLogin.LogIn();

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

            GoToHomePageCommand = new NavigateCommand(_mainNavigationStore, () => new HomepageViewModel(navigationStore, popupNavigationStore));
            //GoToHomePageCommand = new CallbackCommand(Ok);
        }

        private void OnCurrentMainWindowViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentMainViewModel));
        }

        private void OnCurrentPopupViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentPopupViewModel));
        }

        private void OnUserProfileChanged()
        {
            OnPropertyChanged(nameof(UserProfile));
        }

        public CommandBase GoToHomePageCommand { get; }


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

            //PlaylistItemsView.playlistItemsList = playlistItems;
        }
    }
}
