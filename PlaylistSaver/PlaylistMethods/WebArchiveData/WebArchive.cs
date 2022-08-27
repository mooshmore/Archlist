using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveData
{
    public static class WebArchive
    {
        public static async Task<string> GetLatestRecord(string url)
        {
            string response = await new HttpClient().GetStringAsync("https://archive.org/wayback/available?url=" + url);
            WebArchiveResponse webArchiveResponse = JsonConvert.DeserializeObject<WebArchiveResponse>(response);
            if (webArchiveResponse.archived_snapshots == null)
                return null;
            else
                return webArchiveResponse.archived_snapshots.closest.url;
        }
    }
}
