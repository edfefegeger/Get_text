using Aspose.Slides;
using System.IO;
public class PptxFileRecognitionService : IFileRecognitionService
{
    public string RecognizeText(IFormFile file)
    {
        string recognizedText = "";

        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);

            var presentation = new Presentation(memoryStream);
            foreach (ISlide slide in presentation.Slides)
            {
                foreach (IShape shape in slide.Shapes)
                {
                    if (shape is ITextFrame textFrame)
                    {
                        recognizedText += textFrame.Text + Environment.NewLine;
                    }
                }
            }

            return recognizedText;
        }
    }
}
