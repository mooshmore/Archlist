using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_link
{
    class AddPlaylists_linkViewModel : ViewModelBase
    {
        public AddPlaylists_linkViewModel(NavigationStore popupNavigationStore)
        {
            CloseViewCommand = new NavigateCommand(popupNavigationStore, null);
        }

        public ICommand CloseViewCommand { get; }
    }
}
