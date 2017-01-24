CREATE FULLTEXT INDEX ON [dbo].[LogEntry]
    ([Data] LANGUAGE 1033)
    KEY INDEX [PK_LogEntry]
    ON [LogSearchIndex];

