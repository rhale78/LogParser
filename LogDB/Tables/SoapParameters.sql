CREATE TABLE [dbo].[SoapParameters] (
    [ID]             INT            IDENTITY (1, 1) NOT NULL,
    [SoapParameters] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_SoapParameters] PRIMARY KEY CLUSTERED ([ID] ASC)
);

