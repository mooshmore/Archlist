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

namespace Archlist.UserData
{
    public class UserProfile
    {
        public UserProfile(string userProfileData)
        {
            JObject userProfileObject = JObject.Parse(userProfileData);

            Name = userProfileObject.SelectToken("name").ToString();
            Email = userProfileObject.SelectToken("email").ToString();
            PictureURL = userProfileObject.SelectToken("picture").ToString();


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

        public string Name { get; set; }
        public string Email { get; set; }
        public string PictureURL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Setting the property as static will make the binding not work.")]
        public string PicturePath => Path.Combine(Directories.UserDataDirectory.FullName, "userPicture.jpg");
        public string WelcomeGreeting { get; set; }
    }
}
