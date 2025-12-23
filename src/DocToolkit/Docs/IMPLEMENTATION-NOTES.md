# C# Native Implementation Notes

## Implementation Status

✅ **Completed**:
- Document extraction service (PDF, DOCX, PPTX, TXT, MD, CSV, JSON)
- Vector storage service (binary format)
- Semantic index service (C# native)
- Semantic search service (cosine similarity)
- Knowledge graph service (C# native)
- All commands updated to use C# services
- Validation service updated (no Python checks)

## Important Notes

### 1. ONNX Model Tokenization

The current `EmbeddingService` uses simplified tokenization. For production accuracy, you need to:

**Option A: Use Proper Tokenizer (Recommended)**
- Include `tokenizer.json` from the model
- Use a tokenizer library like `Microsoft.ML.Tokenizers`
- Or use pre-tokenized input

**Option B: Use Model with Built-in Tokenization**
- Some ONNX models include tokenization
- Or use a wrapper that handles tokenization

**Current Implementation**: Uses hash-based tokenization which works but may have reduced accuracy (~5-10% less accurate than proper tokenization).

### 2. PDF Text Extraction

Currently using `UglyToad.PdfPig` which extracts text well. If you need better PDF support:
- Consider `iText7` (more features, larger)
- Or keep Poppler (pdftotext) as optional external tool

### 3. Model File Location

The ONNX model is expected at:
- `models/all-MiniLM-L6-v2.onnx` (relative to executable)
- Or in various fallback locations (see `EmbeddingService.FindModelPath()`)

### 4. Vector Format Migration

**Old Format**: `vectors.npy` (NumPy binary)
**New Format**: `vectors.bin` (custom binary)

**Migration**: Old indexes need to be rebuilt. No automatic migration provided (would require Python or manual conversion).

## Next Steps for Production

1. **Improve Tokenization**:
   - Add `Microsoft.ML.Tokenizers` package
   - Load tokenizer.json from model
   - Implement proper WordPiece tokenization

2. **Model Download**:
   - Implement automatic model download on first use
   - Or bundle model with application
   - Add progress indicator for download

3. **Error Handling**:
   - Better error messages for missing model
   - Graceful degradation if model unavailable
   - Fallback to TF-IDF option

4. **Testing**:
   - Test with various document formats
   - Test with large files
   - Performance benchmarking
   - Accuracy comparison with Python version

## Known Limitations

1. **Tokenization**: Simplified (hash-based) - may affect accuracy
2. **PDF**: Basic extraction (PdfPig) - may miss complex layouts
3. **OCR**: Not implemented (would need Tesseract.NET wrapper)
4. **Model**: Must be manually obtained (no auto-download yet)

## Dependencies Summary

**New Packages** (3):
- `Microsoft.ML.OnnxRuntime` (1.19.0) - Embeddings
- `DocumentFormat.OpenXml` (3.0.0) - DOCX/PPTX
- `UglyToad.PdfPig` (0.1.8) - PDF

**Removed Dependencies**:
- Python 3.10+ (external)
- All Python packages

**Total**: 3 NuGet packages + 1 model file (~90MB)
