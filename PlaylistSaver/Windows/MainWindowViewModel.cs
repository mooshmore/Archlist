using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.UserData;
using PlaylistSaver.Windows.ViewModels;
using ToastMessageService;

namespace PlaylistSaver.Windows
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // ! Don't change it to static or the binding won't work
        public UserProfile UserProfile => GlobalItems.UserProfile;
        public ToastMessage ToastMessage => ToastMessage.ClassInstance;
        public ViewModelBase CurrentMainViewModel => NavigationStores.MainNavigationStore.CurrentViewModel;
        public ViewModelBase CurrentPopupViewModel => NavigationStores.PopupNavigationStore.CurrentViewModel;

        public MainWindowViewModel()
        {
            GlobalItems.UserProfileChanged += () => RaisePropertyChanged(nameof(UserProfile));
            NavigationStores.MainNavigationStore.CurrentVievModelChanged += () => RaisePropertyChanged(nameof(CurrentMainViewModel));
            NavigationStores.PopupNavigationStore.CurrentVievModelChanged += OnCurrentPopupViewModelChanged;

            GoToHomePageCommand = NavigationStores.GoToHomePageCommand;
            LogOutCommand = new AsyncRelayCommand(OAuthSystem.LogOutAsync);
            HidePopupViewCommand = NavigationStores.HidePopupViewCommand;
        }

        public bool OverlayVisibility => CurrentPopupViewModel != null;

        private void OnCurrentPopupViewModelChanged()
        {
            RaisePropertyChanged(nameof(CurrentPopupViewModel));
            RaisePropertyChanged(nameof(OverlayVisibility));
        }

        public RelayCommand HidePopupViewCommand { get; }
        public NavigateCommand GoToHomePageCommand { get; }
        public AsyncRelayCommand LogOutCommand { get; }
    }
}
