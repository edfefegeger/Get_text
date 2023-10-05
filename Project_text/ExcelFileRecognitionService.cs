using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO;

public class ExcelFileRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        string recognizedText = "";

        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;

            recognizedText = ReadExcelFile(memoryStream);
        }

        return recognizedText;
    }

    private string ReadExcelFile(Stream stream)
    {
        string result = "";

        try
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            result += worksheet.Cells[row, col].Text + "\t";
                        }
                        result += Environment.NewLine;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок, если не удалось прочитать файл
            result = $"Error reading Excel file: {ex.Message}";
        }

        return result;
    }
}
