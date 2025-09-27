using System;

namespace LicenseChain.Unity
{
    /// <summary>
    /// Base exception class for LicenseChain SDK
    /// </summary>
    public class LicenseChainException : Exception
    {
        public string ErrorCode { get; }
        public object Details { get; }

        public LicenseChainException(string message, string errorCode = null, object details = null) 
            : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public LicenseChainException(string message, Exception innerException, string errorCode = null, object details = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Details = details;
        }
    }

    /// <summary>
    /// Exception thrown when API requests fail
    /// </summary>
    public class ApiException : LicenseChainException
    {
        public int StatusCode { get; }

        public ApiException(string message, int statusCode, string errorCode = null, object details = null) 
            : base(message, errorCode, details)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, int statusCode, Exception innerException, string errorCode = null, object details = null) 
            : base(message, innerException, errorCode, details)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Exception thrown when network operations fail
    /// </summary>
    public class NetworkException : LicenseChainException
    {
        public NetworkException(string message, Exception innerException = null) 
            : base(message, innerException, "NETWORK_ERROR")
        {
        }
    }

    /// <summary>
    /// Exception thrown when license validation fails
    /// </summary>
    public class LicenseValidationException : LicenseChainException
    {
        public LicenseValidationException(string message, string errorCode = null, object details = null) 
            : base(message, errorCode, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown when authentication fails
    /// </summary>
    public class AuthenticationException : LicenseChainException
    {
        public AuthenticationException(string message, string errorCode = null, object details = null) 
            : base(message, errorCode, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown when configuration is invalid
    /// </summary>
    public class ConfigurationException : LicenseChainException
    {
        public ConfigurationException(string message, string errorCode = null, object details = null) 
            : base(message, errorCode, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown when rate limits are exceeded
    /// </summary>
    public class RateLimitException : LicenseChainException
    {
        public int RetryAfterSeconds { get; }

        public RateLimitException(string message, int retryAfterSeconds = 0, string errorCode = null, object details = null) 
            : base(message, errorCode, details)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
