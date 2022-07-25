using Helpers;
using PlaylistSaver.PlaylistMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlaylistSaver.Windows.MainWindowViews.PlaylistItems
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
