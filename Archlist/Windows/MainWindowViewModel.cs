using Archlist.Helpers;
using Utilities.WPF.Bases;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows.MainWindowViews.AboutApp;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MsServices.ToastMessageService;
using Utilities;

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
            GlobalItems.UserProfileChanged += UserProfileChanged;
            NavigationStores.MainNavigationStore.CurrentVievModelChanged += () => RaisePropertyChanged(nameof(CurrentMainViewModel));
            NavigationStores.PopupNavigationStore.CurrentVievModelChanged += OnCurrentPopupViewModelChanged;

            SwitchAccountCommand = new RelayCommand(() => OAuthSystem.LogInAsync(true));
            DisplayUserProfileCommand = new RelayCommand(DisplayUserProfileMethod);
            LogInCommand = new RelayCommand(LogIn);

            UserProfileChanged();
        }


        public string LogInText { get; set; }
        public BitmapImage LogInImage { get; set; } 

        private void UserProfileChanged()
        {
            if (UserProfile == null)
            {
                LogInText = "Log in";
                LogInImage = LocalUtilities.GetResourcesBitmapImage(@"Symbols/White/login_64px.png");
                DisplayUserProfile = true;
            }
            else
            {
                LogInText = "Log out";
                LogInImage = LocalUtilities.GetResourcesBitmapImage(@"Symbols/White/logout_64px.png");
                DisplayUserProfile = false;
                ToastMessage.Hide(true);
            }


            RaisePropertyChanged(nameof(DisplayUserProfile));
            RaisePropertyChanged(nameof(LogInText));
            RaisePropertyChanged(nameof(LogInImage));
            RaisePropertyChanged(nameof(UserProfile));
        }

        private void LogIn()
        {
            if (UserProfile == null)
                OAuthSystem.LogInAsync();
            else
                OAuthSystem.LogOutAsync();
        }


        private void DisplayUserProfileMethod()
        {
            DisplayUserProfile = !DisplayUserProfile;
            RaisePropertyChanged(nameof(DisplayUserProfile));
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
        public RelayCommand LogInCommand { get; }

        public RelayCommand SwitchAccountCommand { get; }
        public RelayCommand DisplayUserProfileCommand { get; }
    }
}
