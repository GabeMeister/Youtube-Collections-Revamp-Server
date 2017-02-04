using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using YoutubeCollectionsRevampServer.Models.ApiModels;
using YoutubeCollectionsRevampServer.Models.DatabaseModels;
using YoutubeCollectionsRevampServer.Models.ObjectHolderModels;
using YoutubeCollectionsRevampServer.Models.SignalRMessaging;

namespace YoutubeCollectionsRevampServer
{
    public class YoutubeTasks
    {
        private static string YoutubeLogFile
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\YT_Collections_Dump.log"; }
        }

        #region Channels

        public static void InsertNewYoutubeChannelId(YoutubeCollectionsHub hub, string youtubeId)
        {
            try
            {
                // Check if the youtube id already exists
                if (DbHandler.DoesIdExist("Channels", "YoutubeID", youtubeId))
                {
                    // The user has already registered, they are just logging in from another computer
                    // We need to send all collections data to user
                    hub.NotifyCallerOfExistingYoutubeId();
                }
                else
                {
                    // Channel doesn't exist yet, so we insert it
                    InsertYoutubeChannelIdIntoDatabase(youtubeId);
                    hub.NotifyCallerOfNewYoutubeIdInserted();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error occurred in InsertNewYoutubeChannelId(hub, {0}): {1}", youtubeId, e.Message);
                hub.NotifyCallerOfError(new SignalRMessage(e.Message));
                throw;
            }
            

        }

        public static ChannelHolder InsertYoutubeChannelIdIntoDatabase(string youtubeId)
        {
            ChannelHolder channel = null;
            ChannelListResponse channelResponse = YoutubeApiHandler.FetchUploadsPlaylistByChannel(youtubeId,
                "snippet,contentDetails,statistics");

            if (channelResponse.Items != null && channelResponse.Items.Count > 0)
            {
                channel = new ChannelHolder(channelResponse.Items[0]);
                DbHandler.InsertChannel(channel);
            }

            return channel;
        }

        public static void GetChannelsWithVideosInserted(YoutubeCollectionsHub hub, List<string> notLoadedYoutubeIds)
        {
            List<string> channelsStillNotLoaded =
                DbHandler.GetChannelsToDownloadYoutubeIdsMatchingList(notLoadedYoutubeIds);
            List<string> channelsNowLoaded = notLoadedYoutubeIds.Except(channelsStillNotLoaded).ToList();

            foreach (string loadedChannel in channelsNowLoaded)
            {
                hub.NotifyCallerOfChannelVideosInserted(new ChannelVideosInsertMessage(loadedChannel));
            }
        }

        private static int GetChannelIdFromNewSubscription(string beingSubscribedToYoutubeId)
        {
            // Get actual channel id for the youtube channel being subscribed to
            int beingSubscribedToChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels",
                beingSubscribedToYoutubeId);
            if (beingSubscribedToChannelId == -1)
            {
                // Channel hasn't been inserted into database yet.
                InsertYoutubeChannelIdIntoDatabase(beingSubscribedToYoutubeId);
                beingSubscribedToChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels",
                    beingSubscribedToYoutubeId);
            }

            // We can tell if any videos are loaded by seeing if any videos contain the associated channel id
            bool areVideosLoaded = DbHandler.DoesIdExist("Videos", "ChannelID", beingSubscribedToChannelId);
            if (!areVideosLoaded)
            {
                // If there's no videos for this channel, then we need to flag this channel id to download later
                DbHandler.InsertChannelIntoChannelsToDownload(beingSubscribedToChannelId);
            }

            return beingSubscribedToChannelId;
        }

        public static void CompletelyDeleteChannel(string youtubeChannelId)
        {
            // Get the database channel id
            int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            // Delete the channel's collections
            DbHandler.DeleteChannelCollections(channelId);

            // Delete the channel's uploads videos
            DbHandler.DeleteChannelVideos(channelId);

            // Delete the channel's watched videos
            DbHandler.DeleteChannelWatchedVideos(channelId);

            // Delete the channel's subscriptions
            DbHandler.DeleteChannelSubscriptions(channelId);

            // Delete subscriptions to the channel
            DbHandler.DeleteSubscriptionsToChannel(channelId);

            // Delete the channel itself
            DbHandler.DeleteChannel(channelId);


        }

        #endregion

        #region Subscriptions

        public static void FetchAndInsertChannelSubscriptions(YoutubeCollectionsHub hub, string subscriberYoutubeId)
        {
            int subscriptionIndex = 0;
            string nextPageToken = "";

            // Get actual channel id for the subscriber youtube channel
            int subscriberChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", subscriberYoutubeId);
            Debug.Assert(subscriberChannelId != -1, "Error, fetching subscriptions for non-existant youtube channel id");

            do
            {
                SubscriptionListResponse subscriptionsList =
                    YoutubeApiHandler.FetchSubscriptionsByChannel(subscriberYoutubeId, nextPageToken, "snippet");

                nextPageToken = subscriptionsList.NextPageToken;
                int? subscriptionCount = subscriptionsList.PageInfo.TotalResults;
                foreach (var searchResult in subscriptionsList.Items)
                {
                    string title = searchResult.Snippet.Title;
                    string beingSubscribedToYoutubeId = searchResult.Snippet.ResourceId.ChannelId;
                    string subscriptionThumbnail = searchResult.Snippet.Thumbnails.Medium.Url.Trim();

                    // Get actual channel id for the subscriber youtube channel
                    int beingSubscribedToChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels",
                        beingSubscribedToYoutubeId);
                    if (beingSubscribedToChannelId == -1)
                    {
                        // Channel hasn't been inserted into database yet.
                        InsertYoutubeChannelIdIntoDatabase(beingSubscribedToYoutubeId);
                        beingSubscribedToChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels",
                            beingSubscribedToYoutubeId);
                    }

                    // We can tell if any videos are loaded by seeing if any videos contain the associated channel id
                    bool areVideosLoaded = DbHandler.DoesIdExist("Videos", "ChannelID", beingSubscribedToChannelId);
                    if (!areVideosLoaded)
                    {
                        // If there's no videos for this channel, then we need to flag this channel id to download later
                        DbHandler.InsertChannelIntoChannelsToDownload(beingSubscribedToChannelId);
                    }

                    var message = new SubscriptionInsertMessage(++subscriptionIndex, subscriptionCount,
                        beingSubscribedToYoutubeId, title, subscriptionThumbnail, areVideosLoaded);
                    hub.NotifyCaller(message);
                    DbHandler.InsertSubscription(subscriberChannelId, beingSubscribedToChannelId);

                }
            } while (nextPageToken != null);

        }

        public static void UpdateSubscriptions(YoutubeCollectionsHub hub, string userYoutubeId)
        {
            // Get actual channel id for the subscriber youtube channel
            int userChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(userChannelId != -1, "Error, fetching subscriptions for non-existant youtube channel id");

            List<string> allYoutubeApiSubscriptions =
                YoutubeApiHandler.FetchAllSubscriptions(userYoutubeId).OrderBy(x => x).ToList();
            List<string> allDatabaseSubscriptions =
                DbHandler.SelectYoutubeIdSubscriptionsForUser(userChannelId).OrderBy(x => x).ToList();

            List<string> allAddedSubscriptions = allYoutubeApiSubscriptions.Except(allDatabaseSubscriptions).ToList();
            List<string> allRemovedSubscriptions = allDatabaseSubscriptions.Except(allYoutubeApiSubscriptions).ToList();

            int index = 1;
            int total = allAddedSubscriptions.Count;
            foreach (string addedSubscription in allAddedSubscriptions)
            {
                // Insert the subscription and notify user
                int channelIdBeingSubscribedTo = GetChannelIdFromNewSubscription(addedSubscription);
                bool areVideosLoaded = DbHandler.DoesIdExist("Videos", "ChannelID", channelIdBeingSubscribedTo);

                ChannelHolder beingSubscribedToChannel =
                    DbHandler.PopulateChannelHolderFromTable(channelIdBeingSubscribedTo, "Title,Thumbnail");
                DbHandler.InsertSubscription(userChannelId, channelIdBeingSubscribedTo);

                hub.NotifyCallerOfSubscriptionUpdate(new SubscriptionInsertMessage(index++,
                    total,
                    addedSubscription,
                    beingSubscribedToChannel.Title,
                    beingSubscribedToChannel.Thumbnail,
                    areVideosLoaded));

            }

            foreach (string removedSubscription in allRemovedSubscriptions)
            {
                // Remove subscription
                int subscriptionId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", removedSubscription);
                DbHandler.DeleteSubscription(userChannelId, subscriptionId);

                // Remove subscription from any collections
                DbHandler.DeleteChannelFromAllUserCollections(userChannelId, subscriptionId);

                hub.NotifyCallerOfSubscriptionUpdate(new SubscriptionDeleteMessage(removedSubscription));

            }

        }

        #endregion

        #region Collections

        public static void InsertCollection(string collectionName, string youtubeId)
        {
            int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", youtubeId);

            var collectionToInsert = new CollectionHolder(collectionName, youtubeId, channelId);

            DbHandler.InsertCollection(collectionToInsert);

        }

        public static void RenameCollection(string oldCollectionTitle, string newCollectionTitle, string userYoutubeId)
        {
            int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            // Get the collection id
            int collectionId = DbHandler.SelectCollectionIdByChannelIdAndTitle(channelId, oldCollectionTitle);
            Debug.Assert(collectionId > -1, "Collection not found");

            DbHandler.RenameCollection(collectionId, newCollectionTitle);

        }

        public static void DeleteCollection(string collectionTitle, string userYoutubeId)
        {
            int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            if (channelId > -1)
            {
                // Get the collection id
                int collectionId = DbHandler.SelectCollectionIdByChannelIdAndTitle(channelId, collectionTitle);
                if (collectionId > -1)
                {
                    DbHandler.DeleteCollection(collectionId);
                }
            }

        }

        public static void GetVideosForCollection(YoutubeCollectionsHub hub, string userYoutubeId,
            string collectionTitle)
        {
            try
            {
                // Convert youtube id to channel id
                int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
                Debug.Assert(channelId > 0, "Non existant channel id.");

                // Query database to get collection id
                int collectionId = DbHandler.SelectCollectionIdByChannelIdAndTitle(channelId, collectionTitle);

                // Query for all channels in collection
                List<int> collectionItemIds = DbHandler.SelectCollectionItemsByCollectionId(collectionId);

                // Calculate how many videos we should grab from each channel
                // We add 1 just in case there are more than 20 channels in a collection
                int numVidsPerChannel = (20/collectionItemIds.Count) + 1;

                // Query each channel to get the most recent unwatched videos
                List<int> collectionVideoIds = new List<int>();
                foreach (int collectionItemId in collectionItemIds)
                {
                    List<int> videosForCollectionItem = DbHandler.SelectUnwatchedVideoIdsForUserSubscription(channelId,
                        collectionItemId, numVidsPerChannel);
                    collectionVideoIds.AddRange(videosForCollectionItem);
                }

                // Grab video data for each video
                List<VideoHolder> collectionVideos = DbHandler.SelectVideoInformationForVideoIds(collectionVideoIds);

                // Sort the collection by date
                List<VideoHolder> sortedCollectionVideos =
                    collectionVideos.OrderByDescending(x => x.PublishedAt).ToList();

                // Send to client
                var hubMessage = new CollectionVideosMessage(sortedCollectionVideos);
                hub.SendCollectionVideos(hubMessage);
            }
            catch (Exception e)
            {
                Trace.TraceError("Error occurred in GetVideosForCollection(hub, {0}, {1}): {2}", userYoutubeId,
                    collectionTitle, e.Message);
                hub.NotifyCallerOfError(new SignalRMessage(e.Message));
            }


        }

        public static void SyncUserData(YoutubeCollectionsHub hub, string userYoutubeId)
        {
            int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);

            if (channelId > 0)
            {
                // Iterate through all subscriptions and send them to user
                List<int> subscriptionChannelIds = DbHandler.SelectSubscriptionChannelIdsForUser(channelId);

                int subscriptionIndex = 0;
                int subscriptionCount = subscriptionChannelIds.Count;
                foreach (int subscriptionId in subscriptionChannelIds)
                {
                    ChannelHolder subscription = DbHandler.PopulateChannelHolderFromTable(subscriptionId, "*");
                    bool areVideosLoaded = DbHandler.DoesIdExist("Videos", "ChannelID", subscription.ChannelHolderId);
                    var message = new SubscriptionInsertMessage(++subscriptionIndex, subscriptionCount,
                        subscription.YoutubeId,
                        subscription.Title, subscription.Thumbnail, areVideosLoaded);
                    hub.NotifyCallerOfSubscriptionSync(message);
                }


                // Iterate through all the collections, query for the collection items and send 
                // them to the user
                List<int> userCollectionIds = DbHandler.SelectCollectionIdsForUser(channelId);
                foreach (int collectionId in userCollectionIds)
                {
                    string title = DbHandler.SelectColumnBySingleCondition("Title", "Collections", "CollectionID",
                        collectionId);
                    List<string> collectionItemNames = DbHandler.SelectCollectionItemNamesByCollectionId(collectionId);
                    hub.NotifyCallerOfCollectionSync(new SyncCollectionMessage(title, collectionItemNames));
                }
            }

        }

        #endregion

        #region Collection Items

        public static void InsertCollectionItem(string collectionItemYoutubeId, string collectionTitle,
            string userYoutubeId)
        {
            // Get the user Id
            int userChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(userChannelId > 0, "Inserting collection item into collection with non-existant channel id.");

            // Get collection id
            int collectionId = DbHandler.SelectCollectionIdByChannelIdAndTitle(userChannelId, collectionTitle);
            Debug.Assert(collectionId > 0, "Couldn't find correct collection by id");

            // Get the collection item id
            int collectionItemChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels",
                collectionItemYoutubeId);
            Debug.Assert(collectionItemChannelId > 0, "Couldn't find collection item channel id");

            // Insert collection item
            DbHandler.InsertCollectionItem(collectionItemChannelId, collectionId);


        }

        public static void DeleteCollectionItem(string collectionItemYoutubeId, string collectionTitle,
            string userYoutubeId)
        {
            // Get the user Id
            int userChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(userChannelId > 0, "Inserting collection item into collection with non-existant channel id.");

            // Get collection id
            int collectionId = DbHandler.SelectCollectionIdByChannelIdAndTitle(userChannelId, collectionTitle);
            Debug.Assert(collectionId > 0, "Couldn't find correct collection by id");

            // Get the collection item id
            int collectionItemChannelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels",
                collectionItemYoutubeId);
            Debug.Assert(collectionItemChannelId > 0, "Couldn't find collection item channel id");

            // Delete collection item
            DbHandler.DeleteCollectionItem(collectionId, collectionItemChannelId);
        }

        public static void DeleteChannelCollectionItems(string youtubeChannelId)
        {
            // Get the database channel id
            int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            DbHandler.DeleteChannelCollectionItems(channelId);

        }

        #endregion

        #region Watched Videos

        public static void InsertWatchedVideo(YoutubeCollectionsHub hub, string youtubeVideoId, string userYoutubeId,
            string dateViewed)
        {
            try
            {
                int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);

                if (!DbHandler.DoesIdExist("Videos", "YoutubeID", youtubeVideoId))
                {
                    // Might not have video, we need to fetch it's information
                    // and insert it's meta-data into the database
                    FetchVideoInfo(youtubeVideoId);
                }

                int videoId = DbHandler.SelectIdFromYoutubeId("VideoID", "Videos", youtubeVideoId);

                DbHandler.InsertWatchedVideo(videoId, channelId, dateViewed);

                var message = new WatchedVideoInsertedMessage();
                hub.NotifyCallerWatchedVideoInserted(message);
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception occurred in InsertWatchedVideo(hub, {0}, {1}): {2}", youtubeVideoId,
                    userYoutubeId, e.Message);
                hub.NotifyCallerOfError(new SignalRMessage(e.Message));
            }

        }

        public static void MarkVideoAsWatched(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            try
            {
                int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);

                if (!DbHandler.DoesIdExist("Videos", "YoutubeID", youtubeVideoId))
                {
                    // Might not have video, we need to fetch it's information
                    // and insert it's meta-data into the database
                    FetchVideoInfo(youtubeVideoId);
                }

                int videoId = DbHandler.SelectIdFromYoutubeId("VideoID", "Videos", youtubeVideoId);

                DbHandler.InsertWatchedVideo(videoId, channelId, dateViewed);
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception occurred in MarkVideoAsWatched({0}, {1}, {2}): {3}", youtubeVideoId,
                    userYoutubeId, dateViewed, e.Message);
            }
            
        }

        public static void GetUnwatchedVideos(YoutubeCollectionsHub hub, string userYoutubeId, List<string> youtubeVideoIds)
        {
            int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            List<string> unrecognizedVideoYoutubeIds = new List<string>();
            List<int> recognizedVideoIds = new List<int>();

            foreach (string youtubeVideoId in youtubeVideoIds)
            {
                int videoId = DbHandler.SelectIdFromYoutubeId("VideoID", "Videos", youtubeVideoId);
                if (videoId < 0)
                {
                    // Video doesn't exist in database, we have to insert it
                    // We assume unrecognized videos haven't been watched by the user.
                    unrecognizedVideoYoutubeIds.Add(youtubeVideoId);
                }
                else
                {
                    recognizedVideoIds.Add(videoId);
                }
            }

            // Insert unrecognized videos into database
            FetchVideoInfo(string.Join(",", unrecognizedVideoYoutubeIds));

            unrecognizedVideoYoutubeIds.AddRange(DbHandler.SelectUnwatchedVideosForUser(channelId, recognizedVideoIds));

            var message = new RelatedVideosMessage(unrecognizedVideoYoutubeIds);
            hub.SendUnrecognizedYoutubeVideoIds(message);

        }

        #endregion

        #region Videos

        public static void FetchVideoInfo(string videoIds)
        {
            VideoListResponse videos = YoutubeApiHandler.FetchVideoById(videoIds, "snippet,contentDetails,statistics");

            foreach (var videoResponse in videos.Items)
            {
                VideoHolder video = new VideoHolder(videoResponse);

                int channelId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", video.YoutubeChannelId);
                if (channelId < 0)
                {
                    // Channel doesn't exist in database, we need to insert the channel meta-data
                    // before inserting the video data
                    InsertYoutubeChannelIdIntoDatabase(video.YoutubeChannelId);
                }

                DbHandler.InsertVideo(video);
            }
        }

        #endregion
        
        #region Debug Mode

#if DEBUG
        public static void AddAthlean()
        {
            // Add Athlean x to subscriptions for Gabe J if not there
            int athleanId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", "UCe0TLA0EsQbE-MjuHXevj2A");
            int myChannel = Convert.ToInt32(DbHandler.SelectColumnBySingleCondition("ChannelID", "Channels", "Title", "Gabe J"));

            DbHandler.InsertSubscription(myChannel, athleanId);
        }

        public static void DeleteAthlean()
        {
            // Remove Athlean X subscription for Gabe J if there
            int athleanId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", "UCe0TLA0EsQbE-MjuHXevj2A");
            int myChannel = Convert.ToInt32(DbHandler.SelectColumnBySingleCondition("ChannelID", "Channels", "Title", "Gabe J"));

            DbHandler.DeleteSubscription(myChannel, athleanId);
        }

        
#endif

        #endregion


    }
}
