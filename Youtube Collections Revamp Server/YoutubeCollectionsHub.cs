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
            YoutubeTasks.InsertYoutubeIdIntoDatabase(youtubeId);
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

        #endregion


    }
}