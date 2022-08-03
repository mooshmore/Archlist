using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_userOwned
{
    public class AddPlaylists_userOwnedViewModel : ViewModelBase
    {
        // Using pre .NET 6.0(?) Tuple is mandatory for the binding to work properly
        public ObservableCollection<Tuple<bool, Google.Apis.YouTube.v3.Data.Playlist>> PlaylistsList { get; set; }

        public string CheckedPlaylistsCount 
        { 
            get
            {
                var checkedPlaylistsCount = ReturnCheckedPlaylists().Count;
                return checkedPlaylistsCount switch
                {
                    1 => "1 playlist",
                    _ => $"{checkedPlaylistsCount} playlists",
                };
            }
        }

        public List<Google.Apis.YouTube.v3.Data.Playlist> ReturnCheckedPlaylists()
        {
            List<Google.Apis.YouTube.v3.Data.Playlist> checkedPlaylists = new();

            foreach (var item in PlaylistsList)
            {
                if (item.Item1)
                    checkedPlaylists.Add(item.Item2);
            }
            return checkedPlaylists;
        }

        public AddPlaylists_userOwnedViewModel(NavigationStore popupNavigationStore, HomepageViewModel homepageViewModel)
        {
            CloseViewCommand = new NavigateCommand(popupNavigationStore, null);
            CheckAllPlaylistsCommand = new RelayCommand(CheckAllPlaylists);
            CheckPlaylistCommand = new RelayCommand(CheckPlaylist);
            AddCheckedPlaylistsCommand = new AsyncRelayCommand(AddCheckedPlaylists);
            PlaylistsList = new();
            this.homepageViewModel = homepageViewModel;

            DisplayPlaylists();
        }

        private async Task AddCheckedPlaylists()
        {
            var checkedPlaylists = ReturnCheckedPlaylists();
            await PlaylistsData.CreatePlaylistsData(checkedPlaylists);

            // Refresh the homepage to display newly added playlists
            homepageViewModel.LoadPlaylists();

            // Close the window 
            CloseViewCommand.Execute(null);
        }

        public HomepageViewModel homepageViewModel;

        public RelayCommand CheckAllPlaylistsCommand { get; }
        public RelayCommand CheckPlaylistCommand { get; }
        public AsyncRelayCommand AddCheckedPlaylistsCommand { get; }

        public void CheckPlaylist(object checkedPlaylistObject)
        {
            var checkedPlaylist = (Google.Apis.YouTube.v3.Data.Playlist)checkedPlaylistObject;
            var checkedPlaylistTuple = ReturnPlaylistTuple(checkedPlaylist);
            ChangePlaylistItemCheckState(checkedPlaylistTuple, !checkedPlaylistTuple.Item1);
            OnPropertyChanged(nameof(CheckedPlaylistsCount));
        }

        public Tuple<bool, Google.Apis.YouTube.v3.Data.Playlist> ReturnPlaylistTuple(Google.Apis.YouTube.v3.Data.Playlist playlist)
        {
            foreach (var item in PlaylistsList)
            {
                if (item.Item2 == playlist)
                    return item;
            }
            throw new NotImplementedException("Playlist doesn't exist in the list!");
        }

        public void CheckAllPlaylists()
        {
            for (int i = 0; i < PlaylistsList.Count; i++)
            {
                ChangePlaylistItemCheckState(PlaylistsList[i], true);
            }
            OnPropertyChanged(nameof(CheckedPlaylistsCount));
        }

        public void ChangePlaylistItemCheckState(Tuple<bool, Google.Apis.YouTube.v3.Data.Playlist> playlist, bool checkState)
        {
            var playlistIndex = PlaylistsList.IndexOf(playlist);
            PlaylistsList[playlistIndex] = new Tuple<bool, Google.Apis.YouTube.v3.Data.Playlist>(checkState, playlist.Item2);
        }


        public async Task DisplayPlaylists()
        {
            var playlists = await PlaylistsData.RetrieveUserOwnedPlaylistsData();

            // Convert the response to a list and sort the list alphabetically by playlist title
            List<Google.Apis.YouTube.v3.Data.Playlist> playlistsList  = new(playlists.Items);
            playlistsList = playlistsList.OrderBy(playlist => playlist.Snippet.Title).ToList();

            foreach (var playlist in playlistsList)
            {
                PlaylistsList.Add(new Tuple<bool, Google.Apis.YouTube.v3.Data.Playlist> (false, playlist));
            }
        }

        public ICommand CloseViewCommand { get; }
    }
}
 