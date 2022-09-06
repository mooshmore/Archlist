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

            MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
            };

            MainWindow.Show();
            base.OnStartup(e);

            //Ok();
        }

        private async Task Ok()
        {
            await Task.Delay(5000);
            for (int i = 0; i < 1000; i++)
            {
                Debug.WriteLine("Print 0 " + i);
                string pageCode = await GlobalItems.HttpClient.GetStringAsync("http://web.archive.org/cdx/search/cdx?url=https://www.youtube.com/watch?v=G_1LP3Z6pW4&limit=-99&fl=timestamp&filter=statuscode:200&collapse=timestamp:14");
                Debug.WriteLine("Print 1 " + i);
                pageCode = await GlobalItems.HttpClient.GetStringAsync("http://web.archive.org/web/20200821084126/https://www.youtube.com/watch?v=BmwE62hsB1c");
                Debug.WriteLine("Print 2 " + i);
                pageCode = await GlobalItems.HttpClient.GetStringAsync("http://web.archive.org/cdx/search/cdx?url=https://www.youtube.com/watch?v=JFSytRgNJIc&limit=-99&fl=timestamp&filter=statuscode:200&collapse=timestamp:10");
                Debug.WriteLine("Print 3 " + i);
                pageCode = await GlobalItems.HttpClient.GetStringAsync("http://web.archive.org/web/20211201152435/https://www.youtube.com/watch?v=JFSytRgNJIc");
                Debug.WriteLine("Print 4 " + i);
                pageCode = await GlobalItems.HttpClient.GetStringAsync("http://web.archive.org/web/20201121143809/https://www.youtube.com/watch?v=SBdnWamuPc8");
                Debug.WriteLine("Print 5 " + i);
                pageCode = await GlobalItems.HttpClient.GetStringAsync("http://web.archive.org/cdx/search/cdx?url=https://www.youtube.com/watch?v=kaAQ3LannNs&limit=-99&fl=timestamp&filter=statuscode:200&collapse=timestamp:14");
                Debug.WriteLine("Print 6 " + i);
                pageCode = await GlobalItems.HttpClient.GetStringAsync("http://web.archive.org/web/20200402060440/https://www.youtube.com/watch?v=MxadwbsZgMg");
                Debug.WriteLine("Done");

            }
        }
    }

}
