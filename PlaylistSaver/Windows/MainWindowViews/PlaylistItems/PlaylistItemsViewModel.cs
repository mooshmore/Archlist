using Google.Apis.YouTube.v3.Data;
using Helpers;
using Newtonsoft.Json;
using PlaylistSaver.Helpers;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.PlaylistMethods.Models;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.Windows.MainWindowViews.PlaylistItems
{
    public class PlaylistItemsViewModel : ViewModelBase
    {
        public ObservableCollection<DisplayPlaylistItem> PlaylistsItemsList { get; set; }
        public DisplayPlaylist DisplayedPlaylist { get; set; }

        public PlaylistItemsViewModel(NavigationStore navigationStore, DisplayPlaylist displayPlaylist)
        {
            DisplayedPlaylist = displayPlaylist;
            //PlaylistData = playlistData;

            LoadPlaylistItems(displayPlaylist);


            ChangeExpandedStateCommannd = new RelayCommand(ChangeExpandedState);
        }

        public RelayCommand ChangeExpandedStateCommannd { get; }

        public void ChangeExpandedState(object parameter)
        {
            var displayPlaylist = (DisplayPlaylistItem)parameter;

            displayPlaylist.DescriptionHeight = 200;
            displayPlaylist.Description = "XD";

            var ok = displayPlaylist.DescriptionHeight;
            RaisePropertyChanged(nameof(displayPlaylist.DescriptionHeight));
        }

        private void LoadPlaylistItems(DisplayPlaylist displayPlaylist)
        {
            PlaylistsItemsList = new();

            // Get the data from the most recent file
            FileInfo playlistDataFile = displayPlaylist.DataDirectory.LastCreatedDirectory()?.LastCreatedFile();
            if (playlistDataFile == null)
                return;

            PlaylistItemListResponse playlist = playlistDataFile.Deserialize<PlaylistItemListResponse>();

            foreach (var playlistItem in playlist.Items)
            {
                // ! Don't add videos that are unavailable
                if (PlaylistItemsData.VideoIsAvailable(playlistItem))
                    PlaylistsItemsList.Add(new DisplayPlaylistItem(playlistItem, displayPlaylist.Id));
            }
        }
    }
}
