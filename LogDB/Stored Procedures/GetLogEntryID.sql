-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[GetLogEntryID] 
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
	SELECT ID FROM LogEntry WHERE LogDateTime=convert(datetime2,@LogTime) AND ProcessedFilesID=@FileProcessID
	and LogEventTypeID= @LogEventTypeID
	and LogSourceID=@LogSourceID
	and LogURLOriginID=@LogURLOriginID
	and (SoapCallNumber=@SoapCallNumber and @SoapCallNumber!=NULL)
	AND Data=@Data
END