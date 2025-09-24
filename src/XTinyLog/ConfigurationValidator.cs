using System;
using System.Collections.Generic;
using System.IO;

namespace XTinyLog
{
    /// <summary>
    /// Provides validation for log configuration settings
    /// </summary>
    internal static class ConfigurationValidator
    {
        /// <summary>
        /// Validates the provided log configuration and returns validation results
        /// </summary>
        /// <param name="config">The configuration to validate</param>
        /// <returns>Validation result containing any errors found</returns>
        public static ValidationResult Validate(ILogConfiguration config)
        {
            var result = new ValidationResult();

            if (config == null)
            {
                result.AddError("Configuration cannot be null");
                return result;
            }

            ValidateLogDirectory(config.LogDirectory, result);
            ValidateApplicationName(config.ApplicationName, result);
            ValidateFileName(config.FileName, result);
            ValidateMaxFileSize(config.MaxFileSize, result);

            return result;
        }

        private static void ValidateLogDirectory(string logDirectory, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                result.AddError("LogDirectory cannot be null or empty");
                return;
            }

            try
            {
                // Check if the path is valid
                Path.GetFullPath(logDirectory);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                result.AddError($"LogDirectory contains invalid characters or is too long: {logDirectory}");
                return;
            }

            // Check if we can create the directory (if it doesn't exist)
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    var directoryInfo = Directory.CreateDirectory(logDirectory);
                    if (!directoryInfo.Exists)
                    {
                        result.AddError($"Unable to create log directory: {logDirectory}");
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is DirectoryNotFoundException || ex is IOException)
            {
                result.AddError($"Cannot access or create log directory '{logDirectory}': {ex.Message}");
            }
        }

        private static void ValidateApplicationName(string applicationName, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                result.AddError("ApplicationName cannot be null or empty");
                return;
            }

            // Check for invalid path characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invalidChar in invalidChars)
            {
                if (applicationName.Contains(invalidChar))
                {
                    result.AddError($"ApplicationName contains invalid character '{invalidChar}': {applicationName}");
                    return;
                }
            }

            if (applicationName.Length > 255)
            {
                result.AddError($"ApplicationName is too long (maximum 255 characters): {applicationName}");
            }
        }

        private static void ValidateFileName(Func<string> fileName, ValidationResult result)
        {
            if (fileName == null)
            {
                result.AddError("FileName function cannot be null");
                return;
            }

            try
            {
                var fileNameValue = fileName.Invoke();
                
                if (string.IsNullOrWhiteSpace(fileNameValue))
                {
                    result.AddError("FileName function cannot return null or empty string");
                    return;
                }

                // Check for invalid path characters
                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (var invalidChar in invalidChars)
                {
                    if (fileNameValue.Contains(invalidChar))
                    {
                        result.AddError($"FileName contains invalid character '{invalidChar}': {fileNameValue}");
                        return;
                    }
                }

                if (fileNameValue.Length > 255)
                {
                    result.AddError($"FileName is too long (maximum 255 characters): {fileNameValue}");
                }
            }
            catch (Exception ex)
            {
                result.AddError($"FileName function threw an exception: {ex.Message}");
            }
        }

        private static void ValidateMaxFileSize(long maxFileSize, ValidationResult result)
        {
            if (maxFileSize <= 0)
            {
                result.AddError($"MaxFileSize must be greater than 0: {maxFileSize}");
            }

            if (maxFileSize > long.MaxValue / 1024) // Prevent overflow when converting to bytes
            {
                result.AddError($"MaxFileSize is too large: {maxFileSize}");
            }
        }
    }

    /// <summary>
    /// Contains the results of configuration validation
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// Gets a value indicating whether the validation passed without errors
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// Gets the collection of validation errors
        /// </summary>
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        /// <summary>
        /// Adds an error to the validation result
        /// </summary>
        /// <param name="error">The error message to add</param>
        internal void AddError(string error)
        {
            _errors.Add(error);
        }

        /// <summary>
        /// Gets a formatted string containing all validation errors
        /// </summary>
        /// <returns>A string with all errors, or empty string if no errors</returns>
        public string GetErrorsAsString()
        {
            return string.Join(Environment.NewLine, _errors);
        }
    }
}