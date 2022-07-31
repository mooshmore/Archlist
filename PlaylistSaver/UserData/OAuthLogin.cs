
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PlaylistSaver.Enums;

namespace PlaylistSaver.UserData
{
    public static class OAuthLogin
    {
        private static string clientID;
        private static string clientSecret;

        private static readonly string[] scopes = { YouTubeService.Scope.YoutubeReadonly, "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/userinfo.profile" };

        private static UserCredential credentials;
        public static YouTubeService youtubeService;

        public static async void LogInAsync()
        {
            // Log in the user
            credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientID,
                    ClientSecret = clientSecret
                },
                scopes, user: "user", CancellationToken.None).Result;

            // Refresh the token if it has expired
            if (credentials.Token.IsExpired(SystemClock.Default))
                credentials.RefreshTokenAsync(CancellationToken.None).Wait();

            // Create youtube service instance with retrieved user cridentials & data
            youtubeService = new Google.Apis.YouTube.v3.YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });

            await CreateUserProfileAsync();
        }
        
        private async static Task CreateUserProfileAsync()
        {
            string currentUserProfileData = await GetUserProfileDataAsync();
            UserProfile currentUserProfile = new(currentUserProfileData);

            // If there exists user profile that has been used previously check if
            // it was the same profile as currently
            if (SavedUserProfileExists())
            {
                string previousUserProfileData = ReadSavedUserProfileDataAsync();
                UserProfile previousUserProfile = new(previousUserProfileData);

                // Check if the account is the same as previous account by checking the email
                if (currentUserProfile.Email != previousUserProfile.Email)
                {

                }
            }

            // Save the data into the file
            File.WriteAllText(Path.Combine(Directories.UserDataDirectory.FullName, "userProfile.json"), currentUserProfileData);

            GlobalItems.UserProfile = currentUserProfile;
        }

        private static async Task<string> GetUserProfileDataAsync()
        {
            // builds the  request
            string userinfoRequestURI = "https://www.googleapis.com/oauth2/v3/userinfo";

            // sends the request
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add($"Authorization: Bearer {credentials.Token.AccessToken}");

            // gets the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
            using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
            {
                // reads response body
                return await userinfoResponseReader.ReadToEndAsync();
            }
        }

        private static string ReadSavedUserProfileDataAsync()
        {
            return File.ReadAllText(Path.Combine(Directories.UserDataDirectory.FullName, "userProfile.json"));
        }

        private static bool SavedUserProfileExists()
        {
            return File.Exists(Path.Combine(Directories.UserDataDirectory.FullName, "userProfile.json"));
        }

        public static async void SwitchAccount()
        {
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(credentials, CancellationToken.None);
        }

        public static async Task<bool> LogOut()
        {
            //bool response = Task.Run(async () => LogOut().Result).Result;
            return await credentials.RevokeTokenAsync(CancellationToken.None);
        }

        public static void LoadSecretData()
        {
            JObject clientSecretFile = JObject.Parse(File.ReadAllText("clientSecret.json"));

            clientID = clientSecretFile.SelectToken("installed.client_id").ToString();
            clientSecret = clientSecretFile.SelectToken("installed.client_secret").ToString();
        }
    }
}
