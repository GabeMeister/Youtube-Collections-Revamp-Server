using System;
using System.IO;

namespace YoutubeCollectionsRevampServerUnitTests
{
    public class YoutubeCollectionsLogger
    {
        private const string defaultLogFilePath = @"C:\Users\Gabe\Desktop\YoutubeCollectionsLogFile.log";
        private string logFilePath;

        public YoutubeCollectionsLogger()
        {
            logFilePath = defaultLogFilePath;
        }

        public YoutubeCollectionsLogger(string fileName)
        {
            logFilePath = fileName;
        }

        public void Log(string msg)
        {
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                string formattedMsg = string.Format("{0}: {1}", DateTime.Now, msg);
                writer.WriteLine(formattedMsg);
            }
        }


    }
}
