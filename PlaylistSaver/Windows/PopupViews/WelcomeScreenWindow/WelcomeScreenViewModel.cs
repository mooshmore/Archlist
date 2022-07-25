using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData;
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
            LogInCommand = new WelcomeScreenCommand();
        }

        public ICommand LogInCommand { get; }

    }

    public class WelcomeScreenCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            OAuthLogin.LogIn();
            //OAuthLogin.LogOut();

            var mainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
            };

            mainWindow.Show();
            
        }
    }
}
