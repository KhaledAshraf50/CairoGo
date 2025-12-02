using ClosedXML.Excel;
using System.Data;

namespace CairoGo.Data
{
    public class ExcelReader
    {
        public static void AnalyzeExcelFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine($"Analyzing Excel File: {Path.GetFileName(filePath)}");
            Console.WriteLine($"{new string('=', 80)}\n");

            try
            {
                using var workbook = new XLWorkbook(filePath);

                Console.WriteLine($"Total Sheets: {workbook.Worksheets.Count}\n");

                foreach (var worksheet in workbook.Worksheets)
                {
                    Console.WriteLine($"\n{new string('─', 80)}");
                    Console.WriteLine($"Sheet Name: {worksheet.Name}");
                    Console.WriteLine($"Total Rows: {worksheet.RowsUsed().Count()}");
                    Console.WriteLine($"Total Columns: {worksheet.ColumnsUsed().Count()}");
                    Console.WriteLine($"{new string('─', 80)}");

                    if (worksheet.RowsUsed().Count() == 0)
                    {
                        Console.WriteLine("  ⚠ Sheet is empty");
                        continue;
                    }

                    // Get header row
                    var headerRow = worksheet.Row(1);
                    var headers = new List<(int Column, string Name, string SampleValue)>();

                    Console.WriteLine("\nColumn Headers and Sample Data:");
                    Console.WriteLine($"{"#",-5} {"Column Name",-30} {"Sample Value (Row 2)",-40}");

                    for (int col = 1; col <= worksheet.ColumnsUsed().Count(); col++)
                    {
                        var headerCell = headerRow.Cell(col);
                        var headerValue = headerCell.GetString().Trim();

                        if (!string.IsNullOrWhiteSpace(headerValue))
                        {
                            // Get sample value from row 2
                            string sampleValue = "";
                            if (worksheet.RowsUsed().Count() > 1)
                            {
                                var sampleCell = worksheet.Row(2).Cell(col);
                                sampleValue = sampleCell.GetString().Trim();
                                
                                // If empty, try to get value as number/date
                                if (string.IsNullOrWhiteSpace(sampleValue))
                                {
                                    if (sampleCell.DataType == XLDataType.Number)
                                        sampleValue = $"Number: {sampleCell.GetDouble()}";
                                    else if (sampleCell.DataType == XLDataType.DateTime)
                                        sampleValue = $"Date: {sampleCell.GetDateTime()}";
                                    else if (sampleCell.DataType == XLDataType.Boolean)
                                        sampleValue = $"Boolean: {sampleCell.GetBoolean()}";
                                }

                                if (sampleValue.Length > 40)
                                    sampleValue = sampleValue.Substring(0, 37) + "...";
                            }

                            headers.Add((col, headerValue, sampleValue));
                            Console.WriteLine($"{col,-5} {headerValue,-30} {sampleValue,-40}");
                        }
                    }

                    // Analyze data types for first few rows
                    if (worksheet.RowsUsed().Count() > 1)
                    {
                        Console.WriteLine("\nData Type Analysis (first 10 rows):");
                        Console.WriteLine($"{"Column",-30} {"Data Type",-20} {"Sample Values"}");
                        Console.WriteLine($"{new string('-', 80)}");

                        foreach (var (colIndex, colName, _) in headers.Take(10))
                        {
                            var dataTypes = new HashSet<string>();
                            var sampleValues = new List<string>();

                            for (int row = 2; row <= Math.Min(11, worksheet.RowsUsed().Count()); row++)
                            {
                                var cell = worksheet.Row(row).Cell(colIndex);
                                var cellValue = cell.GetString().Trim();

                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    dataTypes.Add("Text");
                                    if (sampleValues.Count < 3)
                                        sampleValues.Add(cellValue.Length > 30 ? cellValue.Substring(0, 27) + "..." : cellValue);
                                }
                                else
                                {
                                    if (cell.DataType == XLDataType.Number)
                                    {
                                        dataTypes.Add("Number");
                                        if (sampleValues.Count < 3)
                                            sampleValues.Add(cell.GetDouble().ToString());
                                    }
                                    else if (cell.DataType == XLDataType.DateTime)
                                    {
                                        dataTypes.Add("DateTime");
                                        if (sampleValues.Count < 3)
                                            sampleValues.Add(cell.GetDateTime().ToString("yyyy-MM-dd"));
                                    }
                                    else if (cell.DataType == XLDataType.Boolean)
                                    {
                                        dataTypes.Add("Boolean");
                                        if (sampleValues.Count < 3)
                                            sampleValues.Add(cell.GetBoolean().ToString());
                                    }
                                }
                            }

                            var typeStr = string.Join(", ", dataTypes) ?? "Unknown";
                            var samplesStr = string.Join(" | ", sampleValues.Take(3)) ?? "No data";
                            
                            if (samplesStr.Length > 40)
                                samplesStr = samplesStr.Substring(0, 37) + "...";

                            Console.WriteLine($"{colName,-30} {typeStr,-20} {samplesStr}");
                        }
                    }

                    // Show first 3 data rows (excluding header)
                    if (worksheet.RowsUsed().Count() > 1)
                    {
                        Console.WriteLine("\nFirst 3 Data Rows Preview:");
                        for (int row = 2; row <= Math.Min(4, worksheet.RowsUsed().Count()); row++)
                        {
                            Console.WriteLine($"\nRow {row}:");
                            foreach (var (colIndex, colName, _) in headers.Take(5))
                            {
                                var cellValue = worksheet.Row(row).Cell(colIndex).GetString().Trim();
                                if (string.IsNullOrWhiteSpace(cellValue))
                                {
                                    var cell = worksheet.Row(row).Cell(colIndex);
                                    if (cell.DataType == XLDataType.Number)
                                        cellValue = cell.GetDouble().ToString();
                                    else if (cell.DataType == XLDataType.DateTime)
                                        cellValue = cell.GetDateTime().ToString();
                                }
                                Console.WriteLine($"  {colName}: {cellValue}");
                            }
                        }
                    }
                }

                        Console.WriteLine($"\n{new string('=', 80)}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Excel file: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}

