create table Videos (
	VideoID SERIAL PRIMARY KEY,
	YoutubeID CHAR(15) NOT NULL,
	ChannelID INT NOT NULL references Channels(ChannelID),
	Title CHAR(150) NOT NULL,
	Thumbnail CHAR(100),
	Duration CHAR(15),
	ViewCount INT,
	PublishedAt TIMESTAMP
);

create table WatchedVideos (
	WatchedVideoID SERIAL PRIMARY KEY,
	ChannelID INT REFERENCES Channels(ChannelID) NOT NULL,
	VideoID INT REFERENCES Videos(VideoID) NOT NULL,
	DateViewed TIMESTAMP
);

create table Channels (
	ChannelID SERIAL PRIMARY KEY,
	YoutubeID CHAR(30) NOT NULL,
	Title CHAR(150) NOT NULL,
	Description CHAR(1000),
	UploadPlaylist CHAR(30),
	Thumbnail CHAR(100),
	ViewCount BIGINT,
	SubscriberCount BIGINT,
	VideoCount BIGINT
);

create table Subscriptions (
	SubscriberChannelID INT NOT NULL references Channels(ChannelID),
	BeingSubscribedToChannelID INT NOT NULL references Channels(ChannelID)
);

create table Collections (
	CollectionID SERIAL PRIMARY KEY,
	OwnerChannelID INT NOT NULL REFERENCES Channels(ChannelID),
	Title CHAR(50) NOT NULL
);

create table CollectionItems (
	CollectionItemID SERIAL PRIMARY KEY,
	CollectionID INT NOT NULL REFERENCES Collections(CollectionID) ON DELETE CASCADE,
	ItemChannelID INT NOT NULL REFERENCES Channels(ChannelID) ON DELETE CASCADE
);



