using Google.Apis.YouTube.v3.Data;
using Helpers;
using Newtonsoft.Json;
using Archlist.Helpers;
using Archlist.PlaylistMethods;
using Archlist.PlaylistMethods.Models;
using Archlist.ProgramData;
using Archlist.ProgramData.Bases;
using Archlist.ProgramData.Commands;
using Archlist.ProgramData.Stores;
using Archlist.Windows.MainWindowViews.Homepage;
using Archlist.Windows.PopupViews.PlaylistItemInfo;
using Archlist.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Archlist.Windows.MainWindowViews.PlaylistItems
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
            LoadMissingItems(MissingItemsType.Recent);
            SetMissingItemsInfo();
            SetLastUpdateTime();

            if (MissingItemsList.Count != 0)
            {
                DisplayMissingItemsPanel = true;
                RaisePropertyChanged(nameof(DisplayMissingItemsPanel));
            }

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

        public NavigateCommand ReturnToHomePageCommand { get; } = NavigationStores.GoToHomePageCommand;

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
                    PlaylistsItemsList.Add(new DisplayPlaylistItem(playlistItem, DisplayedPlaylist.IsUnavailable));
            }
        }
    }
}
