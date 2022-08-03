using Helpers;
using PlaylistSaver.PlaylistMethods;
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

namespace PlaylistSaver.Windows.MainWindowViews.Homepage
{
    public partial class HomepageViewModel : ViewModelBase
    {
        public ObservableCollection<DisplayPlaylist> playlistsList { get; set; }

        public UserProfile UserProfile => GlobalItems.UserProfile;

        public HomepageViewModel(NavigationStore navigationStore, NavigationStore popupNavigationStore)
        {

            LoadPlaylists();
            OpenAddPlaylist_userOwnedViewCommand = new NavigateCommand(popupNavigationStore, () => new AddPlaylists_userOwnedViewModel(popupNavigationStore, this));
            OpenAddPlaylist_linkCommand = new NavigateCommand(popupNavigationStore, () => new AddPlaylists_linkViewModel(popupNavigationStore, this));

            //DeletePlaylistsCommand = new NavigateCommand(popupNavigationStore)
            GlobalItems.userProfileChanged += OnUserProfileChanged;

            OpenPlaylistCommand = new NavigateCommand(navigationStore, () => new PlaylistItemsViewModel(navigationStore));
        }


        private void OnUserProfileChanged()
        {
            OnPropertyChanged(nameof(UserProfile));
        }

        public ICommand OpenAddPlaylist_userOwnedViewCommand { get; }
        public ICommand OpenAddPlaylist_linkCommand { get; }
        public ICommand DeletePlaylistsCommand { get; }


        public CommandBase OpenPlaylistCommand { get; }

        public void LoadPlaylists()
        {
            playlistsList = new();
            var youtubePlaylistsList = PlaylistsData.ReadSavedPlaylists();
            List<DisplayPlaylist> tempList = new();
            foreach (var playlistItem in youtubePlaylistsList)
            {
                tempList.Add(new DisplayPlaylist(playlistItem.Snippet.Title, playlistItem.Id, playlistItem.ContentDetails.ItemCount.ToString(), playlistItem.Snippet.ChannelTitle));
            }
            playlistsList = new ObservableCollection<DisplayPlaylist>(tempList);
            OnPropertyChanged(nameof(playlistsList));
        }
    }
}
