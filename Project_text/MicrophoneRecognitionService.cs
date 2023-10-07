using System.IO;
using Microsoft.AspNetCore.Http;
using System.Speech.Recognition;
using System.Speech.Synthesis;

public class MicrophoneRecognitionService : IFileRecognitionService
{
    private readonly SpeechRecognitionEngine _recognizer;
    private readonly SpeechSynthesizer _synthesizer;

    public MicrophoneRecognitionService()
    {
        _recognizer = new SpeechRecognitionEngine();
        _synthesizer = new SpeechSynthesizer();
    }

    public void StartListening()
    {
        _recognizer.SetInputToDefaultAudioDevice();
        _recognizer.LoadGrammar(new DictationGrammar());
        _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

        _recognizer.RecognizeAsync(RecognizeMode.Multiple);
    }

    private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        string recognizedText = e.Result.Text;
        // Делайте что-то с распознанным текстом
        _synthesizer.SpeakAsync("Вы сказали: " + recognizedText);
    }

    public string RecognizeText(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using (MemoryStream memoryStream = new MemoryStream())
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
        else
        {
            return "Audio file not found";
        }
    }
}
