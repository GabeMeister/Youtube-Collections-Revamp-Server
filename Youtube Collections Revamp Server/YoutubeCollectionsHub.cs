using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading;
using YoutubeCollectionsRevampServer.Controllers.YoutubeTasks;
using YoutubeCollectionsRevampServer.Models;
using YoutubeCollectionsRevampServer.Models.SignalRMessaging;
using System.Web.Caching;
using System.Diagnostics;

namespace YoutubeCollectionsRevampServer
{
    [HubName("YoutubeCollectionsServer")]
    public class YoutubeCollectionsHub : Hub
    {
        public YoutubeCollectionsHub()
        {
        }


        #region SignalR Communication
        public void InsertYoutubeId(string youtubeId)
        {
            YoutubeTasks.InsertYoutubeChannelIdIntoDatabase(youtubeId);
            this.Clients.Caller.onChannelIdInserted();
        }

        public void FetchAndInsertChannelSubscriptions(string youtubeId)
        {
            YoutubeTasks.FetchAndInsertChannelSubscriptions(this, youtubeId);
            this.Clients.Caller.onSubscriptionsInserted();
        }

        public void FetchAndInsertNewChannelSubscriptions(string userYoutubeId)
        {
            YoutubeTasks.FetchAndInsertNewChannelSubscriptions(this, userYoutubeId);
            this.Clients.Caller.onNewSubscriptionsInserted();
        }

        public void InsertCollection(string collectionName, string youtubeId)
        {
            YoutubeTasks.InsertCollection(collectionName, youtubeId);
        }

        public void RenameCollection(string oldCollectionTitle, string newCollectionTitle, string userYoutubeId)
        {
            YoutubeTasks.RenameCollection(oldCollectionTitle, newCollectionTitle, userYoutubeId);
        }

        public void InsertCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            YoutubeTasks.InsertCollectionItem(collectionItemYoutubeId, collectionTitle, userYoutubeId);
        }

        public void DeleteCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            YoutubeTasks.DeleteCollectionItem(collectionItemYoutubeId, collectionTitle, userYoutubeId);
        }

        public void InsertWatchedVideo(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            YoutubeTasks.InsertWatchedVideo(this, youtubeVideoId, userYoutubeId, dateViewed);
        }

        public void MarkVideoAsWatched(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            YoutubeTasks.MarkVideoAsWatched(youtubeVideoId, userYoutubeId, dateViewed);
        }

        public void RestartInitialization()
        {
            // Delete Gabe's channel
            YoutubeTasks.CompletelyDeleteChannel("UC4LVLoBN0xbOb5xJuA0ia9A");
        }

        public void RestartCollectionItems()
        {
            // Delete all collection items from Gabe's channel
            YoutubeTasks.DeleteChannelCollectionItems("UC4LVLoBN0xbOb5xJuA0ia9A");
        }

        public void GetUnwatchedVideos(string userYoutubeId, List<string> youtubeIds)
        {
            YoutubeTasks.GetUnwatchedVideos(this, userYoutubeId, youtubeIds);
        }

        public void GetVideosForCollection(string userYoutubeId, string collectionTitle)
        {
            YoutubeTasks.GetVideosForCollection(this, userYoutubeId, collectionTitle);
        }

        public void ModifyHttpCache()
        {
            HttpRuntime.Cache.Insert("NewChannelsToDownloadTimeLeft", 
                1, 
                null, 
                DateTime.Now.AddSeconds(1), 
                Cache.NoSlidingExpiration, 
                CacheItemPriority.NotRemovable, 
                new CacheItemRemovedCallback(YoutubeTasks.DownloadMissingChannels));

            var current = HttpContext.Current;
            string value = current.Cache.Get("NewChannelsToDownloadTimeLeft").ToString();
            Debug.WriteLine(value);
        }

        public void GetChannelsNotDownloaded(List<string> youtubeIds)
        {
            List<string> channelsToDownloadYoutubeIds = YoutubeTasks.GetChannelsNotDownloaded(youtubeIds);
            var message = new ChannelsToDownloadMessage(channelsToDownloadYoutubeIds);
            this.Clients.Caller.onChannelsToDownloadFetched(message);
        }


        #endregion



        #region Youtube Callbacks
        public void NotifyCaller(SignalRMessage message)
        {
            this.Clients.Caller.onProgressChanged(message);
        }

        public void SendUnrecognizedYoutubeVideoIds(SignalRMessage message)
        {
            this.Clients.Caller.onRelatedVideosChange(message);
        }

        public void SendCollectionVideos(SignalRMessage message)
        {
            this.Clients.Caller.onRelatedVideosChange(message);
        }

        public void NotifyCallerWatchedVideoInserted(SignalRMessage message)
        {
            this.Clients.Caller.onWatchedVideoInserted(message);
        }

        #endregion


    }
}