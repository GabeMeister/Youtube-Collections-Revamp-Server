namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class ChannelVideosInsertMessage : SignalRMessage
    {
        public string ChannelId;

        public ChannelVideosInsertMessage(string channelId)
        {
            Message = "ChannelVideosInsert";
            ChannelId = channelId;
        }
    }
}