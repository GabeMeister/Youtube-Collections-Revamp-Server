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
            YoutubeTasks.InsertYoutubeIdIntoDatabase(youtubeId);
            this.Clients.Caller.onChannelIdInserted();
        }

        public void FetchAndInsertChannelSubscriptions(string youtubeId)
        {
            YoutubeTasks.FetchAndInsertChannelSubscriptions(this, youtubeId);
            this.Clients.Caller.onSubscriptionsInserted();
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