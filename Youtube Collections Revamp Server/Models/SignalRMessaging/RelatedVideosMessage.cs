using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class RelatedVideosMessage : SignalRMessage
    {
        public List<string> UnwatchedYoutubeVideoIds {get; set;}

        public RelatedVideosMessage(List<string> youtubeVideoIds)
        {
            Message = "RelatedVideos";
            UnwatchedYoutubeVideoIds = youtubeVideoIds;
        }
    }
}