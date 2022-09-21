using Helpers;
using Newtonsoft.Json.Linq;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Archlist.Helpers;
using Archlist.PlaylistMethods;
using ToastMessageService;

namespace Archlist.UserData
{
    public class UserProfile
    {
        /// <summary>
        /// Constructor used for creating a new user profile.
        /// </summary>
        public UserProfile(JObject userProfileData, string channelId = "")
        {
            // One constructor is used when creating the channel when the channelId is given,
            // the other one is used when reading the locally saved user profiles data
            if (channelId == "")
                ChannelId = userProfileData.SelectToken("channelId").ToString();
            else
                ChannelId = channelId;

            Id = userProfileData.SelectToken("sub").ToString();

            Name = userProfileData.SelectToken("name").ToString();

            Email = userProfileData.SelectToken("email").ToString();
            // If user has a different channel on a single youtube account his email will be
            // auto generated and not worth displaying, so a "Youtube channel" is displayed instead
            if (Email.EndsWith("@pages.plusgoogle.com"))
                DisplayEmail = "Youtube channel";
            else
                DisplayEmail = userProfileData.SelectToken("email").ToString();

            PictureURL = userProfileData.SelectToken("picture").ToString();

            WelcomeGreeting = GenerateWelcomeMessage();
        }

        private string GenerateWelcomeMessage()
        {
            return new List<string>()
            {
                $"Hiya, {Name}",
                $"Hey there, {Name}",
                $"Sup, {Name}",
                $"Hello there, general {Name}",
                $"Hi there, {Name}",
                $"Howdy, {Name}",
                $"Ahoy, {Name}",
                "What’s cookin’, good lookin’?",
                "Hey there, hot stuff",
                $"So… we meet at last, {Name}",
                $"Greetings, {Name}.",
                $"What does a horse eat? Hayyyyyyy.",
                $"Ello, matey.",
                $"Ghostbusters, watccha want?",
                $"I thought I would never see you again, {Name}",
                $"Howdy-doody, {Name}",
                $"Konnichiwa, {Name} - San (◕‿◕✿)"
            }.Random();
        }

        /// <summary>
        /// Checks if the user is logged in and if he has a associated Youtube channel.
        /// </summary>
        /// <returns>True if the user is logged in; False if not.</returns>
        public static bool CheckUserLoggedIn()
        {
            if (GlobalItems.UserProfile == null)
            {
                ToastMessage.InformationDialog("You are not logged in.", IconType.Error);
                return false;
            }
                return true;
        }


        /// <summary>
        /// Checks if the user is logged in and if he has a associated Youtube channel.
        /// </summary>
        /// <returns>True if the user is logged in and has a channel; False if any not.</returns>
        public static bool CheckUserProfile()
        {
            if (GlobalItems.UserProfile == null)
            {
                ToastMessage.InformationDialog("You are not logged in.", IconType.Error);
                return false;
            }
            else if (GlobalItems.UserProfile.ChannelId == "")
            {
                ToastMessage.InformationDialog("Your Google account doesn't have a associated Youtube channel.\nChoose a different account.", IconType.Error);
                return false;
            }
            else
                return true;
        }

        public string Id { get; }
        public string ChannelId { get; }
        public string Name { get; }
        public string Email { get; }
        public string DisplayEmail { get; }
        public string PictureURL { get; }
        public WriteableBitmap Picture => DirectoryExtensions.CreateWriteableBitmap(Path.Combine(UserProfileDirectory.FullName, "userPicture.jpg"));
        public string WelcomeGreeting { get; }

        public DirectoryInfo UserProfileDirectory => new DirectoryInfo(Path.Combine(Directories.UsersDataDirectory.FullName, Id));
    }
}
