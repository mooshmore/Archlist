using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver
{
    public static class Enums
    {
        public enum PrivacyStatus
        {
            Public,
            Unlisted,
            Private
        }

        public enum ImageQuality
        {
            /// <summary>
            /// default
            /// </summary>
            Minimum = 0,
            /// <summary>
            /// medium
            /// </summary>
            Low = 1,
            /// <summary>
            /// high
            /// </summary>
            Medium = 2,
            /// <summary>
            /// standard
            /// </summary>
            High = 3,
            /// <summary>
            /// maxres
            /// </summary>
            Maximum = 4
        }
    }
}
