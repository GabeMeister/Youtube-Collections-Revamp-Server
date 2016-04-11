using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YoutubeCollectionsRevampServer.Models.ObjectHolderModels;
using System.Diagnostics;
using NpgsqlTypes;

namespace YoutubeCollectionsRevampServer.Models.DatabaseModels
{
    public class DBHandler
    {
        public static string DatabaseConnStr = @"Server=127.0.0.1;Port=5432;User Id=postgres;Password=4321;Database=YoutubeCollections";

        // ============================ GENERAL
        #region GENERAL
        public static int RetrieveIdFromYoutubeId(string idColumnToSelect, string table, string youtubeId)
        {
            int id = -1;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectByIdSql(idColumnToSelect, table, "YoutubeID", youtubeId);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    id = Convert.ToInt32(reader[idColumnToSelect].ToString().Trim());
                }

                conn.Close();
            }

            return id;
        }

        public static string RetrieveColumnBySingleCondition(string columnToSelect, string table, string columnToQuery, string queryValue)
        {
            string value = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectByIdSql(columnToSelect, table, columnToQuery, queryValue);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    value = reader[columnToSelect].ToString().Trim();
                }

                conn.Close();
            }

            return value;
        }

        public static List<string> RetrieveColumnFromTable(string columnToSelect, string table)
        {
            List<string> youtubeIds = new List<string>();

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectAllSql(columnToSelect, table);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string youtubeId = reader[columnToSelect].ToString().Trim();
                        youtubeIds.Add(youtubeId);
                    }
                }

                conn.Close();
            }

            return youtubeIds;
        }

        public static List<ObjectHolder> RetrieveColumnsFromTable(Type itemType, string columnsToSelect, string table)
        {
            List<ObjectHolder> items = new List<ObjectHolder>();

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectAllSql(columnsToSelect, table).Replace(";", " offset 3000;");
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ConstructorInfo constructor = itemType.GetConstructor(new[] { typeof(NpgsqlDataReader) });
                        ObjectHolder newItem = constructor.Invoke(new object[] { reader }) as ObjectHolder;

                        items.Add(newItem);
                    }
                }

                conn.Close();
            }

            return items;
        }

        #endregion

        // ============================ JOINS
        #region JOINS

        #endregion

        // ============================ CHANNELS
        #region CHANNELS
        public static int InsertChannel(ChannelHolder channel)
        {
            int rowsAffected = 0;

            // We check if the same youtube channel id has already been inserted
            if (!DoesItemExist("Channels", "YoutubeID", channel.YoutubeId))
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Now we actually insert the channel because we know it's not in the database
                    string insertSQL = SqlBuilder.FetchInsertChannelSql(channel);
                    NpgsqlCommand insertCommand = new NpgsqlCommand(insertSQL, conn);
                    rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected < 1)
                    {
                        throw new Exception("Channel insert didn't complete correctly.");
                    }

                    conn.Close();
                }
            }

            return rowsAffected;
        }

        public static void DeleteChannel(int channelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", channelId), "Deleting non-existant channel");

            // Delete the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = string.Format("delete from Channels where ChannelID={0};", Sanitize(channelId));
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);

                // The user may have no videos, so returning no rows affected is ok
                deleteCommand.ExecuteNonQuery();

                conn.Close();
            }
        }

        #endregion

        // ============================ SUBSCRIPTIONS
        #region SUBSCRIPTIONS
        public static int InsertSubscription(int subscriberChannelId, int beingSubscribedToChannelId)
        {
            if (subscriberChannelId == -1 || beingSubscribedToChannelId == -1)
            {
                // The id is -1 if the channel doesn't exist or isn't available.
                return -1;
            }


            int rowsAffected = 0;

            // We check if the subscription already exists.
            if (!DoesSubscriptionExist(subscriberChannelId, beingSubscribedToChannelId))
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Now we actually insert the channel because we know it's not in the database
                    string insertSQL = SqlBuilder.InsertSubscriptionByChannelIdSql(subscriberChannelId, beingSubscribedToChannelId);
                    NpgsqlCommand insertCommand = new NpgsqlCommand(insertSQL, conn);
                    rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected < 1)
                    {
                        throw new Exception("Subscription insert didn't complete correctly.");
                    }

                    conn.Close();
                }
            }

            return rowsAffected;
        }

        public static bool DoesSubscriptionExist(int subscriberChannelId, int beingSubscribedToChannelId)
        {
            bool doesExist = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectBySubscriberIdsSql("count(*)", subscriberChannelId, beingSubscribedToChannelId);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                int count = Convert.ToInt16(selectCommand.ExecuteScalar());

                if (count > 0)
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }
        
        public static void DeleteChannelSubscriptions(int channelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", channelId), "Deleting subscriptions of non-existant channel");

            // Delete the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = string.Format("delete from Subscriptions where SubscriberChannelID={0};", Sanitize(channelId));
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);

                // The user may have no watched videos, so returning no rows affected is ok
                deleteCommand.ExecuteNonQuery();

                conn.Close();
            }
        }

        public static void DeleteSubscriptionsToChannel(int channelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", channelId), "Deleting subscriptions TO channel that doesn't exist.");

            // Delete the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = string.Format("delete from Subscriptions where BeingSubscribedToChannelID={0};", Sanitize(channelId));
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);

                // There might be no subscribers to the specified channel so that's ok if there's 0 rows affected
                deleteCommand.ExecuteNonQuery();

                conn.Close();
            }
        }

        #endregion

        // ============================ COLLECTIONS
        #region COLLECTIONS

        public static int InsertCollection(CollectionHolder collection)
        {
            int rowsAffected = 0;

            // Check that the owner channel id exists
            bool exists = DoesItemExist("Channels", "ChannelID", collection.OwnerChannelId);
            Debug.Assert(exists, "InsertCollection(): Unrecognized youtube channel id: " + collection.OwnerYoutubeChannelId);
            
            // Make sure there isn't already a collection with same name
            if (!DoesCollectionExist(collection.OwnerChannelId, collection.Title))
            {
                // Insert the collection
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Now we actually insert the channel because we know it's not in the database
                    string insertSQL = SqlBuilder.InsertCollectionSql(collection.OwnerChannelId, collection.Title);
                    NpgsqlCommand insertCommand = new NpgsqlCommand(insertSQL, conn);
                    rowsAffected = insertCommand.ExecuteNonQuery();

                    Debug.Assert(rowsAffected > 0, "Collection insert didn't complete correctly.");

                    conn.Close();
                }
            }


            return rowsAffected;
        }

        public static void RenameCollection(string ownerYoutubeChannelId, string origCollectionTitle, string newCollectionTitle)
        {
            // TODO
            throw new NotImplementedException();
        }

        public static void DeleteCollection(int collectionId)
        {
            // Check that the collection exists
            bool exists = DoesItemExist("Collections", "CollectionID", collectionId);
            Debug.Assert(exists, "Trying to delete non-existant collection.");

            // We have to delete the collection items first
            DeleteCollectionItemsForCollection(collectionId);

            
            // Now delete the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = SqlBuilder.DeleteCollectionSql(collectionId);
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);
                int rowsAffected = deleteCommand.ExecuteNonQuery();

                Debug.Assert(rowsAffected > 0, "Collection delete didn't complete correctly.");

                conn.Close();
            }

        }

        public static bool DoesCollectionExist(int ownerChannelId, string collectionTitle)
        {
            bool doesExist = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectByChannelIdAndCollectionTitle("count(*)", ownerChannelId, collectionTitle);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                int count = Convert.ToInt16(selectCommand.ExecuteScalar());

                if (count > 0)
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }

        public static void DeleteChannelCollections(int channelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", channelId), "Deleting collections from non-existant channel");

            List<int> collectionIds = new List<int>();

            // Get all collections for channel
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = string.Format("select CollectionID from Collections where OwnerChannelId={0};", Sanitize(channelId));
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);

                // The user may have no collections, so returning no rows affected is ok
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

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

        public static int SelectCollectionIdByChannelIdAndTitle(int channelId, string title)
        {
            int collectionId = -1;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = "select coll.CollectionID from Collections coll inner join Channels ch on ch.ChannelID = coll.OwnerChannelID where coll.OwnerChannelID=:id and coll.Title=:title;";
                var command = new NpgsqlCommand(selectSql, conn);
                command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer));
                command.Parameters.Add(new NpgsqlParameter("title", NpgsqlDbType.Varchar));
                command.Parameters[0].Value = channelId;
                command.Parameters[1].Value = title;
                NpgsqlDataReader reader = command.ExecuteReader();

                reader.Read();

                Debug.Assert(reader.HasRows, "Couldn't find correct collection.");

                // We just want the collection id
                collectionId = Convert.ToInt32(reader["CollectionID"].ToString().Trim());


                conn.Close();
            }


            return collectionId;
        }

        #endregion

        // ============================ COLLECTION ITEMS
        #region COLLECTION ITEMS
        public static int InsertCollectionItem(CollectionItemHolder collectionItem)
        {
            int rowsAffected = 0;

            int collectionId = collectionItem.CollectionId;
            int itemChannelId = collectionItem.ItemChannelId;

            // Check that the collection exists
            bool exists = DoesItemExist("Collections", "CollectionID", collectionId);
            Debug.Assert(exists, "Unrecognized youtube collection id: " + collectionId);

            // Check that the channel id exists
            exists = DoesItemExist("Channels", "ChannelID", itemChannelId);
            if (!exists)
            {
                throw new Exception("Unrecognized channel id: " + itemChannelId);
            }



            // Make sure there isn't already a channel in the collection with the same name
            if (!DoesCollectionItemExist(collectionId, itemChannelId))
            {
                // Insert the collection
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    string insertSQL = SqlBuilder.InsertCollectionItemSql(collectionId, itemChannelId);
                    NpgsqlCommand insertCommand = new NpgsqlCommand(insertSQL, conn);
                    rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected < 1)
                    {
                        throw new Exception("Collection item insert didn't complete correctly.");
                    }

                    conn.Close();
                }
            }



            return rowsAffected;
        }

        public static void InsertCollectionItem(int itemChannelId, int collectionId)
        {
            // Insert the collection
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string sql = @"insert into CollectionItems (CollectionID, ItemChannelID) values (:collID,:channelID);";
                var command = new NpgsqlCommand(sql, conn);
                command.Parameters.Add(new NpgsqlParameter("collID", NpgsqlDbType.Integer));
                command.Parameters.Add(new NpgsqlParameter("channelID", NpgsqlDbType.Integer));
                command.Parameters[0].Value = collectionId;
                command.Parameters[1].Value = itemChannelId;
                
                int rowsAffected = command.ExecuteNonQuery();
                Debug.Assert(rowsAffected > 0, "Collection Item insert didn't happen correctly.");

                conn.Close();
            }
        }

        public static int DeleteCollectionItem(int collectionId, int itemChannelId)
        {
            int rowsAffected = 0;

            // Check that the collection item exists
            bool exists = DoesCollectionItemExist(collectionId, itemChannelId);
            Debug.Assert(exists, "Trying to delete non-existant collection item.");

            // Delete the collection item
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = SqlBuilder.DeleteCollectionItemSql(collectionId, itemChannelId);
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);
                rowsAffected = deleteCommand.ExecuteNonQuery();
                Debug.Assert(rowsAffected > 0, "Collection item delete didn't complete correctly.");

                conn.Close();
            }

            return rowsAffected;
        }

        public static bool DoesCollectionItemExist(int collectionId, int channelId)
        {
            bool doesExist = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectCollectionItemByChannelId("count(*)", collectionId, channelId);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                int count = Convert.ToInt16(selectCommand.ExecuteScalar());

                if (count > 0)
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }

        public static void DeleteCollectionItemsForCollection(int collectionId)
        {
            bool exists = DoesItemExist("Collections", "CollectionID", collectionId);
            Debug.Assert(exists, "Attempting to delete items of non-existant collection");

            // Start by getting all the collection items
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string sql = string.Format("delete from CollectionItems where CollectionID={0};", Sanitize(collectionId));
                NpgsqlCommand deleteCommand = new NpgsqlCommand(sql, conn);

                // The collection may have no collection items, so returning no rows affected is ok
                deleteCommand.ExecuteReader();

                conn.Close();
            }
        }

        public static void DeleteChannelCollectionItems(int channelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", channelId), "Deleting collections from non-existant channel");

            List<int> collectionIds = new List<int>();

            // Get all collections for channel
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = string.Format("select CollectionID from Collections where OwnerChannelId={0};", Sanitize(channelId));
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);

                // The user may have no collections, so returning no rows affected is ok
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

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
        public static int InsertVideo(VideoHolder video)
        {
            int rowsAffected = 0;

            // We check if the same youtube channel id has already been inserted
            bool alreadyExists = DoesItemExist("Videos", "YoutubeID", video.YoutubeId);

            if (!alreadyExists)
            {
                // We actually insert the video because we know it's not in the database
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    // Have to get the actual channel id first, from the youtube id
                    // The channel id may or may not be in database already
                    video.ChannelId = Convert.ToUInt64(RetrieveIdFromYoutubeId("ChannelID", "Channels", video.YoutubeChannelId));
                    Debug.Assert(video.ChannelId > 0, "Inserting video of non-existant channel. Channel must have been already inserted before this point.");

                    string insertSQL = SqlBuilder.InsertVideoSql(video);
                    NpgsqlCommand insertCommand = new NpgsqlCommand(insertSQL, conn);
                    rowsAffected = insertCommand.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Video insert didn't complete correctly.");
                    
                    conn.Close();
                }
            }

            return rowsAffected;
        }
        
        public static void DeleteChannelVideos(int channelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", channelId), "Deleting videos of non-existant channel");

            // Delete the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = string.Format("delete from videos where ChannelID={0};", Sanitize(channelId));
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);

                // The user may have no videos, so returning no rows affected is ok
                deleteCommand.ExecuteNonQuery();

                conn.Close();
            }
        }

        public static void DeleteChannelWatchedVideos(int channelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", channelId), "Deleting watched videos of non-existant channel");

            // Delete the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = string.Format("delete from WatchedVideos where ChannelID={0};", Sanitize(channelId));
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);

                // The user may have no watched videos, so returning no rows affected is ok
                deleteCommand.ExecuteNonQuery();

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
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    string insertSql = string.Format("insert into WatchedVideos (ChannelID, VideoID, DateViewed) values ({0}, {1}, '{2}');", 
                        Sanitize(channelId), Sanitize(videoId), Sanitize(dateViewed));
                    var command = new NpgsqlCommand(insertSql, conn);
                    
                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Assert(rowsAffected > 0, "Unable to insert watched video");

                    conn.Close();
                }
            }

        }

        public static bool DoesWatchedVideoExist(int videoId, int channelId)
        {
            bool doesExist = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = "select count(*) from WatchedVideos where ChannelID=:channelId and VideoID=:videoId;";
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                selectCommand.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Integer));
                selectCommand.Parameters.Add(new NpgsqlParameter("videoId", NpgsqlDbType.Integer));
                selectCommand.Parameters[0].Value = channelId;
                selectCommand.Parameters[1].Value = videoId;

                int count = Convert.ToInt16(selectCommand.ExecuteScalar());

                if (count > 0)
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }

        #endregion



        #region Utilities
        public static bool DoesItemExist(string table, string columnToQuery, string id)
        {
            bool doesExist = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectByIdSql("count(*)", table, columnToQuery, id);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                int count = Convert.ToInt16(selectCommand.ExecuteScalar());

                if (count > 0)
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }

        public static bool DoesItemExist(string table, string columnToQuery, int id)
        {
            bool doesExist = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = SqlBuilder.SelectByIdSql("count(*)", table, columnToQuery, id);
                NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, conn);
                int count = Convert.ToInt16(selectCommand.ExecuteScalar());

                if (count > 0)
                {
                    doesExist = true;
                }

                conn.Close();
            }

            return doesExist;
        }

        private static string Sanitize(object str)
        {
            return str.ToString().Replace("'", "''").Trim();
        }


        
        #endregion

    }
}
