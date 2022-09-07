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
using System.Windows;
using System.Windows.Input;

namespace Archlist.Windows.PopupViews.WelcomeScreenWindow
{
    public class WelcomeScreenViewModel : ViewModelBase
    {
        public WelcomeScreenViewModel(Window window)
        {
            LogInCommand = new RelayCommand(LogIn);
            Window = window;
        }

        public RelayCommand LogInCommand { get; }
        public Window Window { get; }
        public string AppVersion => GlobalItems.AppVersion;

        public void LogIn()
        {
            OAuthSystem.LogInAsync();

            // And yeah, I know that this isn't mvvm and I'm kinda cheating here,
            // but I just wanna finish this app and I have no clue and time to figure out
            // how to handle two separate windows with mvvm. Fix it if you got nothing to do.


            var mainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
            };

            mainWindow.Show();
            mainWindow.Activate();

            Window.Close();
        }
    }
}
