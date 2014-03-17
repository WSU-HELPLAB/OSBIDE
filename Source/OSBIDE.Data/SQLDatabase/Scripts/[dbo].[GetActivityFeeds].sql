-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetActivityFeeds]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetActivityFeeds]

	 @DateReceivedMin datetime
	,@DateReceivedMax datetime
	,@EventLogIds nvarchar(max)
	,@EventTypes nvarchar(max)
	,@CourseId int=0
	,@RoleId int=99
	,@SenderIds nvarchar(max)
	,@MinLogId int=0
	,@MaxLogId int=0
	,@OffsetN int=0
	,@TopN int=20
as
begin

	set nocount on;

	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- Subject Eventlogs
	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	create table #events (Id int, LogType varchar(100), DateReceived datetime, SenderId int, IsResult bit)

	-- take care build events with errors
	declare @eventTypeFilters nvarchar(max); declare @buildEventIncluded bit=0
	if len(ltrim(rtrim(@EventTypes))) > 0 set @eventTypeFilters = @EventTypes
	else set @eventTypeFilters = N'''AskForHelpEvent'',''BuildEvent'',''ExceptionEvent'',''FeedPostEvent'',''HelpfulMarkGivenEvent'',''LogCommentEvent'',''SubmitEvent'''
	if (select charindex('BuildEvent', @eventTypeFilters)) > 0
	begin
		set @buildEventIncluded=1
		set @eventTypeFilters=(select replace(@eventTypeFilters,'''BuildEvent'',',''))
	end

	declare @sql nvarchar(max)
	set @sql =N'insert into #events'
	set @sql+=N' select top(' + cast(@TopN as nvarchar(64)) + ') s.Id, s.LogType, s.DateReceived, s.SenderId, 1'
	set @sql+=N' from [dbo].[EventLogs] s with (nolock)'
	set @sql+=N' inner join [dbo].[EventView] ev with (nolock) on ev.EventLogId=s.Id'
	set @sql+=N' left join (select buildErrors=count(BuildErrorTypeId), LogId from [dbo].[BuildErrors] with (nolock) group by LogId) be on s.Id=be.LogId'
	set @sql+=N' inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType>=' + cast(@RoleId as varchar(32)) + ' or ' + cast(@RoleId as varchar(32)) + '=99)'
	set @sql+=N' inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId'
	set @sql+=N' where (s.LogType in (' + @eventTypeFilters + ')'
	set @sql+=case when @buildEventIncluded=1 then N' or s.LogType=''BuildEvent'' and be.buildErrors>0)' else N')' end
	set @sql+=case when len(@SenderIds)>0 then N' and s.SenderId in (' + @SenderIds + ')' else N'' end
	set @sql+=case when len(@EventLogIds)>0 then N' and s.Id in (' + @EventLogIds + ')' else '' end
	set @sql+=case when @MinLogId>0 then N' and s.Id > ' + cast(@MinLogId as varchar(64)) else '' end
	set @sql+=case when @MaxLogId>0 then N' and s.Id < ' + cast(@MaxLogId as varchar(64)) else '' end
	set @sql+=N' and s.DateReceived > ''' + cast(@DateReceivedMin as varchar(32)) + N''''
	set @sql+=N' and s.DateReceived < ''' + cast(@DateReceivedMax as varchar(32)) + N''''
	set @sql+=N' order by s.DateReceived desc'
	exec sp_executesql @sql

	-- for comment events add their sources
	insert into #events
	select s.Id, s.LogType, s.DateReceived, s.SenderId, 0
	from [dbo].[EventLogs] s with (nolock)
	inner join [dbo].[LogCommentEvents] cs with (nolock) on cs.SourceEventLogId=s.Id
	inner join #events e on e.Id=cs.EventLogId
	inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType=@RoleId or @RoleId=-1)
	inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId
	where s.Id not in (select id from #events)

	-- for helpful mark events add their comments and comments sources
	insert into #events
	select s.Id, s.LogType, s.DateReceived, s.SenderId, 0
	from #events e
	inner join [dbo].[HelpfulMarkGivenEvents] hm with (nolock) on hm.EventLogId=e.Id
	inner join [dbo].[LogCommentEvents] cs with (nolock) on cs.EventLogId=hm.LogCommentEventId
	inner join [dbo].[EventLogs] s with (nolock) on s.Id=cs.SourceEventLogId
	inner join CourseUserRelationships cr with (nolock) on cr.UserId=s.SenderId and (cr.RoleType=@RoleId or @RoleId=-1)
	inner join [dbo].[OsbideUsers] u with (nolock) on u.Id=s.SenderId and cr.CourseId=u.DefaultCourseId
	where s.Id not in (select id from #events)

	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- Top level results
	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- EventLogs 
	select Id, LogType, DateReceived, SenderId, IsResult from #events

	-- Event and Comment Users 
	select distinct a.Id
		 , a.Email
		 , a.FirstName
		 , a.LastName
		 , a.SchoolId
		 , a.InstitutionId
		 , a.ReceiveEmailOnNewAskForHelp
		 , a.ReceiveEmailOnNewFeedPost
		 , a.ReceiveNotificationEmails
		 , a.DefaultCourseId
		 , a.LastVsActivity
		 , DefaultCourseNumber=c.CourseNumber
		 , DefaultCourseNamePrefix=c.Prefix
	from [dbo].[OsbideUsers] a with (nolock)
	inner join [dbo].[Courses] c with (nolock) on c.Id=a.DefaultCourseId
	inner join #events b on b.SenderId=a.Id

	-- ActivityComments 
	select a.Id
		 , a.EventLogId
		 , a.SourceEventLogId
		 , a.EventDate
		 , a.SolutionName
		 , a.Content
		 , c.LogType
		 , c.SenderId
		 , c.DateReceived
	from [dbo].[LogCommentEvents] a with (nolock)
	inner join #events b on b.Id=a.SourceEventLogId
	inner join [dbo].[EventLogs] c with (nolock) on c.Id=b.Id
	
	-- UserSubscriptions 
	select a.UserId
		 , a.LogId
	from [dbo].[EventLogSubscriptions] a with (nolock)
	inner join #events b on b.Id=a.LogId

	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	-- Detailed event types
	-------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------
	select a.Id, a.EventLogId, a.EventDate, a.Code, a.SolutionName, a.UserComment
	from [dbo].[AskForHelpEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.SolutionName
	from [dbo].[BuildEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.Content, a.DocumentName, a.EventAction, a.SolutionName
	from [dbo].[CutCopyPasteEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.DebugOutput, a.DocumentName, a.ExecutionAction, a.LineNumber, a.SolutionName
	from [dbo].[DebugEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.SolutionName
	from [dbo].[EditorActivityEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.DocumentName, a.ExceptionAction, a.ExceptionCode, a.ExceptionDescription, a.ExceptionName, a.ExceptionType, a.LineContent, a.LineNumber, a.SolutionName
	from [dbo].[ExceptionEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.Comment, a.SolutionName
	from [dbo].[FeedPostEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.LogCommentEventId, a.SolutionName
	from [dbo].[HelpfulMarkGivenEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.Content, a.SolutionName, a.SourceEventLogId
	from [dbo].[LogCommentEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.DocumentId, a.SolutionName
	from [dbo].[SaveEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

	select a.Id, a.EventLogId, a.EventDate, a.AssignmentId, a.SolutionName
	from [dbo].[SubmitEvents] a with (nolock)
	inner join #events b on b.Id=a.EventLogId

end


