-- select 
-- c.Title,c.YoutubeID,c.ChannelID,count(*)
-- from channels c
-- inner join videos v
-- on c.channelid=v.channelid 
-- group by c.ChannelID
-- order by count(*) desc;



select 
c2.Title,v.Title,v.VideoID
from Channels c
inner join Subscriptions s on s.SubscriberChannelID=c.ChannelID
inner join Channels c2 on s.BeingSubscribedTochannelID=c2.ChannelID
inner join Videos v on v.ChannelID=c2.ChannelID
where 
c.ChannelID=57810
and c2.ChannelID=88
and v.VideoID not in
(
select VideoId from WatchedVideos
where ChannelID=57810
)
order by v.PublishedAt desc
limit 5;

-- select * from Subscriptions;

select * from channels c where c.title like '%Madeon%';
-- 
-- select * from collections;
-- 
-- select * from CollectionItems;