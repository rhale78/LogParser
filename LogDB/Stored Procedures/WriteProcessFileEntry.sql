-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE WriteProcessFileEntry 
	-- Add the parameters for the stored procedure here
	@Filename nvarchar(75), 
	@Server nvarchar(25)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO ProcessedFiles(Filename,Server) Values (@Filename,@Server)
END