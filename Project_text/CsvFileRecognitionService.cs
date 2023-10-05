using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;


public class CsvFileRecognitionService : IFileRecognitionService
{
    public static string RecognizeCsv(IFormFile file)
    {
        using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true // Если первая строка файла - заголовок
            };

            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<dynamic>(); // Динамический тип для обработки разных форматов CSV
                var recognizedText = new StringBuilder();

                foreach (var record in records)
                {
                    recognizedText.AppendLine(string.Join("\t", (IDictionary<string, object>)record));
                }

                return recognizedText.ToString();
            }
        }
    }

    public string RecognizeText(IFormFile file)
    {
        throw new NotImplementedException();
    }
}
