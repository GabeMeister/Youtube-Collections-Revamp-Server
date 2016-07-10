namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class WatchedVideoInsertedMessage : SignalRMessage
    {
        public WatchedVideoInsertedMessage()
        {
            Message = "WatchedVideoInserted";
        }
    }
}