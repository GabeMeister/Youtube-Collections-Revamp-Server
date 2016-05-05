using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Npgsql;
using YoutubeCollectionsRevampServer.Models.SignalRMessaging;
using YoutubeCollectionsRevampServer.Models.ObjectHolderModels;
using YoutubeCollectionsRevampServer.Models.DatabaseModels;
using YoutubeCollectionsRevampServer.Models.ApiModels;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Diagnostics;
using Microsoft.AspNet.SignalR;



namespace YoutubeCollectionsRevampServer.Controllers.YoutubeTasks
{
    public class YoutubeTasks
    {
        private const int MAX_RESULTS = 50;
        private const string YOUTUBE_LOG_FILE = @"C:\Users\Gabe\Desktop\YT_Collections_Dump.log";

        #region BEING USED

        public static ChannelHolder InsertYoutubeChannelIdIntoDatabase(string youtubeId)
        {
            ChannelHolder channel = null;
            ChannelListResponse channelResponse = YoutubeApiHandler.FetchUploadsPlaylistByChannel(youtubeId, "snippet,contentDetails,statistics");

            if (channelResponse.Items != null && channelResponse.Items.Count > 0)
            {
                channel = new ChannelHolder(channelResponse.Items[0]);
                DBHandler.InsertChannel(channel);
            }

            return channel;
        }

        public static void FetchAndInsertChannelSubscriptions(YoutubeCollectionsHub hub, string subscriberYoutubeId)
        {
            int subscriptionIndex = 0;
            int? subscriptionCount = 0;
            string nextPageToken = string.Empty;
            SubscriptionListResponse subscriptionsList;

            // Get actual channel id for the subscriber youtube channel
            int subscriberChannelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", subscriberYoutubeId);
            Debug.Assert(subscriberChannelId != -1, "Error, fetching subscriptions for non-existant youtube channel id");

            

            do
            {
                subscriptionsList = YoutubeApiHandler.FetchSubscriptionsByChannel(subscriberYoutubeId, nextPageToken, "snippet");
                
                nextPageToken = subscriptionsList.NextPageToken;
                subscriptionCount = subscriptionsList.PageInfo.TotalResults;

                if (subscriptionsList != null)
                {
                    foreach (var searchResult in subscriptionsList.Items)
                    {
                        string title = searchResult.Snippet.Title;
                        string beingSubscribedToYoutubeId = searchResult.Snippet.ResourceId.ChannelId;
                        string subscriptionThumbnail = searchResult.Snippet.Thumbnails.Medium.Url.ToString().Trim();

                        // Get actual channel id for the subscriber youtube channel
                        int beingSubscribedToChannelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", beingSubscribedToYoutubeId);
                        if (beingSubscribedToChannelId == -1)
                        {
                            // Channel hasn't been inserted into database yet.
                            InsertYoutubeChannelIdIntoDatabase(beingSubscribedToYoutubeId);
                            beingSubscribedToChannelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", beingSubscribedToYoutubeId);
                        }

                        // We can tell if any videos are loaded by seeing if any videos contain the associated channel id
                        bool areVideosLoaded = DBHandler.DoesItemExist("Videos", "ChannelID", beingSubscribedToChannelId);

                        var message = new SubscriptionInsertMessage(++subscriptionIndex, subscriptionCount, beingSubscribedToYoutubeId, title, subscriptionThumbnail, areVideosLoaded);
                        hub.NotifyCaller(message);
                        DBHandler.InsertSubscription(subscriberChannelId, beingSubscribedToChannelId);
                        
                        
                    }
                }
            }
            while (nextPageToken != null);

        }

        public static void FetchAndInsertNewChannelSubscriptions(YoutubeCollectionsHub hub, string subscriberYoutubeId)
        {
            // TODO
        }

        public static void InsertCollection(string collectionName, string youtubeId)
        {
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", youtubeId);

            var collectionToInsert = new CollectionHolder(collectionName, youtubeId, channelId);

            DBHandler.InsertCollection(collectionToInsert);

        }

        public static void RenameCollection(string oldCollectionTitle, string newCollectionTitle, string userYoutubeId)
        {
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            // Get the collection id
            int collectionId = DBHandler.SelectCollectionIdByChannelIdAndTitle(channelId, oldCollectionTitle);
            Debug.Assert(collectionId > -1, "Collection not found");

            DBHandler.RenameCollection(collectionId, newCollectionTitle);

        }

        public static void InsertCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            // Get the user Id
            int userChannelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(userChannelId > 0, "Inserting collection item into collection with non-existant channel id.");

            // Get collection id
            int collectionId = DBHandler.SelectCollectionIdByChannelIdAndTitle(userChannelId, collectionTitle);
            Debug.Assert(collectionId > 0, "Couldn't find correct collection by id");

            // Get the collection item id
            int collectionItemChannelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", collectionItemYoutubeId);
            Debug.Assert(collectionItemChannelId > 0, "Couldn't find collection item channel id");

            // Insert collection item
            DBHandler.InsertCollectionItem(collectionItemChannelId, collectionId);


        }

        public static void DeleteCollectionItem(string collectionItemYoutubeId, string collectionTitle, string userYoutubeId)
        {
            // Get the user Id
            int userChannelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(userChannelId > 0, "Inserting collection item into collection with non-existant channel id.");

            // Get collection id
            int collectionId = DBHandler.SelectCollectionIdByChannelIdAndTitle(userChannelId, collectionTitle);
            Debug.Assert(collectionId > 0, "Couldn't find correct collection by id");

            // Get the collection item id
            int collectionItemChannelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", collectionItemYoutubeId);
            Debug.Assert(collectionItemChannelId > 0, "Couldn't find collection item channel id");

            // Delete collection item
            DBHandler.DeleteCollectionItem(collectionId, collectionItemChannelId);
        }

        public static void CompletelyDeleteChannel(string youtubeChannelId)
        {
            // Get the database channel id
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            // Delete the channel's collections
            DBHandler.DeleteChannelCollections(channelId);

            // Delete the channel's uploads videos
            DBHandler.DeleteChannelVideos(channelId);

            // Delete the channel's watched videos
            DBHandler.DeleteChannelWatchedVideos(channelId);

            // Delete the channel's subscriptions
            DBHandler.DeleteChannelSubscriptions(channelId);

            // Delete subscriptions to the channel
            DBHandler.DeleteSubscriptionsToChannel(channelId);

            // Delete the channel itself
            DBHandler.DeleteChannel(channelId);


        }

        public static void DeleteChannelCollectionItems(string youtubeChannelId)
        {
            // Get the database channel id
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            DBHandler.DeleteChannelCollectionItems(channelId);

        }

        public static void InsertWatchedVideo(YoutubeCollectionsHub hub, string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            
            if (!DBHandler.DoesItemExist("Videos", "YoutubeID", youtubeVideoId))
            {
                // Might not have video, we need to fetch it's information
                // and insert it's meta-data into the database
                FetchVideoInfo(youtubeVideoId);
            }

            int videoId = DBHandler.RetrieveIdFromYoutubeId("VideoID", "Videos", youtubeVideoId);

            DBHandler.InsertWatchedVideo(videoId, channelId, dateViewed);

            var message = new WatchedVideoInsertedMessage();
            hub.NotifyCallerWatchedVideoInserted(message);
        }

        public static void MarkVideoAsWatched(string youtubeVideoId, string userYoutubeId, string dateViewed)
        {
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            
            if (!DBHandler.DoesItemExist("Videos", "YoutubeID", youtubeVideoId))
            {
                // Might not have video, we need to fetch it's information
                // and insert it's meta-data into the database
                FetchVideoInfo(youtubeVideoId);
            }

            int videoId = DBHandler.RetrieveIdFromYoutubeId("VideoID", "Videos", youtubeVideoId);

            DBHandler.InsertWatchedVideo(videoId, channelId, dateViewed);
        }

        public static void FetchVideoInfo(string videoIds)
        {
            VideoListResponse videos = YoutubeApiHandler.FetchVideoById(videoIds, "snippet,contentDetails,statistics");

            foreach (var videoResponse in videos.Items)
            {
                VideoHolder video = new VideoHolder(videoResponse);
                
                int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", video.YoutubeChannelId);
                if (channelId < 0)
                {
                    // Channel doesn't exist in database, we need to insert the channel meta-data
                    // before inserting the video data
                    InsertYoutubeChannelIdIntoDatabase(video.YoutubeChannelId);
                }
                
                DBHandler.InsertVideo(video);
            }
        }

        public static void GetUnwatchedVideos(YoutubeCollectionsHub hub, string userYoutubeId, List<string> youtubeVideoIds)
        {
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            List<string> unrecognizedVideoYoutubeIds = new List<string>();
            List<int> recognizedVideoIds = new List<int>();

            foreach(string youtubeVideoId in youtubeVideoIds)
            {
                int videoId = DBHandler.RetrieveIdFromYoutubeId("VideoID", "Videos", youtubeVideoId);
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

            unrecognizedVideoYoutubeIds.AddRange(DBHandler.GetUnwatchedVideosForUser(channelId, recognizedVideoIds));

            var message = new RelatedVideosMessage(unrecognizedVideoYoutubeIds);
            hub.SendUnrecognizedYoutubeVideoIds(message);

        }

        public static void GetVideosForCollection(YoutubeCollectionsHub hub, string userYoutubeId, string collectionTitle)
        {
            // Convert youtube id to channel id
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", userYoutubeId);
            Debug.Assert(channelId > 0, "Non existant channel id.");

            // Query database to get collection id
            int collectionId = DBHandler.SelectCollectionIdByChannelIdAndTitle(channelId, collectionTitle);

            // Query for all channels in collection
            List<int> collectionItemIds = DBHandler.SelectCollectionItemsByCollectionId(collectionId);

            // Calculate how many videos we should grab from each channel
            // We add 1 just in case there are more than 20 channels in a collection
            int numVidsPerChannel = (20 / collectionItemIds.Count) + 1;

            // Query each channel to get the most recent unwatched videos
            List<int> collectionVideoIds = new List<int>();
            foreach (int collectionItemId in collectionItemIds)
            {
                List<int> videosForCollectionItem = DBHandler.SelectUnwatchedVideoIdsForUserSubscription(channelId, collectionItemId, numVidsPerChannel);
                collectionVideoIds.AddRange(videosForCollectionItem);
            }

            // Grab video data for each video
            List<VideoHolder> collectionVideos = DBHandler.SelectVideoInformationForVideoIds(collectionVideoIds);

            // Sort the collection by date
            List<VideoHolder> sortedCollectionVideos = collectionVideos.OrderByDescending(x => x.PublishedAt).ToList();

            // Send to client
            var hubMessage = new CollectionVideosMessage(sortedCollectionVideos);
            hub.SendCollectionVideos(hubMessage);

        }


        #endregion



        public static void FetchAllUploadsForAllChannelSubscriptions(string youtubeId)
        {
            int subscriptionCount = 0;
            string nextPageToken = string.Empty;
            SubscriptionListResponse subscriptionsList;

            do
            {
                subscriptionsList = YoutubeApiHandler.FetchSubscriptionsByChannel(youtubeId, nextPageToken, "snippet");
                subscriptionCount += subscriptionsList.Items.Count;
                nextPageToken = subscriptionsList.NextPageToken;

                if (subscriptionsList != null)
                {
                    foreach (var searchResult in subscriptionsList.Items)
                    {
                        Debug.WriteLine(searchResult.Snippet.Title);
                        //FetchChannelUploads(searchResult.Snippet.ResourceId.ChannelId);
                    }
                }
            }
            while (nextPageToken != null);

            Debug.WriteLine("Total Subscription Count: " + subscriptionCount);
        }

        public static void FetchChannelUploads(string youtubeId)
        {
            int vidCount = 0;
            ChannelHolder channel = InsertYoutubeChannelIdIntoDatabase(youtubeId);

            if (channel != null)
            {
                Debug.WriteLine("************* " + channel.Title + " | " + channel.YoutubeId + " *************");

                string nextPageToken = string.Empty;
                string uploadsPlaylistId = channel.UploadPlaylist;
                PlaylistItemListResponse searchListResponse;
                bool wasSuccessfulFetch = true;

                do
                {
                    try
                    {
                        wasSuccessfulFetch = false;
                        searchListResponse = YoutubeApiHandler.FetchVideosByPlaylist(uploadsPlaylistId, nextPageToken, "snippet");
                        vidCount += searchListResponse.Items.Count;
                        nextPageToken = searchListResponse.NextPageToken;

                        if (searchListResponse != null)
                        {
                            string videoIds = string.Empty;

                            if (searchListResponse.Items != null && searchListResponse.Items.Count > 0)
                            {
                                foreach (var searchResult in searchListResponse.Items)
                                {
                                    videoIds += searchResult.Snippet.ResourceId.VideoId + ",";
                                }

                                // Remove last comma
                                videoIds = videoIds.Substring(0, videoIds.Length - 1);

                                Trace.WriteLine("YUP");
                                FetchVideoInfo(videoIds);
                            }

                        }

                        wasSuccessfulFetch = true;
                    }
                    catch (Exception e)
                    {
                        // Log the error and attempt the api query again
                        using (StreamWriter writer = File.AppendText(YOUTUBE_LOG_FILE))
                        {
                            writer.WriteLine("Error on " + channel.Title + " with " + nextPageToken + " as page token");
                        }
                    }

                }
                while (nextPageToken != null || wasSuccessfulFetch != true);

                
                Debug.WriteLine("Total Video Count: " + vidCount);
            }
            

        }

        public static void MultiThreadedFetchAllChannelUploads()
        {
            // TODO
            //Thread t1 = new Thread(() => YoutubeTasks.FetchChannelUploads(AssociatedPress));
            //Thread t2 = new Thread(() => YoutubeTasks.FetchChannelUploads(WildFilmsIndia));
            //Thread t3 = new Thread(() => YoutubeTasks.FetchChannelUploads(TheYoungTurks));
            //Thread t4 = new Thread(() => YoutubeTasks.FetchChannelUploads(TVCultura));
            //Thread t5 = new Thread(() => YoutubeTasks.FetchChannelUploads(TheTelegraph));
            //Thread t6 = new Thread(() => YoutubeTasks.FetchChannelUploads(TomoNewsUS));

            //t1.Start();
            //t2.Start();
            //t3.Start();
            //t4.Start();
            //t5.Start();
            //t6.Start();

            //t1.Join();
            //t2.Join();
            //t3.Join();
            //t4.Join();
            //t5.Join();
            //t6.Join();
        }
        
        public static void FetchChannelUploadsFromStream(StreamReader reader)
        {
            using (reader)
            {
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    string[] tokens = line.Split('\t');
                    string channelId = tokens[0];
                    string youtubeId = tokens[1];
                    string title = tokens[2];

                    FetchChannelUploads(youtubeId);

                }
            }
        }

        public static void WriteAllChannelIdsToStream(StreamWriter writer)
        {
            using (writer)
            {
                string selectAllChannelsSql = SqlBuilder.SelectAllSql("ChannelID,YoutubeID,Title", "Channels");

                using (NpgsqlConnection conn = new NpgsqlConnection(DBHandler.DatabaseConnStr))
                {
                    conn.Open();
                    
                    NpgsqlCommand command = new NpgsqlCommand(selectAllChannelsSql, conn);
                    NpgsqlDataReader reader = command.ExecuteReader();
                    
                    while(reader.Read())
                    {
                        string channelId = reader["ChannelID"].ToString().Trim();
                        string youtubeId = reader["YoutubeID"].ToString().Trim();
                        string title = reader["Title"].ToString().Trim();
                        writer.WriteLine("{0}\t{1}\t{2}", channelId, youtubeId, title);
                    }

                    conn.Close();
                }

                
            }
        }

        public static void AddPublishedAtTimeStamps()
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHandler.DatabaseConnStr))
                {
                    conn.Open();

                    // We check if the same youtube channel id has already been inserted
                    string selectSql = "select YoutubeID from videos where publishedAt is NULL;";
                    NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                    NpgsqlDataReader reader = selectCommand.ExecuteReader();

                    List<string> youtubeIds = new List<string>();
                    VideoListResponse videoResponse = null;

                    while(reader.Read())
                    {
                        youtubeIds.Add(reader["YoutubeId"].ToString());
                    }

                    reader.Close();

                    int count = 0;
                    int totalCount = 0;
                    string videoList = string.Empty;
                    // API call with each video id
                    foreach(string youtubeId in youtubeIds)
                    {
                        if (count == 49)
                        {
                            // To make it 50
                            videoList += youtubeId.Trim();
                            count++;

                            videoResponse = YoutubeApiHandler.FetchVideoById(videoList, "snippet");

                            foreach(Video video in videoResponse.Items)
                            {
                                DateTime? publishedAt = video.Snippet.PublishedAt;

                                string udpateSql = string.Format("update videos set PublishedAt='{0}' where YoutubeID='{1}';", publishedAt.Value.ToString("yyyy-MM-dd HH:MM:ss").Trim(), video.Id.Trim());

                                NpgsqlCommand updateCommand = new NpgsqlCommand(udpateSql, conn);
                                int rowsAffected = updateCommand.ExecuteNonQuery();

                                if (rowsAffected < 1)
                                {
                                    throw new Exception("Video update didn't complete correctly.");
                                }
                            }


                            
                            videoList = string.Empty;
                            totalCount += count;
                            Debug.WriteLine("Updated {0} videos", totalCount);
                            count = 0;
                        }
                        else
                        {
                            videoList += youtubeId.Trim() + ",";
                        }

                        count++;
                    }



                    videoResponse = YoutubeApiHandler.FetchVideoById(videoList.TrimEnd(','), "snippet");

                    foreach (Video video in videoResponse.Items)
                    {
                        DateTime? publishedAt = video.Snippet.PublishedAt;

                        string udpateSql = string.Format("update videos set PublishedAt='{0}' where YoutubeID='{1}';", publishedAt.Value.ToString("yyyy-MM-dd HH:MM:ss").Trim(), video.Id.Trim());

                        NpgsqlCommand updateCommand = new NpgsqlCommand(udpateSql, conn);
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            throw new Exception("Video update didn't complete correctly.");
                        }
                    }

                    videoList = string.Empty;
                    totalCount += count;
                    Debug.WriteLine("Updated {0} videos", totalCount);
                    count = 0;

                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error with video insert: " + e.Message);
            }
        }

        public static void BuildThumbnailCollage(string youtubeId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(DBHandler.DatabaseConnStr))
                {
                    conn.Open();

                    // We check if the same youtube channel id has already been inserted
                    string selectByChannelSql = "select v.title, v.thumbnail, v.publishedat, c.title from videos v inner join channels c on c.channelid=v.channelid where c.YoutubeID='{0}' order by publishedat;";

                    NpgsqlCommand selectCommand = new NpgsqlCommand(string.Format(selectByChannelSql, youtubeId), conn);
                    NpgsqlDataReader reader = selectCommand.ExecuteReader();

                    List<string> thumbnails = new List<string>();
                    string imgTagFormat = "<img src=\"{0}\" height=\"50\" width=\"80\">";

                    while (reader.Read())
                    {
                        string thumbnail = reader["Thumbnail"].ToString().Trim();
                        Debug.WriteLine("Added " + thumbnail);
                        thumbnails.Add(string.Format(imgTagFormat, thumbnail));
                    }

                    reader.Close();


                    using (StreamWriter writer = new StreamWriter(@"C:\Users\Gabe\Desktop\test.html"))
                    {
                        foreach (string image in thumbnails)
                        {
                            writer.WriteLine(image);
                        }
                    }



                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error with video insert: " + e.Message);
            }
        }

        

        public static void DetectChannelSubscriptions()
        {
            List<ObjectHolder> allYoutubeChannelIds = DBHandler.RetrieveColumnsFromTable(typeof(ChannelHolder), "YoutubeID,Title", "Channels");

            foreach (ObjectHolder apiHolder in allYoutubeChannelIds)
            {
                ChannelHolder channel = apiHolder as ChannelHolder;
                bool status = YoutubeApiHandler.DoesChannelHavePublicSubscriptions(channel.YoutubeId);

                if (status)
                {
                    //Debug.WriteLine(channel.Title.PadRight(40) + channel.YoutubeId.PadRight(20) + " PUBLIC!");
                }
                else
                {
                    Debug.WriteLine(channel.Title.PadRight(40) + channel.YoutubeId.PadRight(20) + " private");
                }
            }
        }

        public static void UpdateAllMissingChannelUploads()
        {
            // Get all channel youtube ids
            List<ObjectHolder> allYoutubeChannelIds = DBHandler.RetrieveColumnsFromTable(typeof(ChannelHolder), "YoutubeID,Title", "Channels");

            int count = 1;
            // API request 1 video
            foreach(ObjectHolder apiResponse in allYoutubeChannelIds)
            {
                ChannelHolder channel = apiResponse as ChannelHolder;

                if (!AreUploadsUpToDate(channel.YoutubeId))
                {
                    Debug.WriteLine(count++ + ". " + channel.Title + " out of date. Fetching latest uploads...");

                    using (StreamWriter writer = File.AppendText(YOUTUBE_LOG_FILE))
                    {
                        writer.WriteLine("Fetching latest uploads for " + channel.Title);
                    }

                    FetchMissingChannelUploads(channel.YoutubeId);
                }
                else
                {
                    Debug.WriteLine(count++ + ". " + channel.Title + " is up to date!");
                }
            }
        }
        
        public static bool AreUploadsUpToDate(string youtubeChannelId)
        {
            // TEMPORARY SOLUTION
            // To check if uploads are up to date, we just have to request 1 video from the channel's uploads. 
            // If we have that video, then uploads are up to date
            bool status = false;

            // Get the uploads playlist id
            string uploadPlaylistId = DBHandler.RetrieveColumnBySingleCondition("UploadPlaylist", "Channels", "YoutubeID", youtubeChannelId);

            // Fetch 1 video from the uploads playlist id
            PlaylistItemListResponse response = YoutubeApiHandler.FetchVideosByPlaylist(uploadPlaylistId, "", "snippet", 1);

            if (response != null && response.Items.Count > 0)
            {
                string latestVideoId = response.Items[0].Snippet.ResourceId.VideoId;

                // Query database for latest video
                status = DBHandler.DoesItemExist("Videos", "YoutubeID", latestVideoId);
            }

            return status;
        }

        public static void FetchMissingChannelUploads(string youtubeChannelId)
        {
            // Check if completely new channel
            if (!DBHandler.DoesItemExist("Channels", "YoutubeID", youtubeChannelId))
            {
                // Channel doesn't exist. Throw an exception because this function will only
                // query for videos 5 at a time
                throw new Exception("FetchMissingChannelUploads(): Unrecognized youtube channel id: " + youtubeChannelId);
            }

            // Fetch actual channel id from youtube channel id
            int channelId = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);

            // Check if completely new channel with no videos written to database
            if (!DBHandler.DoesItemExist("Videos", "ChannelID", channelId))
            {
                // For channels with no videos previously written to database, we just fetch
                // all the channel uploads
                FetchChannelUploads(youtubeChannelId);
            }
            else
            {
                // For channels that have most of their videos inserted but the channel has a few more videos, 
                // we execute this code

                bool upToDate = false;
                string pageToken = string.Empty;
                int newVideoCount = 0;

                // Get the uploads playlist
                string uploadPlaylistId = DBHandler.RetrieveColumnBySingleCondition("UploadPlaylist", "Channels", "YoutubeID", youtubeChannelId);

                do
                {
                    // Fetch 5 videos at a time
                    PlaylistItemListResponse response = YoutubeApiHandler.FetchVideosByPlaylist(uploadPlaylistId, pageToken, "snippet", 5);
                    pageToken = response.NextPageToken;

                    foreach (PlaylistItem item in response.Items)
                    {
                        string youtubeVideoId = item.Snippet.ResourceId.VideoId;

                        // Check if video is in database
                        if (!DBHandler.DoesItemExist("Videos", "YoutubeID", youtubeVideoId))
                        {
                            // Perform api call and write to database
                            FetchVideoInfo(youtubeVideoId);
                            newVideoCount++;
                        }
                        else
                        {
                            upToDate = true;
                            break;
                        }
                    }

                }
                while (!upToDate && pageToken != null);

                Debug.WriteLine("Found " + newVideoCount + " new video(s)");
            }

            
        }

        public static void FetchAllSubscriptionsToAllChannels()
        {
            //int count = 1;
            //ChannelHolder channel = null;

            //// Get all channel youtube ids from database
            //List<ObjectHolder> allYoutubeChannelIds = DBHandler.RetrieveColumnsFromTable(typeof(ChannelHolder), "YoutubeID,Title", "Channels");

            //try
            //{
            //    // API request 1 video
            //    foreach (ObjectHolder apiResponse in allYoutubeChannelIds)
            //    {
            //        channel = apiResponse as ChannelHolder;

            //        if (YoutubeApiHandler.DoesChannelHavePublicSubscriptions(channel.YoutubeId))
            //        {
            //            Debug.WriteLine(count++ + ". Fetching subscriptions for " + channel.Title);
            //            FetchAndInsertChannelSubscriptions(channel.YoutubeId);
            //        }
            //        else
            //        {
            //            count++;
            //        }
            //    }
            //}
            //catch(Exception)
            //{
            //    Debug.WriteLine(DateTime.Now + ": Program crashed on #{0}: {1}", count, channel.Title);

            //    using (StreamWriter writer = File.AppendText(YOUTUBE_LOG_FILE))
            //    {
            //        writer.WriteLine(DateTime.Now + ": Program crashed on #{0}: {1}", count, channel.Title);
            //    }
            //}
            

            //// Check if channel has public subscriptions

            //// Record subscriptions of channel to database
        }

        public static void InsertCollectionsData()
        {
            //CollectionHolder collection = new CollectionHolder();
            //collection.OwnerChannelId = 117;
            //collection.Title = "Funny";

            //DBHandler.InsertCollection(collection);


            CollectionItemHolder collectionItem = new CollectionItemHolder();
            collectionItem.CollectionId = 1;
            collectionItem.ItemChannelId = 76;

            DBHandler.InsertCollectionItem(collectionItem);
        }

        public static void DeleteCollectionsData()
        {
            DBHandler.DeleteCollection(1);
        }

    }
}
