
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
using MsServices.ToastMessageService;
using System.Reflection;
using System.Diagnostics;
using Utilities;
using Archlist.PlaylistMethods;
using System.Collections.Generic;
using Google.Apis.YouTube.v3.Data;
using System.Windows;

namespace Archlist.UserData
{
    public static class OAuthSystem
    {
        private static string clientID;
        private static string clientSecret;

        private static readonly string[] scopes = { YouTubeService.Scope.YoutubeReadonly, "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/userinfo.profile" };

        private static UserCredential credentials;
        public static YouTubeService YoutubeService { get; set; }

        public static async Task LogInAsync(bool swtichAccounts = false)
        {
            if (swtichAccounts && credentials != null)
            {
                ToastMessage.Loading("Switching account");
                await GoogleWebAuthorizationBroker.ReauthorizeAsync(credentials, CancellationToken.None);
            }
            else
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
                await credentials.RefreshTokenAsync(CancellationToken.None);

            // Create youtube service instance with retrieved user cridentials & data
            YoutubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });

            await CreateUserProfileAsync();
            await ToastMessage.Hide();

            Settings.WasPreviouslyLoggedIn = true;
            Settings.SettingsInstance.Save();
            Application.Current.MainWindow.Activate();
        }

        public static async Task LogOutAsync()
        {
            // If credentials are null then the user wasn't logged in in the first place.
            if (credentials != null)
            {
                try
                {
                    await credentials.RevokeTokenAsync(CancellationToken.None);
                }
                catch (Exception)
                {
                    // User wasn't already logged in because
                    // token has been revoked or it has expired
                    GlobalItems.UserProfile = null;
                    return;
                }
                GlobalItems.UserProfile = null;
                ToastMessage.Display("Logged out!");
                return;
            }
        }

        private async static Task CreateUserProfileAsync()
        {
            string userProfileString = await GetUserProfileDataAsync();

            // Check if the user has granted the acces to youtube data
            if (!credentials.Token.Scope.Contains("https://www.googleapis.com/auth/youtube.readonly"))
            {
                ToastMessage.InformationDialog("Log in failed: you need to grant acces to your Youtube account data for this app to work.\nTry logging in again.");
                await credentials.RevokeTokenAsync(CancellationToken.None);
                GlobalItems.UserProfile = null;
                return;
            }

            Channel userChannel = await ChannelsData.GetCurrentUserChannelAsync();

            string userChannelId;
            if (userChannel == null)
                userChannelId = "";
            else
                userChannelId = userChannel.Id;


            // Add a channelId to the user profile data
            JObject userProfileData = JObject.Parse(userProfileString);
            userProfileData["channelId"] = userChannelId;

            UserProfile currentUserProfile = new(userProfileData, userChannelId);

            // Save the data into the file
            DirectoryInfo userDataDirectory = Directories.UsersDataDirectory.CreateSubdirectory(currentUserProfile.Id);
            FileInfo userDataFile = userDataDirectory.CreateSubfile("userProfile.json");
            userDataFile.WriteAllText(userProfileData.ToString());

            await LocalUtilities.DownloadImageAsync(currentUserProfile.PictureURL, Path.Combine(userDataDirectory.FullName, "userPicture.jpg"));
            GlobalItems.UserProfile = currentUserProfile;
            UserProfile.CheckUserProfile();
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
                FileInfo userDataFile = new(Path.Combine(userDirectory.FullName, "userProfile.json"));
                JObject userData = JObject.Parse(userDataFile.ReadAllText());
                userProfiles.Add(new UserProfile(userData));
            }
            return userProfiles;
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
