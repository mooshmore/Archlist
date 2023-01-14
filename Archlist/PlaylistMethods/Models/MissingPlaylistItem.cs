using Google.Apis.YouTube.v3.Data;
using Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archlist.PlaylistMethods.Models
{
    public class MissingPlaylistItem : PlaylistItem
    {
        public MissingPlaylistItem(PlaylistItem playlistItem)
        {
            // Skip on deserializing
            if (playlistItem != null)
                playlistItem.CopyProperties(this);
        }

        public bool RecoveryFailed { get; set; } = true;
        public bool SourcedFromWebArchive { get; set; }
        public string WebArchiveLink { get; set; }

        public int ExistingSnapshotsCount { get; set; }
        public DateTime FoundMissingDate { get; set; }
        public string RemovalReasonShort { get; set; }
        public string RemovalReasonFull { get; set; }
    }
}
