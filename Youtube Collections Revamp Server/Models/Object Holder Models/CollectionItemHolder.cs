using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Youtube_Collections_Server.Model.Object_Holder_Models
{
    public class CollectionItemHolder : ObjectHolder
    {
        public int CollectionItemHolderId { get; set; }
        public int CollectionId { get; set; }
        public int ItemChannelId { get; set; }
    }
}
