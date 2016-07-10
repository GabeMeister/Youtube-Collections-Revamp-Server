using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(YoutubeCollectionsRevampServer.Startup))]

namespace YoutubeCollectionsRevampServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
