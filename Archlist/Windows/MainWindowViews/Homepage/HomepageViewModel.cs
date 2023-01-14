using Utilities;
using Archlist.Helpers;
using Archlist.PlaylistMethods;
using Archlist.PlaylistMethods.Models;
using Utilities.WPF.Bases;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows.MainWindowViews.PlaylistItems;
using Archlist.Windows.PopupViews.AddPlaylists.AddPlaylists_link;
using Archlist.Windows.PopupViews.AddPlaylists.AddPlaylists_userOwned;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using MsServices.ToastMessageService;
using System.Threading.Tasks;
using System;

namespace Archlist.Windows.MainWindowViews.Homepage
{
    public partial class HomepageViewModel : ViewModelBase
    {
        public ObservableCollection<DisplayPlaylist> PlaylistsList { get; set; }
        public ObservableCollection<DisplayPlaylist> MissingItemsPlaylistsList { get; set; }
        public ObservableCollection<DisplayPlaylist> UnavailablePlaylistsList { get; set; }

        public bool DisplayMissingItemsPlaylistsList { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Setting the property as static will make the binding not work.")]
        public UserProfile UserProfile => GlobalItems.UserProfile;

        public string MissingItemsText { get; set; }
        public string AllPlaylistsTitle => DisplayMissingItemsPanel ? "All other playlists" : "All playlists";
        public bool DisplayMissingItemsPanel { get; set; }
        public bool DisplayAllItemsPanel { get; set; }
        public bool DisplayUnavailableItemsPanel { get; set; }
        public bool DisplayNothingHerePanel { get; set; } = false;
        public BitmapImage MissingItemsImage { get; set; }

        public static HomepageViewModel Instance { get; set; }

        public HomepageViewModel()
        {
            OnUserProfileChanged();
            RefreshDisplayedPlaylistsData();

            PullPlaylistDataCommand = new AsyncRelayCommand(PullPlaylistData);
            RemovePlaylistCommand = new RelayCommand(RemovePlaylist);
            OpenPlaylistCommand = new RelayCommand(OpenPlaylist);
            UpdateCurrentPlaylistCommand = new RelayCommand(UpdateCurrentDisplayPlaylist);
            PullAllPlaylistsCommand = new AsyncRelayCommand(PullAllPlaylistsData);
            MarkAsSeenCommand = new RelayCommand(MarkAsSeen);
            MarkAllAsSeenCommand = new RelayCommand(MarkAllAsSeen);
            RemoveAllPlaylistsCommand = new RelayCommand(RemoveAllPlaylists);
            CheckPlaylistAvailabilityCommand = new AsyncRelayCommand(CheckPlaylistAvailability);

            OpenAddPlaylist_userOwnedViewCommand = new RelayCommand(OpenAddPlaylists_userOwned);
            OpenAddPlaylist_linkCommand = new RelayCommand(OpenAddPlaylists_link);

            GlobalItems.UserProfileChanged += OnUserProfileChanged;

            //if (!userChannelExists)
            //{
            //    ToastMessage.InformationDialog("Your Google account doesn't have a associated Youtube channel.\nChoose a different account.", IconType.Error);
            //}

            Instance = this;
        }

        private void OpenAddPlaylists_userOwned()
        {
            if (UserProfile.CheckUserProfile())
            {
                var navigateCommand = new NavigateCommand(NavigationStores.PopupNavigationStore, () => new AddPlaylists_userOwnedViewModel());
                navigateCommand.Execute(navigateCommand);
            }
        }

        private void OpenAddPlaylists_link()
        {
            if (UserProfile.CheckUserProfile())
            { 
                var navigateCommand = new NavigateCommand(NavigationStores.PopupNavigationStore, () => new AddPlaylists_linkViewModel());
                navigateCommand.Execute(navigateCommand);
            }
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
                MissingItemsImage = LocalUtilities.GetResourcesBitmapImage(@"Symbols/Other/positiveGreen_ok_32px.png");
            }
            else
            {
                MissingItemsImage = LocalUtilities.GetResourcesBitmapImage(@"Symbols/RemovalRed/box_important_64px.png");

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


        private void OnUserProfileChanged()
        {
            RaisePropertyChanged(nameof(UserProfile));
        }

        public RelayCommand OpenPlaylistCommand { get; }
        public RelayCommand UpdateCurrentPlaylistCommand { get; }

        private void OpenPlaylist(object displayPlaylist)
        {
            var navigateCommand = new NavigateCommand(NavigationStores.MainNavigationStore, () => new PlaylistItemsViewModel((DisplayPlaylist)displayPlaylist));
            navigateCommand.Execute(displayPlaylist);
        }

        public RelayCommand OpenAddPlaylist_userOwnedViewCommand { get; }
        public RelayCommand OpenAddPlaylist_linkCommand { get; }

        public void RefreshDisplayedPlaylistsData()
        {
            LoadPlaylists();
            SetMissingItemsText();
        }

        public void LoadPlaylists()
        {
            PlaylistsList = new();
            MissingItemsPlaylistsList = new();
            List<DisplayPlaylist> allPlaylists = ReadPlaylists(false);
            List<DisplayPlaylist> unavailablePlaylists = ReadPlaylists(true);

            // Separate playlists with missing items and other playlists
            var missingItemsPlaylistsList = allPlaylists.Where(playlist => playlist.MissingItemsCount > 0).ToList();
            missingItemsPlaylistsList.ForEach(item => allPlaylists.Remove(item));

            PlaylistsList = new ObservableCollection<DisplayPlaylist>(allPlaylists.OrderBy(playlist => playlist.Title));
            MissingItemsPlaylistsList = new ObservableCollection<DisplayPlaylist>(missingItemsPlaylistsList.OrderBy(playlist => playlist.Title));
            UnavailablePlaylistsList = new ObservableCollection<DisplayPlaylist>(unavailablePlaylists.OrderBy(playlist => playlist.Title));

            DisplayMissingItemsPanel = missingItemsPlaylistsList.Count > 0;
            DisplayUnavailableItemsPanel = unavailablePlaylists.Count > 0;
            DisplayAllItemsPanel = PlaylistsList.Count > 0;

            DisplayNothingHerePanel = !DisplayAllItemsPanel && !DisplayMissingItemsPanel;

            RaisePropertyChanged(nameof(AllPlaylistsTitle));
            RaisePropertyChanged(nameof(DisplayMissingItemsPanel));
            RaisePropertyChanged(nameof(DisplayUnavailableItemsPanel));
            RaisePropertyChanged(nameof(DisplayAllItemsPanel));
            RaisePropertyChanged(nameof(DisplayNothingHerePanel));

            RaisePropertyChanged(nameof(PlaylistsList));
            RaisePropertyChanged(nameof(MissingItemsPlaylistsList));
            RaisePropertyChanged(nameof(UnavailablePlaylistsList));
        }

        private static List<DisplayPlaylist> ReadPlaylists(bool readUnavailablePlaylists)
        {
            List<DisplayPlaylist> allPlaylists = new();
            var youtubePlaylistsList = PlaylistsData.ReadSavedPlaylists(readUnavailablePlaylists);

            foreach (var playlistItem in youtubePlaylistsList)
            {
                allPlaylists.Add(new DisplayPlaylist(playlistItem, readUnavailablePlaylists));
            }

            return allPlaylists;
        }
    }
}
