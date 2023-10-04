using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OfficeOpenXml;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Tesseract;
using iText.Kernel.Pdf;
using PdfiumViewer;

namespace Project_text.Controllers
{
    [ApiController]
    [Route("OCR")]
    public class OCRController : ControllerBase
    {
        [HttpPost("UploadFile")]
        [SwaggerOperation(Summary = "Upload a file and extract text.")]
        [SwaggerResponse(200, "Successful operation", typeof(string))]
        [SwaggerResponse(400, "Invalid input", null)]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    string recognizedText = RecognizeText(file);
                    return Ok(new { Text = recognizedText });
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    Console.WriteLine($"Error processing file: {ex.Message}");
                    return BadRequest($"Error processing file: {ex.Message}");
                }
            }
            return BadRequest("No file or empty file provided.");
        }

        [HttpGet("DownloadFile")]
        [SwaggerOperation(Summary = "Download a file with extracted text.")]
        public IActionResult DownloadFile(string text)
        {
            var byteArray = Encoding.UTF8.GetBytes(text);
            var fileStream = new MemoryStream(byteArray);

            // Указать подходящий MIME тип для вашего файла (например, "text/plain" для текстовых файлов)
            var contentType = "text/plain"; // Измените MIME тип в соответствии с типом файла, который вы возвращаете

            // Вернуть файловый результат
            return File(fileStream, contentType, "output.txt");
        }

        private string RecognizeText(IFormFile file)
        {
            string? fileExtension = Path.GetExtension(file.FileName)?.ToLower();
            string recognizedText = "";

            using (var engine = new TesseractEngine(@"C:\Users\Super PC\source\repos\Project_text\Project_text\tessdata", "rus+eng+spa+chi_sim+fra", EngineMode.Default))
            {
                try
                {
                    if (fileExtension == ".pdf")
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            using (var pdfReader = new PdfReader(memoryStream))
                            {
                                using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                                {
                                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                                    {
                                        PdfPage pdfPage = pdfDocument.GetPage(i);
                                        recognizedText += iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfPage);
                                    }
                                }
                            }
                        }
                    }
                    else if (fileExtension == ".jpg" || fileExtension == ".png")
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);
                            System.IO.File.WriteAllBytes("image.tiff", memoryStream.ToArray());
                            using (var img = Pix.LoadFromFile("image.tiff"))
                            {
                                using (var page = engine.Process(img))
                                {
                                    recognizedText = page.GetText();
                                }
                            }

                            if (string.IsNullOrWhiteSpace(recognizedText))
                            {
                                recognizedText = "Recognition failed";
                            }
                        }
                    }
                    else if (fileExtension == ".txt")
                    {
                        using (var reader = new StreamReader(file.OpenReadStream()))
                        {
                            recognizedText = reader.ReadToEnd();
                        }
                    }
                    else if (fileExtension == ".xlsx")
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);
                            using (var package = new ExcelPackage(memoryStream))
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
                                            recognizedText += worksheet.Cells[row, col].Text + "\t";
                                        }
                                        recognizedText += Environment.NewLine;
                                    }
                                }
                            }
                        }
                    }

                    return recognizedText;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
