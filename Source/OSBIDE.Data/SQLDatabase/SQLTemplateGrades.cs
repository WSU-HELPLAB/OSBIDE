namespace OSBIDE.Data.SQLDatabase
{
    public static class SQLTemplateGrades
    {
        public static string Template
        {
            get
            {
                return @"
insert into [dbo].[StudentGrades]
                ([StudentId]
                ,[Deliverable]
                ,[Grade]
                ,[CreatedBy]
                ,[CreatedOn])
                values ";
            }
        }
    }
}
