using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;

[ApiController]
[Route("OCR")]
public class OCRController : ControllerBase
{
    private readonly IFileRecognitionService _fileRecognitionService;

    public OCRController(IFileRecognitionService fileRecognitionService)
    {
        _fileRecognitionService = fileRecognitionService;
    }

    [HttpPost("UploadFile")]
    public IActionResult UploadFile(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            try
            {
                string recognizedText = _fileRecognitionService.RecognizeText(file);
                return Ok(new { Text = recognizedText });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file: {ex.Message}");
                return BadRequest($"Error processing file: {ex.Message}");
            }
        }
        return BadRequest("No file or empty file provided.");
    }
}
