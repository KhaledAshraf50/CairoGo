// Temporary helper to read Excel structure and understand data types
using ClosedXML.Excel;

namespace CairoGo.Data
{
    public static class ReadExcelStructure
    {
        public static Dictionary<string, object> ReadExcelFileStructure(string filePath)
        {
            var result = new Dictionary<string, object>();

            if (!File.Exists(filePath))
            {
                result["Error"] = "File not found";
                return result;
            }

            try
            {
                using var workbook = new XLWorkbook(filePath);
                
                var sheetsInfo = new List<Dictionary<string, object>>();

                foreach (var worksheet in workbook.Worksheets)
                {
                    var sheetInfo = new Dictionary<string, object>
                    {
                        ["Name"] = worksheet.Name,
                        ["RowCount"] = worksheet.RowsUsed().Count(),
                        ["ColumnCount"] = worksheet.ColumnsUsed().Count()
                    };

                    if (worksheet.RowsUsed().Count() == 0)
                    {
                        sheetInfo["Columns"] = new List<object>();
                        sheetsInfo.Add(sheetInfo);
                        continue;
                    }

                    // Read headers
                    var headerRow = worksheet.Row(1);
                    var columns = new List<Dictionary<string, object>>();

                    for (int col = 1; col <= worksheet.ColumnsUsed().Count(); col++)
                    {
                        var headerCell = headerRow.Cell(col);
                        var headerValue = headerCell.GetString().Trim();

                        if (string.IsNullOrWhiteSpace(headerValue))
                            continue;

                        var columnInfo = new Dictionary<string, object>
                        {
                            ["Index"] = col,
                            ["Name"] = headerValue
                        };

                        // Analyze first 5 rows to determine data type
                        if (worksheet.RowsUsed().Count() > 1)
                        {
                            var sampleValues = new List<object>();
                            var dataTypes = new HashSet<string>();

                            for (int row = 2; row <= Math.Min(6, worksheet.RowsUsed().Count()); row++)
                            {
                                var cell = worksheet.Row(row).Cell(col);
                                var cellValue = cell.GetString().Trim();

                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    dataTypes.Add("Text");
                                    if (sampleValues.Count < 3)
                                        sampleValues.Add(cellValue);
                                }
                                else
                                {
                                    if (cell.DataType == XLDataType.Number)
                                    {
                                        dataTypes.Add("Number");
                                        var numValue = cell.GetDouble();
                                        if (sampleValues.Count < 3)
                                            sampleValues.Add(numValue);
                                    }
                                    else if (cell.DataType == XLDataType.DateTime)
                                    {
                                        dataTypes.Add("DateTime");
                                        if (sampleValues.Count < 3)
                                            sampleValues.Add(cell.GetDateTime().ToString("yyyy-MM-dd HH:mm"));
                                    }
                                    else if (cell.DataType == XLDataType.Boolean)
                                    {
                                        dataTypes.Add("Boolean");
                                        if (sampleValues.Count < 3)
                                            sampleValues.Add(cell.GetBoolean());
                                    }
                                }
                            }

                            columnInfo["DataTypes"] = dataTypes.ToList();
                            columnInfo["SampleValues"] = sampleValues;
                        }

                        columns.Add(columnInfo);
                    }

                    sheetInfo["Columns"] = columns;
                    sheetsInfo.Add(sheetInfo);
                }

                result["Sheets"] = sheetsInfo;
                result["TotalSheets"] = workbook.Worksheets.Count;
            }
            catch (Exception ex)
            {
                result["Error"] = ex.Message;
                result["StackTrace"] = ex.StackTrace;
            }

            return result;
        }
    }
}

