select * from Collections limit 10;
select * from CollectionItems limit 10;




select count(*) from Collections;
select count(*) from CollectionItems;




-- delete from CollectionItems;
-- delete from Collections;




select * from Channels where title='Gabe J'; -- 58173





-- Number of collections for user
select 
count(*) 
from Collections co
inner join Channels ch on ch.ChannelID=co.OwnerChannelID
where ch.Title like '%Ethan Cabbage%';







-- All Collection Items
select
channelItem.ChannelID,
channelItem.Title,
ownerChannel.Title,
ownerChannel.YoutubeID,
co.Title
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ownerChannel on ownerChannel.ChannelID=co.OwnerChannelID
inner join Channels channelItem on channelItem.ChannelID=ci.ItemChannelID
order by ownerChannel.Title,channelItem.Title;







-- All Collections w/ Channel Info
select ch.YoutubeID,ch.Title,coll.Title 
from Collections coll
inner join Channels ch on ch.ChannelID = coll.OwnerChannelID
order by ch.Title,coll.Title;







-- Collections for particular user
select 
ch.ChannelID,ch.Title,coll.Title
from Channels ch
inner join Collections coll on ch.ChannelID=coll.OwnerChannelID
where ch.Title='Gabe J'
order by ch.Title,coll.Title;







-- Show collection items for specific user
select ci.CollectionItemId, ch.ChannelID, ch.Title, co.Title, ci.ItemChannelId 
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ch on ch.ChannelID=co.OwnerChannelID
where ch.Title='Gabe J';







-- Show all collection items
select ch2.Title,ch.Title,co.Title,ci.CollectionItemId
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ch on ch.ChannelID=co.OwnerChannelID
inner join Channels ch2 on ch2.ChannelID=ci.ItemChannelID
order by ch.Title;







-- Count of collection items in collection
select count(*)
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ch on ch.ChannelID=co.OwnerChannelID;







select 
c2.Title,v.Title,v.VideoID
from Channels c
inner join Subscriptions s on s.SubscriberChannelID=c.ChannelID
inner join Channels c2 on s.BeingSubscribedTochannelID=c2.ChannelID
inner join Videos v on v.ChannelID=c2.ChannelID
where 
c.Title='Gabe J'  -- Gabe J's channel
and c2.ChannelID=91 -- The channel being subscribed to
and v.VideoID not in
(
select VideoId from WatchedVideos
where ChannelID=57810
)
order by v.PublishedAt desc
limit 5;







select * from CollectionItems ci
inner join Channels ch on ch.ChannelID=ci.ItemChannelID;







select ItemChannelID from CollectionItems
group by ItemChannelID
order by ItemChannelID;


