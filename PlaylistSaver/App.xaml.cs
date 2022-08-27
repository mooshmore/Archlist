﻿using PlaylistSaver.PlaylistMethods;
using PlaylistSaver.ProgramData;
using PlaylistSaver.ProgramData.Commands;
using PlaylistSaver.ProgramData.Stores;
using PlaylistSaver.Windows;
using PlaylistSaver.Windows.MainWindowViews.Homepage;
using PlaylistSaver.Windows.MainWindowViews.PlaylistItems;
using PlaylistSaver.Windows.PopupViews.AddPlaylists.AddPlaylists_link;
using PlaylistSaver.Windows.PopupViews.WelcomeScreenWindow;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlaylistSaver
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

            MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
            };

            MainWindow.Show();
            base.OnStartup(e);
        }
    }

}
