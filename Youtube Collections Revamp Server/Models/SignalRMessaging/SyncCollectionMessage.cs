using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class SyncCollectionMessage : SignalRMessage
    {
        public string Title { get; set; }
        public List<string> ChannelTitles { get; set; }

        public SyncCollectionMessage(string title, List<string> channelTitles)
        {
            Message = "SyncCollection";
            Title = title;
            ChannelTitles = channelTitles;
        }
    }
}