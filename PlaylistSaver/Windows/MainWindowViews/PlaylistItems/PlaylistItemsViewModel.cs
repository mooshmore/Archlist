using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.Windows.MainWindowViews.PlaylistItems
{
    public class PlaylistItemsViewModel : ViewModelBase
    {
        private Playlist _displayedPlaylist;

        public PlaylistItemsViewModel(NavigationStore navigationStore)
        {

        }
    }
}
