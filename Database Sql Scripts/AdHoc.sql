-- select VideoID, YoutubeID from videos where youtubeid='uhOBd34q2kM';
select VideoID, YoutubeID from videos where VideoID=143518;
select title from channels offset 557;


select ChannelID,YoutubeID,Title from Channels limit 100;

select * from channels where Title='MrSuicideSheep';
select * from channels where channelid=1008;

select count(*) from channels;
select count(*) from subscriptions;
select count(*) from videos;


select * from channels where title like '%Jimmy Fallon%';


select * from videos where youtubeid='Pkm2MlXHo5A';

select * from Collections;
select * from CollectionItems;
-- select * from Channels limit 5;


select subscriberchannelid, count(*) from subscriptions group by subscriberchannelid order by count(*) desc;

delete from CollectionItems where CollectionID=3 and ItemChannelID=101;
delete from Collections where CollectionID=2 cascade;