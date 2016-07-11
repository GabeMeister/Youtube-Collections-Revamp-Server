select * from WatchedVideos;


select count(*) from WatchedVideos;


delete from WatchedVideos;


select c2.Title, v.YoutubeID, v.Title, c.Title, wv.DateViewed
from WatchedVideos wv
inner join Videos v on v.VideoID=wv.VideoID
inner join Channels c2 on c2.ChannelID=wv.ChannelID
inner join Channels c on c.ChannelID=v.ChannelID;


select c2.ChannelID, c2.Title, v.YoutubeID, v.Title, c.Title, wv.DateViewed
from WatchedVideos wv
inner join Videos v on v.VideoID=wv.VideoID
inner join Channels c2 on c2.ChannelID=wv.ChannelID
inner join Channels c on c.ChannelID=v.ChannelID
where v.YoutubeID = 'WXePdL_XlpE'
and c2.ChannelID=57810;



