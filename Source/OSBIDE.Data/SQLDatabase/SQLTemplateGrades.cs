namespace OSBIDE.Data.SQLDatabase
{
    public static class SQLTemplateGrades
    {
        public const string Template = @"
insert into [dbo].[StudentGrades]
                ([StudentId]
                ,[Grade]
                ,[CourseId]
                ,[Deliverable]
                ,[CreatedBy]
                ,[CreatedOn])
                values ";
    }
}
