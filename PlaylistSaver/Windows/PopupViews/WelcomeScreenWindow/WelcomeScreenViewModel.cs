using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData;
using PlaylistSaver.ProgramData.Bases;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.UserData;
using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlaylistSaver.Windows.PopupViews.WelcomeScreenWindow
{
    public class WelcomeScreenViewModel : ViewModelBase
    {
        public WelcomeScreenViewModel()
        {
            LogInCommand = new RelayCommand(OpenOAuthLoginPage);
        }

        public RelayCommand LogInCommand { get; }

        public void OpenOAuthLoginPage()
        {
            OAuthSystem.LogInAsync();
            //OAuthLogin.LogOut();

            //var mainWindow = new MainWindow()
            //{
            //    DataContext = new MainWindowViewModel(new NavigationStore())
            //};

            //mainWindow.Show();
        }
    }
}
