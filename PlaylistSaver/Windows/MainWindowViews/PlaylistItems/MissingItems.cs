using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.Helpers;
using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.PlaylistMethods.Models;
using PlaylistSaver.ProgramData.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlaylistSaver.Windows.MainWindowViews.PlaylistItems
{
    public partial class PlaylistItemsViewModel
    {
        // Using pre .NET 6.0(?) Tuple is mandatory for the binding to work properly
        public ObservableCollection<DisplayPlaylistItem> MissingItemsList { get; set; }
        public string MissingItemsText { get; set; }
        public BitmapImage MissingItemsImage { get; set; }
        public bool MarkAllAsSeenVisibility { get; set; }
        public RelayCommand ExpandMissingItemsPanelCommand { get; set; }
        public RelayCommand MarkAllAsSeenCommand { get; }
        public RelayCommand SeePreviousItemsCommand { get; }
        public bool DisplayMissingItemsPanel { get; set; } = false;
        public string SeePreviousItemsText { get; set; } = "See all previous missing items";
        public bool DisplayNothingHere { get; set; }

        private void LoadMissingItems(string itemsType)
        {
            var missingItemsFile = new FileInfo(Path.Combine(DisplayedPlaylist.MissingItemsDirectory.FullName, $"{itemsType}.json"));
            var missingItems = missingItemsFile.Deserialize<List<MissingPlaylistItem>>();

            MissingItemsList = new();
            missingItems.ForEach(item => MissingItemsList.Add(new DisplayPlaylistItem(item, DisplayedPlaylist.Id)));

            DisplayNothingHere = MissingItemsList.Count == 0;
            RaisePropertyChanged(nameof(MissingItemsList));
            RaisePropertyChanged(nameof(DisplayNothingHere));
        }

        private string CurrentlyDisplayedItems = "seen";

        private void SeePreviousItems()
        {
            if (CurrentlyDisplayedItems == "seen")
            {
                LoadMissingItems("recent");
                CurrentlyDisplayedItems = "recent";
                SeePreviousItemsText = "See all previous missing items";

            }
            else
            {
                LoadMissingItems("seen");
                CurrentlyDisplayedItems = "seen";
                SeePreviousItemsText = "See recent missing items";
            }

            DisplayMissingItemsPanel = true;
            RaisePropertyChanged(nameof(DisplayMissingItemsPanel));
            RaisePropertyChanged(nameof(SeePreviousItemsText));
        }

        private void MarkAllAsSeen()
        {
            DisplayedPlaylist.MarkAsSeen();
            MissingItemsList = new();
            SetMissingItemsInfo();
            DisplayMissingItemsPanel = false;
            RaisePropertyChanged(nameof(DisplayMissingItemsPanel));
        }


        private void ChangeDisplayMissingItemsPanel()
        {
            DisplayMissingItemsPanel = !DisplayMissingItemsPanel;
            RaisePropertyChanged(nameof(DisplayMissingItemsPanel));
        }

        private void SetMissingItemsInfo()
        {
            if (MissingItemsList.Count == 0)
            {
                MarkAllAsSeenVisibility = false;
                MissingItemsImage = LocalHelpers.GetResourcesBitmapImage(@"Symbols/Other/positiveGreen_ok_32px.png");
                MissingItemsText = "No missing videos have been found";
            }
            else
            {
                MarkAllAsSeenVisibility = true;
                MissingItemsImage = LocalHelpers.GetResourcesBitmapImage(@"Symbols/RemovalRed/box_important_32px.png");

                if (MissingItemsList.Count == 1)
                    MissingItemsText = "1 missing video has been found";
                else
                    MissingItemsText = $"{MissingItemsList.Count} missing videos have been found";
            }

            RaisePropertyChanged(nameof(MissingItemsText));
            RaisePropertyChanged(nameof(MissingItemsImage));
            RaisePropertyChanged(nameof(MarkAllAsSeenVisibility));
        }
    }
}
