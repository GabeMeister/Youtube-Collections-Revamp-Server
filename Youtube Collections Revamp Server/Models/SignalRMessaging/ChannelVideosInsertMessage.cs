using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class ChannelVideosInsertMessage : SignalRMessage
    {
        public string ChannelId;

        public ChannelVideosInsertMessage(string channelId)
        {
            Message = "ChannelVideosInsert";
            ChannelId = channelId;
        }
    }
}