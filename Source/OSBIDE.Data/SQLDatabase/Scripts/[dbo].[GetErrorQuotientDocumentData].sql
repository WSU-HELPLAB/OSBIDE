-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- sproc [GetErrorQuotientDocumentData]
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
alter procedure [dbo].[GetErrorQuotientDocumentData]

	 @buildIds nvarchar(max)
as
begin

	set nocount on;

	declare @builds table(buildId int)
	insert into @builds select buildId=cast(items as int) from [dbo].[Split](@buildIds, ',')

	select distinct d.BuildId, d.DocumentId, el.[Column], el.Line
	from @builds b
	inner join [dbo].[BuildDocuments] d with(nolock) on d.BuildId=b.BuildId
	inner join [dbo].[CodeDocumentErrorListItems] celi with(nolock) on celi.CodeFileId=d.DocumentId
	inner join [dbo].[ErrorListItems] el with(nolock) on el.Id=celi.ErrorListItemId

end

