select * from Videos limit 10;





select count(*) from Videos;




create index video_index_by_channel_id on Videos(channelid);




-- before: 4000126





-- delete from Videos;





select * from Videos where youtubeid='Q-NvghFjsFI' limit 1;





select 
c.Title,
c.ChannelID
from Videos v
inner join Channels c on c.ChannelID=v.ChannelID
group by c.ChannelID
order by c.ChannelID;





select v.Title,v.PublishedAt from Videos v
inner join Channels ch on ch.ChannelID=v.ChannelID
where ch.Title like '%Secular Talk%'
order by v.PublishedAt desc;






select * from videos limit 1;







update Videos set Title='Atomtot - Abschaum',
Thumbnail='https://i.ytimg.com/vi/k7hBcVg6-Cs/mqdefault.jpg',
Duration='00:02:38',
ViewCount=195
where VideoID=1431468;











