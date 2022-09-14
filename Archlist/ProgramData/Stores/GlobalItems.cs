using Archlist.UserData;
using Archlist.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Archlist.ProgramData.Stores
{
    public static class GlobalItems
    {
        public static HttpClient HttpClient { get; } = new HttpClient();

        public static string AppVersion { get; set; } = "v 0.8.7";

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
