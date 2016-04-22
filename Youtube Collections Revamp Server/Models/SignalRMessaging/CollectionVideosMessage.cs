using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YoutubeCollectionsRevampServer.Models.ObjectHolderModels;

namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class CollectionVideosMessage : SignalRMessage
    {
        public List<VideoHolder> CollectionVideos { get; set; }

        public CollectionVideosMessage()
        {
            Message = "CollectionVideos";
            CollectionVideos = new List<VideoHolder>();
        }

        public CollectionVideosMessage(List<VideoHolder> videos)
        {
            Message = "CollectionVideos";
            CollectionVideos = videos;
        }

    }
}