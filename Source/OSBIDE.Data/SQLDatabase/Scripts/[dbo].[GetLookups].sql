-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc get criteria lookups
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------


alter procedure [dbo].[GetAgeLookup]

as
begin

	set nocount on;
	select distinct Age as Age from [dbo].[OsbideSurveys] with (nolock) where Age is not null

end
go

alter procedure [dbo].[GetCourseLookup]

as
begin

	set nocount on;

	select CourseId=Id, CourseName=Prefix + ' ' + CourseNumber + ', ' + Season + ' ' + cast([Year] as varchar(4))
	from [dbo].[Courses]

end
go

alter procedure [dbo].[GetDeleverableLookup]

as
begin

	set nocount on;

	select distinct Deliverable from [dbo].[StudentGrades] with (nolock)

end
go








