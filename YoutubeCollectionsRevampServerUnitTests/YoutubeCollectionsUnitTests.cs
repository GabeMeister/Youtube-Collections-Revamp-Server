using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeCollectionsRevampServer;
using YoutubeCollectionsRevampServer.Models.DatabaseModels;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeCollectionsRevampServerUnitTests
{
    [TestClass]
    public class YoutubeCollectionsUnitTests
    {
        #region ACTUAL TESTS
        [TestMethod]
        public void TestRestartCollections()
        {
            // Gabe J youtube channel id
            string youtubeChannelId = "UC4LVLoBN0xbOb5xJuA0ia9A";
            
            // Make sure we got the right channel id
            int channelIdToDelete = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Assert.AreNotEqual(channelIdToDelete, -1, "Channel wasn't found");
            
            // Check that everything was ACTUALLY deleted

            // Get the database channel id
            int channelIdThatShouldNotExist = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Assert.AreEqual(channelIdThatShouldNotExist, -1, "Channel still not deleted from database");

            // Delete the channel's collections
            string columnValue = DbHandler.SelectColumnBySingleCondition("CollectionID", "Collections", "OwnerChannelID", channelIdToDelete.ToString());
            Assert.AreEqual(columnValue, null, "Not all collections were removed from database.");

            // Delete the channel's uploads videos
            columnValue = DbHandler.SelectColumnBySingleCondition("VideoID", "Videos", "ChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all uploads were removed from database.");

            // Delete the channel's watched videos
            columnValue = DbHandler.SelectColumnBySingleCondition("WatchedVideoID", "WatchedVideos", "ChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all watched videos were removed from database.");

            // Delete the channel's subscriptions
            columnValue = DbHandler.SelectColumnBySingleCondition("SubscriptionID", "Subscriptions", "SubscriberChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all instances of Gabe's subscriptions were removed from database.");

            // Delete subscriptions to the channel
            columnValue = DbHandler.SelectColumnBySingleCondition("SubscriptionID", "Subscriptions", "BeingSubscribedToChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all subscriptions to Gabe's channel were removed from database.");

        }

        [TestMethod]
        public void TestAddAthlean()
        {
            // Add Athlean x to subscriptions for Gabe J if not there
            int athleanId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", "UCe0TLA0EsQbE-MjuHXevj2A");
            int myChannel = Convert.ToInt32(DbHandler.SelectColumnBySingleCondition("ChannelID", "Channels", "Title", "Gabe J"));

            DbHandler.InsertSubscription(myChannel, athleanId);

        }

        [TestMethod]
        public void TestRemoveAthlean()
        {
            // Remove Athlean X subscription for Gabe J if there
            int athleanId = DbHandler.SelectIdFromYoutubeId("ChannelID", "Channels", "UCe0TLA0EsQbE-MjuHXevj2A");
            int myChannel = Convert.ToInt32(DbHandler.SelectColumnBySingleCondition("ChannelID", "Channels", "Title", "Gabe J"));

            DbHandler.DeleteSubscription(myChannel, athleanId);

        }


        #endregion

        #region Debug Tasks

        [TestMethod]
        public void DeleteUserFromDatabase()
        {
            YoutubeTasks.CompletelyDeleteChannel("UCZzpPbDsnio4g3oKwgNBd7A");
        }

        #endregion
    }
}
