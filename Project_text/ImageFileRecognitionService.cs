using Tesseract;
using Pix = Tesseract.Pix;

public class ImageFileRecognitionService : IFileRecognitionService
{
    private readonly TesseractEngine engine;

    public ImageFileRecognitionService(string tessdataPath, string languages)
    {
        engine = new TesseractEngine(tessdataPath, languages, EngineMode.Default);
    }

    public string RecognizeText(IFormFile file)
    {
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            System.IO.File.WriteAllBytes("image.tiff", memoryStream.ToArray());
            using (var img = Pix.LoadFromFile("image.tiff"))
            {
                using (var page = engine.Process(img))
                {
                    string recognizedText = page.GetText();
                    if (string.IsNullOrWhiteSpace(recognizedText))
                    {
                        recognizedText = "Recognition failed";
                    }
                    return recognizedText;
                }
            }
        }
    }
}
