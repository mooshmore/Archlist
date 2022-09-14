
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json.Linq;
using Archlist.ProgramData.Stores;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Archlist.Helpers;
using ToastMessageService;
using System.Reflection;
using System.Diagnostics;
using Helpers;
using Archlist.PlaylistMethods;
using System.Collections.Generic;

namespace Archlist.UserData
{
    public static class OAuthSystem
    {
        private static string clientID;
        private static string clientSecret;

        private static readonly string[] scopes = { YouTubeService.Scope.YoutubeReadonly, "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/userinfo.profile" };

        private static UserCredential credentials;
        public static YouTubeService YoutubeService { get; set; }

        public static async Task LogInAsync()
        {
            ToastMessage.Loading("Logging in");

            // Log in the user
            // This for some reason throws exception System.ObjectDisposedException.
            // I don't know why, but it works, so I don't ask.
            Debug.WriteLine("Known exception at LogInAsync() :");

            credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientID,
                ClientSecret = clientSecret
            },
            scopes, user: "user", CancellationToken.None);

            // Refresh the token if it has expired
            if (credentials.Token.IsExpired(SystemClock.Default))
                // GoogleWebAuthorizationBroker.ReauthorizeAsync ?
                await credentials.RefreshTokenAsync(CancellationToken.None);

            // Create youtube service instance with retrieved user cridentials & data
            YoutubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });

            // ! Check if the user has given all privelages to read data

            await CreateUserProfileAsync();
            await ToastMessage.Hide();
            Settings.WasPreviouslyLoggedIn = true;
            Settings.SettingsInstance.Save();
        }
        
        private async static Task CreateUserProfileAsync()
        {
            string userProfileString = await GetUserProfileDataAsync();
            string currentUserChannelId = (await ChannelsData.GetCurrentUserChannelAsync()).Id;

            // Add a channelId to the user profile data
            JObject userProfileData = JObject.Parse(userProfileString);
            userProfileData["channelId"] = currentUserChannelId;

            UserProfile currentUserProfile = new(userProfileData, currentUserChannelId);

            // Save the data into the file
            DirectoryInfo userDataDirectory = Directories.UsersDataDirectory.CreateSubdirectory(currentUserProfile.ID);
            FileInfo userDataFile = userDataDirectory.CreateSubfile("userProfile.json");
            userDataFile.WriteAllText(userProfileData.ToString());

            await LocalHelpers.DownloadImageAsync(currentUserProfile.PictureURL, Path.Combine(userDataDirectory.FullName, "userPicture.jpg"));
            GlobalItems.UserProfile = currentUserProfile;
        }

        private static async Task<string> GetUserProfileDataAsync()
        {
            // Build the request
            string userinfoRequestURI = "https://www.googleapis.com/oauth2/v3/userinfo";
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add($"Authorization: Bearer {credentials.Token.AccessToken}");

            // Get the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();

            // Read the response
            using StreamReader userinfoResponseReader = new(userinfoResponse.GetResponseStream());
                return await userinfoResponseReader.ReadToEndAsync();
        }

        public static List<UserProfile> ReadSavedUserProfiles()
        {
            List<UserProfile> userProfiles = new();
            foreach (var userDirectory in Directories.UsersDataDirectory.GetDirectories())
            {
                FileInfo userDataFile = new FileInfo(Path.Combine(userDirectory.FullName, "userProfile.json"));
                JObject userData = JObject.Parse(userDataFile.ReadAllText());
                userProfiles.Add(new UserProfile(userData));
            }
            return userProfiles;
        }

        private static string ReadSavedUserProfileData()
        {
            return File.ReadAllText(Path.Combine(Directories.UsersDataDirectory.FullName, "userProfile.json"));
        }

        private static bool SavedUserProfileExists()
        {
            return File.Exists(Path.Combine(Directories.UsersDataDirectory.FullName, "userProfile.json"));
        }

        public static async Task SwitchAccountAsync()
        {
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(credentials, CancellationToken.None);
            await CreateUserProfileAsync();
        }

        public static async Task LogOutAsync()
        {
            await credentials.RevokeTokenAsync(CancellationToken.None);
            ToastMessage.Display("Logged out!");
            return;
        }

        public static void LoadSecretData()
        {

            JObject clientSecretFile;

            // App publish version path
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"clientSecret.json")))
                clientSecretFile = JObject.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"clientSecret.json")));
            else
                throw new NotImplementedException(@"
                    Missing a client secret file! 
                    This file cannot be available publicly inside the repo. 
                    If you are a recruiter or want to test out/contribute to app development, contact me at:
                    e-mail: mooshmore@gmail.com
                    or Discord: mooshmore#0763 (definitely quicker response here)
                    and I will provide it to you!"
                );

            clientID = clientSecretFile.SelectToken("installed.client_id").ToString();
            clientSecret = clientSecretFile.SelectToken("installed.client_secret").ToString();
        }
    }
}
