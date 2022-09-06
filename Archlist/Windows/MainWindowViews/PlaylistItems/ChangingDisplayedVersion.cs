using Helpers;
using Archlist.PlaylistMethods.Models;
using Archlist.ProgramData.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archlist.Windows.MainWindowViews.PlaylistItems
{
    public partial class PlaylistItemsViewModel
    {
        #region Properties

        public ObservableCollection<string> PlaylistSavedDays { get; set; }
        public ObservableCollection<string> PlaylistSavedHours { get; set; }
        public DisplayPlaylist DisplayedPlaylist { get; set; }

        public string _displayedDay;
        public string DisplayedDay
        {
            get => _displayedDay;
            set
            {
                _displayedDay = value;
                RaisePropertyChanged();
            }
        }

        public string _displayedHour;
        public string DisplayedHour
        {
            get => _displayedHour;
            set
            {
                _displayedHour = value;
                RaisePropertyChanged();
            }
        }

        public bool _displayedHourPanelVisibility;
        public bool DisplayedHourPanelVisibility
        {
            get => _displayedHourPanelVisibility;
            set
            {
                _displayedHourPanelVisibility = value;
                RaisePropertyChanged();
            }
        }

        public bool _displayedDayPanelVisibility;
        public bool DisplayedDayPanelVisibility
        {
            get => _displayedDayPanelVisibility;
            set
            {
                _displayedDayPanelVisibility = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand DisplayDayPanelCommand { get; }
        public RelayCommand DisplayHourPanelCommand { get; }
        public RelayCommand ChangeDisplayedDayCommand { get; }
        public RelayCommand ChangeDisplayedHourCommand { get; }
        public RelayCommand DisplayPlaylistItemInfoCommand { get; }

        #endregion
        private void DisplayDayPanel()
        {
            DisplayedHourPanelVisibility = false;
            DisplayedDayPanelVisibility = !DisplayedDayPanelVisibility;
        }

        private void DisplayHourPanel()
        {
            DisplayedDayPanelVisibility = false;
            DisplayedHourPanelVisibility = !DisplayedHourPanelVisibility;
        }

        private void ChangeDisplayedDay(object day)
        {
            DisplayedDay = (string)day;
            DisplayedDayPanelVisibility = false;
            LoadSavedHours();

            LoadPlaylistItems();
        }

        private void ChangeDisplayedHour(object hour)
        {
            DisplayedHour = (string)hour;
            DisplayedHourPanelVisibility = false;

            LoadPlaylistItems();
        }

        private void LoadSavedDays()
        {
            var savedDays = DisplayedPlaylist.DataDirectory.GetSubDirectoriesNames();
            // Reverse so that they will be in order from newest to oldest
            savedDays.Reverse();
            PlaylistSavedDays = new ObservableCollection<string>(savedDays);
        }

        private void LoadSavedHours()
        {
            if (DisplayedDay != null)
            {
                DirectoryInfo selectedDayDirectory = new(Path.Combine(DisplayedPlaylist.DataDirectory.FullName, DisplayedDay));
                List<string> hourFilesNames = selectedDayDirectory.GetSubFilesNames();
                hourFilesNames = hourFilesNames.Select(s => s.Replace("=", ":").Replace(".json", "")).ToList();
                // Reverse so that they will be in order from newest to oldest
                hourFilesNames.Reverse();

                PlaylistSavedHours = new ObservableCollection<string>(hourFilesNames);
                RaisePropertyChanged(nameof(PlaylistSavedHours));
                DisplayedHour = PlaylistSavedHours[0];
            }
        }
    }
}
