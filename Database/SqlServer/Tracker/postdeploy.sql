/*
Post-Deployment Script Template                            
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.        
 Use SQLCMD syntax to include a file in the post-deployment script.            
 Example:      :r .\myfile.sql                                
 Use SQLCMD syntax to reference a variable in the post-deployment script.        
 Example:      :setvar TableName MyTable                            
               SELECT * FROM [$(TableName)]                    
--------------------------------------------------------------------------------------
*/

-- Table [dbo].[Priority] data
MERGE INTO [dbo].[Priority] AS t
USING 
(
    VALUES
    (
        1, 
        'High', 
        1, 
        'A High Priority'
    ), 
    (
        2, 
        'Normal', 
        2, 
        'A Normal Priority'
    ), 
    (
        3, 
        'Low', 
        3, 
        'A Low Priority'
    )
) 
AS s
(
    [Id], 
    [Name], 
    [Order], 
    [Description]
)
ON 
(
    t.[Id] = s.[Id]
)
WHEN NOT MATCHED BY TARGET THEN 
    INSERT 
    (
        [Id], 
        [Name], 
        [Order], 
        [Description]
    )
    VALUES 
    (
        s.[Id], 
        s.[Name], 
        s.[Order], 
        s.[Description]
    )
WHEN MATCHED THEN 
    UPDATE SET 
        t.[Name] = s.[Name], 
        t.[Order] = s.[Order], 
        t.[Description] = s.[Description]
OUTPUT $action as [Action];


SET IDENTITY_INSERT [dbo].[Role] ON

-- Table [dbo].[Role] data
MERGE INTO [dbo].[Role] AS t
USING 
(
    VALUES
    (
        1, 
        'Admin', 
        'Admin Role'
    ), 
    (
        2, 
        'Manager', 
        NULL
    ), 
    (
        3, 
        'Newb', 
        NULL
    ), 
    (
        4, 
        'Nobody', 
        NULL
    ), 
    (
        5, 
        'Power User', 
        NULL
    )
) 
AS s
(
    [Id], 
    [Name], 
    [Description]
)
ON 
(
    t.[Id] = s.[Id]
)
WHEN NOT MATCHED BY TARGET THEN 
    INSERT 
    (
        [Id], 
        [Name], 
        [Description]
    )
    VALUES 
    (
        s.[Id], 
        s.[Name], 
        s.[Description]
    )
WHEN MATCHED THEN 
    UPDATE SET 
        t.[Name] = s.[Name], 
        t.[Description] = s.[Description]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[Role] OFF


SET IDENTITY_INSERT [dbo].[Status] ON

-- Table [dbo].[Status] data
MERGE INTO [dbo].[Status] AS t
USING 
(
    VALUES
    (
        1, 
        'Not Started', 
        NULL, 
        1
    ), 
    (
        2, 
        'In Progress', 
        NULL, 
        2
    ), 
    (
        3, 
        'Completed', 
        NULL, 
        3
    ), 
    (
        4, 
        'Waiting on someone else', 
        NULL, 
        4
    ), 
    (
        5, 
        'Deferred', 
        NULL, 
        5
    ), 
    (
        6, 
        'Done', 
        NULL, 
        6
    )
) 
AS s
(
    [Id], 
    [Name], 
    [Description], 
    [Order]
)
ON 
(
    t.[Id] = s.[Id]
)
WHEN NOT MATCHED BY TARGET THEN 
    INSERT 
    (
        [Id], 
        [Name], 
        [Description], 
        [Order]
    )
    VALUES 
    (
        s.[Id], 
        s.[Name], 
        s.[Description], 
        s.[Order]
    )
WHEN MATCHED THEN 
    UPDATE SET 
        t.[Name] = s.[Name], 
        t.[Description] = s.[Description], 
        t.[Order] = s.[Order]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[Status] OFF


SET IDENTITY_INSERT [dbo].[User] ON

-- Table [dbo].[User] data
MERGE INTO [dbo].[User] AS t
USING 
(
    VALUES
    (
        1, 
        'william.adama@battlestar.com', 
        'William', 
        'Adama', 
        NULL, 
        '1+v5rvSXnyX7tvwTKfM+aq+s0hDmNXsduGZfq8sQv1ggPnGlQdDdBdbUP0bUmbMbiU40PvRQWKRAy5QUd1xrAA', 
        '?#nkY', 
        'Data Merge 635324524904242477', 
        1, 
        '2014-04-07 07:26:54', 
        '2009-05-06 17:46:20', 
        NULL, 
        NULL
    ), 
    (
        2, 
        'laura.roslin@battlestar.com', 
        'Laura', 
        'Roslin', 
        NULL, 
        'Sx/jwRHFW/CQpO0E6G8d+jo344AmAKfX+C48a0iAZyMrb4sE8VoDuyZorbhbLZAX9f4UZk67O7eCjk854OrYSg', 
        'Ph)6;', 
        'Data Merge 635324524904242477', 
        1, 
        '2014-04-07 07:26:54', 
        '2009-05-06 17:47:00', 
        NULL, 
        NULL
    ), 
    (
        3, 
        'kara.thrace@battlestar.com', 
        'Kara', 
        'Thrace', 
        NULL, 
        '5KhtS4ax7G1aGkq97w02ooVZMmJp8bcySEKMSxruXu/Z/wRKoNAxMlkjRVY1yLavrC3GRYQZjy5b6JW8cR5EWg', 
        '!]@2/', 
        'Data Merge 635324524147981355', 
        1, 
        '2014-04-07 07:26:54', 
        '2009-05-06 17:47:43', 
        NULL, 
        NULL
    ), 
    (
        4, 
        'lee.adama@battlestar.com', 
        'Lee', 
        'Adama', 
        NULL, 
        'IrK8OhI2n4Ev3YA4y5kP7vy+n2CffX2MgcONbAh6/kZpNZYBYoYyrMhqdYztehL0NAIdvcInQ6zOjMplq+zWQA', 
        'e@_a{', 
        'Data Merge 635324524147981355', 
        1, 
        '2014-04-07 07:26:54', 
        '2009-05-06 17:48:02', 
        NULL, 
        NULL
    ), 
    (
        5, 
        'gaius.baltar@battlestar.com', 
        'Gaius', 
        'Baltar', 
        NULL, 
        '7tfajMaEerDNVgi6Oi6UJ6JxsUXZ0u4zQeUrZQxnaXJQ2f2vd9AzBR4sVOaH7LZtCjQopMzlQ38QqNEnpK0B/g', 
        '_qpA2', 
        'Data Merge 635324524147981355', 
        1, 
        '2014-04-07 07:26:54', 
        '2009-05-06 17:48:26', 
        NULL, 
        NULL
    ), 
    (
        6, 
        'saul.tigh@battlestar.com', 
        'Saul', 
        'Tigh', 
        NULL, 
        'wzkR89zRXe7hND1jqAP9xgupYJBvEZcjsfUe3TaU45kxRajjjS9u0fOTLK+uglzk67EGochJdeoikWs7hxMNRA', 
        'h]-zG', 
        'Data Merge 635324524147981355', 
        1, 
        '2014-04-07 07:26:54', 
        '2009-05-06 17:49:26', 
        NULL, 
        NULL
    )
) 
AS s
(
    [Id], 
    [EmailAddress], 
    [FirstName], 
    [LastName], 
    [Avatar], 
    [PasswordHash], 
    [PasswordSalt], 
    [Comment], 
    [IsApproved], 
    [LastLoginDate], 
    [LastActivityDate], 
    [LastPasswordChangeDate], 
    [AvatarType]
)
ON 
(
    t.[Id] = s.[Id]
)
WHEN NOT MATCHED BY TARGET THEN 
    INSERT 
    (
        [Id], 
        [EmailAddress], 
        [FirstName], 
        [LastName], 
        [Avatar], 
        [PasswordHash], 
        [PasswordSalt], 
        [Comment], 
        [IsApproved], 
        [LastLoginDate], 
        [LastActivityDate], 
        [LastPasswordChangeDate], 
        [AvatarType]
    )
    VALUES 
    (
        s.[Id], 
        s.[EmailAddress], 
        s.[FirstName], 
        s.[LastName], 
        s.[Avatar], 
        s.[PasswordHash], 
        s.[PasswordSalt], 
        s.[Comment], 
        s.[IsApproved], 
        s.[LastLoginDate], 
        s.[LastActivityDate], 
        s.[LastPasswordChangeDate], 
        s.[AvatarType]
    )
WHEN MATCHED THEN 
    UPDATE SET 
        t.[EmailAddress] = s.[EmailAddress], 
        t.[FirstName] = s.[FirstName], 
        t.[LastName] = s.[LastName], 
        t.[Avatar] = s.[Avatar], 
        t.[PasswordHash] = s.[PasswordHash], 
        t.[PasswordSalt] = s.[PasswordSalt], 
        t.[Comment] = s.[Comment], 
        t.[IsApproved] = s.[IsApproved], 
        t.[LastLoginDate] = s.[LastLoginDate], 
        t.[LastActivityDate] = s.[LastActivityDate], 
        t.[LastPasswordChangeDate] = s.[LastPasswordChangeDate], 
        t.[AvatarType] = s.[AvatarType]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[User] OFF


SET IDENTITY_INSERT [dbo].[Task] ON

-- Table [dbo].[Task] data
MERGE INTO [dbo].[Task] AS t
USING 
(
    VALUES
    (
        1, 
        1, 
        1, 
        2, 
        'Make it to Earth', 
        'Find and make it to earth while avoiding the cylons.', 
        NULL, 
        NULL, 
        NULL, 
        1, 
        'laura.roslin@battlestar.com'
    )
) 
AS s
(
    [Id], 
    [StatusId], 
    [PriorityId], 
    [CreatedId], 
    [Summary], 
    [Details], 
    [StartDate], 
    [DueDate], 
    [CompleteDate], 
    [AssignedId], 
    [LastModifiedBy]
)
ON 
(
    t.[Id] = s.[Id]
)
WHEN NOT MATCHED BY TARGET THEN 
    INSERT 
    (
        [Id], 
        [StatusId], 
        [PriorityId], 
        [CreatedId], 
        [Summary], 
        [Details], 
        [StartDate], 
        [DueDate], 
        [CompleteDate], 
        [AssignedId], 
        [LastModifiedBy]
    )
    VALUES 
    (
        s.[Id], 
        s.[StatusId], 
        s.[PriorityId], 
        s.[CreatedId], 
        s.[Summary], 
        s.[Details], 
        s.[StartDate], 
        s.[DueDate], 
        s.[CompleteDate], 
        s.[AssignedId], 
        s.[LastModifiedBy]
    )
WHEN MATCHED THEN 
    UPDATE SET 
        t.[StatusId] = s.[StatusId], 
        t.[PriorityId] = s.[PriorityId], 
        t.[CreatedId] = s.[CreatedId], 
        t.[Summary] = s.[Summary], 
        t.[Details] = s.[Details], 
        t.[StartDate] = s.[StartDate], 
        t.[DueDate] = s.[DueDate], 
        t.[CompleteDate] = s.[CompleteDate], 
        t.[AssignedId] = s.[AssignedId], 
        t.[LastModifiedBy] = s.[LastModifiedBy]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[Task] OFF


GO
