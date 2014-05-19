using System;
using System.IO;
using System.Text;

using OfficeOpenXml;

namespace OSBIDE.Data.SQLDatabase
{
    public static class ExcelToSQL
    {
        private const int BATCH_SIZE = 500;
        public static void Execute(string filePath, string fileExtension, string sqlTemplate, string paramTemplate, string[] paramList)
        {
            using (var xlPackage = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = xlPackage.Workbook.Worksheets[1];

                var query = new StringBuilder(sqlTemplate);
                var batches = worksheet.Dimension.End.Row / BATCH_SIZE;
                for (var b = 0; b < batches + 1; b++)
                {
                    var firstRow = b == 0 ? 2 : b * BATCH_SIZE;
                    var lastRow = (b + 1) * BATCH_SIZE > worksheet.Dimension.End.Row ? worksheet.Dimension.End.Row : (b + 1) * BATCH_SIZE;
                    for (var i = firstRow; i < lastRow; i++)
                    {
                        // compose a row
                        query.Append("(");

                        for (var j = 1; j < worksheet.Dimension.End.Column + 1; j++)
                        {
                            // compose a column
                            var column = worksheet.Cells[i, j].Value;

                            if (column == null)
                            {
                                query.Append("Null,");
                            }
                            else if (j == 1 || j == 3 || j == 7 || j == 8)
                            {
                                query.AppendFormat("{0},", Convert.ToInt32(column));
                            }
                            else
                            {
                                query.AppendFormat("'{0}',", column.ToString().Replace("'", "''"));
                            }
                        }
                        query.AppendFormat(paramTemplate, paramList);

                        query.Append("),");
                    }
                }

                DynamicSQLExecutor.Execute(query.ToString().TrimEnd(','));
            }
        }
    }
}
