using Archlist.ProgramData;
using Archlist.ProgramData.Stores;
using Archlist.UserData;
using Archlist.Windows;
using Archlist.Windows.MainWindowViews.Homepage;
using Archlist.Windows.PopupViews.WelcomeScreenWindow;
using MsServices.DialogService;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Utilities.Systems;

namespace Archlist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Create(this);

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

            //_dialogService.ShowDialog("Notification", result =>
            //{
            //    var test = result;
            //});

            base.OnStartup(e);
        }

        IDialogService _dialogService = new DialogService();
    }

}
