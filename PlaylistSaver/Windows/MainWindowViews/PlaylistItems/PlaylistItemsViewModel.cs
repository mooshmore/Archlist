using PlaylistSaver.PlaylistMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.Windows.ViewModels
{
    public class PlaylistItemsViewModel : ViewModelBase
    {
        private Playlist _displayedPlaylist;
        public Playlist DisplayedPlaylist
        {
            get
            {
                return _displayedPlaylist;
            }
            set
            {
                _displayedPlaylist = value;
                OnPropertyChanged(nameof(Playlist));
            }
        }

        public PlaylistItemsViewModel()
        {

        }
    }
}
