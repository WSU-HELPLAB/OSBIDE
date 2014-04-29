using System;

namespace OSBIDE.Data.SQLDatabase
{
    public class ExcelImport
    {
        public static void UploadGrades(string fileLocation, string fileExtension, int createdBy)
        {
            ExcelToSQL.Execute(fileLocation,
                                fileExtension,
                                SQLTemplateGrades.Template, "{0},'{1}'",
                                new string[] { createdBy.ToString(), DateTime.Now.ToString() });
        }
        public static void UploadSurveys(string fileLocation, string fileExtension, int surveyYear, string surveySemester, int createdBy)
        {
            ExcelToSQL.Execute(fileLocation,
                                fileExtension,
                                SQLTemplateSurveys.Template, "{0},'{1}',{2},'{3}'",
                                new string[] { surveyYear.ToString(), surveySemester, createdBy.ToString(), DateTime.Now.ToString() });
        }
    }
}