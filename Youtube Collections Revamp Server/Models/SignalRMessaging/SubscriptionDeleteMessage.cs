namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class SubscriptionDeleteMessage : SignalRMessage
    {
        public string YoutubeIdAffected;

        public SubscriptionDeleteMessage(string youtubeIdAffected)
        {
            Message = "SubscriptionDelete";
            YoutubeIdAffected = youtubeIdAffected;
        }
    }
}