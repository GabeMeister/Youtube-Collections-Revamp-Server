
-- insert into WatchedVideos (ChannelID, VideoID, DateViewed) values (57810, 1124758, '2016-04-09 19:30:12');
-- select * from WatchedVideos;

select c2.Title, v.Title, c.Title, wv.DateViewed
from WatchedVideos wv
inner join Videos v on v.VideoID=wv.VideoID
inner join Channels c2 on c2.ChannelID=wv.ChannelID
inner join Channels c on c.ChannelID=v.ChannelID;

-- select * from channels where title='DopeRelease';