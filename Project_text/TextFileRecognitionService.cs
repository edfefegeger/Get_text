using System.IO;

public class TextFileRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            return reader.ReadToEnd();
        }
    }
}
