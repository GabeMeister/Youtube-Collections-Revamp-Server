select 
ch.ChannelID,ch.YoutubeID,ch.Title,coll.CollectionID,coll.Title
from Channels ch
inner join Collections coll
on ch.ChannelID=coll.OwnerChannelID
where ch.YoutubeID='UC4LVLoBN0xbOb5xJuA0ia9A';

select * from Collections;
select * from CollectionItems;
delete from CollectionItems;

-- select * from Channels where ChannelID=57781;

-- TODO
delete from collections where ownerchannelid=57781;


select count(*) from CollectionItems where CollectionID=87 and ItemChannelID=7;
delete from CollectionItems where CollectionID=7 and ItemChannelID=63;

insert into Channels (YoutubeID,Title,Description,UploadPlaylist,Thumbnail,ViewCount,SubscriberCount,VideoCount) values ('UC4LVLoBN0xbOb5xJuA0ia9A','Gabe J','','UU4LVLoBN0xbOb5xJuA0ia9A','https://yt3.ggpht.com/-Wke8c6qgqps/AAAAAAAAAAI/AAAAAAAAAAA/QB25nYoHmyI/s240-c-k-no-rj-c0xffffff/photo.jpg',748,4,6);

alter table Channels alter column Thumbnail type character(150);