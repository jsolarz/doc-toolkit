using DocToolkit.ifx.Interfaces.IEngines;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Presentation;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for text extraction from various document formats.
/// Encapsulates algorithm volatility: extraction logic could change (new formats, improved OCR).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Document format support and extraction algorithms
/// Pattern: Pure function - accepts file path, returns extracted text
/// Service Boundary: Called by Managers (SemanticIndexManager, KnowledgeGraphManager, SummarizeManager)
/// </remarks>
public class DocumentExtractionEngine : IDocumentExtractionEngine
{
    /// <summary>
    /// Extracts text from a document file.
    /// </summary>
    /// <param name="filePath">Path to the document file</param>
    /// <returns>Extracted text, or empty string if extraction fails</returns>
    /// <remarks>
    /// Pure function: Accepts file path, returns text. File I/O is encapsulated.
    /// Supports: TXT, MD, CSV, JSON, DOCX, PPTX, PDF, Images (OCR not yet implemented)
    /// </remarks>
    public string ExtractText(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        var ext = Path.GetExtension(filePath).ToLower();

        return ext switch
        {
            ".txt" or ".md" or ".csv" or ".json" => ExtractFromText(filePath),
            ".docx" => ExtractFromDocx(filePath),
            ".pptx" => ExtractFromPptx(filePath),
            ".pdf" => ExtractFromPdf(filePath),
            ".png" or ".jpg" or ".jpeg" => ExtractFromImage(filePath),
            _ => string.Empty
        };
    }

    private string ExtractFromText(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractFromDocx(string filePath)
    {
        try
        {
            using var wordDocument = WordprocessingDocument.Open(filePath, false);
            var body = wordDocument.MainDocumentPart?.Document?.Body;
            if (body == null)
            {
                return string.Empty;
            }

            var paragraphs = body.Elements<Paragraph>()
                .Select(p => p.InnerText)
                .Where(text => !string.IsNullOrWhiteSpace(text));

            return string.Join("\n", paragraphs);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractFromPptx(string filePath)
    {
        try
        {
            using var presentationDocument = PresentationDocument.Open(filePath, false);
            var presentationPart = presentationDocument.PresentationPart;
            if (presentationPart == null)
            {
                return string.Empty;
            }

            var slides = new List<string>();
            if (presentationPart.Presentation?.SlideIdList != null)
            {
                foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<DocumentFormat.OpenXml.Presentation.SlideId>())
                {
                    if (slideId.RelationshipId?.Value != null)
                    {
                        var slidePart = presentationPart.GetPartById(slideId.RelationshipId.Value) as SlidePart;
                        if (slidePart?.Slide != null)
                        {
                            var texts = slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>()
                                .Select(t => t.Text)
                                .Where(text => !string.IsNullOrWhiteSpace(text));
                            slides.AddRange(texts);
                        }
                    }
                }
            }

            return string.Join("\n", slides);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractFromPdf(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var textParts = new List<string>();

            foreach (var page in document.GetPages())
            {
                var text = string.Join(" ", page.GetWords().Select(w => w.Text));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    textParts.Add(text);
                }
            }

            return string.Join("\n", textParts);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractFromImage(string filePath)
    {
        // OCR requires Tesseract executable (not Python)
        // For now, return empty - OCR is optional
        // Could integrate Tesseract.NET wrapper if needed
        return string.Empty;
    }
}
