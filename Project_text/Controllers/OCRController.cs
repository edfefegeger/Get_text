using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Tesseract;
using OfficeOpenXml;

namespace Project_text.Controllers
{
    public class OCRController : Controller
    {
        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string recognizedText = RecognizeText(file);
                return Json(new { Text = recognizedText });
            }
            return BadRequest();
        }

        public string RecognizeText(IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            string recognizedText = "";

            using (var engine = new TesseractEngine(@"C:\Users\Super PC\source\repos\Project_text\Project_text\tessdata", "rus+eng+spa+chi_sim+fra", EngineMode.Default))
            {
                try
                {
                    if (fileExtension == ".pdf")
                    {
                        // Обработка PDF файлов
                        using (var memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);

                            // Используйте using для объектов iTextSharp
                            using (var pdfReader = new PdfReader(memoryStream))
                            {
                                using (var pdfDocument = new PdfDocument(pdfReader))
                                {
                                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                                    {
                                        PdfPage pdfPage = pdfDocument.GetPage(i);
                                        recognizedText += PdfTextExtractor.GetTextFromPage(pdfPage);
                                    }
                                }
                            }
                        }
                    }
                    else if (fileExtension == ".jpg" || fileExtension == ".png")
                    {
                        // Обработка изображений (jpg, png)
                        using (var memoryStream = new MemoryStream())
                        {
                            file.CopyTo(memoryStream);

                            // Сохранение изображения на диск перед загрузкой в память
                            System.IO.File.WriteAllBytes("image.tiff", memoryStream.ToArray());

                            // Попытка загрузки изображения из файла
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
                        // Обработка текстовых файлов
                        using (var reader = new StreamReader(file.OpenReadStream()))
                        {
                            recognizedText = reader.ReadToEnd();
                        }
                    }
                    else if (fileExtension == ".xlsx")
                    {
                        // Обработка файлов Excel (.xlsx)
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
                    // Логирование ошибки
                    Console.WriteLine($"Error processing file: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
