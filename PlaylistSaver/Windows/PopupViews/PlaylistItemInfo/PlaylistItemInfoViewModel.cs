using Helpers;
using PlaylistSaver.Helpers;
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
using System.Windows.Media.Imaging;
using ToastMessageService;

namespace PlaylistSaver.Windows.PopupViews.PlaylistItemInfo
{
    public class PlaylistItemInfoViewModel : ViewModelBase
    {
        public RelayCommand CloseViewCommand { get; } = NavigationStores.HidePopupViewCommand;
        public DisplayPlaylistItem DisplayPlaylistItem { get; }
        public RelayCommand CopyTitleCommand { get; }
        public bool DisplayRemovalInfo { get; }

        public bool DisplayInfoPanel { get; } = false;
        public bool DisplayFoundSnaphots { get; } = false;
        public string InfoText { get;}
        public BitmapImage InfoImage { get; }
        public string FoundSnapshotsText { get; } = "";
        public string FoundSnapshotsCountText { get; } = "";
        public string WebArchiveLink { get; } = "";

        public string YoutubeSearchLink { get; } = "";


        public PlaylistItemInfoViewModel(DisplayPlaylistItem displayPlaylist)
        {
            DisplayPlaylistItem = displayPlaylist;
            DisplayRemovalInfo = displayPlaylist.RemovalReasonShort != null;
            RaisePropertyChanged(nameof(DisplayRemovalInfo));
            CopyTitleCommand = new RelayCommand(() => System.Windows.Clipboard.SetText(DisplayPlaylistItem.Title));
            YoutubeSearchLink = "https://www.youtube.com/results?search_query=" + System.Net.WebUtility.UrlEncode(displayPlaylist.Title);
            CopyIDCommand = new RelayCommand(CopyID);

            if (displayPlaylist.RecoveryFailed && displayPlaylist.FoundSnapshotsCount == 0)
            {
                DisplayInfoPanel = true;
                InfoText = "Program couldn't recover any data for this item, neither by checking locally saved data or a web archive.";
                InfoImage = LocalHelpers.GetResourcesBitmapImage("Symbols/RemovalRed/database_fail_32px.png");
            }
            // Item was recovered using web archive
            else if (displayPlaylist.FoundSnapshotsCount > 0)
            {
                DisplayInfoPanel = true;
                DisplayFoundSnaphots = true;
                FoundSnapshotsText = $" Snapshot{LocalHelpers.SEnding(displayPlaylist.FoundSnapshotsCount)}";
                FoundSnapshotsCountText = displayPlaylist.FoundSnapshotsCount.ToString();
                if (displayPlaylist.FoundSnapshotsCount == 99)
                    FoundSnapshotsCountText = "99+";
                else
                    FoundSnapshotsCountText = displayPlaylist.FoundSnapshotsCount.ToString();

                WebArchiveLink = displayPlaylist.WebArchiveLink;

                if (displayPlaylist.SourcedFromWebArchive)
                {
                    InfoText = "This data was recovered from a web archive, and it might be incomplete and glitchy (please report if it is).\nYou can check the web archive page by yourself by clicking this text.";
                    InfoImage = LocalHelpers.GetResourcesBitmapImage("Logos/WebArchive/webArchiveLogo_32px.png");
                }
                else
                {
                    InfoText = "Web archive snapshot of this video was found, but program was unable to parse it.\nYou can check the web archive page by yourself by clicking this text. \nIf the web archive page looks valid, please report this issue.";
                    InfoImage = LocalHelpers.GetResourcesBitmapImage("Logos/WebArchive/webArchiveLogo_removalRed_32px.png");
                }
            }
        }

        public RelayCommand CopyIDCommand { get; }

        public void CopyID()
        {
            System.Windows.Clipboard.SetText(DisplayPlaylistItem.Id);
            ToastMessage.Display("ID copied!");
        }
    }
}
