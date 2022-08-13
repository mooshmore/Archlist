using Helpers;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.PlaylistMethods.Models;
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
        public ObservableCollection<DisplayPlaylist> PlaylistsList { get; set; }
        public UserProfile UserProfile => GlobalItems.UserProfile;

        public HomepageViewModel(NavigationStore navigationStore, NavigationStore popupNavigationStore)
        {
            LoadPlaylists();

            OnUserProfileChanged();

            OpenAddPlaylist_userOwnedViewCommand = new NavigateCommand(popupNavigationStore, () => new AddPlaylists_userOwnedViewModel(popupNavigationStore, this));
            OpenAddPlaylist_linkCommand = new NavigateCommand(popupNavigationStore, () => new AddPlaylists_linkViewModel(popupNavigationStore, this));
            PullPlaylistDataCommand = new RelayCommand(PullPlaylistData);
            OpenPlaylistCommand = new RelayCommand(OpenPlaylist);
            UpdateCurrentPlaylistCommand = new RelayCommand(UpdateCurrentDisplayPlaylist);

            GlobalItems.UserProfileChanged += OnUserProfileChanged;
            NavigationStore = navigationStore;
        }

        public RelayCommand UpdateCurrentPlaylistCommand { get; }
        public DisplayPlaylist CurrentDisplayPlaylist { get; set; }

        public void UpdateCurrentDisplayPlaylist(object playlist)
        {
            CurrentDisplayPlaylist = (DisplayPlaylist)playlist;
        }

        private void OnUserProfileChanged()
        {
            RaisePropertyChanged(nameof(UserProfile));
        }

        public NavigationStore NavigationStore; 
        public RelayCommand OpenPlaylistCommand { get; }

        private void OpenPlaylist(object displayPlaylist)
        {
            var navigateCommand = new NavigateCommand(NavigationStore, () => new PlaylistItemsViewModel(NavigationStore, (DisplayPlaylist)displayPlaylist));
            navigateCommand.Execute(displayPlaylist);
        }

        private void PullPlaylistData()
        {
            PlaylistItemsData.PullPlaylistsItemsDataAsync(CurrentDisplayPlaylist.Id.CreateNewList());
        }

        public ICommand OpenAddPlaylist_userOwnedViewCommand { get; }
        public ICommand OpenAddPlaylist_linkCommand { get; }
        public RelayCommand PullPlaylistDataCommand { get; }


        public void LoadPlaylists()
        {
            PlaylistsList = new();
            var youtubePlaylistsList = PlaylistsData.ReadSavedPlaylists();
            List<DisplayPlaylist> tempList = new();
            foreach (var playlistItem in youtubePlaylistsList)
            {
                tempList.Add(new DisplayPlaylist(playlistItem));
            }
            PlaylistsList = new ObservableCollection<DisplayPlaylist>(tempList);
            RaisePropertyChanged(nameof(PlaylistsList));
        }
    }
}
