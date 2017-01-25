-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE WriteLogEntry
	-- Add the parameters for the stored procedure here
	@LogTime nvarchar(32),
	@FileProcessID int,
	@LogEventTypeID int,
	@LogSourceID int,
	@LogURLOriginID int,
	@SoapCallNumber nvarchar(10),
	@Data nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO LogEntry(LogDateTime,ProcessedFilesID,LogEventTypeID,LogSourceID,LogURLOriginID,SoapCallNumber,Data) Values (convert(datetime2,@LogTime),@FileProcessID,@LogEventTypeID,@LogSourceID,@LogURLOriginID,@SoapCallNumber,@Data)
END