-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
create PROCEDURE [dbo].[WriteSoapParameter]
	-- Add the parameters for the stored procedure here
	@Name nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO SoapParameters(SoapParameters) Values (@Name)
END