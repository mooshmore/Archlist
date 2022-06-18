using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistSaver.PlaylistMethods
{
    internal static class OAuthLogin
    {
        private static readonly string clientID = "";
        private static readonly string clientSecret = "";
        private static readonly string[] scopes = { YouTubeService.Scope.YoutubeReadonly };

        private static UserCredential credentials;
        internal static YouTubeService youtubeService;

        internal static void LogIn()
        {

            credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientID,
                    ClientSecret = clientSecret
                },
                scopes, user: "user", CancellationToken.None).Result;


            if (credentials.Token.IsExpired(SystemClock.Default))
            {
                //! ponowny login zamiast refresh tokena ?
                credentials.RefreshTokenAsync(CancellationToken.None).Wait();
            }

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });
        }

        internal static async void SwitchAccount()
        {
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(credentials, CancellationToken.None);
        }

        internal static async Task<bool> LogOut()
        {
            //bool response = Task.Run(async () => LogOut().Result).Result;
            return await credentials.RevokeTokenAsync(CancellationToken.None);
        }
    }
}
