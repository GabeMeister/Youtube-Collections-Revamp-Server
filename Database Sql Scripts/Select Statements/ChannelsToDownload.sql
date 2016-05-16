insert into ChannelsToDownload (channelid) values (23);

select * from ChannelsToDownload;

select count(*) from ChannelsToDownload;

delete from ChannelsToDownload;

select * from channels where channelid='30338';
insert into ChannelsToDownload (ChannelID) values (30338);

select ctd.*, c.Title, c.YoutubeID 
from ChannelsToDownload ctd
inner join Channels c 
on c.ChannelID=ctd.ChannelID
where c.YoutubeID in ('UC3I2GFN_F8WudD_2jUZbojA', 'UCji0SCC4OOUnpWkC9HP-4Gw');







