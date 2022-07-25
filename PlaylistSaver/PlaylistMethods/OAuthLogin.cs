using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistSaver.PlaylistMethods
{
    public static class OAuthLogin
    {
        private static string clientID;
        private static string clientSecret;

        private static readonly string[] scopes = { YouTubeService.Scope.YoutubeReadonly };

        private static UserCredential credentials;
        public static YouTubeService youtubeService;

        public static void LogIn()
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
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });
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
