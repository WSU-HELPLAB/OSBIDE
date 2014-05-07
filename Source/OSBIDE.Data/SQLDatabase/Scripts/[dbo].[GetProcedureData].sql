-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetProcedureData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
alter procedure [dbo].[GetProcedureData]

	 @dateFrom DateTime
	,@dateTo DateTime
	,@nameToken nvarchar(255)
	,@Gender int
	,@ageFrom int
	,@ageTo int
	,@courseId int
	,@deliverable nvarchar(255)
	,@gradeFrom decimal
	,@gradeTo decimal
as
begin

	set nocount on;

	declare @minDate datetime='1-1-2010'
	declare @anyValue int=-1

	--declare @dateFrom datetime='1-1-2000'
	--declare @dateTo datetime=getDate()
	--declare @Gender int=1
	--declare @courseId int=1
	--declare @gradeFrom decimal=60
	--declare @gradeTo decimal=100
	--declare @ageFrom int=16
	--declare @ageTo int=30
	--declare @deliverable nvarchar(255)='homework 1'
	--declare @nameToken nvarchar(255)='ro'
	
	create table #temp
	(
		UserId int,
		InstitutionId int,
		FirstName nvarchar(255),
		LastName nvarchar(255),
		Age int,
		Gender int,
		Ethnicity nvarchar(255),
		Prefix nvarchar(255),
		CourseNumber nvarchar(255),
		Season nvarchar(255),
		[Year] int,
		Deliverable nvarchar(255),
		Grade decimal,
		LastActivity datetime
	)

	insert into #temp
	select UserId=u.Id, u.InstitutionId, u.FirstName, u.LastName,
		   s.Age, Gender=u.GenderValue, Ethnicity=s.Ethnicity_Ethn,
		   c.Prefix, c.CourseNumber, c.Season, c.[Year],
		   g.Deliverable, g.Grade, LastActivity=max(e.DateReceived)
	from [dbo].[OsbideUsers] u with (nolock)
	inner join [dbo].[OsbideSurveys] s with (nolock) on s.UserInstitutionId=u.InstitutionId
													and (s.Age between @ageFrom and @ageTo
													     or @ageFrom=@anyValue and s.Age<=@ageTo
														 or @ageTo=@anyValue and s.Age>=@ageFrom
														 or @ageFrom=@anyValue and @ageTo=@anyValue)
	inner join [dbo].[EventLogs] e with (nolock) on e.SenderId=u.Id
													and (e.DateReceived between @dateFrom and @dateTo
													     or @dateFrom<@minDate and e.DateReceived<=@dateTo
														 or @dateTo<@minDate and e.DateReceived>=@dateFrom
														 or @dateFrom<@minDate and @dateTo<@minDate)
	inner join [dbo].[StudentGrades] g with (nolock) on g.StudentId=u.Id and (g.Deliverable=@deliverable or @deliverable='Any')
													and (g.Grade between @gradeFrom and @gradeTo
													     or @gradeFrom=@anyValue and g.Grade<=@gradeTo
														 or @gradeTo=@anyValue and g.Grade>=@gradeFrom
														 or @gradeFrom=@anyValue and @gradeTo=@anyValue)
	inner join [dbo].[Courses] c with (nolock) on c.Id=g.CourseId and (c.Id=@courseId or @courseId=@anyValue)
	where u.RoleValue=1 and (u.GenderValue=@Gender or @Gender=1)
	group by u.Id, u.InstitutionId, u.FirstName, u.LastName, s.Age, u.GenderValue, s.Ethnicity_Ethn, c.Prefix, c.CourseNumber, c.Year, c.Season, g.Deliverable, g.Grade

	create table #ret
	(
		UserId int,
		InstitutionId int,
		FirstName nvarchar(255),
		LastName nvarchar(255),
		Age int,
		Gender int,
		Ethnicity nvarchar(255),
		Deliverable nvarchar(255),
		Grade decimal,
		LastActivity datetime,
		Prefix nvarchar(255),
		CourseNumber nvarchar(255),
		Season nvarchar(255),
		[Year] int
	)

	if len(ltrim(rtrim(@nameToken)))=0
		insert into #ret
		select UserId, InstitutionId, FirstName , LastName, Age, Gender, Ethnicity, Deliverable, Grade, LastActivity
				,Prefix, CourseNumber, Season, [Year]=cast([Year] as varchar(4))
		from #temp
	else
	begin
		declare @sql nvarchar(2000)
		set @sql =N'insert into #ret select UserId, InstitutionId, FirstName, LastName, Age, Gender, Ethnicity, Deliverable, Grade, LastActivity'
		set @sql+=N',Prefix, CourseNumber, Season, [Year]=cast([Year] as varchar(4))'
		set @sql+=N' from #temp where firstName like ''%' + @nameToken + '%'' or LastName like ''%' + @nameToken + '%'''
		execute sp_executesql @sql
	end

	select UserId, InstitutionId, FirstName , LastName, Age, Gender, Ethnicity, Deliverable, Grade, LastActivity,Prefix, CourseNumber, Season, [Year]
	from #ret

end


/*

exec [dbo].[GetProcedureData] @dateFrom='2000-01-01 00:00:00',
@dateTo='2014-05-03 00:00:00',
@nameToken='',@Gender=1,@ageFrom=17,@ageTo=28
@courseId=-1,@deliverable=N'Any',@gradeFrom=0,@gradeTo=0

select * from [dbo].[Courses]


		select UserId=1
		,InstitutionId=1
		,FirstName='test'
		,LastName='test'
		,Age=1
		,Gender=1
		,Ethnicity='test'
		,Deliverable='test'
		,Grade=3.2
		,LastActivity=getDate()
		,Prefix='test'
		,CourseNumber='test'
		,Season='test'
		,[Year]=100



*/

