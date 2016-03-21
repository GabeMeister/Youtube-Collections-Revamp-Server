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


-- Show corresponding channels and collections for all collection items
select ci.CollectionItemId, ch.Title, co.Title, ci.ItemChannelId 
from CollectionItems ci
inner join Collections co on co.CollectionID=ci.CollectionID
inner join Channels ch on ch.ChannelID=co.OwnerChannelID;



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










