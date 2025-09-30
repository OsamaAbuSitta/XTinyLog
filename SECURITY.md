# Security Policy

## Supported Versions

We actively support the following versions of XTinyLog with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 2.x.x   | :white_check_mark: |
| 1.x.x   | :x:                |

## Reporting a Vulnerability

The XTinyLog team takes security vulnerabilities seriously. We appreciate your efforts to responsibly disclose your findings, and will make every effort to acknowledge your contributions.

### How to Report a Security Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them using one of the following methods:

#### 1. GitHub Security Advisories (Preferred)

1. Go to the [XTinyLog repository](https://github.com/OsamaAbuSitta/XTinyLog)
2. Click on the "Security" tab
3. Click "Report a vulnerability"
4. Fill out the security advisory form with details

#### 2. Email Report

Send an email to the project maintainers with the following information:

- **Subject**: `[SECURITY] Brief description of the vulnerability`
- **Description**: Detailed description of the vulnerability
- **Steps to reproduce**: Clear steps to reproduce the issue
- **Impact**: Potential impact and severity assessment
- **Affected versions**: Which versions are affected
- **Suggested fix**: If you have suggestions for fixing the issue

**Contact**: Create a private issue in the repository requesting contact information for security reports.

### What to Include in Your Report

Please include as much of the following information as possible:

- **Type of vulnerability** (e.g., buffer overflow, SQL injection, cross-site scripting, etc.)
- **Full paths of source file(s)** related to the manifestation of the vulnerability
- **Location of the affected source code** (tag/branch/commit or direct URL)
- **Special configuration required** to reproduce the issue
- **Step-by-step instructions** to reproduce the issue
- **Proof-of-concept or exploit code** (if possible)
- **Impact of the issue**, including how an attacker might exploit it

### Response Timeline

We will acknowledge receipt of your vulnerability report within **48 hours** and will send a more detailed response within **7 days** indicating the next steps in handling your report.

After the initial reply to your report, we will:

- Confirm the problem and determine the affected versions
- Audit code to find any potential similar problems
- Prepare fixes for all supported releases
- Release patched versions as soon as possible

### Disclosure Policy

- We ask that you give us a reasonable amount of time to fix the issue before any disclosure to the public or a third party
- We will credit you in the security advisory (unless you prefer to remain anonymous)
- We will coordinate the disclosure timeline with you

### Security Update Process

When a security vulnerability is confirmed:

1. **Assessment**: We assess the severity and impact
2. **Fix Development**: We develop and test a fix
3. **Release**: We release patched versions for all supported releases
4. **Advisory**: We publish a security advisory with details
5. **Notification**: We notify users through multiple channels

### Severity Guidelines

We use the following severity levels based on [CVSS 3.1](https://www.first.org/cvss/):

| Severity | CVSS Score | Description |
|----------|------------|-------------|
| **Critical** | 9.0 - 10.0 | Vulnerabilities that can be exploited remotely without authentication |
| **High** | 7.0 - 8.9 | Vulnerabilities that can be exploited with minimal user interaction |
| **Medium** | 4.0 - 6.9 | Vulnerabilities that require significant user interaction or local access |
| **Low** | 0.1 - 3.9 | Vulnerabilities with limited impact or complex exploitation requirements |

### Security Best Practices for Users

To help keep your applications secure when using XTinyLog:

#### 1. Keep Updated
- Always use the latest version of XTinyLog
- Subscribe to security advisories for notifications
- Regularly update your dependencies

#### 2. Secure Configuration
- **Log Directory Permissions**: Ensure log directories have appropriate file system permissions
- **Sensitive Data**: Never log sensitive information (passwords, API keys, personal data)
- **File Paths**: Validate and sanitize any user-provided file paths
- **Access Control**: Restrict access to log files in production environments

#### 3. Production Considerations
```csharp
// Example of secure configuration
var secureLog = XLog.Create(config =>
{
    // Use secure directory with restricted permissions
    config.LogDirectory = @"C:\SecureLogs\MyApp";
    
    // Limit file size to prevent disk space attacks
    config.MaxFileSize = 10 * 1024 * 1024; // 10MB max
    
    // Use application-specific naming
    config.ApplicationName = () => "MySecureApp";
});

// Never log sensitive information
secureLog.Info("User login successful"); // ✅ Good
secureLog.Info($"User {username} logged in with password {password}"); // ❌ Bad
```

#### 4. HTML Output Security
- **Content Security Policy**: Implement CSP headers when serving log files via web
- **Access Control**: Protect log file URLs with authentication
- **HTTPS Only**: Always serve log files over HTTPS in production
- **Input Sanitization**: XTinyLog automatically escapes HTML content, but verify in your environment

### Known Security Considerations

#### File System Access
- XTinyLog requires write access to the configured log directory
- Log files are created with default system permissions
- Consider setting restrictive permissions on log directories

#### HTML Output
- Log content is automatically HTML-escaped to prevent XSS
- JavaScript in log files is limited to XTinyLog's interactive features
- No external resources are loaded by default

#### Thread Safety
- XTinyLog is designed to be thread-safe
- File locking mechanisms prevent corruption during concurrent access
- Async operations use proper synchronization

### Security Advisories

Security advisories for XTinyLog are published in:

- [GitHub Security Advisories](https://github.com/OsamaAbuSitta/XTinyLog/security/advisories)
- [NuGet Package Vulnerabilities](https://www.nuget.org/packages/XTinyLog)
- Release notes for security updates

### Contact Information

For security-related questions or concerns that are not vulnerabilities:

- Create an issue with the `security` label
- Participate in [GitHub Discussions](https://github.com/OsamaAbuSitta/XTinyLog/discussions)

### Legal

By reporting security vulnerabilities, you agree that:

- You will not publicly disclose the vulnerability until we have had a reasonable opportunity to address it
- You will not exploit the vulnerability for malicious purposes
- You are acting in good faith to improve the security of XTinyLog

We commit to:

- Acknowledge your report within 48 hours
- Provide regular updates on our progress
- Credit you appropriately (if desired) when we publish the security advisory
- Not pursue legal action against researchers who follow this policy

---

Thank you for helping keep XTinyLog and our users safe! 🔒