select * from Collections;
select * from CollectionItems;


select count(*) from Collections;
select count(*) from CollectionItems;


-- delete from Collections;
-- delete from CollectionItems;


select * from Channels where title='Gabe J'; -- 58173
select * from Channels where title like '%ATHLEAN-X%'; -- 9833


select * from Collections co
inner join Channels ch on ch.ChannelID=co.OwnerChannelID;


select count(*) from Collections co
inner join Channels ch on ch.ChannelID=co.OwnerChannelID
where ch.Title='Gabe J';


select
ci.CollectionItemID,
channelItem.ChannelID,
channelItem.Title,
ownerChannel.ChannelID,
ownerChannel.Title,
co.Title
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ownerChannel on ownerChannel.ChannelID=co.OwnerChannelID
inner join Channels channelItem on channelItem.ChannelID=ci.ItemChannelID
where ownerChannel.ChannelID=58176
and channelItem.ChannelID=9833;