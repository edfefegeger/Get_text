using Aspose.Words;

public class DocxFileRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        string recognizedText = "";

        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);

            var doc = new Document(memoryStream);
            recognizedText = doc.GetText();

            return recognizedText;
        }
    }
}
