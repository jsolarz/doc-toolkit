# Changelog

All notable changes to the Documentation Toolkit will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `requirements.txt` for Python dependency management
- `CHANGELOG.md` for version history tracking
- `CONTRIBUTING.md` with contribution guidelines
- `LICENSE` file (MIT License)
- `docs/design.md` with high-level system design
- `scripts/validate-setup.ps1` for environment validation
- Root `.gitignore` for Python cache and IDE files
- Error handling and Python dependency validation to all scripts
- Temporary file cleanup in semantic indexing and search scripts
- Cross-platform path handling improvements

### Fixed
- Incomplete `.gitignore` template in `init-project.ps1`
- Typo in `AGENTS.md` ("Deaign.md" → "Design.md")
- Windows path separator issues in Python scripts
- Missing `scikit-learn` dependency documentation
- Parameter naming inconsistency in `generate-doc.ps1`

### Changed
- Updated `PyPDF2` to `pypdf` in requirements (newer package name)
- Improved error messages with actionable guidance
- Enhanced path resolution in `build-knowledge-graph.ps1`

### Security
- Temporary Python files now created in system temp directory instead of project directories

## [1.0.0] - Initial Release

### Added
- Project initialization scripts
- Document generation from templates
- Semantic indexing with sentence transformers
- Semantic search functionality
- Knowledge graph generation
- Source summarization
- Multiple document templates (PRD, RFP, Tender, SOW, Architecture, Solution, SLA, Spec, API, Data)
- Cursor IDE integration with rules and snippets
