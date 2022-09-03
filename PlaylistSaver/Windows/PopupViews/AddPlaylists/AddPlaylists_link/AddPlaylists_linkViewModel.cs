using Google.Apis.YouTube.v3.Data;
using Helpers;
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
using System.Windows.Input;
using System.Windows.Navigation;

namespace PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_link
{
    class AddPlaylists_linkViewModel : ViewModelBase
    {
        public ObservableCollection<Playlist> PlaylistsList { get; set; } = new();

        public string PlaylistListCount
        {
            get
            {
                var checkedPlaylistsCount = PlaylistsList.Count;
                return checkedPlaylistsCount switch
                {
                    1 => "1 playlist",
                    _ => $"{checkedPlaylistsCount} playlists",
                };
            }
        }

        public AddPlaylists_linkViewModel()
        {
            CloseViewCommand = new NavigateCommand(NavigationStores.PopupNavigationStore, null);
            AddPlaylistToListCommand = new AsyncRelayCommand(AddPlaylistToList);
            RemovePlaylistFromListCommand = new RelayCommand(RemovePlaylistFromList);
            AddPlaylistsCommand = new AsyncRelayCommand(AddPlaylistsAsync);
        }

        /// <summary>
        /// Adds the playlist with the given link to the PlaylistsList,
        /// so that they can be previewed.
        /// </summary>
        /// <returns></returns>
        private async Task AddPlaylistsAsync()
        {
            await PlaylistsData.PullPlaylistsDataAsync(new List<Playlist>(PlaylistsList));

            // Refresh the homepage to display newly added playlists
            HomepageViewModel.Instance.LoadPlaylists();

            // Close the window 
            CloseViewCommand.Execute(null);

            List<string> addedPlaylistsIds = PlaylistsList.Select(playlist => playlist.Id).ToList();
            await PlaylistItemsData.PullPlaylistsItemsDataAsync(addedPlaylistsIds);
            await Task.Delay(50);
            HomepageViewModel.Instance.RefreshData();
        }

        private void RemovePlaylistFromList(object parameter)
        {
            var checkedPlaylist = (Playlist)parameter;
            foreach (var playlist in PlaylistsList)
            {
                if (playlist.Id == checkedPlaylist.Id)
                {
                    PlaylistsList.Remove(playlist);
                    RaisePropertyChanged(nameof(PlaylistListCount));

                    return;
                }
            }
        }

        private string _linkTextBoxValue;
        public string LinkTextBoxValue
        {
            get => _linkTextBoxValue;
            set
            {
                _linkTextBoxValue = value;
                AddingInfoText = "";
                RaisePropertyChanged();
            }
        }

        private string _addingInfoText;
        public string AddingInfoText
        {
            get => _addingInfoText;
            set
            {
                _addingInfoText = value;
                RaisePropertyChanged();
            }
        }

        private async Task AddPlaylistToList()
        {
            string playlistId = LinkTextBoxValue;

            // If the value is a full link trim it so that only Id is left
            if (playlistId.IndexOf("=") != -1)
                playlistId = playlistId.TrimFrom("=");

            // Check if the playlist isn't already added to the list
            if (PlaylistsList.Where(p => p.Id == playlistId).ToList().Count != 0)
            {
                AddingInfoText = "Playlist is already in the list.";
                return;
            }

            // Check if the playlist isn't already being tracked
            foreach (var playlistDirectory in Directories.PlaylistsDirectory.GetDirectories())
            {
                if (playlistDirectory.Name == playlistId)
                {
                    AddingInfoText = "Playlist is already being tracked.";
                    return;
                }
            }

            var playlistResponse = await PlaylistsData.RetrievePlaylistsDataAsync(playlistId.CreateNewList());

            // 
            if (playlistResponse.Items.Count == 0)
            {
                AddingInfoText = "Given playlist was not found.\nRemember that private playlists and watch later playlists cannot be added.";
                return;
            }

            foreach (var item in playlistResponse.Items)
            {
                PlaylistsList.Add(item);
            }

            LinkTextBoxValue = "";
            AddingInfoText = "";
            RaisePropertyChanged(nameof(PlaylistListCount));

        }

        public ICommand CloseViewCommand { get; }
        public AsyncRelayCommand AddPlaylistToListCommand { get; }
        public RelayCommand RemovePlaylistFromListCommand { get; }

        public AsyncRelayCommand AddPlaylistsCommand { get; }

    }
}
