using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Helpers;

namespace PlaylistSaver.Windows.Views
{
    /// <summary>
    /// Interaction logic for PlaylistItemsView.xaml
    /// </summary>
    public partial class PlaylistItemsView : UserControl
    {
        public PlaylistItemsViewModel playlistItemsViewModel;

        public static List<PlaylistItem> playlistItemsList;

        public PlaylistItemsView()
        {
            InitializeComponent();

            PlaylistItemListBox.ItemsSource = playlistItemsList;

            PlaylistThumbnail.Source = DirectoryExtensions.GetImage(GlobalItems.imagesPath + "no_thumbnail.jpg");
        }
    }
}
