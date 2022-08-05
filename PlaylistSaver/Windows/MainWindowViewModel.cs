using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.UserData;
using PlaylistSaver.Windows.MainWindowViews;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.MainWindowViews.PlaylistItems;
using PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_link;
using PlaylistSaver.Windows.PopupViews.WelcomeScreenWindow;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlaylistSaver.Windows
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore _mainNavigationStore;
        private readonly NavigationStore _popupNavigationStore;
        public ViewModelBase CurrentMainViewModel => _mainNavigationStore.CurrentViewModel;
        public ViewModelBase CurrentPopupViewModel => _popupNavigationStore.CurrentViewModel;

        // ! Don't change it to static or the binding won't work
        public UserProfile UserProfile => GlobalItems.UserProfile;

        public MainWindowViewModel(NavigationStore navigationStore, NavigationStore popupNavigationStore)
        {
            _mainNavigationStore = navigationStore;
            _popupNavigationStore = popupNavigationStore;

            _mainNavigationStore.CurrentVievModelChanged += OnCurrentMainWindowViewModelChanged;
            _popupNavigationStore.CurrentVievModelChanged += OnCurrentPopupViewModelChanged;
            GlobalItems.UserProfileChanged += OnUserProfileChanged;

            GoToHomePageCommand = new NavigateCommand(_mainNavigationStore, () => new HomepageViewModel(navigationStore, popupNavigationStore));
            LogOutCommand = new AsyncRelayCommand(OAuthSystem.LogOut);
        }

        private void OnCurrentMainWindowViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentMainViewModel));
        }

        private void OnCurrentPopupViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentPopupViewModel));
        }

        private void OnUserProfileChanged()
        {
            OnPropertyChanged(nameof(UserProfile));
        }

        public CommandBase GoToHomePageCommand { get; }
        public AsyncRelayCommand LogOutCommand { get; }
    }
}
