using Google.Apis.YouTube.v3.Data;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistSaver.PlaylistMethods.Models
{
    public class MissingPlaylistItem : PlaylistItem
    {
        public MissingPlaylistItem(PlaylistItem playlistItem)
        {
            /// Skip on deserializing
            if (playlistItem != null)
                playlistItem.CopyProperties(this);
        }

        public DateTime FoundMissingDate { get; set; }
        public string RemovalReasonShort { get; set; }
        public string RemovalReasonFull { get; set; }
    }
}
