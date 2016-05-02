using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeCollectionsRevampServer;
using YoutubeCollectionsRevampServer.Models.DatabaseModels;
using System.Collections.Generic;
using System.Linq;
using YoutubeCollectionsRevampServer.Models.ApiModels;

namespace YoutubeCollectionsRevampServerUnitTests
{
    [TestClass]
    public class YoutubeCollectionsUnitTests
    {
        [TestMethod]
        public void TestRestartCollections()
        {
            string youtubeChannelId = "UC4LVLoBN0xbOb5xJuA0ia9A";
            var hub = new YoutubeCollectionsHub();

            // Make sure we got the right channel id
            int channelIdToDelete = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Assert.AreNotEqual(channelIdToDelete, -1, "Channel wasn't found");

            // We totally start over and remove everything about Gabe's channel
            hub.RestartInitialization();


            // Check that everything was ACTUALLY deleted

            // Get the database channel id
            int channelIdThatShouldNotExist = DBHandler.RetrieveIdFromYoutubeId("ChannelID", "Channels", youtubeChannelId);
            Assert.AreEqual(channelIdThatShouldNotExist, -1, "Channel still not deleted from database");

            // Delete the channel's collections
            // TODO make sure postgres can handle ints to strings
            string columnValue = DBHandler.RetrieveColumnBySingleCondition("CollectionID", "Collections", "OwnerChannelID", channelIdToDelete.ToString());
            Assert.AreEqual(columnValue, null, "Not all collections were removed from database.");

            // Delete the channel's uploads videos
            columnValue = DBHandler.RetrieveColumnBySingleCondition("VideoID", "Videos", "ChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all uploads were removed from database.");

            // Delete the channel's watched videos
            columnValue = DBHandler.RetrieveColumnBySingleCondition("WatchedVideoID", "WatchedVideos", "ChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all watched videos were removed from database.");

            // Delete the channel's subscriptions
            columnValue = DBHandler.RetrieveColumnBySingleCondition("SubscriptionID", "Subscriptions", "SubscriberChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all instances of Gabe's subscriptions were removed from database.");

            // Delete subscriptions to the channel
            columnValue = DBHandler.RetrieveColumnBySingleCondition("SubscriptionID", "Subscriptions", "BeingSubscribedToChannelID", channelIdToDelete);
            Assert.AreEqual(columnValue, null, "Not all subscriptions to Gabe's channel were removed from database.");

        }

        [TestMethod]
        public void MarkVideosDownloadedOrNot()
        {
            List<int> allChannelIds = DBHandler.RetrieveColumnFromTable("ChannelID", "Channels").Select(x => int.Parse(x)).ToList();
            var logger = new YoutubeCollectionsLogger();

            foreach(int channelId in allChannelIds)
            {
                bool areVideosPresent = DBHandler.DoesItemExist("Videos", "ChannelID", channelId);
                string channelName = DBHandler.RetrieveColumnBySingleCondition("Title", "Channels", "ChannelID", channelId);
                DBHandler.SetAreVideosLoadedForChannel(channelId, areVideosPresent);
                logger.Log(string.Format("{0}: {1}", channelName, areVideosPresent ? "yes" : "no"));
            }
        }


    }
}
