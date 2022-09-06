using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Archlist.Windows.PopupViews.WelcomeScreenWindow
{
    /// <summary>
    /// Interaction logic for WelcomeScreenView.xaml
    /// </summary>
    public partial class WelcomeScreenView : Window
    {
        #region Window Chrome

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
            System.Windows.Application.Current.Shutdown();
        }

        // State change
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                ChromeBorder.BorderThickness = new Thickness(8);
            }
            else
            {
                ChromeBorder.BorderThickness = new Thickness(0);
            }
        }

        #endregion

        public WelcomeScreenView()
        {
            InitializeComponent();

            // Window chrome event
            StateChanged += MainWindowStateChangeRaised;
        }
    }
}
