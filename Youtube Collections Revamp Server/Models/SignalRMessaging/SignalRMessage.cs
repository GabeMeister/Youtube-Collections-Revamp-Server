namespace YoutubeCollectionsRevampServer.Models.SignalRMessaging
{
    public class SignalRMessage
    {
        public string Message;

        public SignalRMessage()
        {
            Message = "";
        }

        public SignalRMessage(string msg)
        {
            Message = msg;
        }
    }
}