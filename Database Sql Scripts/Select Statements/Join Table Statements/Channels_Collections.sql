select 
ch.ChannelID,ch.YoutubeID,ch.Title,coll.CollectionID,coll.Title
from Channels ch
inner join Collections coll
on ch.ChannelID=coll.OwnerChannelID
where ch.YoutubeID='UC4LVLoBN0xbOb5xJuA0ia9A';


-- select * from Collections;
-- 
-- select * from Channels where ChannelID=57781;

-- TODO
delete from collections where ownerchannelid=57781;