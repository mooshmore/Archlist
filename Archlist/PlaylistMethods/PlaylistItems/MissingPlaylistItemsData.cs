using Google.Apis.YouTube.v3.Data;
using Utilities;
using Archlist.Helpers;
using Archlist.PlaylistMethods.Models;
using Archlist.ProgramData.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebArchiveData;

namespace Archlist.PlaylistMethods
{
    /// <summary>
    /// Methods for playlist items data that are strictly related to missing items.
    /// </summary>
    public static class MissingPlaylistItemsData
    {
        public static void SaveMissingItemsData(KeyValuePair<string, PlaylistItemListResponse> playlist, List<MissingPlaylistItem> missingItems)
        {
            // Save missing items data
            var missingItemsFile = new FileInfo(Path.Combine(Directories.AllPlaylistsDirectory.FullName, playlist.Key, "missingItems", "recent.json"));
            var previousMissingItems = missingItemsFile.Deserialize<List<MissingPlaylistItem>>();
            // Merge previously missing recent items with the new ones
            missingItems.AddRange(previousMissingItems);
            missingItemsFile.Serialize(missingItems);
        }


    }
}
