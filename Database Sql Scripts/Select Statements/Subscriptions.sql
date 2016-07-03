select 
* 
from Subscriptions s
inner join Channels c on c.ChannelID=s.BeingSubscribedToChannelID
where s.SubscriberChannelID=114;