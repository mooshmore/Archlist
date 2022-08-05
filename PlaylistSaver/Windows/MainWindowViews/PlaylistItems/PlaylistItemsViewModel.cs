using Google.Apis.YouTube.v3.Data;
using Helpers;
using Newtonsoft.Json;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.PlaylistMethods.Models;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }



        private void LoadPlaylistItems(DisplayPlaylist displayPlaylist)
        {
            PlaylistsItemsList = new();

            // Get the data from the most recent file
            FileInfo playlistDataFile = displayPlaylist.DataDirectory.LastCreatedDirectory()?.LastCreatedFile();
            if (playlistDataFile == null)
                return;

            // Parse the data to a playlist 
            string playlistText = File.ReadAllText(playlistDataFile.FullName);

            PlaylistItemListResponse playlist = JsonConvert.DeserializeObject<PlaylistItemListResponse>(playlistText);

            foreach (var playlistItem in playlist.Items)
            {
                PlaylistsItemsList.Add(new DisplayPlaylistItem(playlistItem, displayPlaylist.Id));
            }
        }
    }
}
