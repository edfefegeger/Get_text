using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;

public class ExcelFileRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        string fileExtension = Path.GetExtension(file.FileName)?.ToLower();
        switch (fileExtension)
        {
            case ".xlsx":
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    using (var package = new ExcelPackage(memoryStream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            string recognizedText = "";
                            int rowCount = worksheet.Dimension.Rows;
                            int colCount = worksheet.Dimension.Columns;

                            for (int row = 1; row <= rowCount; row++)
                            {
                                for (int col = 1; col <= colCount; col++)
                                {
                                    recognizedText += worksheet.Cells[row, col].Text + "\t";
                                }
                                recognizedText += Environment.NewLine;
                            }
                            return recognizedText;
                        }
                    }
                }
                return "";
            default:
                throw new NotSupportedException("File format not supported.");
        }
    }
}
