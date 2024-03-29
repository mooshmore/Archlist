﻿using Utilities.WPF.Bases;
using Archlist.Windows.MainWindowViews.Homepage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Archlist.ProgramData.Stores
{
    public static class NavigationStores
    {
        public static NavigationSystem MainNavigationStore { get; set; } = new();
        public static NavigationSystem PopupNavigationStore { get; set; } = new();

        public static NavigateCommand GoToHomePageCommand { get; } = new NavigateCommand(MainNavigationStore, () => new HomepageViewModel());
        public static RelayCommand HidePopupViewCommand { get; } = new RelayCommand(() => PopupNavigationStore.CurrentViewModel = null);

    }
}
