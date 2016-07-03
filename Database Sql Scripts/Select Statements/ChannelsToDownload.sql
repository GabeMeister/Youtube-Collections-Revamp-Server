insert into ChannelsToDownload (channelid) values (23);

select * from ChannelsToDownload;

select count(*) from ChannelsToDownload;

delete from ChannelsToDownload;

select * from channels where channelid='30338';
insert into ChannelsToDownload (ChannelID) values (30338);

select c.YoutubeID 
                                            from ChannelsToDownload ctd
                                            inner join Channels c 
                                            on c.ChannelID=ctd.ChannelID
                                            where c.YoutubeID in (''UCe0TLA0EsQbE-MjuHXevj2A'',''UCbpMy0Fg74eXXkvxJrtEn3w'',''UCKf0UqBiCQI4Ol0To9V0pKQ'',''UCEOXxzW2vU0P-0THehuIIeg'',''UCjIsp4yW--Avg5qnd-yP45Q'',''UCSy0eOtiWe6TOQS8y6PpgaQ'',''UC3I2GFN_F8WudD_2jUZbojA'',''UCCb9_Kn8F_Opb3UCGm-lILQ'',''UCkUTBwZKwA9ojYqzj6VRlMQ'',''UCBsBn98N5Gmm4-9FB6_fl9A'',''UCji0SCC4OOUnpWkC9HP-4Gw'',''UCFFYKAZDmU_y7z1flmIayAA'',''UCqOecsBLULnuUtls0tISTCw'');




