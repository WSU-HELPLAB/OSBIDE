-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetTrends]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetTrends]

	 @num int
as
begin
	set nocount on;

	select top(@num) HashtagId, Hashtag, Counts from
	(
		select HashtagId=h.Id, Hashtag=h.Content, Counts=count(distinct e.Id)
		from [dbo].[FeedPostHashtags] t
		inner join [dbo].[HashTags] h on h.Id=t.HashtagId
		inner join [dbo].[FeedPostEvents] e on e.EventLogId = t.FeedPostId
		group by h.Id, h.Content
	)
	sub
	order by Counts desc

end