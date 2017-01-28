select * from ChannelsToDownload limit 10;







select count(*) from ChannelsToDownload;






-- delete from ChannelsToDownload


select * from channels where channelid='30338';


select 
ch.Title,
ch.YoutubeID
from ChannelstoDownload ctd
inner join Channels ch on ch.ChannelID=ctd.ChannelID
order by ch.Title;


insert into ChannelsToDownload (ChannelID) values (30338);

select * from Channels where CHannelID=62736;




