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

namespace Archlist.UserData
{
    public class UserProfile
    {
        public UserProfile(JObject userProfileData, string channelID)
        {
            ID = userProfileData.SelectToken("sub").ToString();
            ChannelID = channelID;

            Name = userProfileData.SelectToken("name").ToString();

            Email = userProfileData.SelectToken("email").ToString();
            // If user has a different channel on a single youtube account his email will be
            // auto generated and not worth displaying, so a "Youtube channel" is displayed instead
            if (Email.EndsWith("@pages.plusgoogle.com"))
                DisplayEmail = "Youtube channel";
            else
                DisplayEmail = userProfileData.SelectToken("email").ToString();

            PictureURL = userProfileData.SelectToken("picture").ToString();

            List<string> greetings = new()
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
            };

            WelcomeGreeting = greetings.Random();
        }

        public UserProfile(JObject userProfileData)
        {
            ID = userProfileData.SelectToken("sub").ToString();
            ChannelID = userProfileData.SelectToken("channelId").ToString();

            Name = userProfileData.SelectToken("name").ToString();

            Email = userProfileData.SelectToken("email").ToString();
            // If user has a different channel on a single youtube account his email will be
            // auto generated and not worth displaying, so a "Youtube channel" is displayed instead
            if (Email.EndsWith("@pages.plusgoogle.com"))
                DisplayEmail = "Youtube channel";
            else
                DisplayEmail = userProfileData.SelectToken("email").ToString();

            PictureURL = userProfileData.SelectToken("picture").ToString();

            List<string> greetings = new()
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
            };

            WelcomeGreeting = greetings.Random();
        }

        public string ID { get; }
        public string ChannelID { get; }
        public string Name { get; }
        public string Email { get; }
        public string DisplayEmail { get; }
        public string PictureURL { get; }
        public WriteableBitmap Picture => DirectoryExtensions.CreateWriteableBitmap(Path.Combine(UserProfileDirectory.FullName, "userPicture.jpg"));
        public string WelcomeGreeting { get; }

        public DirectoryInfo UserProfileDirectory => new DirectoryInfo(Path.Combine(Directories.UsersDataDirectory.FullName, ID));
    }
}
