using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class SubscriptionInsertMessage : SignalRMessage
    {
        public string SubscriptionYoutubeChannelId;
        public string SubscriptionChannelTitle;
        public string SubscriptionChannelThumbnail;
        public bool AreVideosLoaded;

        public SubscriptionInsertMessage(int subscriptionIndex, 
            int? totalSubscriptions, 
            string beingSubscribedToYoutubeChannelId, 
            string subscriptionTitle, 
            string subscriptionThumbnail,
            bool areVideosLoaded)
        {
            Debug.Assert(totalSubscriptions != null, "Error: totalSubscriptions is null");

            Message = string.Format("Fetching {0}/{1}...", subscriptionIndex, totalSubscriptions);
            SubscriptionYoutubeChannelId = beingSubscribedToYoutubeChannelId;
            SubscriptionChannelTitle = subscriptionTitle;
            SubscriptionChannelThumbnail = subscriptionThumbnail;
            AreVideosLoaded = areVideosLoaded;
        }
    }
}