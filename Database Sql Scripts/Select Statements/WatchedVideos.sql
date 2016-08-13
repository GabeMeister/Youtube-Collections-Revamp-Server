select * from WatchedVideos;





select count(*) from WatchedVideos;





-- delete from WatchedVideos;





-- All watched videos sorted by users
select c2.Title, v.Title, c.Title, wv.DateViewed
from WatchedVideos wv
inner join Videos v on v.VideoID=wv.VideoID
inner join Channels c2 on c2.ChannelID=wv.ChannelID
inner join Channels c on c.ChannelID=v.ChannelID
order by c2.Title,c.Title,wv.DateViewed;





-- All watched videos for particular user
select c2.Title, v.Title, c.Title, wv.DateViewed
from WatchedVideos wv
inner join Videos v on v.VideoID=wv.VideoID
inner join Channels c2 on c2.ChannelID=wv.ChannelID
inner join Channels c on c.ChannelID=v.ChannelID
where c2.Title='Gabe J'
order by c2.Title,c.Title,wv.DateViewed;

































