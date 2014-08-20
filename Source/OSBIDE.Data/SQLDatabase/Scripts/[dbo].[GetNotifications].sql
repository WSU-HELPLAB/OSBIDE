-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetNotifications]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetNotifications]

	 @user int
	,@num int
	,@getAll bit
as
begin
	set nocount on;
	select top(@num) u.FirstName, u.LastName, EventLogId=e.Id, t.Viewed, UserId=u.Id
	from [dbo].[FeedPostUserTags] t
	inner join [dbo].[EventLogs] e on t.FeedPostId = e.Id
	inner join [dbo].[OsbideUsers] u on e.SenderId = u.Id
	where t.UserId = @user and ( t.Viewed = 0 or @getAll = 1)
end
