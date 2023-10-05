using Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.POIFS.FileSystem;
using System.IO;

public class DocFileRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        string recognizedText = "";

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;

                POIFSFileSystem poifs = new POIFSFileSystem(memoryStream);
                HSSFWorkbook workbook = new HSSFWorkbook(poifs);
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    HSSFSheet sheet = (HSSFSheet)workbook.GetSheetAt(i);
                    for (int rowIdx = 0; rowIdx <= sheet.LastRowNum; rowIdx++)
                    {
                        HSSFRow row = (HSSFRow)sheet.GetRow(rowIdx);
                        if (row != null)
                        {
                            for (int colIdx = 0; colIdx < row.LastCellNum; colIdx++)
                            {
                                HSSFCell cell = (HSSFCell)row.GetCell(colIdx);
                                if (cell != null)
                                {
                                    recognizedText += cell.ToString() + "\t";
                                }
                            }
                            recognizedText += Environment.NewLine;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок чтения файла DOC
            Console.WriteLine($"Error reading Word file: {ex.Message}");
        }

        return recognizedText;
    }
}
