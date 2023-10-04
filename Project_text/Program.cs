using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using Tesseract;
using iText.Kernel.Pdf;
using OfficeOpenXml;
using iText.Kernel.Pdf.Canvas.Parser;

namespace Project_text
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.UseStaticFiles();

            app.MapGet("/", async context =>
            {
                await context.Response.SendFileAsync("C:/Users/Super PC/source/repos/Project_text/Project_text/Views/Home/Index.cshtml");
            });

            app.MapPost("/OCR/UploadFile", (HttpContext context) =>
            {
                var file = context.Request.Form.Files["file"];
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        string recognizedText = RecognizeText(file);
                        return Results.Json(new { Text = recognizedText });
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибки
                        Console.WriteLine($"Error processing file: {ex.Message}");
                        return Results.BadRequest($"Error processing file: {ex.Message}");
                    }
                }
                return Results.BadRequest("No file or empty file provided.");
            });

            app.MapPost("/OCR/SetHtmlDirectory", (HttpContext context) =>
            {
                var htmlDirectory = context.Request.Form["htmlDirectory"];
                // Добавьте обработку htmlDirectory (например, сохранение в переменной или конфигурации)

                return Results.Ok();
            });

            app.Run();
        }

        public static string RecognizeText(IFormFile file)
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
                            File.WriteAllBytes("image.tiff", memoryStream.ToArray());

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
