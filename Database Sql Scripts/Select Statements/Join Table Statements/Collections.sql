-- All Collections showing channels with it
select coll.CollectionID,coll.OwnerChannelID,ch.Title,coll.Title 
from Collections coll
inner join Channels ch on ch.ChannelID = coll.OwnerChannelID;


-- Particular collection for user
select coll.CollectionID 
from Collections coll
inner join Channels ch on ch.ChannelID = coll.OwnerChannelID
where coll.OwnerChannelID=57792
and coll.Title='Late Night';


-- Show collection items for specific user
select ci.CollectionItemId, ch.ChannelID, ch.Title, co.Title, ci.ItemChannelId 
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ch on ch.ChannelID=co.OwnerChannelID
where ch.ChannelID=58173;



-- Show all collection items for user specified collection
select ch.Title,co.Title,ci.CollectionItemId
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ch on ch.ChannelID=co.OwnerChannelID
where ch.Title='Gabe J'
and co.Title='Electronic';


-- Count of collection items in collection
select count(*)
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ch on ch.ChannelID=co.OwnerChannelID
where ch.Title='Gabe J'
and co.Title='Electronic';







delete from CollectionItems ci
where ci.CollectionItemID in
(
	select ci.CollectionItemId
	from CollectionItems ci
	inner join Collections co on co.CollectionID=ci.CollectionID
	inner join Channels ch on ch.ChannelID=co.OwnerChannelID
	where ch.ChannelID=58173
);

