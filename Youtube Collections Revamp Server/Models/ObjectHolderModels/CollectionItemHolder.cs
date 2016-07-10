namespace YoutubeCollectionsRevampServer.Models.ObjectHolderModels
{
    public class CollectionItemHolder : ObjectHolder
    {
        public int CollectionItemHolderId { get; set; }
        public int CollectionId { get; set; }
        public int ItemChannelId { get; set; }
    }
}
