using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using Microsoft.AspNetCore.Http;

public class FileSpeechRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine())
            {
                recognizer.SetInputToWaveStream(memoryStream);
                recognizer.LoadGrammar(new DictationGrammar());

                RecognitionResult result = recognizer.Recognize();

                if (result != null)
                {
                    return result.Text;
                }
                else
                {
                    return "Recognition failed";
                }
            }
        }
    }
}
