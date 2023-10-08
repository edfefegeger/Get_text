using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Tesseract;
using iText.Kernel.Pdf;
using OfficeOpenXml;
using Aspose.Slides;
using Microsoft.AspNetCore.Mvc;
using Xceed.Words.NET;
using System.Speech.Recognition;

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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            app.Run();
        }
        static string RecognizeAudioFromWav(string filePath, params string[] languages)
        {
            try
            {
                var result = "";
                var autoResetEvent = new System.Threading.AutoResetEvent(false);

                using (var recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(languages[0])))
                {
                    recognizer.LoadGrammar(new DictationGrammar());
                    recognizer.SpeechRecognized += (sender, e) =>
                    {
                        if (e.Result != null && e.Result.Text != null)
                        {
                            result += e.Result.Text + " ";
                        }
                    };

                    recognizer.SpeechRecognitionRejected += (sender, e) =>
                    {
                        if (e.Result != null && e.Result.Text != null)
                        {
                            result += $"Recognition rejected: {e.Result.Text} ";
                        }
                    };

                    recognizer.RecognizeCompleted += (sender, e) =>
                    {
                        if (e.Error != null)
                        {
                            result = e.Error.Message;
                        }

                        autoResetEvent.Set();
                    };

                    recognizer.SetInputToWaveFile(filePath);
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);

                    autoResetEvent.WaitOne();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recognizing audio: {ex.Message}");
                return $"Error recognizing audio: {ex.Message}";
            }
        }


        public static string RecognizeText(IFormFile file)
        {
            

            string? fileExtension = Path.GetExtension(file.FileName)?.ToLower();
            string recognizedText = ""; // Инициализация переменной значением по умолчанию

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;

                switch (fileExtension)
                {
                    case ".pdf":
                        // Обработка PDF (требуется библиотека iText7)
                        using (var pdfMemoryStream = new MemoryStream(memoryStream.ToArray()))
                        {
                            using (var pdfReader = new PdfReader(pdfMemoryStream))
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
                        break;

                    case ".jpg":
                    case ".png":
                        // Обработка изображений (требуется библиотека Tesseract)
                        using (var img = Pix.LoadFromMemory(memoryStream.ToArray()))
                        {
                            using (var engine = new TesseractEngine(@"C:\Users\Super PC\source\repos\Project_text\Project_text\tessdata", "rus+eng+spa+chi_sim+fra", EngineMode.Default))
                            {
                                using (var page = engine.Process(img))
                                {
                                    recognizedText = page.GetText();
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(recognizedText))
                        {
                            recognizedText = "Recognition failed";
                        }
                        break;

                    case ".txt":
                        // Чтение текстовых файлов
                        using (var reader = new StreamReader(memoryStream))
                        {
                            recognizedText = reader.ReadToEnd();
                        }
                        break;

                    case ".xls":
                        break;
                    case ".xlsx":
                        recognizedText = new ExcelFileRecognitionService().RecognizeText(file);
                        break;

                    case ".docx":
                    case ".doc":
                        // Обработка файлов Word 
                        recognizedText = new DocxFileRecognitionService().RecognizeText(file);
                        break;

                    case ".pptx":
                        using (var presentation = new Presentation(memoryStream))
                        {
                            foreach (ISlide slide in presentation.Slides)
                            {
                                var textFrames = slide.Shapes.OfType<IAutoShape>().Select(shape => shape.TextFrame);
                                foreach (var textFrame in textFrames)
                                {
                                    recognizedText += textFrame.Text + Environment.NewLine;
                                }
                            }
                        }
                        break;

                    case ".csv":
                        recognizedText = CsvFileRecognitionService.RecognizeCsv(file);
                        break;

                    case ".wav":
                    
                        var tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, memoryStream.ToArray());
                        recognizedText = RecognizeAudioFromWav(tempFilePath, "en-US", "fr-FR", "es-ES", "ru-RU");


                        break;

                    default:
                        recognizedText = "Unknown file format";
                        break;
                }
            }

            return recognizedText;
        }

        
    }
}