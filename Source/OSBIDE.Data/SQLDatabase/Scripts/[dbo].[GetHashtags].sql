-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetHashtags]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
create procedure [dbo].[GetHashtags]

	 @tag varchar(800)
	,@ishandle bit
as
begin

	set nocount on;

	if @ishandle=1
		select Id, Tag=[FirstName]+[LastName], IsHandle=cast(1 as bit) from [dbo].[OsbideUsers] where [FirstName]+[LastName] like @tag
	else
		select Id, Tag=Content, IsHandle=cast(0 as bit) from [dbo].[HashTags] where [Content] like @tag

end