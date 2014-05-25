using System;

namespace OSBIDE.Data.SQLDatabase
{
    public class ExcelImport
    {
        public static void UploadGrades(string fileLocation, string fileExtension, int courseId, string deliverable, int createdBy)
        {
            ExcelToSQL.Execute(fileLocation,
                                fileExtension,
                                SQLTemplateGrades.Template, "{0},'{1}',{2},'{3}'",
                                new string[] { courseId.ToString(), deliverable, createdBy.ToString(), DateTime.Now.ToString() },
                                new int[] { 1, 2 });
        }
        public static void UploadSurveys(string fileLocation, string fileExtension, int courseId, int createdBy)
        {
            ExcelToSQL.Execute(fileLocation,
                                fileExtension,
                                SQLTemplateSurveys.Template, "{0},{1},'{2}'",
                                new string[] { courseId.ToString(), createdBy.ToString(), DateTime.Now.ToString() },
                                new int[] { 1, 3, 7, 8 });
        }
    }
}