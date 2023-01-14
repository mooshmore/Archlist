using Archlist.PlaylistMethods;
using Archlist.PlaylistMethods.Models;
using Archlist.PlaylistMethods.Playlists;
using Utilities.WPF.Bases;
using Archlist.ProgramData.Stores;
using Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsServices.ToastMessageService;

namespace Archlist.Windows.MainWindowViews.Homepage
{
    public partial class HomepageViewModel : ViewModelBase
    {
        public RelayCommand MarkAsSeenCommand { get; }
        public AsyncRelayCommand PullPlaylistDataCommand { get; }
        public RelayCommand RemovePlaylistCommand { get; }

        private DisplayPlaylist _currentDisplayPlaylist;
        public DisplayPlaylist CurrentDisplayPlaylist
        {
            get
            {
                // There is some real fuckery with wpf here.
                // I mean, excuse me my language, but this is making me go mad.

                // This method is called by clicking a option in a menu item of the playlist.
                // To open the menu, you have to click on the image.
                // When you click on the image, a UpdateCurrentDisplayPlaylist method is triggered and CurrentDisplayPlaylist is set.
                // So, fuckin wpf, explain it to me, how is it possible to click the image and click the menu option,
                // without triggering the UpdateCurrentDisplayPlaylist method? I will actually go fucking mad.

                // And, its not relying on the user clicking the menu and the option ultra fast, its just random
                // and will trigger randomly to fuck up your day.

                // Anyways, no matter how much I'll curse on wpf, shit won't fix itself, so this will check first
                // if the CurrentDisplayPlaylist is null (aka if the Update method has triggered) and
                // display a message to open the menu again when the Update method somehow hasn't triggered.

                // It isn't a fix or even a workaround the problem, but I have really no idea how to fix it,
                // so this at least won't crash the program.

                // Fortunatelly this happens very rarely, so it shouldn't be that much of a pain.
                if (_currentDisplayPlaylist == null)
                    ToastMessage.Display("A rare error occured, please open the side menu again and try again.");

                return _currentDisplayPlaylist;
                }
            set => _currentDisplayPlaylist = value;
        }

        public void UpdateCurrentDisplayPlaylist(object playlist)
        {
            CurrentDisplayPlaylist = (DisplayPlaylist)playlist;
        }

        private async Task PullPlaylistData()
        {
            if (CurrentDisplayPlaylist != null)
            {
                await PlaylistItemsData.PullPlaylistsItemsDataAsync(CurrentDisplayPlaylist.Id.CreateNewList());
                RefreshDisplayedPlaylistsData();
            }
        }

        private void MarkAsSeen()
        {
            if (CurrentDisplayPlaylist != null)
            {
                CurrentDisplayPlaylist.MarkAsSeen();
                RefreshDisplayedPlaylistsData();
            }
        }

        private void RemovePlaylist()
        {
            if (CurrentDisplayPlaylist != null)
            {
                MissingItemsPlaylistsList.Remove(CurrentDisplayPlaylist);
                PlaylistsList.Remove(CurrentDisplayPlaylist);

                Directories.AllPlaylistsDirectory.RemoveDirectory(CurrentDisplayPlaylist.Id);
                Directories.UnavailablePlaylistsDirectory.RemoveDirectory(CurrentDisplayPlaylist.Id);
                SetMissingItemsText();
            }
        }

        private async Task CheckPlaylistAvailability()
        {
            var readdedPlaylists = await MissingPlaylistsData.UpdateUnavailablePlaylists();

            if (readdedPlaylists.Count == 0)
                ToastMessage.Information("Playlist is still unavailable :/");
            else
            {
                string returnedPlaylistsInfo = "";
                foreach (var playlist in readdedPlaylists)
                {
                    returnedPlaylistsInfo += playlist.Snippet.Localized.Title + $" ({playlist.Snippet.ChannelTitle})" + ", ";
                }

                if (returnedPlaylistsInfo != "")
                    ToastMessage.InformationDialog("Following playlists were found available again:\n" + returnedPlaylistsInfo.TrimToLast(", "));

                RefreshDisplayedPlaylistsData();
            }

        }

        public RelayCommand MarkAllAsSeenCommand { get; }
        public AsyncRelayCommand PullAllPlaylistsCommand { get; }
        public RelayCommand RemoveAllPlaylistsCommand { get; }
        public AsyncRelayCommand CheckPlaylistAvailabilityCommand { get; }

        private void RemoveAllPlaylists()
        {
            MissingItemsPlaylistsList.Clear();
            PlaylistsList.Clear();

            Directories.AllPlaylistsDirectory.ClearDirectory();
            Directories.UnavailablePlaylistsDirectory.ClearDirectory();
            RefreshDisplayedPlaylistsData();
        }

        public void MarkAllAsSeen()
        {
            foreach (var playlist in MissingItemsPlaylistsList)
            {
                playlist.MarkAsSeen();
            }
            RefreshDisplayedPlaylistsData();
        }

        private async Task PullAllPlaylistsData()
        {
            await PlaylistItemsData.PullAllPlaylistsItemsDataAsync();
            RefreshDisplayedPlaylistsData();
        }
    }
}
