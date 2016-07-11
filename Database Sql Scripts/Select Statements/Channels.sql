select * from Channels limit 10;


select count(*) from Channels;


delete from Channels;


select * from Channels
where Title='Gabe J';


select 
ch.ChannelID,ch.YoutubeID,ch.Title,coll.CollectionID,coll.Title
from Channels ch
inner join Collections coll
on ch.ChannelID=coll.OwnerChannelID
where ch.YoutubeID='UC4LVLoBN0xbOb5xJuA0ia9A';


select 
v.title,v.publishedat,c.title,c.channelid
from videos v 
inner join channels c 
on c.channelid=v.channelid 
-- where c.YoutubeID='UC8-Th83bH_thdKZDJCrn88g'
where c.ChannelID=76;


select 
c.Title,c.YoutubeID,c.ChannelID,count(*)
from channels c
inner join videos v
on c.channelid=v.channelid 
-- where c.YoutubeID='UC8-Th83bH_thdKZDJCrn88g'
group by c.ChannelID
order by count(*) desc;



















