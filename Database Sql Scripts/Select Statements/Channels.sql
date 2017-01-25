select * from Channels limit 10;







select count(*) from Channels;








delete from Channels;







select * from Channels
where Title like '%Red Bull%';







select 
* 
from Channels c
where c.ChannelID=60232;








select * from Channels
where Title like '%Collier Mayo%';








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






select 
ChannelID, Title,VideoCount
from Channels 
where viewcount=videocount 
and videoCount != 0
order by videocount desc;






select ChannelID,VideoCount from Channels where ChannelID = ANY('{62830,62831,62832,62833,62834,62835,62836,16026,60844,38099}')













