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

        public static string RetrieveColumnBySingleCondition(string columnToSelect, string table, string columnToQuery, int queryValue)
        {
            string value = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSql = string.Format(@"select {0} from {1} where {2}={3};", 
                    Sanitize(columnToSelect),
                    Sanitize(table), 
                    Sanitize(columnToQuery), 
                    Sanitize(queryValue));
                SqlBuilder.SelectByIdSql(columnToSelect, table, columnToQuery, queryValue);
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
                // We first insert the channel into the Channels table
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

                // Get the channel id of the channel we just inserted
                int channelId = RetrieveIdFromYoutubeId("ChannelID", "Channels", channel.YoutubeId);

                InsertChannelIntoChannelsToDownload(channelId);
                
            }

            return rowsAffected;
        }

        public static void InsertChannelIntoChannelsToDownload(int channelId)
        {
            if (!DoesItemExist("ChannelsToDownload", "ChannelID", channelId))
            {
                // We then log this channel to the ChannelsToDownload table, as it will have to be queued to download later.
                using (var conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    string insertSQL = string.Format("insert into ChannelsToDownload (ChannelID) values ({0});", Sanitize(channelId));

                    var insertCommand = new NpgsqlCommand(insertSQL, conn);
                    int channelsToDownloadRowsAffected = insertCommand.ExecuteNonQuery();

                    Debug.Assert(channelsToDownloadRowsAffected > 0, "Channel didn't insert into ChannelsToDownload correctly.");

                    conn.Close();
                }
            }
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

        public static void SetAreVideosLoadedForChannel(int channelId, bool areVideosLoaded)
        {
            bool exists = DoesItemExist("Channels", "ChannelID", channelId);
            Debug.Assert(exists, "Non-existant channel id");

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                // Now we actually insert the channel because we know it's not in the database
                string updateSql = string.Format("update Channels set AreVideosLoaded={0} where ChannelID={1}", areVideosLoaded, channelId);
                var updateCommand = new NpgsqlCommand(updateSql, conn);
                int rowsAffected = updateCommand.ExecuteNonQuery();

                Debug.Assert(rowsAffected > 0, "Channel update didn't complete correctly.");
                
                conn.Close();
            }
        }

        public static ChannelHolder PopulateChannelHolderFromTable(int channelId, string columnsToSelect)
        {
            ChannelHolder channel = null;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string sql = string.Format("select {0} from Channels where ChannelID=@ChannelID;", columnsToSelect);
                var command = new NpgsqlCommand(sql, conn);
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
        public static void RemoveChannelToDownload(int channelId)
        {
            Debug.Assert(DoesItemExist("ChannelsToDownload", "ChannelID", channelId), "Deleting non-existant channel to download");

            // Delete the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string deleteSql = string.Format("delete from ChannelsToDownload where ChannelID={0};", Sanitize(channelId));
                NpgsqlCommand deleteCommand = new NpgsqlCommand(deleteSql, conn);

                // The user may have no videos, so returning no rows affected is ok
                deleteCommand.ExecuteNonQuery();

                conn.Close();
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
            var quotedYoutubeIds = new List<string>();

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                // TODO: figure out why this isn't working
                // youtubeIds.ForEach(x => Quotify(x));
                for (int i = 0; i < youtubeIds.Count; i++)
                {
                    quotedYoutubeIds.Add(Quotify(Sanitize(youtubeIds[i])));
                }

                string sql = string.Format(@"select c.YoutubeID 
                                            from ChannelsToDownload ctd
                                            inner join Channels c 
                                            on c.ChannelID=ctd.ChannelID
                                            where c.YoutubeID in ({0});",
                                            string.Join(",", quotedYoutubeIds));
                var selectCommand = new NpgsqlCommand(sql, conn);

                // The user may have no videos, so returning no rows affected is ok
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

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

        public static bool DeleteSubscription(int subscriberChannelId, int beingSubscribedToChannelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", subscriberChannelId), "Deleting subscriptions of non-existant channel");
            Debug.Assert(DoesItemExist("Channels", "ChannelID", beingSubscribedToChannelId), "Channel is being subscribed to a non-existant channel");

            bool isSuccessful = false;

            // Delete the subscription
            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = new NpgsqlCommand("delete from Subscriptions where SubscriberChannelID=@SubscriberChannelID and BeingSubscribedToChannelID=@BeingSubscribedToChannelID;", conn);
                command.Parameters.AddWithValue("@SubscriberChannelID", subscriberChannelId);
                command.Parameters.AddWithValue("@BeingSubscribedToChannelID", beingSubscribedToChannelId);

                // The user may have no watched videos, so returning no rows affected is ok
                int affectedRows = command.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    isSuccessful = true;
                }

                conn.Close();
            }

            return isSuccessful;
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

        public static List<string> GetYoutubeIdSubscriptionsForUser(int channelId)
        {
            List<string> allSubscriptions = new List<string>();

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                var command = new NpgsqlCommand(@"select 
                                                    c.YoutubeID
                                                    from Subscriptions s
                                                    inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
                                                    where s.SubscriberChannelID=@ChannelID;", conn);
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

        public static void RenameCollection(int collectionId, string newCollectionTitle)
        {
            // Rename the collection
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string updateSql = string.Format("update Collections set title='{0}' where CollectionID={1};", newCollectionTitle, collectionId);
                NpgsqlCommand updateCommand = new NpgsqlCommand(updateSql, conn);
                int rowsAffected = updateCommand.ExecuteNonQuery();
                Debug.Assert(rowsAffected > 0, "Collection update didn't complete correctly.");

                conn.Close();
            }
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

                string selectSql = string.Format("select CollectionID from Collections where OwnerChannelID={0} and Title='{1}';", channelId, title);
                NpgsqlCommand command = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = command.ExecuteReader();
                Debug.Assert(reader.HasRows, "Collection not found");
                reader.Read();

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

        public static bool DeleteChannelFromAllUserCollections(int collectionItemChannelId, int userChannelId)
        {
            Debug.Assert(DoesItemExist("Channels", "ChannelID", collectionItemChannelId), "Trying to delete non-existant collection item.");
            Debug.Assert(DoesItemExist("Channels", "ChannelID", userChannelId), "Trying to delete collection item for non-existant user channel.");

            bool wasDeleted = false;

            using (var conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                NpgsqlCommand command = conn.CreateCommand();
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
                
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    wasDeleted = true;
                }

                conn.Close();
            }

            return wasDeleted;
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

        public static List<int> SelectCollectionItemsByCollectionId(int collectionId)
        {
            List<int> collectionItemIds = new List<int>();
            using (NpgsqlConnection conn = new NpgsqlConnection(DBHandler.DatabaseConnStr))
            {
                conn.Open();

                string selectSql = string.Format("select ItemChannelID from CollectionItems where CollectionId={0};", collectionId);
                NpgsqlCommand command = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int collectionItemId = Convert.ToInt32(reader["ItemChannelID"].ToString());
                    collectionItemIds.Add(collectionItemId);
                }


                conn.Close();
            }

            return collectionItemIds;
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

                    Debug.WriteLine(video.Title);
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

        public static List<VideoHolder> SelectVideoInformationForVideoIds(IEnumerable<int> videoIds)
        {
            Debug.Assert(videoIds.Count() > 0, "No video ids to query for");

            List<VideoHolder> collectionVideos = new List<VideoHolder>();
            using (NpgsqlConnection conn = new NpgsqlConnection(DBHandler.DatabaseConnStr))
            {
                conn.Open();

                string selectSql = string.Format(@"select v.YoutubeID, v.ChannelID, v.Title, v.Thumbnail, v.Duration, v.ViewCount, v.PublishedAt, c.Title as ChannelTitle
                                                    from Videos v 
                                                    inner join Channels c on v.ChannelID=c.ChannelID
                                                    where v.VideoID in ({0});", string.Join(",", videoIds));
                NpgsqlCommand command = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var video = new VideoHolder(reader);
                    collectionVideos.Add(video);
                }

                conn.Close();
            }

            return collectionVideos;
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

        public static IEnumerable<string> GetWatchedVideosForUser(int channelId, IEnumerable<int> youtubeVideoIds)
        {   
            List<int> watchedVideoIds = new List<int>();
            List<string> watchedYoutubeIds = new List<string>();

            foreach(int videoId in youtubeVideoIds)
            {
                // We actually insert the video because we know it's not in the database
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
                {
                    conn.Open();

                    string selectSQL = string.Format("select VideoID from WatchedVideos where ChannelID={0} and VideoID in ({1});", Sanitize(channelId), Sanitize(string.Join(",", youtubeVideoIds)));
                    var selectCommand = new NpgsqlCommand(selectSQL, conn);
                    NpgsqlDataReader reader = selectCommand.ExecuteReader();

                    // Since we're returning the youtube id of the video, we convert the video ids back to youtube ids
                    while(reader.Read())
                    {
                        int watchedVideoId = Convert.ToInt32(reader["VideoID"]);
                        string watchedYoutubeId = RetrieveColumnBySingleCondition("YoutubeID", "Videos", "VideoID", watchedVideoId.ToString());

                        watchedYoutubeIds.Add(watchedYoutubeId);
                    }
                    
                    conn.Close();
                }
            }

            return watchedYoutubeIds;
        }

        public static IEnumerable<string> GetUnwatchedVideosForUser(int channelId, IEnumerable<int> relatedVideoIds)
        {
            if (!relatedVideoIds.Any())
            {
                return new List<string>();
            }

            List<string> unwatchedYoutubeIds = new List<string>();
            List<int> watchedVideoIds = new List<int>();

            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConnStr))
            {
                conn.Open();

                string selectSQL = string.Format("select VideoID from WatchedVideos where ChannelID='{0}' and VideoID in ({1});", Sanitize(channelId), Sanitize(string.Join(",", relatedVideoIds)));
                var selectCommand = new NpgsqlCommand(selectSQL, conn);
                NpgsqlDataReader reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    int watchedVideoId = Convert.ToInt32(reader["VideoID"]);
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
                string youtubeId = RetrieveColumnBySingleCondition("YoutubeID", "Videos", "VideoID", videoId.ToString());
                unwatchedYoutubeIds.Add(youtubeId);
            }

            return unwatchedYoutubeIds;
        }

        public static List<int> SelectUnwatchedVideoIdsForUserSubscription(int channelId, int subscriptionId, int numVideos)
        {
            List<int> collectionVideoIds = new List<int>();
            using (NpgsqlConnection conn = new NpgsqlConnection(DBHandler.DatabaseConnStr))
            {
                conn.Open();

                string selectSql = string.Format(@"select 
                                                        v.VideoID 
                                                        from Channels c 
                                                        inner join Subscriptions s on s.SubscriberChannelID=c.ChannelID 
                                                        inner join Channels c2 on s.BeingSubscribedTochannelID=c2.ChannelID 
                                                        inner join Videos v on v.ChannelID=c2.ChannelID 
                                                        where c.ChannelID={0}
                                                        and c2.ChannelID={1}
                                                        and v.VideoID not in 
                                                        (select VideoId from WatchedVideos where ChannelID={0})
                                                        order by v.PublishedAt desc limit {2};", channelId, subscriptionId, numVideos);
                NpgsqlCommand command = new NpgsqlCommand(selectSql, conn);
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int videoId = Convert.ToInt32(reader["VideoID"].ToString());
                    collectionVideoIds.Add(videoId);
                }

                conn.Close();
            }

            return collectionVideoIds;
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
                int count = Convert.ToInt32(selectCommand.ExecuteScalar());

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

        private static string Quotify(string str)
        {
            return "'" + str + "'";
        }


        
        #endregion

    }
}
