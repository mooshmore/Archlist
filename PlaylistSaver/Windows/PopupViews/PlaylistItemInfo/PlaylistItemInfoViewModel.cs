using PlaylistSaver.PlaylistMethods.Models;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.Windows.PopupViews.PlaylistItemInfo
{
    public class PlaylistItemInfoViewModel : ViewModelBase
    {
        public NavigateCommand CloseViewCommand { get; }
        public DisplayPlaylistItem DisplayPlaylistItem { get; set; }
        public RelayCommand CopyTitleCommand { get; }
        public bool DisplayRemovalInfo { get; }

        public PlaylistItemInfoViewModel(NavigationStore popupNavigationStore, DisplayPlaylistItem displayPlaylist)
        {
            CloseViewCommand = new NavigateCommand(popupNavigationStore, null);
            DisplayPlaylistItem = displayPlaylist;
            DisplayRemovalInfo = displayPlaylist.RemovalReasonShort != null;
            RaisePropertyChanged(nameof(DisplayRemovalInfo));
            CopyTitleCommand = new RelayCommand(() => System.Windows.Clipboard.SetText(DisplayPlaylistItem.Title));
        }
    }
}
