select * from Subscriptions s limit 10;





select count(*) from Subscriptions s;





select * from Channels c where c.Title='Gabe J';







-- All subscriptions for user
select 
c.Title as BeingSubscribedToChannelTitle,
c2.Title as SubscriberChannelTitle
from Subscriptions s
inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
inner join Channels c2 on c2.ChannelID=s.SubscriberChannelID
where c2.Title='Collier Mayo'
order by BeingSubscribedToChannelTitle;







-- Count of subscriptions for user
select 
count(*)
from Subscriptions s
inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
inner join Channels c2 on c2.ChannelID=s.SubscriberChannelID
where c2.Title='Ethan Cabbage';




-- All users subscribed to channel
select
c.Title,
c2.Title
from Subscriptions s
inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
inner join Channels c2 on c2.ChannelID=s.SubscriberChannelID
where c.Title='Evergreen Smash';

























