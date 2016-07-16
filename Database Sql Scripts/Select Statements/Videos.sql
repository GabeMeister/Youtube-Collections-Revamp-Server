select * from Videos limit 100;

select count(*) from Videos;

-- delete from Videos;

select * from Videos where youtubeid='w_Pplr0hI1s';


select 
c.Title,
c.ChannelID
from Videos v
inner join Channels c on c.ChannelID=v.ChannelID
group by c.ChannelID
order by c.ChannelID;