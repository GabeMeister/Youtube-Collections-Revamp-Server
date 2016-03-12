using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeCollectionsRevampServer.Model.Object_Holder_Models
{
    public class CollectionHolder : ObjectHolder
    {
        public int CollectionHolderId { get; set; }
        public int OwnerChannelId { get; set; }
        public string OwnerYoutubeChannelId { get; set; }
        public string Title { get; set; }
    }
}
