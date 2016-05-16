using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class ChannelsToDownloadMessage : SignalRMessage
    {
        public List<string> YoutubeIds;

        public ChannelsToDownloadMessage()
        {
            Message = "ChannelsToDownload";
            YoutubeIds = new List<string>();
        }

        public ChannelsToDownloadMessage(List<string> youtubeIds)
        {
            Message = "ChannelsToDownload";
            YoutubeIds = youtubeIds;
        }
    }
}