namespace YoutubeCollectionsRevampServer.Models.ObjectHolderModels
{
    public class CollectionHolder : ObjectHolder
    {
        public int CollectionHolderId { get; set; }
        public int OwnerChannelId { get; set; }
        public string OwnerYoutubeChannelId { get; set; }
        public string Title { get; set; }

        public CollectionHolder(string title, string ownerYoutubeChannelId, int ownerChannelId)
        {
            OwnerChannelId = ownerChannelId;
            OwnerYoutubeChannelId = ownerYoutubeChannelId;
            Title = title;
        }
    }
}
