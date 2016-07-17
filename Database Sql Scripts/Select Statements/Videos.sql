select * from Videos limit 10;

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


update Videos set Title='Atomtot - Abschaum',
Thumbnail='https://i.ytimg.com/vi/k7hBcVg6-Cs/mqdefault.jpg',
Duration='00:02:38',
ViewCount=195
where VideoID=1431468;