using PlaylistSaver.UserData;
using PlaylistSaver.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.ProgramData.Stores
{
    public static class GlobalItems
    {
        public static string AppVersion { get; set; } = "0.7.1 Alpha";

        public static event Action UserProfileChanged;

        private static UserProfile _userProfile;

        public static UserProfile UserProfile
        {
            get => _userProfile;
            set
            {
                _userProfile = value;
                OnUserProfileChanged();
            }
        }

        private static void OnUserProfileChanged()
        {
            UserProfileChanged?.Invoke();
        }
    }
}
