using Archlist.ProgramData;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows;
using Archlist.Windows.MainWindowViews.Homepage;
using Archlist.Windows.PopupViews.WelcomeScreenWindow;
using System.Windows;

namespace Archlist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Method for hyperlinks
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Configure and create data, files etc required by the program
            AppConfiguration.Configure();
            NavigationStores.MainNavigationStore.CurrentViewModel = new HomepageViewModel();


            if (Settings.WasPreviouslyLoggedIn)
            {
                new MainWindow()
                {
                    DataContext = new MainWindowViewModel()
                }.Show();
                OAuthSystem.LogInAsync();
            }
            else
            {
                var welcomeScreen = new WelcomeScreenView();
                // Yesh I know this is illegal in mvvm
                welcomeScreen.DataContext = new WelcomeScreenViewModel(welcomeScreen);
                welcomeScreen.Show();
                welcomeScreen.Activate();
            }

            base.OnStartup(e);
        }
    }

}
