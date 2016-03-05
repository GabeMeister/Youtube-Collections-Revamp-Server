using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Youtube_Collections_Revamp_Server.Models
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