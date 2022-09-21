using Newtonsoft.Json;
using Archlist.PlaylistMethods;
using Archlist.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Helpers;
using Archlist.Windows;

namespace Archlist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Chrome

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(8);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;

            // Window chrome event
            StateChanged += MainWindowStateChangeRaised;
            DataContext = new MainWindowViewModel();
            this.Loaded += SetWindowSize;
        }

        private void SetWindowSize(object sender, RoutedEventArgs e)
        {
            // Setting the window size to assure that the window isn't larger than
            // the available area on the screen ( especially notable on low resolution screens
            // and with windows screen scaling turned up)

            this.Width = MethodsCluster.GetValueMax(1500, SystemParameters.WorkArea.Width - 100);
            this.Height = MethodsCluster.GetValueMax(900, SystemParameters.WorkArea.Height - 70);

            this.Left = MethodsCluster.GetValueMin(this.Left, 50);
            this.Top = MethodsCluster.GetValueMin(this.Top, 30);
        }
    }
}
