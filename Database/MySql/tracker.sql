SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `tracker`
--

-- --------------------------------------------------------

--
-- Table structure for table `audit`
--

DROP TABLE IF EXISTS `audit`;
CREATE TABLE `audit` (
  `Id` int(11) NOT NULL,
  `Date` datetime NOT NULL,
  `UserId` int(11) DEFAULT NULL,
  `TaskId` int(11) DEFAULT NULL,
  `Content` longtext NOT NULL,
  `Username` varchar(50) NOT NULL,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RowVersion` binary(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `item`
--

DROP TABLE IF EXISTS `item`;
CREATE TABLE `item` (
  `ItemId` varchar(10) NOT NULL,
  `ProductId` varchar(10) NOT NULL,
  `ListPrice` decimal(10,2) DEFAULT NULL,
  `UnitCost` decimal(10,2) DEFAULT NULL,
  `Supplier` int(11) DEFAULT NULL,
  `Status` varchar(2) DEFAULT NULL,
  `Attr1` varchar(80) DEFAULT NULL,
  `Attr2` varchar(80) DEFAULT NULL,
  `Attr3` varchar(80) DEFAULT NULL,
  `Attr4` varchar(80) DEFAULT NULL,
  `Attr5` varchar(80) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `item`
--

INSERT INTO `item` (`ItemId`, `ProductId`, `ListPrice`, `UnitCost`, `Supplier`, `Status`, `Attr1`, `Attr2`, `Attr3`, `Attr4`, `Attr5`) VALUES
('EST-1', 'FI-SW-01', '16.50', '10.00', 1, 'P', 'Large', NULL, NULL, NULL, NULL),
('EST-10', 'K9-DL-01', '18.50', '12.00', 1, 'P', 'Spotted Adult Female', NULL, NULL, NULL, NULL),
('EST-11', 'RP-SN-01', '18.50', '12.00', 1, 'P', 'Venomless', NULL, NULL, NULL, NULL),
('EST-12', 'RP-SN-01', '18.50', '12.00', 1, 'P', 'Rattleless', NULL, NULL, NULL, NULL),
('EST-13', 'RP-LI-02', '18.50', '12.00', 1, 'P', 'Green Adult', NULL, NULL, NULL, NULL),
('EST-14', 'FL-DSH-01', '58.50', '12.00', 1, 'P', 'Tailless', NULL, NULL, NULL, NULL),
('EST-15', 'FL-DSH-01', '23.50', '12.00', 1, 'P', 'Tailed', NULL, NULL, NULL, NULL),
('EST-16', 'FL-DLH-02', '93.50', '12.00', 1, 'P', 'Adult Female', NULL, NULL, NULL, NULL),
('EST-17', 'FL-DLH-02', '93.50', '12.00', 1, 'P', 'Adult Male', NULL, NULL, NULL, NULL),
('EST-18', 'AV-CB-01', '193.50', '92.00', 1, 'P', 'Adult Male', NULL, NULL, NULL, NULL),
('EST-19', 'AV-SB-02', '15.50', '2.00', 1, 'P', 'Adult Male', NULL, NULL, NULL, NULL),
('EST-2', 'FI-SW-01', '16.50', '10.00', 1, 'P', 'Small', NULL, NULL, NULL, NULL),
('EST-20', 'FI-FW-02', '5.50', '2.00', 1, 'P', 'Adult Male', NULL, NULL, NULL, NULL),
('EST-21', 'FI-FW-02', '5.29', '1.00', 1, 'P', 'Adult Female', NULL, NULL, NULL, NULL),
('EST-22', 'K9-RT-02', '135.50', '100.00', 1, 'P', 'Adult Male', NULL, NULL, NULL, NULL),
('EST-23', 'K9-RT-02', '145.49', '100.00', 1, 'P', 'Adult Female', NULL, NULL, NULL, NULL),
('EST-24', 'K9-RT-02', '255.50', '92.00', 1, 'P', 'Adult Male', NULL, NULL, NULL, NULL),
('EST-25', 'K9-RT-02', '325.29', '90.00', 1, 'P', 'Adult Female', NULL, NULL, NULL, NULL),
('EST-26', 'K9-CW-01', '125.50', '92.00', 1, 'P', 'Adult Male', NULL, NULL, NULL, NULL),
('EST-27', 'K9-CW-01', '155.29', '90.00', 1, 'P', 'Adult Female', NULL, NULL, NULL, NULL),
('EST-28', 'K9-RT-01', '155.29', '90.00', 1, 'P', 'Adult Female', NULL, NULL, NULL, NULL),
('EST-3', 'FI-SW-02', '18.50', '12.00', 1, 'P', 'Toothless', NULL, NULL, NULL, NULL),
('EST-4', 'FI-FW-01', '18.50', '12.00', 1, 'P', 'Spotted', NULL, NULL, NULL, NULL),
('EST-5', 'FI-FW-01', '18.50', '12.00', 1, 'P', 'Spotless', NULL, NULL, NULL, NULL),
('EST-6', 'K9-BD-01', '18.50', '12.00', 1, 'P', 'Male Adult', NULL, NULL, NULL, NULL),
('EST-7', 'K9-BD-01', '18.50', '12.00', 1, 'P', 'Female Puppy', NULL, NULL, NULL, NULL),
('EST-8', 'K9-PO-02', '18.50', '12.00', 1, 'P', 'Male Puppy', NULL, NULL, NULL, NULL),
('EST-9', 'K9-DL-01', '18.50', '12.00', 1, 'P', 'Spotless Male Puppy', NULL, NULL, NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `item 2`
--

DROP TABLE IF EXISTS `item 2`;
CREATE TABLE `item 2` (
  `ItemId` varchar(10) NOT NULL,
  `ProductId` varchar(10) NOT NULL,
  `ListPrice` decimal(10,2) DEFAULT NULL,
  `UnitCost` decimal(10,2) DEFAULT NULL,
  `Supplier` int(11) DEFAULT NULL,
  `Status` varchar(2) DEFAULT NULL,
  `Attr1` varchar(80) DEFAULT NULL,
  `Attr2` varchar(80) DEFAULT NULL,
  `Attr3` varchar(80) DEFAULT NULL,
  `Attr4` varchar(80) DEFAULT NULL,
  `Attr5` varchar(80) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `priority`
--

DROP TABLE IF EXISTS `priority`;
CREATE TABLE `priority` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Order` int(11) NOT NULL,
  `Description` varchar(200) DEFAULT NULL,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RowVersion` binary(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `priority`
--

INSERT INTO `priority` (`Id`, `Name`, `Order`, `Description`, `CreatedDate`, `ModifiedDate`, `RowVersion`) VALUES
(1, 'High', 1, 'A High Priority', '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d1),
(2, 'Normal', 2, 'A Normal Priority', '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d2),
(3, 'Low', 3, 'A Low Priority', '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d3);

-- --------------------------------------------------------

--
-- Table structure for table `product`
--

DROP TABLE IF EXISTS `product`;
CREATE TABLE `product` (
  `ProductId` varchar(10) NOT NULL,
  `Category` varchar(10) NOT NULL,
  `Name` varchar(80) DEFAULT NULL,
  `Descn` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `product`
--

INSERT INTO `product` (`ProductId`, `Category`, `Name`, `Descn`) VALUES
('AV-CB-01', 'BIRDS', 'Amazon Parrot', '<img align="absmiddle" src="Images/Pets/bird1.jpg">Great companion for up to 75 years'),
('AV-SB-02', 'BIRDS', 'Finch', '<img align="absmiddle" src="Images/Pets/bird2.jpg">Great stress reliever'),
('FI-FW-01', 'FISH', 'Koi', '<img align="absmiddle" src="Images/Pets/fish3.jpg">Freshwater fish from Japan'),
('FI-FW-02', 'FISH', 'Goldfish', '<img align="absmiddle" src="Images/Pets/fish4.jpg">Freshwater fish from China'),
('FI-SW-01', 'FISH', 'Angelfish', '<img align="absmiddle" src="Images/Pets/fish1.jpg">Saltwater fish from Australia'),
('FI-SW-02', 'FISH', 'Tiger Shark', '<img align="absmiddle" src="Images/Pets/fish2.jpg">Saltwater fish from Australia'),
('FL-DLH-02', 'CATS', 'Persian', '<img align="absmiddle" src="Images/Pets/cat2.jpg">Friendly house cat, doubles as a princess'),
('FL-DSH-01', 'CATS', 'Manx', '<img align="absmiddle" src="Images/Pets/cat1.jpg">Great for reducing mouse populations'),
('K9-BD-01', 'DOGS', 'Bulldog', '<img align="absmiddle" src="Images/Pets/dog1.jpg">Friendly dog from England'),
('K9-CW-01', 'DOGS', 'Chihuahua', '<img align="absmiddle" src="Images/Pets/dog6.jpg">Great companion dog'),
('K9-DL-01', 'DOGS', 'Dalmation', '<img align="absmiddle" src="Images/Pets/dog3.jpg">Great dog for a fire station'),
('K9-PO-02', 'DOGS', 'Poodle', '<img align="absmiddle" src="Images/Pets/dog2.jpg">Cute dog from France'),
('K9-RT-01', 'DOGS', 'Golden Retriever', '<img align="absmiddle" src="Images/Pets/dog4.jpg">Great family dog'),
('K9-RT-02', 'DOGS', 'Labrador Retriever', '<img align="absmiddle" src="Images/Pets/dog5.jpg">Great hunting dog'),
('RP-LI-02', 'REPTILES', 'Iguana', '<img align="absmiddle" src="Images/Pets/reptile2.jpg">Friendly green friend'),
('RP-SN-01', 'REPTILES', 'Rattlesnake', '<img align="absmiddle" src="Images/Pets/reptile1.jpg">Doubles as a watch dog');

-- --------------------------------------------------------

--
-- Table structure for table `productsummary`
--

DROP TABLE IF EXISTS `productsummary`;
CREATE TABLE `productsummary` (
  `ProductId` varchar(10) NOT NULL,
  `Name` varchar(80) DEFAULT NULL,
  `AvgPrice` decimal(10,2) NOT NULL,
  `Verified` tinyint(1) NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
CREATE TABLE `role` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(150) DEFAULT NULL,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RowVersion` binary(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `role`
--

INSERT INTO `role` (`Id`, `Name`, `Description`, `CreatedDate`, `ModifiedDate`, `RowVersion`) VALUES
(1, 'Admin', NULL, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d4),
(2, 'Manager', NULL, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d5),
(3, 'Newb', NULL, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d6),
(4, 'Nobody', NULL, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d7),
(5, 'Power User', NULL, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d8);

-- --------------------------------------------------------

--
-- Table structure for table `status`
--

DROP TABLE IF EXISTS `status`;
CREATE TABLE `status` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(150) DEFAULT NULL,
  `Order` int(11) NOT NULL,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RowVersion` binary(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `status`
--

INSERT INTO `status` (`Id`, `Name`, `Description`, `Order`, `CreatedDate`, `ModifiedDate`, `RowVersion`) VALUES
(1, 'Not Started', NULL, 1, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007d9),
(2, 'In Progress', NULL, 2, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007da),
(3, 'Completed', NULL, 3, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007db),
(4, 'Waiting on someone else', NULL, 4, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007dc),
(5, 'Deferred', NULL, 5, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007dd),
(6, 'Done', NULL, 6, '2011-09-08 06:57:02', '2011-09-08 06:57:02', 0x00000000000007de);

-- --------------------------------------------------------

--
-- Table structure for table `task`
--

DROP TABLE IF EXISTS `task`;
CREATE TABLE `task` (
  `Id` int(11) NOT NULL,
  `StatusId` int(11) NOT NULL,
  `PriorityId` int(11) DEFAULT NULL,
  `CreatedId` int(11) NOT NULL,
  `Summary` varchar(255) NOT NULL,
  `Details` varchar(2000) DEFAULT NULL,
  `StartDate` datetime DEFAULT NULL,
  `DueDate` datetime DEFAULT NULL,
  `CompleteDate` datetime DEFAULT NULL,
  `AssignedId` int(11) DEFAULT NULL,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RowVersion` binary(8) NOT NULL,
  `LastModifiedBy` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `task`
--

INSERT INTO `task` (`Id`, `StatusId`, `PriorityId`, `CreatedId`, `Summary`, `Details`, `StartDate`, `DueDate`, `CompleteDate`, `AssignedId`, `CreatedDate`, `ModifiedDate`, `RowVersion`, `LastModifiedBy`) VALUES
(1, 1, 1, 2, 'Make it to Earth', 'Find and make it to earth while avoiding the cylons.', NULL, NULL, NULL, 1, '2009-12-18 04:01:58', '2009-12-18 04:01:58', 0x00000000000007df, 'laura.roslin@battlestar.com');

-- --------------------------------------------------------

--
-- Table structure for table `taskextended`
--

DROP TABLE IF EXISTS `taskextended`;
CREATE TABLE `taskextended` (
  `TaskId` int(11) NOT NULL,
  `Browser` varchar(200) DEFAULT NULL,
  `OS` varchar(150) DEFAULT NULL,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RowVersion` binary(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
CREATE TABLE `user` (
  `Id` int(11) NOT NULL,
  `EmailAddress` varchar(250) NOT NULL,
  `FirstName` varchar(200) DEFAULT NULL,
  `LastName` varchar(200) DEFAULT NULL,
  `Avatar` longblob,
  `CreatedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifiedDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RowVersion` binary(8) NOT NULL,
  `PasswordHash` char(86) NOT NULL DEFAULT '',
  `PasswordSalt` char(5) NOT NULL DEFAULT '',
  `Comment` longtext,
  `IsApproved` tinyint(1) NOT NULL DEFAULT '1',
  `LastLoginDate` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `LastActivityDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `LastPasswordChangeDate` datetime DEFAULT NULL,
  `AvatarType` varchar(150) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `user`
--

INSERT INTO `user` (`Id`, `EmailAddress`, `FirstName`, `LastName`, `Avatar`, `CreatedDate`, `ModifiedDate`, `RowVersion`, `PasswordHash`, `PasswordSalt`, `Comment`, `IsApproved`, `LastLoginDate`, `LastActivityDate`, `LastPasswordChangeDate`, `AvatarType`) VALUES
(1, 'william.adama@battlestar.com', 'William', 'Adama', NULL, '2009-05-06 10:46:20', '2009-05-06 10:46:20', 0x00000000000007e0, '1+v5rvSXnyX7tvwTKfM+aq+s0hDmNXsduGZfq8sQv1ggPnGlQdDdBdbUP0bUmbMbiU40PvRQWKRAy5QUd1xrAA', '?#nkY', NULL, 1, NULL, '2009-05-06 10:46:20', NULL, NULL),
(2, 'laura.roslin@battlestar.com', 'Laura', 'Roslin', NULL, '2009-05-06 10:47:00', '2009-05-06 10:47:00', 0x00000000000007e1, 'Sx/jwRHFW/CQpO0E6G8d+jo344AmAKfX+C48a0iAZyMrb4sE8VoDuyZorbhbLZAX9f4UZk67O7eCjk854OrYSg', 'Ph)6;', NULL, 1, NULL, '2009-05-06 10:47:00', NULL, NULL),
(3, 'kara.thrace@battlestar.com', 'Kara', 'Thrace', NULL, '2009-05-06 10:47:43', '2009-05-06 10:47:43', 0x00000000000007e2, '5KhtS4ax7G1aGkq97w02ooVZMmJp8bcySEKMSxruXu/Z/wRKoNAxMlkjRVY1yLavrC3GRYQZjy5b6JW8cR5EWg', '!]@2/', NULL, 1, NULL, '2009-05-06 10:47:43', NULL, NULL),
(4, 'lee.adama@battlestar.com', 'Lee', 'Adama', NULL, '2009-05-06 10:48:02', '2009-05-06 10:48:02', 0x00000000000007e3, 'IrK8OhI2n4Ev3YA4y5kP7vy+n2CffX2MgcONbAh6/kZpNZYBYoYyrMhqdYztehL0NAIdvcInQ6zOjMplq+zWQA', 'e@_a{', NULL, 1, NULL, '2009-05-06 10:48:02', NULL, NULL),
(5, 'gaius.baltar@battlestar.com', 'Gaius', 'Baltar', NULL, '2009-05-06 10:48:26', '2009-05-06 10:48:26', 0x00000000000007e4, '7tfajMaEerDNVgi6Oi6UJ6JxsUXZ0u4zQeUrZQxnaXJQ2f2vd9AzBR4sVOaH7LZtCjQopMzlQ38QqNEnpK0B/g', '_qpA2', NULL, 1, NULL, '2009-05-06 10:48:26', NULL, NULL),
(6, 'saul.tigh@battlestar.com', 'Saul', 'Tigh', NULL, '2009-05-06 10:49:26', '2009-05-06 10:49:26', 0x00000000000007e5, 'wzkR89zRXe7hND1jqAP9xgupYJBvEZcjsfUe3TaU45kxRajjjS9u0fOTLK+uglzk67EGochJdeoikWs7hxMNRA', 'h]-zG', NULL, 1, NULL, '2009-05-06 10:49:26', NULL, NULL);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `audit`
--
ALTER TABLE `audit`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `FK_Audit_Task` (`TaskId`),
  ADD KEY `FK_Audit_User` (`UserId`);

--
-- Indexes for table `item`
--
ALTER TABLE `item`
  ADD PRIMARY KEY (`ItemId`),
  ADD KEY `FK__Item__ProductId__04E4BC85` (`ProductId`);

--
-- Indexes for table `item 2`
--
ALTER TABLE `item 2`
  ADD PRIMARY KEY (`ItemId`),
  ADD KEY `FK__Item 2__ProductI__0A9D95DB` (`ProductId`);

--
-- Indexes for table `priority`
--
ALTER TABLE `priority`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `product`
--
ALTER TABLE `product`
  ADD PRIMARY KEY (`ProductId`);

--
-- Indexes for table `productsummary`
--
ALTER TABLE `productsummary`
  ADD PRIMARY KEY (`ProductId`);

--
-- Indexes for table `role`
--
ALTER TABLE `role`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `status`
--
ALTER TABLE `status`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `task`
--
ALTER TABLE `task`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_Task` (`AssignedId`,`StatusId`),
  ADD KEY `FK_Task_Priority` (`PriorityId`),
  ADD KEY `FK_Task_Status` (`StatusId`),
  ADD KEY `FK_Task_User_Created` (`CreatedId`);

--
-- Indexes for table `taskextended`
--
ALTER TABLE `taskextended`
  ADD PRIMARY KEY (`TaskId`);

--
-- Indexes for table `user`
--
ALTER TABLE `user`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `IX_User` (`EmailAddress`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `audit`
--
ALTER TABLE `audit`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `role`
--
ALTER TABLE `role`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;
--
-- AUTO_INCREMENT for table `status`
--
ALTER TABLE `status`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;
--
-- AUTO_INCREMENT for table `task`
--
ALTER TABLE `task`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;
--
-- AUTO_INCREMENT for table `user`
--
ALTER TABLE `user`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;
--
-- Constraints for dumped tables
--

--
-- Constraints for table `audit`
--
ALTER TABLE `audit`
  ADD CONSTRAINT `FK_Audit_Task` FOREIGN KEY (`TaskId`) REFERENCES `task` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `FK_Audit_User` FOREIGN KEY (`UserId`) REFERENCES `user` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `item`
--
ALTER TABLE `item`
  ADD CONSTRAINT `FK__Item__ProductId__04E4BC85` FOREIGN KEY (`ProductId`) REFERENCES `product` (`ProductId`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `item 2`
--
ALTER TABLE `item 2`
  ADD CONSTRAINT `FK__Item 2__ProductI__0A9D95DB` FOREIGN KEY (`ProductId`) REFERENCES `product` (`ProductId`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `task`
--
ALTER TABLE `task`
  ADD CONSTRAINT `FK_Task_Priority` FOREIGN KEY (`PriorityId`) REFERENCES `priority` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `FK_Task_Status` FOREIGN KEY (`StatusId`) REFERENCES `status` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `FK_Task_User_Assigned` FOREIGN KEY (`AssignedId`) REFERENCES `user` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `FK_Task_User_Created` FOREIGN KEY (`CreatedId`) REFERENCES `user` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `taskextended`
--
ALTER TABLE `taskextended`
  ADD CONSTRAINT `FK_TaskExtended_Task` FOREIGN KEY (`TaskId`) REFERENCES `task` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
