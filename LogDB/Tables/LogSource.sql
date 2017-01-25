CREATE TABLE [dbo].[LogSource] (
    [ID]         INT            IDENTITY (1, 1) NOT NULL,
    [SourceName] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_LogSource] PRIMARY KEY CLUSTERED ([ID] ASC)
);

