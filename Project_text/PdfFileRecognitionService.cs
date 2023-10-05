using iText.Kernel.Pdf;
using System.IO;

public class PdfFileRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using (var pdfReader = new PdfReader(memoryStream))
            {
                using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                {
                    string recognizedText = "";
                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                        PdfPage pdfPage = pdfDocument.GetPage(i);
                        recognizedText += iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfPage);
                    }
                    return recognizedText;
                }
            }
        }
    }
}
