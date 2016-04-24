
-- insert into WatchedVideos (ChannelID, VideoID, DateViewed) values (57810, 1124758, '2016-04-09 19:30:12');
-- select * from WatchedVideos;

-- select * from WatchedVideos where ChannelID=57810 and VideoID not in (30272, 153309);

-- select c2.Title, v.YoutubeID, v.Title, c.Title, wv.DateViewed
-- from WatchedVideos wv
-- inner join Videos v on v.VideoID=wv.VideoID
-- inner join Channels c2 on c2.ChannelID=wv.ChannelID
-- inner join Channels c on c.ChannelID=v.ChannelID;



select c2.ChannelID, c2.Title, v.YoutubeID, v.Title, c.Title, wv.DateViewed
from WatchedVideos wv
inner join Videos v on v.VideoID=wv.VideoID
inner join Channels c2 on c2.ChannelID=wv.ChannelID
inner join Channels c on c.ChannelID=v.ChannelID
where v.YoutubeID = 'WXePdL_XlpE'
and c2.ChannelID=57810;



-- select * from channels where title='DopeRelease';

-- select count(*) from WatchedVideos;

