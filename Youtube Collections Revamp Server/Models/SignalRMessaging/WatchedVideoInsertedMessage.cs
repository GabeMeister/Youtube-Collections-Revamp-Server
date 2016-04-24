using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class WatchedVideoInsertedMessage : SignalRMessage
    {
        public WatchedVideoInsertedMessage()
        {
            Message = "WatchedVideoInserted";
        }
    }
}