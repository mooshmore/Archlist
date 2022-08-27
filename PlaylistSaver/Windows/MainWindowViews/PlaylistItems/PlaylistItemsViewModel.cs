using Google.Apis.YouTube.v3.Data;
using Helpers;
using Newtonsoft.Json;
using PlaylistSaver.Helpers;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.PlaylistMethods.Models;
using PlaylistSaver.ProgramData;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.PopupViews.PlaylistItemInfo;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlaylistSaver.Windows.MainWindowViews.PlaylistItems
{
    public partial class PlaylistItemsViewModel : ViewModelBase
    {
        public ObservableCollection<DisplayPlaylistItem> _playlistsItemsList;
        public ObservableCollection<DisplayPlaylistItem> PlaylistsItemsList
        {
            get => _playlistsItemsList;
            set
            {
                _playlistsItemsList = value;
                RaisePropertyChanged();
            }
        }

        public PlaylistItemsViewModel(DisplayPlaylist displayPlaylist)
        {
            DisplayedPlaylist = displayPlaylist;

            DisplayedDay = DisplayedPlaylist.DataDirectory.LastCreatedDirectory()?.Name;
            DisplayedHour = DisplayedPlaylist.DataDirectory.LastCreatedDirectory()?.LastCreatedFile().Name.Replace("-", ":").Replace(".json", "");

            LoadSavedDays();
            LoadSavedHours();
            LoadPlaylistItems();
            LoadMissingItems("recent");
            SetMissingItemsInfo();
            SetLastUpdateTime();

            if (MissingItemsList.Count != 0)
            {
                DisplayMissingItemsPanel = true;
                RaisePropertyChanged(nameof(DisplayMissingItemsPanel));
            }

            ReturnToHomePageCommand = NavigationStores.GoToHomePageCommand;

            DisplayDayPanelCommand = new RelayCommand(DisplayDayPanel);
            DisplayHourPanelCommand = new RelayCommand(DisplayHourPanel);
            ChangeDisplayedDayCommand = new RelayCommand(ChangeDisplayedDay);
            ChangeDisplayedHourCommand = new RelayCommand(ChangeDisplayedHour);
            DisplayPlaylistItemInfoCommand = new RelayCommand(DisplayPlaylistItemInfo);
            ExpandMissingItemsPanelCommand = new RelayCommand(ChangeDisplayMissingItemsPanel);
            MarkAllAsSeenCommand = new RelayCommand(MarkAllAsSeen);
            SeePreviousItemsCommand = new RelayCommand(SeePreviousItems);
        }

        private void SetLastUpdateTime()
        {
            if (DisplayedPlaylist.DataDirectory.LastCreatedDirectory() != null)
            {
                string lastUpdateDay = DisplayedPlaylist.DataDirectory.LastCreatedDirectory().Name;
                string lastUpdateHour = DisplayedPlaylist.DataDirectory.LastCreatedDirectory().LastCreatedFile().NoExtensionName();

                DateTime lastUpdateDate = DateTime.ParseExact($"{lastUpdateDay} {lastUpdateHour}", "yyyy-MM-dd HH-mm", System.Globalization.CultureInfo.InvariantCulture);
                LastUpdateTime = $"Last updated {TimeAndDate.StringDifference(lastUpdateDate, DateTime.Now)}";
            }
            else
                LastUpdateTime = "Not previously updated";

            RaisePropertyChanged(LastUpdateTime);
        }

        public string LastUpdateTime { get; set; }

        private void DisplayPlaylistItemInfo(object displayPlaylistItem)
        {
            var navigateCommand = new NavigateCommand(NavigationStores.PopupNavigationStore, () => new PlaylistItemInfoViewModel((DisplayPlaylistItem)displayPlaylistItem));
            navigateCommand.Execute(navigateCommand);
        }

        public NavigateCommand ReturnToHomePageCommand { get; }

        private void LoadPlaylistItems()
        {
            PlaylistsItemsList = new();

            if (DisplayedDay == null || DisplayedHour == null)
                return;

            string playlistDataFilePath = Path.Combine(DisplayedPlaylist.DataDirectory.FullName, DisplayedDay, $"{DisplayedHour?.Replace(":", "-")}.json");

            // Get the data from the most recent file
            FileInfo playlistDataFile = new(playlistDataFilePath);
            if (playlistDataFile == null)
                return;

            PlaylistItemListResponse playlist = playlistDataFile.Deserialize<PlaylistItemListResponse>();

            foreach (var playlistItem in playlist.Items)
            {
                // ! Don't add videos that are unavailable
                if (PlaylistItemsData.IsAvailable(playlistItem))
                    PlaylistsItemsList.Add(new DisplayPlaylistItem(playlistItem, DisplayedPlaylist.Id));
            }
        }
    }
}
