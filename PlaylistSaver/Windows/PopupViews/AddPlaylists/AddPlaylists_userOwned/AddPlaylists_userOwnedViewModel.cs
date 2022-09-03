using Google.Apis.YouTube.v3.Data;
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
        public ObservableCollection<Tuple<bool, Playlist>> PlaylistsList { get; set; }

        public string CheckedPlaylistsCount 
        { 
            get
            {
                int checkedPlaylistCount = ReturnCheckedPlaylists().Count;
                return $"{checkedPlaylistCount} playlist" + (checkedPlaylistCount != 1 ? "s" : "");
            }
        }

        public AddPlaylists_userOwnedViewModel()
        {
            CloseViewCommand = new NavigateCommand(NavigationStores.PopupNavigationStore, null);
            CheckAllPlaylistsCommand = new RelayCommand(CheckAllPlaylists);
            CheckPlaylistCommand = new RelayCommand(CheckPlaylist);
            AddCheckedPlaylistsCommand = new AsyncRelayCommand(AddCheckedPlaylistsAsync);
            PlaylistsList = new();

            RetrieveAndDisplayPlaylists();
        }

        public List<Playlist> ReturnCheckedPlaylists()
        {
            List<Playlist> checkedPlaylists = new();

            foreach (var item in PlaylistsList)
            {
                if (item.Item1)
                    checkedPlaylists.Add(item.Item2);
            }
            return checkedPlaylists;
        }

        private async Task AddCheckedPlaylistsAsync()
        {
            var checkedPlaylists = ReturnCheckedPlaylists();
            await PlaylistsData.PullPlaylistsDataAsync(checkedPlaylists);

            // Refresh the homepage to display newly added playlists
            HomepageViewModel.Instance.LoadPlaylists();

            // Close the window 
            CloseViewCommand.Execute(null);

            List<string> addedPlaylistsIds = checkedPlaylists.Select(playlist => playlist.Id).ToList();
            PlaylistItemsData.PullPlaylistsItemsDataAsync(addedPlaylistsIds);
        }

        public RelayCommand CheckAllPlaylistsCommand { get; }
        public RelayCommand CheckPlaylistCommand { get; }
        public AsyncRelayCommand AddCheckedPlaylistsCommand { get; }

        public void CheckPlaylist(object checkedPlaylistObject)
        {
            var checkedPlaylist = (Playlist)checkedPlaylistObject;
            var checkedPlaylistTuple = ReturnPlaylistTuple(checkedPlaylist);
            ChangePlaylistItemCheckState(checkedPlaylistTuple, !checkedPlaylistTuple.Item1);
            RaisePropertyChanged(nameof(CheckedPlaylistsCount));
        }

        public Tuple<bool, Playlist> ReturnPlaylistTuple(Playlist playlist)
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
            RaisePropertyChanged(nameof(CheckedPlaylistsCount));
        }

        public void ChangePlaylistItemCheckState(Tuple<bool, Playlist> playlist, bool checkState)
        {
            var playlistIndex = PlaylistsList.IndexOf(playlist);
            PlaylistsList[playlistIndex] = new Tuple<bool, Playlist>(checkState, playlist.Item2);
        }


        public async Task RetrieveAndDisplayPlaylists()
        {
            var playlists = await PlaylistsData.RetrieveUserOwnedPlaylistsDataAsync();

            // Convert the response to a list and sort the list alphabetically by playlist title
            List<Playlist> playlistsList  = new(playlists.Items);
            playlistsList = playlistsList.OrderBy(playlist => playlist.Snippet.Title).ToList();

            foreach (var playlist in playlistsList)
            {
                PlaylistsList.Add(new Tuple<bool, Playlist> (false, playlist));
            }
        }

        public ICommand CloseViewCommand { get; }
    }
}
 