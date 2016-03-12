using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class SubscriptionInsertMessage : SignalRMessage
    {
        public int SubscriptionChannelId;
        public string SubscriptionChannelTitle;

        public SubscriptionInsertMessage(int subscriptionIndex, int? totalSubscriptions, int beingSubscribedToChannelId, string subscriptionTitle)
        {
            Debug.Assert(totalSubscriptions != null, "Error: totalSubscriptions is null");

            Message = string.Format("Fetching {0}/{1}...", subscriptionIndex, totalSubscriptions);
            SubscriptionChannelId = beingSubscribedToChannelId;
            SubscriptionChannelTitle = subscriptionTitle;
        }
    }
}