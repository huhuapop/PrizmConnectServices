USE [CKG]
GO
/****** Object:  StoredProcedure [dbo].[spPrizm_Connect_BaseFlights]    Script Date: 03/24/2011 09:54:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		huhuafu
-- Create date: 2009-12-23
-- Description:	Connect to Update Flights
-- =============================================
ALTER PROCEDURE [dbo].[spPrizm_Connect_BaseFlights]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @Now datetime 
	set @Now = GETDATE()
	select * from Prizm_Dailies 
	where Schedule between DATEADD(MI,-720,@Now) and DATEADD(MI,720,@Now)
END


