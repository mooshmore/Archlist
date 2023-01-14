using Archlist.PlaylistMethods;
using Archlist.ProgramData;
using Utilities.WPF.Bases;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
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
            LogInCommand = new AsyncRelayCommand(LogIn);
            Window = window;
        }

        public AsyncRelayCommand LogInCommand { get; }
        public Window Window { get; }
        public string AppVersion => GlobalItems.AppVersion;

        public async Task LogIn()
        {
            await OAuthSystem.LogInAsync();

            var mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.Activate();

            // Shhh this isn't here
            Window.Close();
        }
    }
}
