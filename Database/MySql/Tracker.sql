CREATE DATABASE IF NOT EXISTS `Tracker`;
USE `Tracker`
;


/****** Object:  Table `Role`  ******/

CREATE TABLE IF NOT EXISTS `Role`(
	`Id` int AUTO_INCREMENT NOT NULL,
	`Name` varchar(50) NOT NULL,
	`Description` varchar(150) NULL,
	`CreatedDate` datetime NOT NULL DEFAULT NOW(),
	`ModifiedDate` datetime NOT NULL DEFAULT NOW(),
	`RowVersion` timestamp NOT NULL,
 CONSTRAINT `PK_Role` PRIMARY KEY 
(
	`Id` ASC
)
)
;

/****** Object:  Table `User`  ******/

CREATE TABLE IF NOT EXISTS `User`(
	`Id` int AUTO_INCREMENT NOT NULL,
	`EmailAddress` varchar(250) NOT NULL,
	`FirstName` varchar(200) NULL,
	`LastName` varchar(200) NULL,
	`Avatar` BLOB NULL,
	`CreatedDate` datetime NOT NULL DEFAULT NOW(),
	`ModifiedDate` datetime NOT NULL  DEFAULT NOW(),
	`RowVersion` timestamp NOT NULL,
	`PasswordHash` varchar(86) NOT NULL DEFAULT '',
	`PasswordSalt` varchar(5) NOT NULL DEFAULT '',
	`Comment` text NULL,
	`IsApproved` tinyint NOT NULL DEFAULT 1,
	`LastLoginDate` datetime NULL DEFAULT NOW(),
	`LastActivityDate` datetime NOT NULL DEFAULT NOW(),
	`LastPasswordChangeDate` datetime NULL,
	`AvatarType` varchar(150) NULL,
 CONSTRAINT `PK_User` PRIMARY KEY 
(
	`Id` ASC
),UNIQUE INDEX `IX_User`
(
	`EmailAddress` ASC
)
)
;

/****** Object:  Table `UserRole`  ******/


CREATE TABLE IF NOT EXISTS `UserRole`(
	`UserId` int NOT NULL REFERENCES `User`(`Id`),
	`RoleId` int NOT NULL REFERENCES `Role`(`Id`),
 CONSTRAINT `PK_UserRole` PRIMARY KEY 
(
	`UserId` ASC,
	`RoleId` ASC
)
)
;



/****** Object:  Table `Priority`  ******/

CREATE TABLE IF NOT EXISTS `Priority`(
	`Id` int NOT NULL,
	`Name` varchar(50) NOT NULL,
	`Order` int NOT NULL,
	`Description` varchar(200) NULL,
	`CreatedDate` datetime NOT NULL DEFAULT NOW(),
	`ModifiedDate` datetime NOT NULL DEFAULT NOW(),
	`RowVersion` timestamp NOT NULL,
 CONSTRAINT `PK_Priority` PRIMARY KEY 
(
	`Id` ASC
)
)
;

/****** Object:  Table `Status`  ******/

CREATE TABLE IF NOT EXISTS `Status`(
	`Id` int AUTO_INCREMENT NOT NULL,
	`Name` varchar(50) NOT NULL,
	`Description` varchar(150) NULL,
	`Order` int NOT NULL,
	`CreatedDate` datetime NOT NULL DEFAULT NOW(),
	`ModifiedDate` datetime NOT NULL DEFAULT NOW(),
	`RowVersion` timestamp NOT NULL,
 CONSTRAINT `PK_Status` PRIMARY KEY 
(
	`Id` ASC
)
)
;


/****** Object:  Table `Task`  ******/

CREATE TABLE IF NOT EXISTS `Task`(
	`Id` int AUTO_INCREMENT NOT NULL,
	`StatusId` int NOT NULL REFERENCES `Status`(`Id`),
	`PriorityId` int NULL REFERENCES `Priority`(`Id`),
	`CreatedId` int NOT NULL REFERENCES `User`(`Id`),
	`Summary` varchar(255) NOT NULL,
	`Details` varchar(2000) NULL,
	`StartDate` datetime NULL,
	`DueDate` datetime NULL,
	`CompleteDate` datetime NULL,
	`AssignedId` int null REFERENCES `User`(`Id`),
	`CreatedDate` datetime NOT NULL DEFAULT NOW(),
	`ModifiedDate` datetime NOT NULL DEFAULT NOW(),
	`RowVersion` timestamp NOT NULL,
	`LastModifiedBy` varchar(50) NULL,
 CONSTRAINT `PK_Task` PRIMARY KEY 
(
	`Id` ASC
),INDEX `IX_Task`
(
	`AssignedId` ASC,
	`StatusId` ASC
)
)
;


/****** Object:  Table `TaskExtended`  ******/


CREATE TABLE IF NOT EXISTS `TaskExtended`(
	`TaskId` int NOT NULL REFERENCES `Task`(`Id`),
	`Browser` varchar(200) NULL,
	`OS` varchar(150) NULL,
	`CreatedDate` datetime NOT NULL,
	`ModifiedDate` datetime NOT NULL,
	`RowVersion` timestamp NOT NULL,
 CONSTRAINT `PK_TaskExtended` PRIMARY KEY 
(
	`TaskId` ASC
)
)
;


/****** Object:  Table `Audit`  ******/

CREATE TABLE IF NOT EXISTS `Audit`(
	`Id` int AUTO_INCREMENT NOT NULL,
	`Date` datetime NOT NULL,
	`UserId` int NULL REFERENCES `User`(`Id`),
	`TaskId` int NULL REFERENCES `Task`(`Id`),
	`Content` TEXT NOT NULL,
	`Username` varchar(50) NOT NULL,
	`CreatedDate` datetime NOT NULL,
	`RowVersion` timestamp NOT NULL,
 CONSTRAINT `PK_Audit` PRIMARY KEY 
(
	`Id` ASC
)
);


INSERT into `Priority` (`Id`, `Name`, `Order`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (1, N'High', 1, N'A High Priority', CAST(N'2011-09-08 13:57:02.743' AS DateTime), CAST(N'2011-09-08 13:57:02.743' AS DateTime))
;
INSERT into `Priority` (`Id`, `Name`, `Order`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (2, N'Normal', 2, N'A Normal Priority', CAST(N'2011-09-08 13:57:02.743' AS DateTime), CAST(N'2011-09-08 13:57:02.743' AS DateTime))
;
INSERT into `Priority` (`Id`, `Name`, `Order`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (3, N'Low', 3, N'A Low Priority', CAST(N'2011-09-08 13:57:02.743' AS DateTime), CAST(N'2011-09-08 13:57:02.743' AS DateTime))
;

INSERT into `Role` (`Id`, `Name`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (1, N'Admin', NULL, CAST(N'2011-09-08 13:57:02.730' AS DateTime), CAST(N'2011-09-08 13:57:02.730' AS DateTime))
;
INSERT into `Role` (`Id`, `Name`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (2, N'Manager', NULL, CAST(N'2011-09-08 13:57:02.730' AS DateTime), CAST(N'2011-09-08 13:57:02.730' AS DateTime))
;
INSERT into `Role` (`Id`, `Name`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (3, N'Newb', NULL, CAST(N'2011-09-08 13:57:02.730' AS DateTime), CAST(N'2011-09-08 13:57:02.730' AS DateTime))
;
INSERT into `Role` (`Id`, `Name`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (4, N'Nobody', NULL, CAST(N'2011-09-08 13:57:02.730' AS DateTime), CAST(N'2011-09-08 13:57:02.730' AS DateTime))
;
INSERT into `Role` (`Id`, `Name`, `Description`, `CreatedDate`, `ModifiedDate`) VALUES (5, N'Power User', NULL, CAST(N'2011-09-08 13:57:02.730' AS DateTime), CAST(N'2011-09-08 13:57:02.730' AS DateTime))
;


INSERT INTO `Status` (`Id`, `Name`, `Description`, `Order`, `CreatedDate`, `ModifiedDate`) VALUES (1, N'Not Started', NULL, 1, CAST(N'2011-09-08 13:57:02.733' AS DateTime), CAST(N'2011-09-08 13:57:02.733' AS DateTime))
;
INSERT INTO `Status` (`Id`, `Name`, `Description`, `Order`, `CreatedDate`, `ModifiedDate`) VALUES (2, N'In Progress', NULL, 2, CAST(N'2011-09-08 13:57:02.733' AS DateTime), CAST(N'2011-09-08 13:57:02.733' AS DateTime))
;
INSERT INTO `Status` (`Id`, `Name`, `Description`, `Order`, `CreatedDate`, `ModifiedDate`) VALUES (3, N'Completed', NULL, 3, CAST(N'2011-09-08 13:57:02.733' AS DateTime), CAST(N'2011-09-08 13:57:02.733' AS DateTime))
;
INSERT INTO `Status` (`Id`, `Name`, `Description`, `Order`, `CreatedDate`, `ModifiedDate`) VALUES (4, N'Waiting on someone else', NULL, 4, CAST(N'2011-09-08 13:57:02.733' AS DateTime), CAST(N'2011-09-08 13:57:02.733' AS DateTime))
;
INSERT INTO `Status` (`Id`, `Name`, `Description`, `Order`, `CreatedDate`, `ModifiedDate`) VALUES (5, N'Deferred', NULL, 5, CAST(N'2011-09-08 13:57:02.733' AS DateTime), CAST(N'2011-09-08 13:57:02.733' AS DateTime))
;
INSERT INTO `Status` (`Id`, `Name`, `Description`, `Order`, `CreatedDate`, `ModifiedDate`) VALUES (6, N'Done', NULL, 6, CAST(N'2011-09-08 13:57:02.733' AS DateTime), CAST(N'2011-09-08 13:57:02.733' AS DateTime))
;

;
INSERT INTO `Task` (`Id`, `StatusId`, `PriorityId`, `CreatedId`, `Summary`, `Details`, `StartDate`, `DueDate`, `CompleteDate`, `AssignedId`, `CreatedDate`, `ModifiedDate`, `LastModifiedBy`) VALUES (1, 1, 1, 2, N'Make it to Earth', N'Find and make it to earth while avoiding the cylons.', NULL, NULL, NULL, 1, CAST(N'2009-12-18 11:01:58.713' AS DateTime), CAST(N'2009-12-18 11:01:58.713' AS DateTime), N'laura.roslin@battlestar.com')
;

;
INSERT INTO `User` (`Id`, `EmailAddress`, `FirstName`, `LastName`, `Avatar`, `CreatedDate`, `ModifiedDate`, `PasswordHash`, `PasswordSalt`, `Comment`, `IsApproved`, `LastLoginDate`, `LastActivityDate`, `LastPasswordChangeDate`, `AvatarType`) VALUES (1, N'william.adama@battlestar.com', N'William', N'Adama', NULL, CAST(N'2009-05-06 17:46:20.597' AS DateTime), CAST(N'2009-05-06 17:46:20.597' AS DateTime), N'1+v5rvSXnyX7tvwTKfM+aq+s0hDmNXsduGZfq8sQv1ggPnGlQdDdBdbUP0bUmbMbiU40PvRQWKRAy5QUd1xrAA', N'?#nkY', NULL, 1, NULL, CAST(N'2009-05-06 17:46:20.597' AS DateTime), NULL, NULL)
;
INSERT INTO `User` (`Id`, `EmailAddress`, `FirstName`, `LastName`, `Avatar`, `CreatedDate`, `ModifiedDate`, `PasswordHash`, `PasswordSalt`, `Comment`, `IsApproved`, `LastLoginDate`, `LastActivityDate`, `LastPasswordChangeDate`, `AvatarType`) VALUES (2, N'laura.roslin@battlestar.com', N'Laura', N'Roslin', NULL, CAST(N'2009-05-06 17:47:00.330' AS DateTime), CAST(N'2009-05-06 17:47:00.330' AS DateTime), N'Sx/jwRHFW/CQpO0E6G8d+jo344AmAKfX+C48a0iAZyMrb4sE8VoDuyZorbhbLZAX9f4UZk67O7eCjk854OrYSg', N'Ph)6;', NULL, 1, NULL, CAST(N'2009-05-06 17:47:00.330' AS DateTime), NULL, NULL)
;
INSERT INTO `User` (`Id`, `EmailAddress`, `FirstName`, `LastName`, `Avatar`, `CreatedDate`, `ModifiedDate`, `PasswordHash`, `PasswordSalt`, `Comment`, `IsApproved`, `LastLoginDate`, `LastActivityDate`, `LastPasswordChangeDate`, `AvatarType`) VALUES (3, N'kara.thrace@battlestar.com', N'Kara', N'Thrace', NULL, CAST(N'2009-05-06 17:47:43.417' AS DateTime), CAST(N'2009-05-06 17:47:43.417' AS DateTime), N'5KhtS4ax7G1aGkq97w02ooVZMmJp8bcySEKMSxruXu/Z/wRKoNAxMlkjRVY1yLavrC3GRYQZjy5b6JW8cR5EWg', N'!`@2/', NULL, 1, NULL, CAST(N'2009-05-06 17:47:43.417' AS DateTime), NULL, NULL)
;
INSERT INTO `User` (`Id`, `EmailAddress`, `FirstName`, `LastName`, `Avatar`, `CreatedDate`, `ModifiedDate`, `PasswordHash`, `PasswordSalt`, `Comment`, `IsApproved`, `LastLoginDate`, `LastActivityDate`, `LastPasswordChangeDate`, `AvatarType`) VALUES (4, N'lee.adama@battlestar.com', N'Lee', N'Adama', NULL, CAST(N'2009-05-06 17:48:02.367' AS DateTime), CAST(N'2009-05-06 17:48:02.367' AS DateTime), N'IrK8OhI2n4Ev3YA4y5kP7vy+n2CffX2MgcONbAh6/kZpNZYBYoYyrMhqdYztehL0NAIdvcInQ6zOjMplq+zWQA', N'e@_a{', NULL, 1, NULL, CAST(N'2009-05-06 17:48:02.367' AS DateTime), NULL, NULL)
;
INSERT INTO `User` (`Id`, `EmailAddress`, `FirstName`, `LastName`, `Avatar`, `CreatedDate`, `ModifiedDate`, `PasswordHash`, `PasswordSalt`, `Comment`, `IsApproved`, `LastLoginDate`, `LastActivityDate`, `LastPasswordChangeDate`, `AvatarType`) VALUES (5, N'gaius.baltar@battlestar.com', N'Gaius', N'Baltar', NULL, CAST(N'2009-05-06 17:48:26.273' AS DateTime), CAST(N'2009-05-06 17:48:26.273' AS DateTime), N'7tfajMaEerDNVgi6Oi6UJ6JxsUXZ0u4zQeUrZQxnaXJQ2f2vd9AzBR4sVOaH7LZtCjQopMzlQ38QqNEnpK0B/g', N'_qpA2', NULL, 1, NULL, CAST(N'2009-05-06 17:48:26.273' AS DateTime), NULL, NULL)
;
INSERT INTO `User` (`Id`, `EmailAddress`, `FirstName`, `LastName`, `Avatar`, `CreatedDate`, `ModifiedDate`, `PasswordHash`, `PasswordSalt`, `Comment`, `IsApproved`, `LastLoginDate`, `LastActivityDate`, `LastPasswordChangeDate`, `AvatarType`) VALUES (6, N'saul.tigh@battlestar.com', N'Saul', N'Tigh', NULL, CAST(N'2009-05-06 17:49:26.023' AS DateTime), CAST(N'2009-05-06 17:49:26.023' AS DateTime), N'wzkR89zRXe7hND1jqAP9xgupYJBvEZcjsfUe3TaU45kxRajjjS9u0fOTLK+uglzk67E;chJdeoikWs7hxMNRA', N'h`-zG', NULL, 1, NULL, CAST(N'2009-05-06 17:49:26.023' AS DateTime), NULL, NULL)
;
