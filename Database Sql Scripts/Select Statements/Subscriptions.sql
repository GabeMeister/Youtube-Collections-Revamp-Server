select * from Subscriptions s;


select count(*) from Subscriptions s;


-- delete from Subscriptions s;


select * from Channels c where c.Title='Gabe J'; -- 58173
select * from channels where title like '%ATHLEAN-X%'; -- 9833


select 
c.YoutubeID,
c.Title
from Subscriptions s
inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
inner join Channels c2 on c2.ChannelID=s.SubscriberChannelID
where c2.Title='Gabe J'
order by c.title;


select 
count(*)
from Subscriptions s
inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
inner join Channels c2 on c2.ChannelID=s.SubscriberChannelID
where c2.Title='Gabe J';


select 
c.Title as BeingSubscribedToChannelTitle,
c2.Title as SubscriberChannelTitle
from Subscriptions s
inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
inner join Channels c2 on c2.ChannelID=s.SubscriberChannelID
where c2.Title='Gabe J'
order by BeingSubscribedToChannelTitle;



insert into Subscriptions (SubscriberChannelID, BeingSubscribedToChannelID) values (58173, 9833);
delete from Subscriptions where SubscriberChannelID=58173 and BeingSubscribedToChannelID=9833;



