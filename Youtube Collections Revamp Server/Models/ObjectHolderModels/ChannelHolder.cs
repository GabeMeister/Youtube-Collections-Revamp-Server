using Google.Apis.YouTube.v3.Data;
using Npgsql;
using System;

namespace YoutubeCollectionsRevampServer.Models.ObjectHolderModels
{
    public class ChannelHolder : ObjectHolder
    {
        public int ChannelHolderId { get; set; }
        public string YoutubeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UploadPlaylist { get; set; }
        public string Thumbnail { get; set; }
        public ulong? ViewCount { get; set; }
        public ulong? SubscriberCount { get; set; }
        public ulong? VideoCount { get; set; }
        

        public ChannelHolder()
        {
            ChannelHolderId = -1;
            YoutubeId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            UploadPlaylist = string.Empty;
            Thumbnail = string.Empty;
            ViewCount = 0;
            SubscriberCount = 0;
            VideoCount = 0;
        }

        public ChannelHolder(Channel channelResponse)
        {
            // Channel holder id is empty because we are populating from an API call
            ChannelHolderId = -1;
            YoutubeId = channelResponse.Id.Trim();
            Title = channelResponse.Snippet.Title.Trim();
            // The database only holds up to 1000 characters
            // Solution from http://stackoverflow.com/a/3566842/1751481
            Description = channelResponse.Snippet.Description.Trim();
            Description = Description.Substring(0, Math.Min(Description.Length, 1000));

            UploadPlaylist = channelResponse.ContentDetails.RelatedPlaylists.Uploads.Trim();
            Thumbnail = channelResponse.Snippet.Thumbnails.Medium.Url.Trim();
            ViewCount = channelResponse.Statistics.ViewCount;
            SubscriberCount = channelResponse.Statistics.SubscriberCount;
            VideoCount = channelResponse.Statistics.VideoCount;
        }


        public ChannelHolder(NpgsqlDataReader reader)
        {
            if (ColumnExists(reader, "ChannelID"))
            {
                ChannelHolderId = Convert.ToInt32(reader["ChannelID"].ToString().Trim());
            }

            if (ColumnExists(reader, "YoutubeID"))
            {
                YoutubeId = reader["YoutubeID"].ToString().Trim();
            }

            if (ColumnExists(reader, "Title"))
            {
                Title = reader["Title"].ToString().Trim();
            }

            if (ColumnExists(reader, "Description"))
            {
                Description = reader["Description"].ToString().Trim();
            }

            if (ColumnExists(reader, "UploadPlaylist"))
            {
                UploadPlaylist = reader["UploadPlaylist"].ToString().Trim();
            }

            if (ColumnExists(reader, "Thumbnail"))
            {
                Thumbnail = reader["Thumbnail"].ToString().Trim();
            }

            if (ColumnExists(reader, "ViewCount"))
            {
                ViewCount = Convert.ToUInt64(reader["ViewCount"].ToString().Trim());
            }

            if (ColumnExists(reader, "SubscriberCount"))
            {
                SubscriberCount = Convert.ToUInt64(reader["SubscriberCount"].ToString().Trim());
            }

            if (ColumnExists(reader, "VideoCount"))
            {
                VideoCount = Convert.ToUInt64(reader["VideoCount"].ToString().Trim());
            }
        }

        

        
        
    }
}
