using Archlist.PlaylistMethods;
using Archlist.ProgramData;
using Archlist.ProgramData.Bases;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Archlist.Windows.PopupViews.WelcomeScreenWindow
{
    public class WelcomeScreenViewModel : ViewModelBase
    {
        public WelcomeScreenViewModel()
        {
            LogInCommand = new RelayCommand(OpenOAuthLoginPage);
        }

        public RelayCommand LogInCommand { get; }
        public string AppVersion => GlobalItems.AppVersion;

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
