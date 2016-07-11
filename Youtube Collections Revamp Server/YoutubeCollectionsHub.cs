using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using YoutubeCollectionsRevampServer.Models.SignalRMessaging;

namespace YoutubeCollectionsRevampServer
{
    [HubName("YoutubeCollectionsServer")]
    public class YoutubeCollectionsHub : Hub
    {
        #region Collections

        public void InsertCollection(string collectionName, string youtubeId)
        {
            YoutubeTasks.InsertCollection(collectionName, youtubeId);
        }

        public void RenameCollection(string oldCollectionTitle, string newCollectionTitle, string userYoutubeId)
        {
            YoutubeTasks.RenameCollection(oldCollectionTitle, newCollectionTitle, userYoutubeId);
        }

        public void DeleteCollection(string collectionTitleToDelete, string userYoutubeId)
        {
            // TODO
        }

        #endregion

        #region Collection Items

        public void InsertCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            YoutubeTasks.InsertCollectionItem(collectionItemYoutubeId, collectionTitle, userYoutubeId);
        }

        public void DeleteCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            YoutubeTasks.DeleteCollectionItem(collectionItemYoutubeId, collectionTitle, userYoutubeId);
        }

        #endregion

        #region Channels

        public void InsertNewYoutubeChannelId(string youtubeId)
        {
            YoutubeTasks.InsertNewYoutubeChannelId(this, youtubeId);
        }

        public void GetChannelsWithVideosInserted(List<string> notLoadedYoutubeIds)
        {
            YoutubeTasks.GetChannelsWithVideosInserted(this, notLoadedYoutubeIds);
        }

        #endregion

        #region Subscriptions

        public void FetchAndInsertChannelSubscriptions(string youtubeId)
        {
            YoutubeTasks.FetchAndInsertChannelSubscriptions(this, youtubeId);
            Clients.Caller.onSubscriptionsInserted();
        }

        public void UpdateSubscriptions(string userYoutubeId)
        {
            YoutubeTasks.UpdateSubscriptions(this, userYoutubeId);
        }

        #endregion

        #region Videos

        public void MarkVideoAsWatched(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            YoutubeTasks.MarkVideoAsWatched(youtubeVideoId, userYoutubeId, dateViewed);
        }

        public void GetVideosForCollection(string userYoutubeId, string collectionTitle)
        {
            YoutubeTasks.GetVideosForCollection(this, userYoutubeId, collectionTitle);
        }

        #endregion

        #region Watched Videos

        public void InsertWatchedVideo(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            YoutubeTasks.InsertWatchedVideo(this, youtubeVideoId, userYoutubeId, dateViewed);
        }

        public void GetUnwatchedVideos(string userYoutubeId, List<string> youtubeIds)
        {
            YoutubeTasks.GetUnwatchedVideos(this, userYoutubeId, youtubeIds);
        }

        #endregion

        #region Debug Mode

#if DEBUG
        public void TestConnection()
        {
            Clients.Caller.onTestClick();
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

        public void AddAthlean()
        {
            YoutubeTasks.AddAthlean();
        }

        public void DeleteAthlean()
        {
            YoutubeTasks.DeleteAthlean();
        }
#endif

        #endregion

        #region Youtube Callbacks

        public void NotifyCaller(SignalRMessage message)
        {
            Clients.Caller.onProgressChanged(message);
        }

        public void SendUnrecognizedYoutubeVideoIds(SignalRMessage message)
        {
            Clients.Caller.onRelatedVideosChange(message);
        }

        public void SendCollectionVideos(SignalRMessage message)
        {
            Clients.Caller.onRelatedVideosChange(message);
        }

        public void NotifyCallerWatchedVideoInserted(SignalRMessage message)
        {
            Clients.Caller.onWatchedVideoInserted(message);
        }

        public void NotifyCallerOfSubscriptionUpdate(SignalRMessage message)
        {
            Clients.Caller.onSubscriptionUpdated(message);
        }

        public void NotifyCallerOfChannelVideosInserted(SignalRMessage message)
        {
            Clients.Caller.onChannelVideosInserted(message);
        }

        public void NotifyCallerOfNewYoutubeIdInserted()
        {
            Clients.Caller.onNewYoutubeIdInserted();
            
        }

        public void NotifyCallerOfExistingYoutubeId()
        {
            Clients.Caller.onYoutubeIdAlreadyExists();
        }

        #endregion

    }
}