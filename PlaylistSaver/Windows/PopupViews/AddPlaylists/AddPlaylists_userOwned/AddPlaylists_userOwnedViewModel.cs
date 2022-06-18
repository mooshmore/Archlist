using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_userOwned
{
    public class AddPlaylists_userOwnedViewModel : ViewModelBase
    {
        public List<(bool _checked, Playlist playlist)> PlaylistsList { get; set; }

        public AddPlaylists_userOwnedViewModel()
        {

        }

        public Action<(bool _checked, Playlist playlist)> CheckPlaylist()
        {
            return null;
        }
    }
}
 