USE [CKG]
GO
/****** Object:  StoredProcedure [dbo].[spPrizm_Connect_FieldBase]    Script Date: 03/24/2011 09:54:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		huhuafu
-- Create date: 2009-12-23
-- Description:	Connect to Update Flights
-- =============================================
ALTER PROCEDURE [dbo].[spPrizm_Connect_FieldBase]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select * from Prizm_Field_Values
END


