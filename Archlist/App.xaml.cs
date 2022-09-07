using Archlist.PlaylistMethods;
using Archlist.ProgramData;
using Archlist.ProgramData.Commands;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows;
using Archlist.Windows.MainWindowViews.Homepage;
using Archlist.Windows.MainWindowViews.PlaylistItems;
using Archlist.Windows.PopupViews.AddPlaylists.AddPlaylists_link;
using Archlist.Windows.PopupViews.WelcomeScreenWindow;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
                OAuthSystem.LogInAsync();
                new MainWindow()
                {
                    DataContext = new MainWindowViewModel()
                }.Show();
            }
            else
            {
                var welcomeScreen = new WelcomeScreenView();
                welcomeScreen.DataContext = new WelcomeScreenViewModel(welcomeScreen);
                welcomeScreen.Show();
                welcomeScreen.Activate();
            }

            base.OnStartup(e);
        }
    }

}
