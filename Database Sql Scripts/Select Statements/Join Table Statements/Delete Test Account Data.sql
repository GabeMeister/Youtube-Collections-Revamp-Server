-- Get the dummy channel id
select * from channels where youtubeid='UCbWFwb-TieRunM1l3-v5GuA';

-- Delete dummy channel subscriptions
delete from subscriptions where subscriberchannelid=57779;

-- Delete dummy channel
delete from channels where youtubeid='UCbWFwb-TieRunM1l3-v5GuA';









-- Get my channel id
select * from channels where youtubeid='UC4LVLoBN0xbOb5xJuA0ia9A';

-- Delete my channel subscriptions
delete from subscriptions where subscriberchannelid=117;

-- Delete my channel uploads
delete from videos v
where v.channelid in
(select channelid from channels where youtubeid='UC4LVLoBN0xbOb5xJuA0ia9A');

-- Delete any subscriptions to my channel
delete from subscriptions s
where s.beingsubscribedtochannelid in 
(select channelid from channels where youtubeid='UC4LVLoBN0xbOb5xJuA0ia9A');

-- Delete any collections that channel has
delete from collections where ownerchannelid in
(select channelid from channels where youtubeid='UC4LVLoBN0xbOb5xJuA0ia9A');

-- Delete my channel
delete from channels where youtubeid='UC4LVLoBN0xbOb5xJuA0ia9A';







