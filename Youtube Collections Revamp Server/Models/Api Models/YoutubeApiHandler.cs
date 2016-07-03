using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeCollectionsRevampServer.Models.ApiModels
{
    public class YoutubeApiHandler
    {
        private static string API_KEY = "AIzaSyD3PMEr28Ic6lIRisjCQZ1JO98aqHWLpR4";
        private const int MAX_RESULTS = 50;

        public static ChannelListResponse FetchUploadsPlaylistByChannel(string youtubeId, string part)
        {
            var ytService = FetchYoutubeService();

            ChannelsResource.ListRequest channelRequest = ytService.Channels.List(part);
            channelRequest.MaxResults = MAX_RESULTS;
            channelRequest.Id = youtubeId;

            ChannelListResponse channelResponse = channelRequest.Execute();

            return channelResponse;
        }

        public static SubscriptionListResponse FetchSubscriptionsByChannel(string youtubeId, string pageToken, string part)
        {
            var ytService = FetchYoutubeService();

            SubscriptionsResource.ListRequest subscriptionRequest = ytService.Subscriptions.List(part);
            subscriptionRequest.MaxResults = MAX_RESULTS;
            subscriptionRequest.PageToken = pageToken;
            subscriptionRequest.ChannelId = youtubeId;

            SubscriptionListResponse subscriptionResponse = subscriptionRequest.Execute();

            return subscriptionResponse;

        }

        public static PlaylistItemListResponse FetchVideosByPlaylist(string youtubeId, string pageToken, string part)
        {
            var ytService = FetchYoutubeService();

            PlaylistItemsResource.ListRequest playlistRequest = ytService.PlaylistItems.List(part);
            playlistRequest.MaxResults = MAX_RESULTS;

            playlistRequest.PlaylistId = youtubeId;
            playlistRequest.PageToken = pageToken;

            return playlistRequest.Execute();
        }

        public static PlaylistItemListResponse FetchVideosByPlaylist(string youtubeId, string pageToken, string part, int numResults)
        {
            var ytService = FetchYoutubeService();

            PlaylistItemsResource.ListRequest playlistRequest = ytService.PlaylistItems.List(part);
            playlistRequest.MaxResults = numResults;
            playlistRequest.PlaylistId = youtubeId;
            playlistRequest.PageToken = pageToken;

            return playlistRequest.Execute();
        }

        public static VideoListResponse FetchVideoById(string youtubeId, string part)
        {
            var ytService = FetchYoutubeService();

            VideosResource.ListRequest videoRequest = ytService.Videos.List(part);
            videoRequest.MaxResults = MAX_RESULTS;
            videoRequest.Id = youtubeId;

            return videoRequest.Execute();
        }

        public static List<string> FetchAllSubscriptions(string userYoutubeId)
        {
            List<string> youtubeApiSubscriptions = new List<string>();
            string nextPageToken = "";

            do
            {
                SubscriptionListResponse subscriptionsList = FetchSubscriptionsByChannel(userYoutubeId, nextPageToken, "snippet");
                nextPageToken = subscriptionsList.NextPageToken;

                foreach (var searchResult in subscriptionsList.Items)
                {
                    youtubeApiSubscriptions.Add(searchResult.Snippet.ResourceId.ChannelId);
                }
            }
            while (nextPageToken != null);

            return youtubeApiSubscriptions;
        }
        


        public static bool DoesChannelHavePublicSubscriptions(string youtubeId)
        {
            bool isAllowed = true;

            try
            {
                FetchSubscriptionsByChannel(youtubeId, "", "snippet");
            }
            catch (Exception)
            {
                isAllowed = false;
            }

            return isAllowed;
        }
        

        private static YouTubeService FetchYoutubeService()
        {
            return new YouTubeService(new BaseClientService.Initializer() { ApiKey = API_KEY });
        }
    }
}
