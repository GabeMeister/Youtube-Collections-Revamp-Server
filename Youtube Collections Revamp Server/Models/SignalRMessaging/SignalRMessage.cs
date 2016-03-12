using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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