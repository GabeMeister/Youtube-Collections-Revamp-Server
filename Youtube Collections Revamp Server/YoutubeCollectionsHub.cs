using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using YoutubeCollectionsRevampServer.Models.SignalRMessaging;
using System.Web.Caching;
using System.Diagnostics;

namespace YoutubeCollectionsRevampServer
{
    [HubName("YoutubeCollectionsServer")]
    public class YoutubeCollectionsHub : Hub
    {
        #region SignalR Communication

        public void TestConnection()
        {
            Clients.Caller.onTestClick();
        }

        public void InsertYoutubeId(string youtubeId)
        {
            YoutubeTasks.YoutubeTasks.InsertYoutubeChannelIdIntoDatabase(youtubeId);
            this.Clients.Caller.onChannelIdInserted();
        }

        public void FetchAndInsertChannelSubscriptions(string youtubeId)
        {
            YoutubeTasks.YoutubeTasks.FetchAndInsertChannelSubscriptions(this, youtubeId);
            this.Clients.Caller.onSubscriptionsInserted();
        }

        public void InsertCollection(string collectionName, string youtubeId)
        {
            YoutubeTasks.YoutubeTasks.InsertCollection(collectionName, youtubeId);
        }

        public void RenameCollection(string oldCollectionTitle, string newCollectionTitle, string userYoutubeId)
        {
            YoutubeTasks.YoutubeTasks.RenameCollection(oldCollectionTitle, newCollectionTitle, userYoutubeId);
        }

        public void InsertCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            YoutubeTasks.YoutubeTasks.InsertCollectionItem(collectionItemYoutubeId, collectionTitle, userYoutubeId);
        }

        public void DeleteCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            YoutubeTasks.YoutubeTasks.DeleteCollectionItem(collectionItemYoutubeId, collectionTitle, userYoutubeId);
        }

        public void InsertWatchedVideo(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            YoutubeTasks.YoutubeTasks.InsertWatchedVideo(this, youtubeVideoId, userYoutubeId, dateViewed);
        }

        public void MarkVideoAsWatched(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            YoutubeTasks.YoutubeTasks.MarkVideoAsWatched(youtubeVideoId, userYoutubeId, dateViewed);
        }

        public void RestartInitialization()
        {
            // Delete Gabe's channel
            YoutubeTasks.YoutubeTasks.CompletelyDeleteChannel("UC4LVLoBN0xbOb5xJuA0ia9A");
        }

        public void RestartCollectionItems()
        {
            // Delete all collection items from Gabe's channel
            YoutubeTasks.YoutubeTasks.DeleteChannelCollectionItems("UC4LVLoBN0xbOb5xJuA0ia9A");
        }

        public void GetUnwatchedVideos(string userYoutubeId, List<string> youtubeIds)
        {
            YoutubeTasks.YoutubeTasks.GetUnwatchedVideos(this, userYoutubeId, youtubeIds);
        }

        public void GetVideosForCollection(string userYoutubeId, string collectionTitle)
        {
            YoutubeTasks.YoutubeTasks.GetVideosForCollection(this, userYoutubeId, collectionTitle);
        }
        
        public void GetChannelsNotDownloaded(List<string> youtubeIds)
        {
            // TODO: I think we have to remove this
            List<string> channelsToDownloadYoutubeIds = YoutubeTasks.YoutubeTasks.GetChannelsNotDownloaded(youtubeIds);
            var message = new ChannelsToDownloadMessage(channelsToDownloadYoutubeIds);
            this.Clients.Caller.onChannelsToDownloadFetched(message);
        }

        public void UpdateSubscriptions(string userYoutubeId)
        {
            YoutubeTasks.YoutubeTasks.UpdateSubscriptions(this, userYoutubeId);
        }

        public void GetChannelsWithVideosInserted(List<string> notLoadedYoutubeIds)
        {
            YoutubeTasks.YoutubeTasks.GetChannelsWithVideosInserted(this, notLoadedYoutubeIds);
        }

        public void AddAthlean()
        {
            YoutubeTasks.YoutubeTasks.AddAthlean();
        }

        public void DeleteAthlean()
        {
            YoutubeTasks.YoutubeTasks.DeleteAthlean();
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

        public void NotifyCallerOfSubscriptionUpdate(SignalRMessage message)
        {
            Clients.Caller.onSubscriptionUpdated(message);
        }

        public void NotifyCallerOfChannelVideosInserted(SignalRMessage message)
        {
            Clients.Caller.onChannelVideosInserted(message);
        }

        #endregion


    }
}