using Archlist.ProgramData.Bases;
using Archlist.ProgramData.Commands;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows.MainWindowViews.AboutApp;
using Archlist.Windows.ViewModels;
using System;
using System.Threading.Tasks;
using ToastMessageService;

namespace Archlist.Windows
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // ! Don't change it to static or the binding won't work
        public UserProfile UserProfile => GlobalItems.UserProfile;
        public ToastMessage ToastMessage => ToastMessage.ClassInstance;
        public ViewModelBase CurrentMainViewModel => NavigationStores.MainNavigationStore.CurrentViewModel;
        public ViewModelBase CurrentPopupViewModel => NavigationStores.PopupNavigationStore.CurrentViewModel;

        public bool DisplayUserProfile { get; set; } = false;

        public MainWindowViewModel()
        {
            GlobalItems.UserProfileChanged += () => RaisePropertyChanged(nameof(UserProfile));
            NavigationStores.MainNavigationStore.CurrentVievModelChanged += () => RaisePropertyChanged(nameof(CurrentMainViewModel));
            NavigationStores.PopupNavigationStore.CurrentVievModelChanged += OnCurrentPopupViewModelChanged;

            SwitchAccountCommand = new AsyncRelayCommand(SwitchAccount);
            DisplayUserProfileCommand = new RelayCommand(DisplayUserProfileMethod);
        }

        private void DisplayUserProfileMethod()
        {
            DisplayUserProfile = !DisplayUserProfile;
            RaisePropertyChanged(nameof(DisplayUserProfile));
        }

        private async Task SwitchAccount()
        {
            ToastMessage.NotImplemented();
        }

        public bool OverlayVisibility => CurrentPopupViewModel != null;

        private void OnCurrentPopupViewModelChanged()
        {
            RaisePropertyChanged(nameof(CurrentPopupViewModel));
            RaisePropertyChanged(nameof(OverlayVisibility));
        }

        public RelayCommand HidePopupViewCommand { get; } = NavigationStores.HidePopupViewCommand;
        public NavigateCommand GoToHomePageCommand { get; } = NavigationStores.GoToHomePageCommand;
        public NavigateCommand OpenAboutPageCommand { get; } = new NavigateCommand(NavigationStores.MainNavigationStore, () => new AboutAppViewModel());
        public AsyncRelayCommand LogOutCommand { get; } = new AsyncRelayCommand(OAuthSystem.LogOutAsync);
        public AsyncRelayCommand SwitchAccountCommand { get; }
        public RelayCommand DisplayUserProfileCommand { get; }
    }
}
