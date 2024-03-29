﻿using Google.Apis.YouTube.v3.Data;
using Archlist.Helpers;
using Archlist.PlaylistMethods;
using Archlist.PlaylistMethods.Models;
using Utilities.WPF.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Utilities;

namespace Archlist.Windows.MainWindowViews.PlaylistItems
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

        private void LoadMissingItems(MissingItemsType itemsType)
        {
            List<MissingPlaylistItem> missingItems = null;

            if (itemsType == MissingItemsType.Recent)
                missingItems = DisplayedPlaylist.RecentMissingItemsFile.Deserialize<List<MissingPlaylistItem>>();
            else 
                missingItems = DisplayedPlaylist.SeenMissingItemsFile.Deserialize<List<MissingPlaylistItem>>();

            MissingItemsList = new();
            missingItems.ForEach(item => MissingItemsList.Add(new DisplayPlaylistItem(item, DisplayedPlaylist.IsUnavailable, true)));

            DisplayNothingHere = MissingItemsList.Count == 0;
            RaisePropertyChanged(nameof(MissingItemsList));
            RaisePropertyChanged(nameof(DisplayNothingHere));
        }

        private MissingItemsType CurrentlyDisplayedItems = MissingItemsType.Recent;

        private void SeePreviousItems()
        {
            if (CurrentlyDisplayedItems == MissingItemsType.Seen)
            {
                LoadMissingItems(MissingItemsType.Recent);
                CurrentlyDisplayedItems = MissingItemsType.Recent;
                SeePreviousItemsText = "See all previous missing items";

            }
            else
            {
                LoadMissingItems(MissingItemsType.Seen);
                CurrentlyDisplayedItems = MissingItemsType.Seen;
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
                MissingItemsImage = LocalUtilities.GetResourcesBitmapImage(@"Symbols/Other/positiveGreen_ok_32px.png");
                MissingItemsText = "No recent missing videos have been found";
            }
            else
            {
                MarkAllAsSeenVisibility = true;
                MissingItemsImage = LocalUtilities.GetResourcesBitmapImage(@"Symbols/RemovalRed/box_important_64px.png");

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
