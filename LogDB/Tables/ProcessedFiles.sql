CREATE TABLE [dbo].[ProcessedFiles] (
    [ID]       INT           IDENTITY (1, 1) NOT NULL,
    [Filename] NVARCHAR (100) NOT NULL,
    [Server]   NVARCHAR (10) NOT NULL,
    CONSTRAINT [PK_ProcessedFiles] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ProcessedFiles]
    ON [dbo].[ProcessedFiles]([Server] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ProcessedFiles_1]
    ON [dbo].[ProcessedFiles]([Filename] ASC);

