-- select 
-- v.title,v.publishedat,c.title,c.channelid
-- from videos v 
-- inner join channels c 
-- on c.channelid=v.channelid 
-- -- where c.YoutubeID='UC8-Th83bH_thdKZDJCrn88g'
-- where c.ChannelID=76;


select 
c.Title,c.YoutubeID,c.ChannelID,count(*)
from channels c
inner join videos v
on c.channelid=v.channelid 
-- where c.YoutubeID='UC8-Th83bH_thdKZDJCrn88g'
group by c.ChannelID
order by count(*) desc;

-- select 
-- c.Title,c.ChannelID,v.Title
-- from channels c
-- inner join videos v
-- on c.channelid=v.channelid 
-- -- where c.YoutubeID='UC8-Th83bH_thdKZDJCrn88g'
-- where c.ChannelID=117;


select * from channels limit 5;