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
        public ObservableCollection<Google.Apis.YouTube.v3.Data.Playlist> PlaylistsList { get; set; } = new();

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

        public HomepageViewModel homepageViewModel;

        public AddPlaylists_linkViewModel(NavigationStore popupNavigationStore, HomepageViewModel homepageViewModel)
        {
            CloseViewCommand = new NavigateCommand(popupNavigationStore, null);
            AddPlaylistToListCommand = new AsyncRelayCommand(AddPlaylistToList);
            RemovePlaylistFromListCommand = new RelayCommand(RemovePlaylistFromList);
            AddPlaylistsCommand = new AsyncRelayCommand(AddPlaylists);

            this.homepageViewModel = homepageViewModel;
        }

        private async Task AddPlaylists()
        {
            await PlaylistsData.CreatePlaylistsData(new List<Google.Apis.YouTube.v3.Data.Playlist>(PlaylistsList));

            // Refresh the homepage to display newly added playlists
            homepageViewModel.LoadPlaylists();

            // Close the window 
            CloseViewCommand.Execute(null);
        }

        private void RemovePlaylistFromList(object parameter)
        {
            var checkedPlaylist = (Google.Apis.YouTube.v3.Data.Playlist)parameter;
            foreach (var playlist in PlaylistsList)
            {
                if (playlist.Id == checkedPlaylist.Id)
                {
                    PlaylistsList.Remove(playlist);
                    OnPropertyChanged(nameof(PlaylistListCount));

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
                OnPropertyChanged();
            }
        }

        private string _addingInfoText;
        public string AddingInfoText
        {
            get => _addingInfoText;
            set
            {
                _addingInfoText = value;
                OnPropertyChanged();
            }
        }

        private async Task AddPlaylistToList()
        {
            string playlistId = LinkTextBoxValue;

            // If the value is a full link trim it so that only Id is left
            if (playlistId.IndexOf("=") != -1)
                playlistId = playlistId.TrimFromFirst("=", false);

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

            var playlistResponse = await PlaylistsData.RetrievePlaylistsData(playlistId.CreateNewList());

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
            OnPropertyChanged(nameof(PlaylistListCount));

        }

        public ICommand CloseViewCommand { get; }
        public AsyncRelayCommand AddPlaylistToListCommand { get; }
        public RelayCommand RemovePlaylistFromListCommand { get; }

        public AsyncRelayCommand AddPlaylistsCommand { get; }

    }
}
