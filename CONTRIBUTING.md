# Contributing to Documentation Toolkit

Thank you for your interest in contributing to the Documentation Toolkit! This document provides guidelines and instructions for contributing.

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on what is best for the community

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in existing issues
2. Create a new issue with:
   - Clear title and description
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (OS, Python version, PowerShell version)

### Suggesting Enhancements

1. Check existing issues and discussions
2. Create a new issue with:
   - Clear description of the enhancement
   - Use case and benefits
   - Proposed implementation approach (if applicable)

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes following the coding standards below
4. Test your changes thoroughly
5. Update documentation as needed
6. Commit with clear, descriptive messages
7. Push to your fork and create a Pull Request

## Coding Standards

### PowerShell Scripts

- Use proper parameter naming (PascalCase with `-` prefix)
- Include parameter validation
- Add error handling with try-catch blocks
- Use Write-Host for user-facing messages
- Include usage examples in comments
- Validate dependencies before execution

### Python Scripts

- Follow PEP 8 style guidelines
- Use type hints where appropriate
- Include docstrings for functions
- Handle exceptions gracefully
- Use `os.path.join()` for path operations

### Documentation

- Use Markdown format
- Follow existing document structure
- Keep language clear and concise
- Update README.md for user-facing changes
- Update CHANGELOG.md for all changes

## Project Structure

```
/doc-toolkit/
    /scripts/        - PowerShell and Python scripts
    /templates/      - Document templates
    /docs/           - Project documentation
    /.cursor/        - Cursor IDE configuration
```

## Testing

Before submitting a PR:

1. Test on Windows (PowerShell 5+)
2. Verify Python scripts work with Python 3.10+
3. Test with sample documents
4. Ensure error handling works correctly
5. Validate that generated documents are correct

## Commit Messages

Use clear, descriptive commit messages:

- Use present tense ("Add feature" not "Added feature")
- First line should be concise (50 chars or less)
- Include detailed description if needed
- Reference issue numbers if applicable

Examples:
```
Add error handling to semantic-index.ps1

- Validate Python installation
- Check for required packages
- Provide helpful error messages
- Fixes #123
```

## Questions?

If you have questions, please open an issue with the "question" label.

Thank you for contributing!
