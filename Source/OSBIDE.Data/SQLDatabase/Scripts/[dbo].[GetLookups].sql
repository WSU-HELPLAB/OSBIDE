-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc get criteria lookups
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------

create procedure [dbo].[GetAgeLookup]

as
begin

	set nocount on;
	select distinct Age as Age from [dbo].[OsbideSurveys] with (nolock) where Age is not null

end
go

-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetCourseLookup]

as
begin

	set nocount on;

	select CourseId=Id, CourseName=Prefix + ' ' + CourseNumber + ', ' + Season + ' ' + cast([Year] as varchar(4))
	from [dbo].[Courses]

end
go

-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetDeleverableLookup]

as
begin

	set nocount on;

	select distinct Deliverable from [dbo].[StudentGrades] with (nolock)

end
go








