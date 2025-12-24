# Data Model Document
## Documentation Toolkit

**Version**: 1.0  
**Date**: 2024  
**Status**: Current

---

## 1. Overview

This document describes the data models, entities, and data structures used in the Documentation Toolkit. The toolkit uses lightweight data models for semantic indexing, knowledge graphs, and search results. All data is stored in local file system (JSON, binary formats) with no external database dependencies.

---

## 2. Entities

| Entity | Description |
|--------|-------------|
| **IndexEntry** | Represents a single chunk of text in the semantic index with its associated metadata |
| **SearchResult** | Represents a search result with file information, relevance score, and text chunk |
| **GraphData** | Complete knowledge graph structure containing nodes, edges, and statistics |
| **GraphNode** | Individual node in the knowledge graph (file, entity, or topic) |
| **GraphEdge** | Relationship between nodes in the knowledge graph |
| **GraphStats** | Statistics about the knowledge graph (file count, entity count, topic count) |
| **BaseEvent** | Base class for all events in the event bus system |
| **IndexBuiltEvent** | Event published when semantic index is built |
| **GraphBuiltEvent** | Event published when knowledge graph is built |
| **SummaryCreatedEvent** | Event published when summary is created |
| **DocumentProcessedEvent** | Event published for each document processed during indexing |

---

## 3. Attributes

### IndexEntry

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| IndexEntry | File | string | Filename of the source document |
| IndexEntry | Path | string | Full path to the source document |
| IndexEntry | Chunk | string | Text chunk content |
| IndexEntry | Index | int | Zero-based index of the chunk within the file |

### SearchResult

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| SearchResult | File | string | Filename of the matching document |
| SearchResult | Path | string | Full path to the matching document |
| SearchResult | Score | double | Cosine similarity score (0.0 to 1.0) |
| SearchResult | Chunk | string | Text chunk that matched the query |

### GraphData

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| GraphData | Nodes | GraphNodes | Collection of all graph nodes (files, entities, topics) |
| GraphData | Edges | List<GraphEdge> | List of relationships between nodes |
| GraphData | Stats | GraphStats | Statistics about the graph |

### GraphNode

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| GraphNode | Id | string | Unique identifier for the node |
| GraphNode | Type | string | Node type ("file", "entity", "topic") |
| GraphNode | Name | string | Display name of the node |
| GraphNode | Path | string? | File path (for file nodes only, nullable) |

### GraphEdge

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| GraphEdge | Type | string | Relationship type (e.g., "FILE_CONTAINS_ENTITY") |
| GraphEdge | From | string | Source node ID |
| GraphEdge | To | string | Target node ID |
| GraphEdge | Weight | int? | Relationship weight (nullable) |
| GraphEdge | File | string? | Source file path (nullable) |

### GraphStats

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| GraphStats | NumFiles | int | Number of file nodes |
| GraphStats | NumEntities | int | Number of entity nodes |
| GraphStats | NumTopics | int | Number of topic nodes |

### BaseEvent

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| BaseEvent | EventId | string | Unique event identifier (GUID) |
| BaseEvent | Timestamp | DateTime | Event timestamp |
| BaseEvent | EventType | string | Type of event |

### IndexBuiltEvent

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| IndexBuiltEvent | IndexPath | string | Path to the built index |
| IndexBuiltEvent | EntryCount | int | Number of index entries |
| IndexBuiltEvent | VectorCount | int | Number of vectors |

### GraphBuiltEvent

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| GraphBuiltEvent | GraphPath | string | Path to the built graph |
| GraphBuiltEvent | NodeCount | int | Number of nodes |
| GraphBuiltEvent | EdgeCount | int | Number of edges |

### SummaryCreatedEvent

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| SummaryCreatedEvent | SummaryPath | string | Path to the created summary |
| SummaryCreatedEvent | FileCount | int | Number of files summarized |

### DocumentProcessedEvent

| Entity | Attribute | Type | Description |
|--------|-----------|------|-------------|
| DocumentProcessedEvent | FilePath | string | Path to the processed file |
| DocumentProcessedEvent | FileSize | long | Size of the file in bytes |
| DocumentProcessedEvent | FileType | string | File extension |
| DocumentProcessedEvent | CharacterCount | int | Number of characters extracted |

---

## 4. Relationships

### IndexEntry Relationships
- **1:N** with Source File: One source file can have multiple index entries (chunks)
- **1:1** with Vector: Each index entry has one corresponding embedding vector

### SearchResult Relationships
- **N:1** with IndexEntry: Multiple search results can reference the same index entry
- **1:1** with Source File: Each search result references one source file

### GraphData Relationships
- **1:N** with GraphNode: One graph contains many nodes
- **1:N** with GraphEdge: One graph contains many edges
- **1:1** with GraphStats: One graph has one statistics object

### GraphNode Relationships
- **N:N** with GraphNode: Nodes can have multiple relationships (edges) with other nodes
- **1:1** with Source File (for file nodes): Each file node references one source file

### GraphEdge Relationships
- **N:1** with GraphNode (From): Each edge originates from one node
- **N:1** with GraphNode (To): Each edge targets one node

### Event Relationships
- **1:N** with Event Subscribers: One event can have multiple subscribers
- **N:1** with Event Bus: All events flow through the event bus

---

## 5. Data Flows

### Semantic Indexing Flow

```
Source Files (PDF, DOCX, etc.)
    ↓
DocumentExtractionEngine
    ↓
Text Content (string)
    ↓
TextChunkingEngine
    ↓
Text Chunks (List<string>)
    ↓
EmbeddingEngine
    ↓
Embedding Vectors (float[][])
    ↓
VectorStorageAccessor
    ↓
vectors.bin (binary) + index.json (JSON)
```

### Semantic Search Flow

```
User Query (string)
    ↓
EmbeddingEngine
    ↓
Query Vector (float[])
    ↓
VectorStorageAccessor (Load)
    ↓
Index Vectors (float[][]) + Index Entries (List<IndexEntry>)
    ↓
SimilarityEngine
    ↓
Top-K Results (List<(index, score)>)
    ↓
Search Results (List<SearchResult>)
    ↓
Display to User
```

### Knowledge Graph Flow

```
Source Files (PDF, DOCX, etc.)
    ↓
DocumentExtractionEngine
    ↓
Text Content (string)
    ↓
EntityExtractionEngine
    ↓
Entities (List<string>) + Topics (List<string>)
    ↓
Graph Builder
    ↓
GraphData (Nodes + Edges)
    ↓
File System (graph.json, graph.md, graph.gv)
```

### Event Flow

```
Manager (Publisher)
    ↓
EventBus.Publish(event)
    ↓
Event Persistence (SQLite)
    ↓
Event Subscribers (Handlers)
    ↓
Action Execution
```

---

## 6. Storage & Retention

### Storage Formats

| Data Type | Format | Location | Description |
|-----------|--------|----------|-------------|
| Semantic Index | Binary + JSON | `semantic-index/vectors.bin`, `semantic-index/index.json` | Vectors in binary format, metadata in JSON |
| Knowledge Graph | JSON, Markdown, Graphviz | `knowledge-graph/graph.json`, `graph.md`, `graph.gv` | Multiple formats for different use cases |
| Generated Documents | Markdown | `docs/YYYY-MM-DD-type-name.md` | Date-prefixed filenames |
| Event Log | SQLite | `%LocalAppData%\DocToolkit\events.db` | Persistent event storage |
| Templates | Markdown | `templates/*.md` | Document templates |

### Retention Policies

- **Semantic Index**: User-controlled (no automatic deletion, can be regenerated)
- **Knowledge Graph**: User-controlled (no automatic deletion, can be regenerated)
- **Generated Documents**: User-controlled (no automatic deletion)
- **Event Log**: Retained indefinitely (user can manually delete database)
- **Templates**: Version controlled (in Git repository)

### Storage Size Estimates

- **Index Entry**: ~500 bytes (JSON) + ~1KB (vector, 384 floats × 4 bytes)
- **Graph Node**: ~200 bytes (JSON)
- **Graph Edge**: ~150 bytes (JSON)
- **Event**: ~500 bytes (SQLite row)
- **100 Documents Index**: ~150MB (vectors) + ~50KB (metadata)
- **100 Files Graph**: ~500KB (JSON)

---

## 7. Security & Privacy

### PII Handling

- **No PII Collection**: Toolkit does not collect or store personally identifiable information
- **Local Processing**: All processing occurs on local machine
- **No Telemetry**: No usage tracking or analytics

### Encryption

- **At Rest**: No encryption (local file system, user-controlled)
- **In Transit**: N/A (no network access)
- **Event Database**: SQLite database, no encryption (local only)

### Access Control

- **File System**: Standard OS file permissions
- **Event Database**: Local user access only
- **No Authentication**: Local CLI tool, no multi-user support

### Data Privacy

- **User Data**: All source documents are user-provided
- **No Cloud Upload**: All processing local, no external uploads
- **Temporary Files**: Created in system temp directory, cleaned up after execution
- **Embeddings**: Only text chunks stored, no original file content in embeddings

---

## 8. Compliance

### GDPR

- **No Personal Data Collection**: Toolkit does not collect personal data
- **User Control**: Users have full control over their data
- **Right to Deletion**: Users can delete all generated data (indexes, graphs, documents)

### SOC2

- **Not Applicable**: Local CLI tool, no cloud services

### HIPAA

- **Not Applicable**: Not a healthcare application

### General Compliance

- **Data Minimization**: Only necessary data stored (text chunks, not full documents)
- **User Consent**: Implicit (user provides source files)
- **Data Retention**: User-controlled (no automatic deletion)

---

## 9. Appendices

### JSON Schema Examples

#### IndexEntry JSON
```json
{
  "file": "document.pdf",
  "path": "/path/to/document.pdf",
  "chunk": "Text content chunk...",
  "index": 0
}
```

#### GraphData JSON
```json
{
  "nodes": {
    "file": [
      {
        "id": "file:/path/to/file.pdf",
        "type": "file",
        "name": "file.pdf",
        "path": "/path/to/file.pdf"
      }
    ],
    "entity": [
      {
        "id": "entity:ProjectName",
        "type": "entity",
        "name": "ProjectName"
      }
    ],
    "topic": [
      {
        "id": "topic:architecture",
        "type": "topic",
        "name": "architecture"
      }
    ]
  },
  "edges": [
    {
      "type": "FILE_CONTAINS_ENTITY",
      "from": "file:/path/to/file.pdf",
      "to": "entity:ProjectName",
      "weight": 5,
      "file": "/path/to/file.pdf"
    }
  ],
  "stats": {
    "num_files": 10,
    "num_entities": 25,
    "num_topics": 15
  }
}
```

### Binary Format Specifications

#### Vector Storage (vectors.bin)
- **Format**: Binary array of 32-bit floats
- **Structure**: Sequential float arrays, each array is 384 floats (embedding dimension)
- **Endianness**: Little-endian
- **Size**: Number of vectors × 384 × 4 bytes

### Data Migration

- **Index Migration**: Indexes can be regenerated from source (no migration needed)
- **Graph Migration**: Graphs can be regenerated from source (no migration needed)
- **Event Migration**: Events stored in SQLite (standard format, no migration needed)

---

**References**:
- [Technical Documentation](docs/TECHNICAL-DOCUMENTATION.md)
- [Architecture Document](docs/ARCHITECTURE-Documentation-Toolkit.md)
- [PRD](docs/PRD-Documentation-Toolkit.md)
