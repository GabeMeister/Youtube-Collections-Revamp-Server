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

namespace YoutubeCollectionsRevampServer
{
    [HubName("YoutubeCollectionsServer")]
    public class YoutubeCollectionsHub : Hub
    {
        private static CacheItemRemovedCallback _onCacheRemove = null;

        public YoutubeCollectionsHub()
        {
            _onCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
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
            YoutubeTasks.InsertWatchedVideo(youtubeVideoId, userYoutubeId, dateViewed);
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
            HttpRuntime.Cache.Insert("TestItem", 1, null, DateTime.Now.AddSeconds(1), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, _onCacheRemove);
        }

        public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
        {
            this.Clients.Caller.onCacheRemoved("Removed!");
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

        #endregion


    }
}