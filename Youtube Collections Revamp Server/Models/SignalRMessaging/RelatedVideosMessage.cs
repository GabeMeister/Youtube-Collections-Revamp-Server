using System.Collections.Generic;

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