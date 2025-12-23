# ONNX Model Setup

The Documentation Toolkit uses ONNX Runtime for semantic embeddings. You need to obtain the ONNX model file to use semantic indexing and search features.

## Model Information

- **Model**: all-MiniLM-L6-v2
- **Dimensions**: 384
- **Size**: ~90MB
- **Format**: ONNX
- **Source**: Hugging Face (sentence-transformers)

## Getting the Model

### Option 1: Download Pre-converted ONNX Model (Recommended)

1. Visit [Hugging Face Model Hub](https://huggingface.co/models?library=sentence-transformers)
2. Search for "all-MiniLM-L6-v2"
3. Look for ONNX format or convert using `optimum` (see Option 2)

### Option 2: Convert from PyTorch (One-time)

If you have Python available for a one-time conversion:

```python
# Install required packages
pip install optimum[onnxruntime] sentence-transformers

# Convert model
from optimum.onnxruntime import ORTModelForFeatureExtraction
from transformers import AutoTokenizer

model = ORTModelForFeatureExtraction.from_pretrained(
    "sentence-transformers/all-MiniLM-L6-v2",
    export=True
)
model.save_pretrained("./all-MiniLM-L6-v2-onnx")
```

### Option 3: Use Community Pre-converted Models

Some community members provide pre-converted ONNX models. Search for "all-MiniLM-L6-v2 onnx" online.

## Installation

1. Download or convert the model
2. Create a `models/` directory in your project root or where the executable runs
3. Place `all-MiniLM-L6-v2.onnx` in the `models/` directory

```
doc-toolkit/
├── models/
│   └── all-MiniLM-L6-v2.onnx
└── src/
    └── DocToolkit/
```

## Verification

Run the validate command to check if the model is found:

```bash
doc validate
```

## Note on Tokenization

The current implementation uses simplified tokenization. For production use, you should:

1. Include the tokenizer.json from the model
2. Use proper WordPiece/BPE tokenization
3. Handle special tokens correctly

This will improve accuracy. The simplified version works but may have slightly reduced accuracy compared to the Python version.

## Alternative Models

If you need different trade-offs:

- **all-mpnet-base-v2**: Larger (420MB), more accurate
- **paraphrase-MiniLM-L6-v2**: Optimized for similarity tasks
- **all-MiniLM-L12-v2**: 12 layers vs 6 (more accurate, slower)
