CREATE TABLE [dbo].[LogEntry] (
    [ID]               INT            IDENTITY (1, 1) NOT NULL,
    [LogDateTime]      DATETIME2 (7)  NULL,
    [ProcessedFilesID] INT            NULL,
    [LogEventTypeID]   INT            NULL,
    [LogSourceID]      INT            NULL,
    [LogURLOriginID]   INT            NULL,
    [SoapCallNumber]   NCHAR (10)     NULL,
    [SoapParameterID]  INT            NULL,
    [Data]             NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_LogEntry] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_LogEntry_LogEventType] FOREIGN KEY ([LogEventTypeID]) REFERENCES [dbo].[LogEventType] ([ID]),
    CONSTRAINT [FK_LogEntry_LogSource] FOREIGN KEY ([LogSourceID]) REFERENCES [dbo].[LogSource] ([ID]),
    CONSTRAINT [FK_LogEntry_LogURLOrigin] FOREIGN KEY ([LogURLOriginID]) REFERENCES [dbo].[LogURLOrigin] ([ID]),
    CONSTRAINT [FK_LogEntry_ProcessedFiles] FOREIGN KEY ([ProcessedFilesID]) REFERENCES [dbo].[ProcessedFiles] ([ID]),
    CONSTRAINT [FK_LogEntry_SoapParameters] FOREIGN KEY ([SoapParameterID]) REFERENCES [dbo].[SoapParameters] ([ID])
);








GO
CREATE CLUSTERED INDEX [IX_LogEntry]
    ON [dbo].[LogEntry]([LogDateTime] ASC);


GO



GO



GO



GO


