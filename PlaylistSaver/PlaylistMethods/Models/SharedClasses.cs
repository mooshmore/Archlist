using Helpers;
using PlaylistSaver.Helpers;
using PlaylistSaver.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static PlaylistSaver.Enums;

namespace PlaylistSaver.PlaylistMethods
{
    public class SharedClasses
    {
        /// /// <summary>
        /// Information about the thumbnail, like its id or maximum resolution.
        /// </summary>
        public class ThumbnailInfo
        {
            /// <remarks>
            /// https://i.ytimg.com/vi/{ThumbnailId}/sddefault.jpg
            /// Thumbnails have the same id even after they have been changed.
            /// You can tell if they can be changed by checking if the video has the same ETag.
            /// </remarks>
            public string Id { get; set; } = null;
            public ImageQuality? SavedImageQuality { get; set; } = null;
            public ImageQuality? MaximumAvailableQuality { get; set; } = null;

            /// <summary>
            /// The name translated name of the quality in which the youtube saves the thumbnails.
            /// </summary>
            public string ImageQualityName => Id == null ? null : LocalHelpers.ImageQualityTranslator(SavedImageQuality);
            public string FileName => Id == null ? null : $"{Id}-{ImageQualityName}.jpg";
            public string URL => Id == null ? null : $"https://i.ytimg.com/vi/{Id}/{ImageQualityName}.jpg";
        }

        public class ItemInfo
        {
            public string ETag { get; set; }
            public string Id { get; set; }
            public DateTime PublishDate { get; set; }
            public string OwnerChannelId { get; set; }
            public PrivacyStatus? PrivacyStatus { get; set; }

            [IgnoreDataMember]
            public BitmapImage PrivacyStatusImage => PrivacyStatus switch
            {
                Enums.PrivacyStatus.Private => DirectoryExtensions.GetImage(Directories.ImagesPath + @"White\lock_white_64px.png"),
                Enums.PrivacyStatus.Unlisted => DirectoryExtensions.GetImage(Directories.ImagesPath + @"White\link_white_64px.png"),
                Enums.PrivacyStatus.Public => DirectoryExtensions.GetImage(Directories.ImagesPath + @"White\earth_white_64px.png"),
                _ => throw new NotImplementedException("No privacy status"),
            };

        }

        /// <summary>
        /// Information about the items content, like title or description.
        /// </summary>
        public class ContentInfo
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }
    }
}
