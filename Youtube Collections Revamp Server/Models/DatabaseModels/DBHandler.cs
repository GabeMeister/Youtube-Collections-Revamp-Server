using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using YoutubeCollectionsRevampServer.Models.ObjectHolderModels;
using System.Diagnostics;
using NpgsqlTypes;

namespace YoutubeCollectionsRevampServer.Models.DatabaseModels
{
    public class DbHandler
    {
        // Local host database
        //public static string DatabaseConnStr = @"Server=127.0.0.1;Port=5432;User Id=postgres;Password=4321;Database=YoutubeCollections";

        // Azure database (which I stopped because it's too expensive)
        //public static string DatabaseConnStr = @"Server=104.40.49.186;Port=5432;User Id=gabemeister;Password=qwerqwer1234!;Database=youtubecollections";

        // Digital Ocean database, which is currently being used
        public static string DatabaseConnStr = @"Server=104.236.163.200;Port=5432;User Id=gabemeister;Password=qwerQWER1234!;Database=youtubecollections";

        // ============================ GENERAL
        #region GENERAL
        
        public static string SelectColumnBySingleCondition(string columnToSelect, string table, string columnToQuery, string queryValue)
        {
            string value = null;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = string.Format("select {0} from {1} where {2}=@queryValue", 
                    columnToSelect, 
                    table,
                    columnToQuery);
                command.Parameters.AddWithValue("@queryValue", queryValue);

                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    value = reader[columnToSelect].ToString().Trim();
                }

                conn.Close();
            }

            return value;
        }

        public static string SelectColumnBySingleCondition(string columnToSelect, string table, string columnToQuery, int queryValue)
        {
            string value = null;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = string.Format("select {0} from {1} where {2}=@queryValue", columnToSelect, table,
                    columnToQuery);
                command.Parameters.AddWithValue("@queryValue", queryValue);

                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    value = reader[columnToSelect].ToString().Trim();
                }

                conn.Close();
            }

            return value;
        }

        public static int SelectIdFromYoutubeId(string idColumnToSelect, string table, string youtubeId)
        {
            int id = -1;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = string.Format("select {0} from {1} where YoutubeID=@YoutubeID limit 1;", idColumnToSelect, table);
                command.Parameters.AddWithValue("@YoutubeID", youtubeId);

                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    id = Convert.ToInt32(reader[idColumnToSelect].ToString().Trim());
                }

                conn.Close();
            }

            return id;
        }

        #endregion
        
        // ============================ CHANNELS
        #region CHANNELS
        
        public static void InsertChannel(ChannelHolder channel)
        {
            // We check if the same youtube channel id has already been inserted
            if (!DoesIdExist("Channels", "YoutubeID", channel.YoutubeId))
            {
                // We first insert the channel into the Channels table
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Now we actually insert the channel because we know it's not in the database
                    var command = conn.CreateCommand();
                    command.CommandText =
                        "insert into Channels (YoutubeID, Title, Description, UploadPlaylist, Thumbnail, ViewCount, SubscriberCount, VideoCount) values " +
                        "(@YoutubeID, @Title, @Description, @UploadPlaylist, @Thumbnail, @ViewCount, @SubscriberCount, @VideoCount);";
                    command.Parameters.AddWithValue("@YoutubeID", channel.YoutubeId);
                    command.Parameters.AddWithValue("@Title", channel.Title);
                    command.Parameters.AddWithValue("@Description", channel.Description);
                    command.Parameters.AddWithValue("@UploadPlaylist", channel.UploadPlaylist);
                    command.Parameters.AddWithValue("@Thumbnail", channel.Thumbnail);
                    command.Parameters.AddWithValue("@ViewCount", Convert.ToInt64(channel.ViewCount.ToString()));
                    command.Parameters.AddWithValue("@SubscriberCount", Convert.ToInt64(channel.SubscriberCount.ToString()));
                    command.Parameters.AddWithValue("@VideoCount", Convert.ToInt64(channel.ViewCount.ToString()));

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, string.Format("Inserting channel {0} didn't complete correctly.", channel.Title));
                    
                    conn.Close();
                }

                // Get the channel id of the channel we just inserted
                int channelId = SelectIdFromYoutubeId("ChannelID", "Channels", channel.YoutubeId);

                InsertChannelIntoChannelsToDownload(channelId);
            }
        }

        public static void InsertChannelIntoChannelsToDownload(int channelId)
        {
            if (!DoesIdExist("ChannelsToDownload", "ChannelID", channelId))
            {
                // We then log this channel to the ChannelsToDownload table, as it will have to be queued to download later.
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText = "insert into ChannelsToDownload (ChannelID) values (@ChannelID);";
                    command.Parameters.AddWithValue("@ChannelID", channelId);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, string.Format("Channel id of {0} didn't insert into ChannelsToDownload correctly.", channelId));

                    conn.Close();
                }
            }
        }

        public static void DeleteChannel(int channelId)
        {
            if (DoesIdExist("Channels", "ChannelID", channelId))
            {
                // Delete the collection
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText = "delete from Channels where ChannelID=@ChannelID;";
                    command.Parameters.AddWithValue("@ChannelID", channelId);

                    // The user may have no videos, so returning no rows affected is ok
                    command.ExecuteNonQuery();

                    conn.Close();
                }
            }
            
        }

        public static ChannelHolder PopulateChannelHolderFromTable(int channelId, string columnsToSelect)
        {
            ChannelHolder channel = null;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = string.Format("select {0} from Channels where ChannelID=@ChannelID;", columnsToSelect);
                command.Parameters.AddWithValue("@ChannelID", channelId);

                var reader = command.ExecuteReader();
                if (reader.HasRows && reader.Read())
                {
                    channel = new ChannelHolder(reader);
                }

                conn.Close();
            }

            return channel;
        }
        #endregion

        // ============================ CHANNELS TO DOWNLOAD
        #region CHANNELS TO DOWNLOAD
        public static void DeleteChannelToDownload(int channelId)
        {
            if (DoesIdExist("ChannelsToDownload", "ChannelID", channelId))
            {
                // Delete the collection
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText = "delete from ChannelsToDownload where ChannelID=@ChannelID;";
                    command.Parameters.AddWithValue("@ChannelID", channelId);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, string.Format("Unable to delete channel id of {0} from ChannelsToDownload table", channelId));

                    conn.Close();
                }
            }
            
        }

        public static List<string> GetChannelsToDownloadYoutubeIdsMatchingList(List<string> youtubeIds)
        {
            if (youtubeIds.Count == 0)
            {
                // We don't return anything here, just an empty list
                return new List<string>();
            }

            var channelsToDownloadIds = new List<string>();
            
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                var quotedYoutubeIds = new string[youtubeIds.Count];
                for (int i = 0; i < youtubeIds.Count; i++)
                {
                    quotedYoutubeIds[i] = string.Format("@YoutubeID{0}", i);
                    command.Parameters.AddWithValue(quotedYoutubeIds[i], youtubeIds[i]);
                }
                command.CommandText = string.Format(@"select c.YoutubeID 
                                                        from ChannelsToDownload ctd
                                                        inner join Channels c 
                                                        on c.ChannelID=ctd.ChannelID
                                                        where c.YoutubeID in ({0});",
                                                        string.Join(",", quotedYoutubeIds));
                
                // The user may have no videos, so returning no rows affected is ok
                var reader = command.ExecuteReader();
                while(reader.Read())
                {
                    string youtubeId = reader["YoutubeID"].ToString().Trim();
                    channelsToDownloadIds.Add(youtubeId);
                }

                conn.Close();
            }

            return channelsToDownloadIds;
        }
        #endregion

        // ============================ SUBSCRIPTIONS
        #region SUBSCRIPTIONS

        public static List<string> SelectYoutubeIdSubscriptionsForUser(int channelId)
        {
            var allSubscriptions = new List<string>();

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = @"select 
                                        c.YoutubeID
                                        from Subscriptions s
                                        inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
                                        where s.SubscriberChannelID=@ChannelID;";
                command.Parameters.AddWithValue("@ChannelID", channelId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    allSubscriptions.Add(reader["YoutubeID"].ToString().Trim());
                }

                conn.Close();
            }

            return allSubscriptions;
        }

        public static List<int> SelectSubscriptionChannelIdsForUser(int channelId)
        {
            var allSubscriptions = new List<int>();

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = @"select 
                                        c.ChannelID
                                        from Subscriptions s
                                        inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
                                        where s.SubscriberChannelID=@ChannelID;";
                command.Parameters.AddWithValue("@ChannelID", channelId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    allSubscriptions.Add(Convert.ToInt32(reader["ChannelID"].ToString().Trim()));
                }

                conn.Close();
            }

            return allSubscriptions;
        }

        public static void InsertSubscription(int subscriberChannelId, int beingSubscribedToChannelId)
        {
            if (subscriberChannelId == -1 || beingSubscribedToChannelId == -1)
            {
                // The id is -1 if the channel doesn't exist or isn't available.
                return;
            }

            // We check if the subscription already exists.
            if (!DoesSubscriptionExist(subscriberChannelId, beingSubscribedToChannelId))
            {
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Now we actually insert the subscription because we know it's not in the database
                    var command = conn.CreateCommand();
                    command.CommandText =
                        "insert into Subscriptions (SubscriberChannelID, BeingSubscribedToChannelID) values (@SubscriberChannelID, @BeingSubscribedToChannelID);";
                    command.Parameters.AddWithValue("@SubscriberChannelID", subscriberChannelId);
                    command.Parameters.AddWithValue("@BeingSubscribedToChannelID", beingSubscribedToChannelId);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Subscription insert didn't complete correctly.");
                    
                    conn.Close();
                }
            }
        }

        public static void DeleteSubscription(int subscriberChannelId, int beingSubscribedToChannelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", subscriberChannelId), "Deleting a subscription of non-existant channel");
            Debug.Assert(DoesIdExist("Channels", "ChannelID", beingSubscribedToChannelId), "Channel is being subscribed to a non-existant channel");

            if (DoesSubscriptionExist(subscriberChannelId, beingSubscribedToChannelId))
            {
                // Delete the subscription
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText =
                        "delete from Subscriptions where SubscriberChannelID=@SubscriberChannelID and BeingSubscribedToChannelID=@BeingSubscribedToChannelID;";
                    command.Parameters.AddWithValue("@SubscriberChannelID", subscriberChannelId);
                    command.Parameters.AddWithValue("@BeingSubscribedToChannelID", beingSubscribedToChannelId);

                    int affectedRows = command.ExecuteNonQuery();
                    Debug.Assert(affectedRows > 0, "Wasn't able to delete subscription.");
                    
                    conn.Close();
                }
            }
            
        }

        public static bool DoesSubscriptionExist(int subscriberChannelId, int beingSubscribedToChannelId)
        {
            bool doesExist = false;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = @"select BeingSubscribedToChannelID 
                                        from Subscriptions 
                                        where SubscriberChannelID=@SubscriberChannelID 
                                        and BeingSubscribedToChannelID=@BeingSubscribedToChannelID limit 1;";
                command.Parameters.AddWithValue("@SubscriberChannelID", subscriberChannelId);
                command.Parameters.AddWithValue("@BeingSubscribedToChannelID", beingSubscribedToChannelId);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }
        
        public static void DeleteChannelSubscriptions(int subscriberChannelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", subscriberChannelId), "Deleting subscriptions of non-existant channel");

            // Delete the collection
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "delete from Subscriptions where SubscriberChannelID=@SubscriberChannelID;";
                command.Parameters.AddWithValue("@SubscriberChannelID", subscriberChannelId);

                command.ExecuteNonQuery();

                conn.Close();
            }
        }

        public static void DeleteSubscriptionsToChannel(int beingSubscribedToChannelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", beingSubscribedToChannelId), "Deleting subscriptions to a channel that doesn't exist.");
            
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "delete from Subscriptions where BeingSubscribedToChannelID=@BeingSubscribedToChannelID;";
                command.Parameters.AddWithValue("@BeingSubscribedToChannelID", beingSubscribedToChannelId);

                command.ExecuteNonQuery();

                conn.Close();
            }
        }

        #endregion

        // ============================ COLLECTIONS
        #region COLLECTIONS
        public static int SelectCollectionIdByChannelIdAndTitle(int channelId, string title)
        {
            int collectionId = -1;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "select CollectionID from Collections where OwnerChannelID=@OwnerChannelID and Title=@Title;";
                command.Parameters.AddWithValue("@OwnerChannelID", channelId);
                command.Parameters.AddWithValue("@Title", title);

                var reader = command.ExecuteReader();
                reader.Read();
                collectionId = Convert.ToInt32(reader["CollectionID"].ToString().Trim());

                conn.Close();
            }

            return collectionId;
        }

        public static List<int> SelectCollectionIdsForUser(int channelId)
        {
            var collectionIds = new List<int>();
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "select CollectionID from Collections where OwnerChannelID=@OwnerChannelID;";
                command.Parameters.AddWithValue("@OwnerChannelID", channelId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    collectionIds.Add(Convert.ToInt32(reader["CollectionID"].ToString().Trim()));
                }

                conn.Close();
            }

            return collectionIds;
        }

        public static void InsertCollection(CollectionHolder collection)
        {
            // Check that the owner channel id exists
            Debug.Assert(DoesIdExist("Channels", "ChannelID", collection.OwnerChannelId), 
                "Inserting collection with non-existant owner channel id" + collection.OwnerChannelId);
            
            // Make sure there isn't already a collection with same name
            if (!DoesCollectionExist(collection.OwnerChannelId, collection.Title))
            {
                // Insert the collection
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Now we actually insert the channel because we know it's not in the database
                    var command = conn.CreateCommand();
                    command.CommandText =
                        "insert into Collections (OwnerChannelID, Title) values (@OwnerChannelID, @Title);";
                    command.Parameters.AddWithValue("@OwnerChannelID", collection.OwnerChannelId);
                    command.Parameters.AddWithValue("@Title", collection.Title);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Collection insert didn't complete correctly.");

                    conn.Close();
                }
            }

        }

        public static void RenameCollection(int collectionId, string newCollectionTitle)
        {
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "update Collections set Title=@Title where CollectionID=@CollectionID;";
                command.Parameters.AddWithValue("@Title", newCollectionTitle);
                command.Parameters.AddWithValue("@CollectionID", collectionId);

                int rowsAffected = command.ExecuteNonQuery();
                Debug.Assert(rowsAffected > 0, "Collection rename didn't complete correctly.");

                conn.Close();
            }
        }

        public static void DeleteCollection(int collectionId)
        {
            // Check that the collection exists
            if (DoesIdExist("Collections", "CollectionID", collectionId))
            {
                // We have to delete the collection items first
                DeleteCollectionItemsForCollection(collectionId);

                // Now delete the collection
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText = "delete from Collections where CollectionID=@CollectionID;";
                    command.Parameters.AddWithValue("@CollectionID", collectionId);
                    
                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Collection delete didn't complete correctly.");

                    conn.Close();
                }
            }
            

        }

        public static bool DoesCollectionExist(int ownerChannelId, string collectionTitle)
        {
            bool doesExist = false;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "select OwnerChannelID from Collections where OwnerChannelID=@OwnerChannelID and Title=@Title limit 1;";
                command.Parameters.AddWithValue("@OwnerChannelID", ownerChannelId);
                command.Parameters.AddWithValue("@Title", collectionTitle);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }

        public static void DeleteChannelCollections(int channelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", channelId), "Deleting collections from non-existant channel");

            var collectionIds = new List<int>();

            // Get all collections for channel
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "select CollectionID from Collections where OwnerChannelId=@OwnerChannelId;";
                command.Parameters.AddWithValue("@OwnerChannelId", channelId);

                // The user may have no collections, so returning no rows affected is ok
                var reader = command.ExecuteReader();
                while(reader.Read())
                {
                    int collectionId = Convert.ToInt32(reader["CollectionID"].ToString().Trim());
                    collectionIds.Add(collectionId);
                }

                conn.Close();
            }

            // Iterate through all collections and delete one at a time
            foreach(int id in collectionIds)
            {
                DeleteCollection(id);
            }
            

        }

        

        #endregion

        // ============================ COLLECTION ITEMS
        #region COLLECTION ITEMS
        public static List<int> SelectCollectionItemsByCollectionId(int collectionId)
        {
            var collectionItemIds = new List<int>();
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "select ItemChannelID from CollectionItems where CollectionId=@CollectionId;";
                command.Parameters.AddWithValue("@CollectionId", collectionId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int collectionItemId = Convert.ToInt32(reader["ItemChannelID"].ToString());
                    collectionItemIds.Add(collectionItemId);
                }

                conn.Close();
            }

            return collectionItemIds;
        }

        public static List<string> SelectCollectionItemNamesByCollectionId(int collectionId)
        {
            var collectionItemNames = new List<string>();
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = @"select ch.Title
                                        from Collections co
                                        inner join CollectionItems ci on ci.CollectionID=co.CollectionID
                                        inner join Channels ch on ch.ChannelID=ci.ItemChannelID
                                        where co.CollectionID=@CollectionID;";
                command.Parameters.AddWithValue("@CollectionId", collectionId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string channelName = reader["Title"].ToString().Trim();
                    collectionItemNames.Add(channelName);
                }

                conn.Close();
            }

            return collectionItemNames;
        }

        public static void InsertCollectionItem(CollectionItemHolder collectionItem)
        {
            if (!DoesIdExist("Collections", "CollectionID", collectionItem.CollectionId))
            {
                // We ignore requests to insert a collection item for a non-existant collection
                return;
            }

            if (!DoesIdExist("Channels", "ChannelID", collectionItem.ItemChannelId))
            {
                // We ignore requests to insert a collection item for an item with a non-existant channel id
                return;
            }

            int collectionId = collectionItem.CollectionId;
            int itemChannelId = collectionItem.ItemChannelId;
            
            // Make sure there isn't already a channel in the collection with the same name
            if (!DoesCollectionItemExist(collectionId, itemChannelId))
            {
                // Insert the collection
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText =
                        "insert into CollectionItems (CollectionID, ItemChannelID) values (@CollectionID, @ItemChannelID);";
                    command.Parameters.AddWithValue("@CollectionID", collectionId);
                    command.Parameters.AddWithValue("@ItemChannelID", itemChannelId);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Collection item insert didn't complete correctly.");

                    conn.Close();
                }
            }

        }

        public static void InsertCollectionItem(int itemChannelId, int collectionId)
        {
            if (!DoesIdExist("Collections", "CollectionID", collectionId))
            {
                // We ignore requests to insert a collection item for a non-existant collection
                return;
            }

            if (!DoesIdExist("Channels", "ChannelID", itemChannelId))
            {
                // We ignore requests to insert a collection item for an item with a non-existant channel id
                return;
            }

            if (!DoesCollectionItemExist(collectionId, itemChannelId))
            {
                // Insert the collection item
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText =
                        @"insert into CollectionItems (CollectionID, ItemChannelID) values (@CollectionID, @ItemChannelID);";
                    command.Parameters.AddWithValue("@CollectionID", collectionId);
                    command.Parameters.AddWithValue("@ItemChannelID", itemChannelId);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Collection Item insert didn't happen correctly.");

                    conn.Close();
                }
            }
            
        }

        public static void DeleteCollectionItem(int collectionId, int itemChannelId)
        {
            // Check that the collection item exists
            if (DoesCollectionItemExist(collectionId, itemChannelId))
            {
                // Delete the collection item
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText = "delete from CollectionItems where CollectionID=@CollectionID and ItemChannelID=@ItemChannelID;";
                    command.Parameters.AddWithValue("@CollectionID", collectionId);
                    command.Parameters.AddWithValue("@ItemChannelID", itemChannelId);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Collection item delete didn't complete correctly.");

                    conn.Close();
                }
            }

        }

        public static void DeleteChannelFromAllUserCollections(int collectionItemChannelId, int userChannelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", collectionItemChannelId), "Trying to delete non-existant collection item.");
            Debug.Assert(DoesIdExist("Channels", "ChannelID", userChannelId), "Trying to delete collection item for non-existant user channel.");

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = @"delete from CollectionItems ci
                                        where ci.CollectionItemID in
                                        (
	                                        select
                                            ci.CollectionItemID
                                            from CollectionItems ci
                                            inner join Collections co on co.CollectionID=ci.CollectionID
                                            inner join Channels ownerChannel on ownerChannel.ChannelID=co.OwnerChannelID
                                            inner join Channels channelItem on channelItem.ChannelID=ci.ItemChannelID
                                            where ownerChannel.ChannelID=@OwnerChannelID
                                            and channelItem.ChannelID=@ItemChannelID
                                        );";
                command.Parameters.AddWithValue("@OwnerChannelID", userChannelId);
                command.Parameters.AddWithValue("@ItemChannelID", collectionItemChannelId);
                
                command.ExecuteNonQuery();
                
                conn.Close();
            }

        }

        public static bool DoesCollectionItemExist(int collectionId, int channelId)
        {
            bool doesExist = false;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "select CollectionID from CollectionItems where CollectionID=@CollectionID and ItemChannelID=@ItemChannelID;";
                command.Parameters.AddWithValue("@CollectionID", collectionId);
                command.Parameters.AddWithValue("@ItemChannelID", channelId);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    doesExist = true;
                }
                
                conn.Close();
            }

            return doesExist;
        }

        public static void DeleteCollectionItemsForCollection(int collectionId)
        {
            Debug.Assert(DoesIdExist("Collections", "CollectionID", collectionId), 
                "Attempting to delete items of non-existant collection");

            // Start by getting all the collection items
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "delete from CollectionItems where CollectionID=@CollectionID;";
                command.Parameters.AddWithValue("@CollectionID", collectionId);

                // The collection may have no collection items, so returning no rows affected is ok
                command.ExecuteReader();

                conn.Close();
            }
        }

        public static void DeleteChannelCollectionItems(int channelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", channelId), "Deleting collections from non-existant channel");

            var collectionIds = new List<int>();

            // Get all collections for channel
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "select CollectionID from Collections where OwnerChannelId=@OwnerChannelId;";
                command.Parameters.AddWithValue("@OwnerChannelId", channelId);

                // The user may have no collections, so returning no rows affected is ok
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int collectionId = Convert.ToInt32(reader["CollectionID"].ToString().Trim());
                    collectionIds.Add(collectionId);
                }

                conn.Close();
            }

            // Iterate through all collections and delete one at a time
            foreach (int collectionId in collectionIds)
            {
                DeleteCollectionItemsForCollection(collectionId);
            }
        }

        

        #endregion

        // ============================ VIDEOS
        #region VIDEOS
        
        public static List<VideoHolder> SelectVideoInformationForVideoIds(List<int> videoIds)
        {
            if (!videoIds.Any())
            {
                // Just return nothing if no video ids were given to us
                return new List<VideoHolder>();
            }

            var collectionVideoInfo = new List<VideoHolder>();
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                string[] videoPlaceHolders = new string[videoIds.Count];
                for (int i = 0; i < videoIds.Count; i++)
                {
                    videoPlaceHolders[i] = string.Format("@Video{0}", i);
                    command.Parameters.AddWithValue(videoPlaceHolders[i], videoIds[i]);
                }
                command.CommandText =
                    string.Format(@"select v.YoutubeID, 
                                            v.ChannelID, 
                                            v.Title, 
                                            v.Thumbnail, 
                                            v.Duration, 
                                            v.ViewCount, 
                                            v.PublishedAt, 
                                            c.Title as ChannelTitle
                                            from Videos v 
                                            inner join Channels c on v.ChannelID=c.ChannelID
                                            where v.VideoID in ({0});", string.Join(",", videoPlaceHolders));
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var video = new VideoHolder(reader);
                    collectionVideoInfo.Add(video);
                }

                conn.Close();
            }

            return collectionVideoInfo;
        }

        public static void InsertVideo(VideoHolder video)
        {
            // We check if the same youtube channel id has already been inserted
            if (!DoesIdExist("Videos", "YoutubeID", video.YoutubeId))
            {
                // We actually insert the video because we know it's not in the database
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Have to get the actual channel id first, from the youtube id
                    // The channel id may or may not be in database already
                    video.ChannelId = Convert.ToUInt64(SelectIdFromYoutubeId("ChannelID", "Channels", video.YoutubeChannelId));
                    Debug.Assert(video.ChannelId > 0, "Inserting video of non-existant channel. Channel must have been already inserted before this point.");

                    Debug.WriteLine(video.Title);

                    var command = conn.CreateCommand();
                    command.CommandText =
                        "insert into Videos (YoutubeID,ChannelID,Title,Thumbnail,Duration,ViewCount,PublishedAt) values (@YoutubeID,@ChannelID,@Title,@Thumbnail,@Duration,@ViewCount,@PublishedAt);";
                    command.Parameters.AddWithValue("@YoutubeID", video.YoutubeId);
                    command.Parameters.AddWithValue("@ChannelID", Convert.ToInt32(video.ChannelId.ToString()));
                    command.Parameters.AddWithValue("@Title", video.Title);
                    command.Parameters.AddWithValue("@Thumbnail", video.Thumbnail);
                    command.Parameters.AddWithValue("@Duration", video.Duration);
                    command.Parameters.AddWithValue("@ViewCount", Convert.ToInt64(video.ViewCount.ToString()));
                    if (video.PublishedAt != null)
                    {
                        command.Parameters.AddWithValue("@PublishedAt", video.PublishedAt.Value);
                    }
                    

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Video insert didn't complete correctly.");
                    
                    conn.Close();
                }
            }

        }
        
        public static void DeleteChannelVideos(int channelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", channelId), "Deleting videos of non-existant channel");
            
            // Delete the collection
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "delete from videos where ChannelID=@ChannelID;";
                command.Parameters.AddWithValue("@ChannelID", channelId);

                // The user may have no videos, so returning no rows affected is ok
                command.ExecuteNonQuery();

                conn.Close();
            }
        }

        public static void DeleteChannelWatchedVideos(int channelId)
        {
            Debug.Assert(DoesIdExist("Channels", "ChannelID", channelId), "Deleting watched videos of non-existant channel");

            // Delete the collection
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "delete from WatchedVideos where ChannelID=@ChannelID;";
                command.Parameters.AddWithValue("@ChannelID", channelId);

                // The user may have no watched videos, so returning no rows affected is ok
                command.ExecuteNonQuery();

                conn.Close();
            }
        }

        #endregion

        // ============================ WATCHED VIDEOS
        #region WATCHED VIDEOS

        public static void InsertWatchedVideo(int videoId, int channelId, string dateViewed)
        {
            if (!DoesWatchedVideoExist(videoId, channelId))
            {
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText =
                        "insert into WatchedVideos (ChannelID, VideoID, DateViewed) values (@ChannelID, @VideoID, @DateViewed);";
                    command.Parameters.AddWithValue("@ChannelID", channelId);
                    command.Parameters.AddWithValue("@VideoID", videoId);
                    command.Parameters.AddWithValue("@DateViewed", DateTime.Parse(dateViewed));
                    
                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Unable to insert watched video");

                    conn.Close();
                }
            }

        }

        public static bool DoesWatchedVideoExist(int videoId, int channelId)
        {
            bool doesExist = false;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText =
                    "select ChannelID from WatchedVideos where ChannelID=@ChannelID and VideoID=@VideoID;";
                command.Parameters.AddWithValue("@ChannelID", channelId);
                command.Parameters.AddWithValue("@VideoID", videoId);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }

        public static IEnumerable<string> SelectWatchedVideosForUser(int channelId, List<int> youtubeVideoIds)
        {
            var watchedYoutubeIds = new List<string>();

            // We actually insert the video because we know it's not in the database
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                var videoPlaceHolders = new string[youtubeVideoIds.Count];
                for (int i = 0; i < youtubeVideoIds.Count; i++)
                {
                    videoPlaceHolders[i] = string.Format("@Video{0}", i);
                    command.Parameters.AddWithValue(videoPlaceHolders[i], youtubeVideoIds[i]);
                }
                command.CommandText = string.Format("select VideoID from WatchedVideos where ChannelID=@ChannelID and VideoID in ({0});", string.Join(",", videoPlaceHolders));
                
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    // Since we're returning the youtube id of the video, we convert the video ids back to youtube ids
                    int watchedVideoId = Convert.ToInt32(reader["VideoID"].ToString().Trim());
                    string watchedYoutubeId = SelectColumnBySingleCondition("YoutubeID", "Videos", "VideoID", watchedVideoId.ToString());
                    watchedYoutubeIds.Add(watchedYoutubeId);
                }

                conn.Close();
            }

            return watchedYoutubeIds;
        }

        public static IEnumerable<string> SelectUnwatchedVideosForUser(int channelId, List<int> relatedVideoIds)
        {
            if (!relatedVideoIds.Any())
            {
                // If no related video ids are given, we just return an empty list
                return new List<string>();
            }

            var unwatchedYoutubeIds = new List<string>();
            var watchedVideoIds = new List<int>();

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                var videoPlaceHolders = new string[relatedVideoIds.Count];
                for (int i = 0; i < relatedVideoIds.Count; i++)
                {
                    videoPlaceHolders[i] = string.Format("@Video{0}", i);
                    command.Parameters.AddWithValue(videoPlaceHolders[i], relatedVideoIds[i]);
                }
                command.CommandText = string.Format("select VideoID from WatchedVideos where ChannelID=@ChannelID and VideoID in ({0});", string.Join(",", videoPlaceHolders));
                command.Parameters.AddWithValue("@ChannelID", channelId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int watchedVideoId = Convert.ToInt32(reader["VideoID"].ToString().Trim());
                    if (relatedVideoIds.Contains(watchedVideoId))
                    {
                        watchedVideoIds.Add(watchedVideoId);
                    }
                }

                conn.Close();
            }

            // We get all the video ids that the user hasn't seen be calling Except() on the videos
            // that the user has already seen
            IEnumerable<int> unwatchedVideoIds = relatedVideoIds.Except(watchedVideoIds);

            foreach(int videoId in unwatchedVideoIds)
            {
                // Since we're returning the youtube id of the video, we convert the video ids back to youtube ids
                string youtubeId = SelectColumnBySingleCondition("YoutubeID", "Videos", "VideoID", videoId.ToString());
                unwatchedYoutubeIds.Add(youtubeId);
            }

            return unwatchedYoutubeIds;
        }

        public static List<int> SelectUnwatchedVideoIdsForUserSubscription(int channelId, int subscriptionId, int numVideos)
        {
            var collectionVideoIds = new List<int>();

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = string.Format(@"select 
                                                v.VideoID 
                                                from Channels c 
                                                inner join Subscriptions s on s.SubscriberChannelID=c.ChannelID 
                                                inner join Channels c2 on s.BeingSubscribedToChannelID=c2.ChannelID 
                                                inner join Videos v on v.ChannelID=c2.ChannelID 
                                                where c.ChannelID=@SubscriberChannelID
                                                and c2.ChannelID=@BeingSubscribedToChannelID
                                                and v.VideoID not in 
                                                (select VideoId from WatchedVideos where ChannelID=@SubscriberChannelID)
                                                order by v.PublishedAt desc limit {0};", numVideos);
                command.Parameters.AddWithValue("@SubscriberChannelID", channelId);
                command.Parameters.AddWithValue("@BeingSubscribedToChannelID", subscriptionId);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int videoId = Convert.ToInt32(reader["VideoID"].ToString().Trim());
                    collectionVideoIds.Add(videoId);
                }

                conn.Close();
            }

            return collectionVideoIds;
        }

        #endregion



        #region Utilities
        public static bool DoesIdExist(string table, string idColumnName, string id)
        {
            bool doesExist = false;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();

                // TODO: change back to use parameters
                // Temporarily just concatenating a string together.
                // See https://stackoverflow.com/questions/41829625/very-slow-performance-when-using-npgsql-parameters
                command.CommandText = string.Format("select {0} from {1} where {0}='{2}' limit 1;", idColumnName, table, id);
                //command.CommandText = string.Format("select {0} from {1} where {0}=@id limit 1;", idColumnName, table);
                //command.Parameters.AddWithValue("@id", id);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    doesExist = true;
                }
                
                conn.Close();
            }

            return doesExist;
        }

        public static bool DoesIdExist(string table, string idColumnName, int id)
        {
            bool doesExist = false;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = string.Format("select {0} from {1} where {0}=@id limit 1;", idColumnName, table);
                command.Parameters.AddWithValue("@id", id);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }
        

        #endregion

    }
}
