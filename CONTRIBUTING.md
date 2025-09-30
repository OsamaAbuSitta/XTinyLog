# Contributing to XTinyLog

Thank you for your interest in contributing to XTinyLog! We welcome contributions from the community and are pleased to have you join us.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Community](#community)

## 📜 Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (latest version)
- [Git](https://git-scm.com/)
- A code editor ([Visual Studio](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/))

### Supported Frameworks

XTinyLog targets multiple frameworks:
- .NET Framework 4.6.1
- .NET Standard 2.0  
- .NET 6.0
- .NET 8.0

## 🛠️ Development Setup

1. **Fork the repository**
   ```bash
   # Click the "Fork" button on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/XTinyLog.git
   cd XTinyLog
   ```

2. **Add the upstream remote**
   ```bash
   git remote add upstream https://github.com/OsamaAbuSitta/XTinyLog.git
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run tests**
   ```bash
   dotnet test
   ```

6. **Verify everything works**
   ```bash
   # Run the test application
   dotnet run --project TestApp
   ```

### Project Structure

```
XTinyLog/
├── src/
│   └── XTinyLog/              # Main library source code
├── tests/
│   └── XTinyLog.Tests/        # Unit and integration tests
├── samples/
│   └── XTinyLog.Samples/      # Usage examples
├── docs/                      # Documentation files
├── assets/                    # Screenshots and media
├── .github/                   # GitHub workflows and templates
└── build/                     # Build configuration files
```

## 🤝 How to Contribute

### Reporting Bugs

Before creating bug reports, please check the [existing issues](https://github.com/OsamaAbuSitta/XTinyLog/issues) to avoid duplicates.

When creating a bug report, include:

- **Clear title and description**
- **Steps to reproduce** the issue
- **Expected vs actual behavior**
- **Environment details** (OS, .NET version, etc.)
- **Code samples** that demonstrate the issue
- **Log files or screenshots** if applicable

### Suggesting Enhancements

Enhancement suggestions are welcome! Please:

- **Check existing feature requests** first
- **Provide a clear use case** for the enhancement
- **Explain the expected behavior**
- **Consider backward compatibility**

### Contributing Code

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow our [coding standards](#coding-standards)
   - Add tests for new functionality
   - Update documentation as needed

3. **Test your changes**
   ```bash
   dotnet test
   dotnet build --configuration Release
   ```

4. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat: add new feature description"
   ```

5. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request**

## 🔄 Pull Request Process

### Before Submitting

- [ ] Code builds successfully on all target frameworks
- [ ] All tests pass
- [ ] New features have corresponding tests
- [ ] Documentation is updated
- [ ] Code follows project conventions
- [ ] Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/)

### PR Guidelines

1. **Use a clear title** that describes the change
2. **Fill out the PR template** completely
3. **Link related issues** using keywords (fixes #123)
4. **Keep PRs focused** - one feature/fix per PR
5. **Update CHANGELOG.md** if applicable
6. **Be responsive** to feedback and reviews

### Commit Message Format

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat: add async logging methods
fix: resolve thread safety issue in file writing
docs: update README with new configuration options
test: add unit tests for configuration validation
```

## 📏 Coding Standards

### C# Style Guidelines

We follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) with some additions:

#### Naming Conventions
- **PascalCase** for classes, methods, properties, and public fields
- **camelCase** for local variables and private fields
- **PascalCase** for constants
- **Interface names** start with `I` (e.g., `ILogConfiguration`)

#### Code Organization
- **One class per file** (except for small related classes)
- **Namespace matches folder structure**
- **Using statements** at the top, sorted alphabetically
- **XML documentation** for all public APIs

#### Example:
```csharp
namespace XTinyLog
{
    /// <summary>
    /// Represents a log configuration with validation.
    /// </summary>
    public class LogConfiguration : ILogConfiguration
    {
        private readonly string _logDirectory;
        
        /// <summary>
        /// Gets or sets the log directory path.
        /// </summary>
        /// <value>The directory where log files will be stored.</value>
        public string LogDirectory 
        { 
            get => _logDirectory; 
            set => _logDirectory = ValidatePath(value); 
        }
        
        private static string ValidatePath(string path)
        {
            // Validation logic here
            return path;
        }
    }
}
```

### Code Analysis

We use several analyzers to maintain code quality:

- **StyleCop.Analyzers** - Code style enforcement
- **Microsoft.CodeAnalysis.NetAnalyzers** - Code quality rules
- **SonarAnalyzer.CSharp** - Additional quality metrics

Fix any analyzer warnings before submitting PRs.

## 🧪 Testing Guidelines

### Test Structure

- **Unit Tests** - Test individual components in isolation
- **Integration Tests** - Test component interactions
- **Performance Tests** - Verify performance characteristics

### Test Naming

Use descriptive test names that explain the scenario:

```csharp
[Fact]
public void Info_WithValidMessage_WritesToLogFile()
{
    // Arrange
    var logger = XLog.Create();
    var message = "Test message";
    
    // Act
    logger.Info(message);
    
    // Assert
    // Verification logic
}
```

### Test Categories

Use attributes to categorize tests:

```csharp
[Fact]
[Trait("Category", "Unit")]
public void UnitTestExample() { }

[Fact]
[Trait("Category", "Integration")]
public void IntegrationTestExample() { }

[Fact]
[Trait("Category", "Performance")]
public void PerformanceTestExample() { }
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter "Category=Unit"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 Documentation

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Logs an informational message asynchronously.
/// </summary>
/// <param name="message">The message to log.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>A task representing the asynchronous operation.</returns>
/// <example>
/// <code>
/// await logger.InfoAsync("Operation completed successfully");
/// </code>
/// </example>
public async Task InfoAsync(string message, CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### README Updates

When adding new features:

1. Update the main README.md
2. Add code examples
3. Update the feature list
4. Add any new configuration options

### Changelog

Update CHANGELOG.md following [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
## [Unreleased]

### Added
- New async logging methods
- Configuration validation

### Changed
- Improved error handling

### Fixed
- Thread safety issue in file operations
```

## 🌟 Recognition

Contributors will be recognized in:

- **README.md** - Contributors section
- **Release notes** - Major contributions
- **GitHub releases** - Acknowledgments

## 💬 Community

### Getting Help

- **GitHub Issues** - Bug reports and feature requests
- **GitHub Discussions** - Questions and general discussion
- **Stack Overflow** - Tag questions with `xtinylog`

### Communication Guidelines

- Be respectful and inclusive
- Provide context and details
- Search existing issues/discussions first
- Use clear, descriptive titles

## 📞 Contact

- **Project Maintainer**: [Osama Abu-Sitta](https://github.com/OsamaAbuSitta)
- **Email**: [Create an issue for contact](https://github.com/OsamaAbuSitta/XTinyLog/issues)

## 📄 License

By contributing to XTinyLog, you agree that your contributions will be licensed under the [MIT License](LICENSE).

---

Thank you for contributing to XTinyLog! 🎉