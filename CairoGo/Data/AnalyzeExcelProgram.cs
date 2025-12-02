// Run this as a simple console app to analyze the Excel file
// You can call this from Program.cs or run it separately

namespace CairoGo.Data
{
    public static class AnalyzeExcelProgram
    {
        public static void Run(string excelFilePath)
        {
            ExcelReader.AnalyzeExcelFile(excelFilePath);
        }
    }
}

