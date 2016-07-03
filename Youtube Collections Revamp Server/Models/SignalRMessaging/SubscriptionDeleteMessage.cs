using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class SubscriptionDeleteMessage : SignalRMessage
    {
        public string YoutubeIdAffected;

        public SubscriptionDeleteMessage(string youtubeIdAffected)
        {
            Message = "SubscriptionDelete";
            YoutubeIdAffected = youtubeIdAffected;
        }
    }
}