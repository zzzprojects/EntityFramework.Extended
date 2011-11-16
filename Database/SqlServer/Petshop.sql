SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Inventory](
	[ItemId] [varchar](10) NOT NULL,
	[Qty] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ItemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-1', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-10', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-11', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-12', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-13', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-14', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-15', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-16', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-17', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-18', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-19', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-2', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-20', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-21', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-22', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-23', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-24', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-25', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-26', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-27', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-28', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-3', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-4', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-5', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-6', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-7', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-8', 10000)
INSERT [dbo].[Inventory] ([ItemId], [Qty]) VALUES (N'EST-9', 10000)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[CatId] [varchar](10) NOT NULL,
	[Name] [varchar](80) NULL,
	[Descn] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[CatId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Category] ([CatId], [Name], [Descn]) VALUES (N'BIRDS', N'Birds', NULL)
INSERT [dbo].[Category] ([CatId], [Name], [Descn]) VALUES (N'CATS', N'Cats', NULL)
INSERT [dbo].[Category] ([CatId], [Name], [Descn]) VALUES (N'DOGS', N'Dogs', NULL)
INSERT [dbo].[Category] ([CatId], [Name], [Descn]) VALUES (N'FISH', N'Fish', NULL)
INSERT [dbo].[Category] ([CatId], [Name], [Descn]) VALUES (N'REPTILES', N'Reptiles', NULL)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BannerData](
	[FavCategory] [varchar](80) NOT NULL,
	[BannerData] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[FavCategory] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[BannerData] ([FavCategory], [BannerData]) VALUES (N'Birds', N'<img src="Images/bannerBirds.gif">')
INSERT [dbo].[BannerData] ([FavCategory], [BannerData]) VALUES (N'Cats', N'<img src="Images/bannerCats.gif">')
INSERT [dbo].[BannerData] ([FavCategory], [BannerData]) VALUES (N'Dogs', N'<img src="Images/bannerDogs.gif">')
INSERT [dbo].[BannerData] ([FavCategory], [BannerData]) VALUES (N'Fish', N'<img src="Images/bannerFish.gif">')
INSERT [dbo].[BannerData] ([FavCategory], [BannerData]) VALUES (N'Reptiles', N'<img src="Images/bannerReptiles.gif">')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[UserId] [varchar](20) NOT NULL,
	[Email] [varchar](80) NOT NULL,
	[FirstName] [varchar](80) NOT NULL,
	[LastName] [varchar](80) NOT NULL,
	[Status] [varchar](2) NULL,
	[Addr1] [varchar](80) NOT NULL,
	[Addr2] [varchar](80) NULL,
	[City] [varchar](80) NOT NULL,
	[State] [varchar](80) NOT NULL,
	[Zip] [varchar](20) NOT NULL,
	[Country] [varchar](20) NOT NULL,
	[Phone] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Account] ([UserId], [Email], [FirstName], [LastName], [Status], [Addr1], [Addr2], [City], [State], [Zip], [Country], [Phone]) VALUES (N'ACID', N'test@rollback.com', N'Distributed', N'Transaction', N'OK', N'PO Box 4482', N'', N'Carmel', N'CA', N'93921', N'USA', N'831-625-1861')
INSERT [dbo].[Account] ([UserId], [Email], [FirstName], [LastName], [Status], [Addr1], [Addr2], [City], [State], [Zip], [Country], [Phone]) VALUES (N'DotNet', N'yourname@yourdomain.com', N'ABC', N'XYX', N'OK', N'901 San Antonio Road', N'MS UCUP02-206', N'Palo Alto', N'CA', N'94303', N'USA', N'555-555-5555')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[OrderId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [varchar](20) NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[ShipAddr1] [varchar](80) NOT NULL,
	[ShipAddr2] [varchar](80) NULL,
	[ShipCity] [varchar](80) NOT NULL,
	[ShipState] [varchar](80) NOT NULL,
	[ShipZip] [varchar](20) NOT NULL,
	[ShipCountry] [varchar](20) NOT NULL,
	[BillAddr1] [varchar](80) NOT NULL,
	[BillAddr2] [varchar](80) NULL,
	[BillCity] [varchar](80) NOT NULL,
	[BillState] [varchar](80) NOT NULL,
	[BillZip] [varchar](20) NOT NULL,
	[BillCountry] [varchar](20) NOT NULL,
	[Courier] [varchar](80) NOT NULL,
	[TotalPrice] [decimal](10, 2) NOT NULL,
	[BillToFirstName] [varchar](80) NOT NULL,
	[BillToLastName] [varchar](80) NOT NULL,
	[ShipToFirstName] [varchar](80) NOT NULL,
	[ShipToLastName] [varchar](80) NOT NULL,
	[CreditCard] [varchar](20) NOT NULL,
	[ExprDate] [varchar](7) NOT NULL,
	[CardType] [varchar](40) NOT NULL,
	[Locale] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Supplier](
	[SuppId] [int] NOT NULL,
	[Name] [varchar](80) NULL,
	[Status] [varchar](2) NOT NULL,
	[Addr1] [varchar](80) NULL,
	[Addr2] [varchar](80) NULL,
	[City] [varchar](80) NULL,
	[State] [varchar](80) NULL,
	[Zip] [varchar](5) NULL,
	[Phone] [varchar](40) NULL,
PRIMARY KEY CLUSTERED 
(
	[SuppId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Supplier] ([SuppId], [Name], [Status], [Addr1], [Addr2], [City], [State], [Zip], [Phone]) VALUES (1, N'XYZ Pets', N'AC', N'600 Avon Way', N'', N'Los Angeles', N'CA', N'94024', N'212-947-0797')
INSERT [dbo].[Supplier] ([SuppId], [Name], [Status], [Addr1], [Addr2], [City], [State], [Zip], [Phone]) VALUES (2, N'ABC Pets', N'AC', N'700 Abalone Way', N'', N'San Francisco', N'CA', N'94024', N'415-947-0797')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SignOn](
	[UserName] [varchar](20) NOT NULL,
	[Password] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[SignOn] ([UserName], [Password]) VALUES (N'ACID', N'ACID')
INSERT [dbo].[SignOn] ([UserName], [Password]) VALUES (N'DotNet', N'DotNet')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Profile](
	[UserId] [varchar](20) NOT NULL,
	[LangPref] [varchar](80) NOT NULL,
	[FavCategory] [varchar](30) NULL,
	[MyListOpt] [int] NULL,
	[BannerOpt] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Profile] ([UserId], [LangPref], [FavCategory], [MyListOpt], [BannerOpt]) VALUES (N'ACID', N'English', N'Birds', 1, 1)
INSERT [dbo].[Profile] ([UserId], [LangPref], [FavCategory], [MyListOpt], [BannerOpt]) VALUES (N'DotNet', N'English', N'Dogs', 1, 1)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[ProductId] [varchar](10) NOT NULL,
	[Category] [varchar](10) NOT NULL,
	[Name] [varchar](80) NULL,
	[Descn] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IxProduct1] ON [dbo].[Product] 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IxProduct2] ON [dbo].[Product] 
(
	[Category] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IxProduct3] ON [dbo].[Product] 
(
	[Category] ASC,
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IxProduct4] ON [dbo].[Product] 
(
	[Category] ASC,
	[ProductId] ASC,
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'AV-CB-01', N'BIRDS', N'Amazon Parrot', N'<img align="absmiddle" src="Images/Pets/bird1.jpg">Great companion for up to 75 years')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'AV-SB-02', N'BIRDS', N'Finch', N'<img align="absmiddle" src="Images/Pets/bird2.jpg">Great stress reliever')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'FI-FW-01', N'FISH', N'Koi', N'<img align="absmiddle" src="Images/Pets/fish3.jpg">Freshwater fish from Japan')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'FI-FW-02', N'FISH', N'Goldfish', N'<img align="absmiddle" src="Images/Pets/fish4.jpg">Freshwater fish from China')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'FI-SW-01', N'FISH', N'Angelfish', N'<img align="absmiddle" src="Images/Pets/fish1.jpg">Saltwater fish from Australia')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'FI-SW-02', N'FISH', N'Tiger Shark', N'<img align="absmiddle" src="Images/Pets/fish2.jpg">Saltwater fish from Australia')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'FL-DLH-02', N'CATS', N'Persian', N'<img align="absmiddle" src="Images/Pets/cat2.jpg">Friendly house cat, doubles as a princess')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'FL-DSH-01', N'CATS', N'Manx', N'<img align="absmiddle" src="Images/Pets/cat1.jpg">Great for reducing mouse populations')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'K9-BD-01', N'DOGS', N'Bulldog', N'<img align="absmiddle" src="Images/Pets/dog1.jpg">Friendly dog from England')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'K9-CW-01', N'DOGS', N'Chihuahua', N'<img align="absmiddle" src="Images/Pets/dog6.jpg">Great companion dog')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'K9-DL-01', N'DOGS', N'Dalmation', N'<img align="absmiddle" src="Images/Pets/dog3.jpg">Great dog for a fire station')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'K9-PO-02', N'DOGS', N'Poodle', N'<img align="absmiddle" src="Images/Pets/dog2.jpg">Cute dog from France')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'K9-RT-01', N'DOGS', N'Golden Retriever', N'<img align="absmiddle" src="Images/Pets/dog4.jpg">Great family dog')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'K9-RT-02', N'DOGS', N'Labrador Retriever', N'<img align="absmiddle" src="Images/Pets/dog5.jpg">Great hunting dog')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'RP-LI-02', N'REPTILES', N'Iguana', N'<img align="absmiddle" src="Images/Pets/reptile2.jpg">Friendly green friend')
INSERT [dbo].[Product] ([ProductId], [Category], [Name], [Descn]) VALUES (N'RP-SN-01', N'REPTILES', N'Rattlesnake', N'<img align="absmiddle" src="Images/Pets/reptile1.jpg">Doubles as a watch dog')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderStatus](
	[OrderId] [int] NOT NULL,
	[LineNum] [int] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[Status] [varchar](2) NOT NULL,
 CONSTRAINT [PkOrderStatus] PRIMARY KEY CLUSTERED 
(
	[OrderId] ASC,
	[LineNum] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LineItem](
	[OrderId] [int] NOT NULL,
	[LineNum] [int] NOT NULL,
	[ItemId] [varchar](10) NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](10, 2) NOT NULL,
 CONSTRAINT [PkLineItem] PRIMARY KEY CLUSTERED 
(
	[OrderId] ASC,
	[LineNum] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Item](
	[ItemId] [varchar](10) NOT NULL,
	[ProductId] [varchar](10) NOT NULL,
	[ListPrice] [decimal](10, 2) NULL,
	[UnitCost] [decimal](10, 2) NULL,
	[Supplier] [int] NULL,
	[Status] [varchar](2) NULL,
	[Attr1] [varchar](80) NULL,
	[Attr2] [varchar](80) NULL,
	[Attr3] [varchar](80) NULL,
	[Attr4] [varchar](80) NULL,
	[Attr5] [varchar](80) NULL,
PRIMARY KEY CLUSTERED 
(
	[ItemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IxItem] ON [dbo].[Item] 
(
	[ProductId] ASC,
	[ItemId] ASC,
	[ListPrice] ASC,
	[Attr1] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-1', N'FI-SW-01', CAST(16.50 AS Decimal(10, 2)), CAST(10.00 AS Decimal(10, 2)), 1, N'P', N'Large', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-10', N'K9-DL-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Spotted Adult Female', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-11', N'RP-SN-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Venomless', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-12', N'RP-SN-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Rattleless', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-13', N'RP-LI-02', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Green Adult', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-14', N'FL-DSH-01', CAST(58.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Tailless', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-15', N'FL-DSH-01', CAST(23.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Tailed', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-16', N'FL-DLH-02', CAST(93.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Adult Female', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-17', N'FL-DLH-02', CAST(93.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Adult Male', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-18', N'AV-CB-01', CAST(193.50 AS Decimal(10, 2)), CAST(92.00 AS Decimal(10, 2)), 1, N'P', N'Adult Male', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-19', N'AV-SB-02', CAST(15.50 AS Decimal(10, 2)), CAST(2.00 AS Decimal(10, 2)), 1, N'P', N'Adult Male', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-2', N'FI-SW-01', CAST(16.50 AS Decimal(10, 2)), CAST(10.00 AS Decimal(10, 2)), 1, N'P', N'Small', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-20', N'FI-FW-02', CAST(5.50 AS Decimal(10, 2)), CAST(2.00 AS Decimal(10, 2)), 1, N'P', N'Adult Male', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-21', N'FI-FW-02', CAST(5.29 AS Decimal(10, 2)), CAST(1.00 AS Decimal(10, 2)), 1, N'P', N'Adult Female', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-22', N'K9-RT-02', CAST(135.50 AS Decimal(10, 2)), CAST(100.00 AS Decimal(10, 2)), 1, N'P', N'Adult Male', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-23', N'K9-RT-02', CAST(145.49 AS Decimal(10, 2)), CAST(100.00 AS Decimal(10, 2)), 1, N'P', N'Adult Female', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-24', N'K9-RT-02', CAST(255.50 AS Decimal(10, 2)), CAST(92.00 AS Decimal(10, 2)), 1, N'P', N'Adult Male', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-25', N'K9-RT-02', CAST(325.29 AS Decimal(10, 2)), CAST(90.00 AS Decimal(10, 2)), 1, N'P', N'Adult Female', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-26', N'K9-CW-01', CAST(125.50 AS Decimal(10, 2)), CAST(92.00 AS Decimal(10, 2)), 1, N'P', N'Adult Male', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-27', N'K9-CW-01', CAST(155.29 AS Decimal(10, 2)), CAST(90.00 AS Decimal(10, 2)), 1, N'P', N'Adult Female', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-28', N'K9-RT-01', CAST(155.29 AS Decimal(10, 2)), CAST(90.00 AS Decimal(10, 2)), 1, N'P', N'Adult Female', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-3', N'FI-SW-02', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Toothless', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-4', N'FI-FW-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Spotted', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-5', N'FI-FW-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Spotless', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-6', N'K9-BD-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Male Adult', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-7', N'K9-BD-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Female Puppy', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-8', N'K9-PO-02', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Male Puppy', NULL, NULL, NULL, NULL)
INSERT [dbo].[Item] ([ItemId], [ProductId], [ListPrice], [UnitCost], [Supplier], [Status], [Attr1], [Attr2], [Attr3], [Attr4], [Attr5]) VALUES (N'EST-9', N'K9-DL-01', CAST(18.50 AS Decimal(10, 2)), CAST(12.00 AS Decimal(10, 2)), 1, N'P', N'Spotless Male Puppy', NULL, NULL, NULL, NULL)
ALTER TABLE [dbo].[Item]  WITH CHECK ADD FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([ProductId])
GO
ALTER TABLE [dbo].[Item]  WITH CHECK ADD FOREIGN KEY([Supplier])
REFERENCES [dbo].[Supplier] ([SuppId])
GO
ALTER TABLE [dbo].[LineItem]  WITH CHECK ADD FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([OrderId])
GO
ALTER TABLE [dbo].[OrderStatus]  WITH CHECK ADD FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([OrderId])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([Category])
REFERENCES [dbo].[Category] ([CatId])
GO
