CREATE TABLE [dbo].[LogEventType] (
    [ID]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (10) NOT NULL,
    CONSTRAINT [PK_LogEventType] PRIMARY KEY CLUSTERED ([ID] ASC)
);

