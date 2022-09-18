using Archlist.ProgramData;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows;
using Archlist.Windows.MainWindowViews.Homepage;
using Archlist.Windows.PopupViews.WelcomeScreenWindow;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
            // Release command:
            // dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true /p:IncludeNativeLibrariesForSelfExtract=true -p:IncludeAllContentForSelfExtract=true

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
                // Yesh I know this is illegal terms of mvvm
                welcomeScreen.DataContext = new WelcomeScreenViewModel(welcomeScreen);
                welcomeScreen.Show();
                welcomeScreen.Activate();
            }

            base.OnStartup(e);
        }
    }

}
