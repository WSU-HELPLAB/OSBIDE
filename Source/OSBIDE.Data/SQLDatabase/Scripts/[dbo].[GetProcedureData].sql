-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetProcedureData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetProcedureData]

	 @dateFrom DateTime='1-1-2000'
	,@dateTo DateTime='1-1-2050'
	,@nameToken nvarchar(255)
	,@Gender int
	,@ageFrom int
	,@ageTo int
	,@courseId int
	,@courseYear int
	,@gradeFrom decimal
	,@gradeTo decimal
	,@overallGradeFrom decimal
	,@overallGradeTo decimal
as
begin

	set nocount on;

	select s.Name, s.Age, s.Gender_Gend, s.Ethnicity_Ethn, s.Class_Class, s.Semester, s.[Year]
	from OsbideUsers u
	inner join OsbideSurvey s on s.UserInstitutionId=u.InstitutionId
	where LastVsActivity between @dateFrom and @DateTo
		and RoleValue=1


end




