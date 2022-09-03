using Helpers;
using PlaylistSaver.Helpers;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.PlaylistMethods.Models;
using PlaylistSaver.ProgramData;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.UserData;
using PlaylistSaver.Windows.MainWindowViews.PlaylistItems;
using PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_link;
using PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_userOwned;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToastMessageService;

namespace PlaylistSaver.Windows.MainWindowViews.Homepage
{
    public partial class HomepageViewModel : ViewModelBase
    {
        public ObservableCollection<DisplayPlaylist> PlaylistsList { get; set; }
        public ObservableCollection<DisplayPlaylist> MissingItemsPlaylistsList { get; set; }
        public bool DisplayMissingItemsPlaylistsList { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Setting the property as static will make the binding not work.")]
        public UserProfile UserProfile => GlobalItems.UserProfile;
        public string MissingItemsText { get; set; }
        public string AllPlaylistsTitle => DisplayMissingItemsPanel ? "All other playlists" : "All playlists";
        public bool DisplayMissingItemsPanel { get; set; }
        public bool DisplayAllItemsPanel { get; set; }
        public bool DisplayNothingHerePanel { get; set; } = false;
        public BitmapImage MissingItemsImage { get; set; }
        public RelayCommand MarkAsSeenCommand { get; }

        public static HomepageViewModel Instance { get; set; }

        public HomepageViewModel()
        {
            LoadPlaylists();
            OnUserProfileChanged();
            SetMissingItemsText();

            PullPlaylistDataCommand = new RelayCommand(PullPlaylistData);
            RemovePlaylistCommand = new RelayCommand(RemovePlaylist);
            OpenPlaylistCommand = new RelayCommand(OpenPlaylist);
            UpdateCurrentPlaylistCommand = new RelayCommand(UpdateCurrentDisplayPlaylist);
            PullAllPlaylistsCommand = new AsyncRelayCommand(PullAllPlaylistsData);
            MarkAsSeenCommand = new RelayCommand(MarkAsSeen);

            GlobalItems.UserProfileChanged += OnUserProfileChanged;

            Instance = this;
        }

        private void RemovePlaylist()
        {
            ToastMessage.NotImplemented();
        }

        private void SetMissingItemsText()
        {
            // The total amount of playlists that have at least 1 video missing
            int missingFromPlaylists = 0;
            // The total amount of videos missing from all playlists combined
            int missingItemsTotal = 0;

            foreach (var playlist in MissingItemsPlaylistsList)
            {
                if (playlist.MissingItemsCount > 0)
                {
                    missingItemsTotal += playlist.MissingItemsCount;
                    missingFromPlaylists++;
                }
            }

            if (missingItemsTotal == 0)
            {
                MissingItemsText = "No missing videos have been found.";
                MissingItemsImage = LocalHelpers.GetResourcesBitmapImage(@"Symbols/Other/positiveGreen_ok_32px.png");
            }
            else
            {
                MissingItemsImage = LocalHelpers.GetResourcesBitmapImage(@"Symbols/RemovalRed/box_important_64px.png");

                if (missingItemsTotal == 1)
                    MissingItemsText = "1 video is missing from your 1 playlist.";
                else if (missingItemsTotal > 1 && missingFromPlaylists == 1)
                    MissingItemsText = $"{missingItemsTotal} videos are missing from 1 of your playlists.";
                else if (missingItemsTotal > 1 && missingFromPlaylists > 1)
                    MissingItemsText = $"{missingItemsTotal} videos are missing from {missingFromPlaylists} of your playlists.";
            }

            RaisePropertyChanged(nameof(MissingItemsText));
            RaisePropertyChanged(nameof(MissingItemsImage));
        }

        public DisplayPlaylist CurrentDisplayPlaylist { get; set; }

        public void UpdateCurrentDisplayPlaylist(object playlist)
        {
            CurrentDisplayPlaylist = (DisplayPlaylist)playlist;
        }

        private void MarkAsSeen()
        {
            CurrentDisplayPlaylist.MarkAsSeen();
            LoadPlaylists();
            SetMissingItemsText();
        }

        private void OnUserProfileChanged()
        {
            RaisePropertyChanged(nameof(UserProfile));
        }

        public RelayCommand OpenPlaylistCommand { get; }
        public RelayCommand UpdateCurrentPlaylistCommand { get; }
        public AsyncRelayCommand PullAllPlaylistsCommand { get; }
        public RelayCommand RemovePlaylistCommand { get; }

        private void OpenPlaylist(object displayPlaylist)
        {
            var navigateCommand = new NavigateCommand(NavigationStores.MainNavigationStore, () => new PlaylistItemsViewModel((DisplayPlaylist)displayPlaylist));
            navigateCommand.Execute(displayPlaylist);
        }

        private void PullPlaylistData()
        {
            // There is some real fuckery with wpf here.
            // I mean, excuse me my language, but this is making me go mad.

            // This method is called by clicking a option in a menu item of the playlist.
            // To open the menu, you have to click on the image.
            // When you click on the image, a UpdateCurrentDisplayPlaylist method is triggered and CurrentDisplayPlaylist is set.
            // So, fuckin wpf, explain it to me, how is it possible to click the image and click the menu option,
            // without triggering the UpdateCurrentDisplayPlaylist method? I will actually go fucking mad.

            // And, its not relying on the user clicking the menu and the option ultra fast, its just random
            // and will trigger randomly to fuck up your day.

            // Anyways, no matter how much I'll curse on wpf, shit won't fix itself, so this will check first
            // if the CurrentDisplayPlaylist is null (aka if the Update method has triggered) and
            // display a message to open the menu again when the Update method somehow hasn't triggered.

            // It isn't a fix or even a workaround the problem, but I have really no idea how to fix it,
            // so this at least won't crash the program.

            // Fortunatelly this happens very rarely, so it shouldn't be that much of a pain.
            if (CurrentDisplayPlaylist == null)
            {
                ToastMessage.Display("A rare error occured, please open the side menu again and try again.");
            }
            else
            {
                PlaylistItemsData.PullPlaylistsItemsDataAsync(CurrentDisplayPlaylist.Id.CreateNewList());
                LoadPlaylists();
                SetMissingItemsText();
            }

        }

        private async Task PullAllPlaylistsData()
        {
            await PlaylistItemsData.PullAllPlaylistsItemsDataAsync();
            LoadPlaylists();
            SetMissingItemsText();
        }

        public NavigateCommand OpenAddPlaylist_userOwnedViewCommand { get; } = new NavigateCommand(NavigationStores.PopupNavigationStore, () => new AddPlaylists_userOwnedViewModel());
        public NavigateCommand OpenAddPlaylist_linkCommand { get; } = new NavigateCommand(NavigationStores.PopupNavigationStore, () => new AddPlaylists_linkViewModel());
        public RelayCommand PullPlaylistDataCommand { get; }


        public void LoadPlaylists()
        {
            PlaylistsList = new();
            MissingItemsPlaylistsList = new();

            var youtubePlaylistsList = PlaylistsData.ReadSavedPlaylists();
            List<DisplayPlaylist> allPlaylists = new();
            foreach (var playlistItem in youtubePlaylistsList)
            {
                allPlaylists.Add(new DisplayPlaylist(playlistItem));
            }

            // Separate playlists with missing items and other playlists
            var missingItemsPlaylistsList = allPlaylists.Where(playlist => playlist.MissingItemsCount > 0).ToList();
            missingItemsPlaylistsList.ForEach(item => allPlaylists.Remove(item));

            MissingItemsPlaylistsList = new ObservableCollection<DisplayPlaylist>(missingItemsPlaylistsList.OrderBy(playlist => playlist.Title));
            PlaylistsList = new ObservableCollection<DisplayPlaylist>(allPlaylists.OrderBy(playlist => playlist.Title));

            DisplayMissingItemsPanel = missingItemsPlaylistsList.Count > 0;
            DisplayAllItemsPanel = PlaylistsList.Count > 0;

            DisplayNothingHerePanel = !DisplayAllItemsPanel && !DisplayMissingItemsPanel;

            RaisePropertyChanged(nameof(AllPlaylistsTitle));
            RaisePropertyChanged(nameof(DisplayMissingItemsPanel));
            RaisePropertyChanged(nameof(DisplayAllItemsPanel));
            RaisePropertyChanged(nameof(MissingItemsPlaylistsList));
            RaisePropertyChanged(nameof(DisplayNothingHerePanel));
            RaisePropertyChanged(nameof(PlaylistsList));
        }
    }
}
