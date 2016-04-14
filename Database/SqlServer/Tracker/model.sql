CREATE TABLE [dbo].[Audit] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [Date]        DATETIME      NOT NULL,
    [UserId]      INT           NULL,
    [TaskId]      INT           NULL,
    [Content]     VARCHAR (MAX) NOT NULL,
    [Username]    NVARCHAR (50) NOT NULL,
    [CreatedDate] DATETIME      CONSTRAINT [DF_Audit_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [RowVersion]  ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Audit] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Audit_Task] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Task] ([Id]),
    CONSTRAINT [FK_Audit_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);

GO
CREATE TABLE [dbo].[Priority] (
    [Id]           INT            NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [Order]        INT            NOT NULL,
    [Description]  NVARCHAR (200) NULL,
    [CreatedDate]  DATETIME       CONSTRAINT [DF__Priority__CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedDate] DATETIME       CONSTRAINT [DF__Priority__ModifiedDate] DEFAULT (getdate()) NOT NULL,
    [RowVersion]   ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Priority] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[Role] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [Description]  NVARCHAR (150) NULL,
    [CreatedDate]  DATETIME       CONSTRAINT [DF__Role__CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedDate] DATETIME       CONSTRAINT [DF__Role__ModifiedDate] DEFAULT (getdate()) NOT NULL,
    [RowVersion]   ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[Status] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [Description]  NVARCHAR (150) NULL,
    [Order]        INT            NOT NULL,
    [CreatedDate]  DATETIME       CONSTRAINT [DF__Status__CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedDate] DATETIME       CONSTRAINT [DF__Status__ModifiedDate] DEFAULT (getdate()) NOT NULL,
    [RowVersion]   ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Status] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[Task] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [StatusId]       INT             NOT NULL,
    [PriorityId]     INT             NULL,
    [CreatedId]      INT             NOT NULL,
    [Summary]        NVARCHAR (255)  NOT NULL,
    [Details]        NVARCHAR (2000) NULL,
    [StartDate]      DATETIME        NULL,
    [DueDate]        DATETIME        NULL,
    [CompleteDate]   DATETIME        NULL,
    [AssignedId]     INT             NULL,
    [CreatedDate]    DATETIME        CONSTRAINT [DF__Task__CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedDate]   DATETIME        CONSTRAINT [DF__Task__ModifiedDate] DEFAULT (getdate()) NOT NULL,
    [RowVersion]     ROWVERSION      NOT NULL,
    [LastModifiedBy] NVARCHAR (50)   NULL,
    CONSTRAINT [PK_Task] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Task_Priority] FOREIGN KEY ([PriorityId]) REFERENCES [dbo].[Priority] ([Id]),
    CONSTRAINT [FK_Task_Status] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Status] ([Id]),
    CONSTRAINT [FK_Task_User_Assigned] FOREIGN KEY ([AssignedId]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_Task_User_Created] FOREIGN KEY ([CreatedId]) REFERENCES [dbo].[User] ([Id])
);

GO
CREATE TABLE [dbo].[TaskExtended] (
    [TaskId]       INT            NOT NULL,
    [Browser]      NVARCHAR (200) NULL,
    [OS]           NVARCHAR (150) NULL,
    [CreatedDate]  DATETIME       DEFAULT (getdate()) NOT NULL,
    [ModifiedDate] DATETIME       DEFAULT (getdate()) NOT NULL,
    [RowVersion]   ROWVERSION     NOT NULL,
    CONSTRAINT [PK_TaskExtended] PRIMARY KEY CLUSTERED ([TaskId] ASC),
    CONSTRAINT [FK_TaskExtended_Task] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Task] ([Id])
);

GO
CREATE TABLE [dbo].[User] (
    [Id]                     INT             IDENTITY (1, 1) NOT NULL,
    [EmailAddress]           NVARCHAR (250)  NOT NULL,
    [FirstName]              NVARCHAR (200)  NULL,
    [LastName]               NVARCHAR (200)  NULL,
    [Avatar]                 VARBINARY (MAX) NULL,
    [CreatedDate]            DATETIME        CONSTRAINT [DF__User__CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedDate]           DATETIME        CONSTRAINT [DF__User__ModifiedDate] DEFAULT (getdate()) NOT NULL,
    [RowVersion]             ROWVERSION      NOT NULL,
    [PasswordHash]           CHAR (86)       CONSTRAINT [DF__User__PasswordHash] DEFAULT ('') NOT NULL,
    [PasswordSalt]           CHAR (5)        CONSTRAINT [DF__User__PasswordSalt] DEFAULT ('') NOT NULL,
    [Comment]                TEXT            NULL,
    [IsApproved]             BIT             CONSTRAINT [DF__User__IsApproved] DEFAULT ((1)) NOT NULL,
    [LastLoginDate]          DATETIME        CONSTRAINT [DF__User__LastLoginDate] DEFAULT (getdate()) NULL,
    [LastActivityDate]       DATETIME        CONSTRAINT [DF__User__LastActivityDate] DEFAULT (getdate()) NOT NULL,
    [LastPasswordChangeDate] DATETIME        NULL,
    [AvatarType]             NVARCHAR (150)  NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[UserRole] (
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_UserRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role] ([Id]),
    CONSTRAINT [FK_UserRole_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_Task]
    ON [dbo].[Task]([AssignedId] ASC, [StatusId] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_User]
    ON [dbo].[User]([EmailAddress] ASC);

GO
