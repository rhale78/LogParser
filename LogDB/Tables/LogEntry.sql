CREATE TABLE [dbo].[LogEntry] (
    [ID]               INT            IDENTITY (1, 1) NOT NULL,
    [LogDateTime]      DATETIME2 (7)  NULL,
    [ProcessedFilesID] INT            NULL,
    [Data]             NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_LogEntry] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_LogEntry_ProcessedFiles] FOREIGN KEY ([ProcessedFilesID]) REFERENCES [dbo].[ProcessedFiles] ([ID])
);


GO
CREATE CLUSTERED INDEX [IX_LogEntry]
    ON [dbo].[LogEntry]([LogDateTime] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LogEntry_1]
    ON [dbo].[LogEntry]([ProcessedFilesID] ASC);

