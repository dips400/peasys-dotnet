namespace Peasys
{
    /// <summary>
    /// Abstract the concept of exception encountered during the manipulation of the Peasys library.
    /// </summary>
    public abstract class PeaException : Exception
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="PeaException"/> class.
        /// </summary>
        protected internal PeaException() : base("Exception comming from the Peasys librairy") { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        protected internal PeaException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        /// <param name="innerException">Inner exception used to throw this exception.</param>
        protected internal PeaException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents the concept of exception encountered during the connexion to the AS/400 server using Peasys.
    /// </summary>
    public class PeaConnexionException : PeaException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="PeaConnexionException"/> class.
        /// </summary>
        protected internal PeaConnexionException() : base("Exception during connection to the server") { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaConnexionException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        protected internal PeaConnexionException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaConnexionException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        /// <param name="innerException">Inner exception used to throw this exception.</param>
        protected internal PeaConnexionException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents the concept of exception encountered when using invalid identifiers during the connexion to the AS/400 server using Peasys.
    /// </summary>
    public class PeaInvalidCredentialsException : PeaConnexionException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidCredentialsException"/> class.
        /// </summary>
        protected internal PeaInvalidCredentialsException() : base("The credentials that you have provided are invalid") { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidCredentialsException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        protected internal PeaInvalidCredentialsException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidCredentialsException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        /// <param name="innerException">Inner exception used to throw this exception.</param>
        protected internal PeaInvalidCredentialsException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents the concept of exception encountered when using invalid license key during the connexion to the AS/400 server using Peasys.
    /// </summary>
    public class PeaInvalidLicenseKeyException : PeaConnexionException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidLicenseKeyException"/> class.
        /// </summary>
        protected internal PeaInvalidLicenseKeyException() : base("The license key that you have provided is not valid") { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidLicenseKeyException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        protected internal PeaInvalidLicenseKeyException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidLicenseKeyException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        /// <param name="innerException">Inner exception used to throw this exception.</param>
        protected internal PeaInvalidLicenseKeyException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents the concept of exception encountered when querying the AS/400 server using Peasys.
    /// </summary>
    public class PeaQueryException : PeaException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="PeaQueryException"/> class.
        /// </summary>
        protected internal PeaQueryException() : base("There is an issue with the query that you have provided") { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaQueryException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        protected internal PeaQueryException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaQueryException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        /// <param name="innerException">Inner exception used to throw this exception.</param>
        protected internal PeaQueryException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents the concept of exception encountered when using a wrond SQL syntax for querying the AS/400 server using Peasys.
    /// </summary>
    public class PeaInvalidSyntaxQueryException : PeaQueryException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidSyntaxQueryException"/> class.
        /// </summary>
        protected internal PeaInvalidSyntaxQueryException() : base("The query that you have provided has an invalid format") { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidSyntaxQueryException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        protected internal PeaInvalidSyntaxQueryException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaInvalidSyntaxQueryException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        /// <param name="innerException">Inner exception used to throw this exception.</param>
        protected internal PeaInvalidSyntaxQueryException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents the concept of exception encountered when trying to perform an operation not yet supported by Peasys.
    /// </summary>
    public class PeaUnsupportedOperationException : PeaQueryException
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="PeaUnsupportedOperationException"/> class.
        /// </summary>
        protected internal PeaUnsupportedOperationException() : base("The operation that you are trying to do is not yet supported by Peasys") { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaUnsupportedOperationException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        protected internal PeaUnsupportedOperationException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the <see cref="PeaUnsupportedOperationException"/> class.
        /// </summary>
        /// <param name="message">Message describing the origin of the exception.</param>
        /// <param name="innerException">Inner exception used to throw this exception.</param>
        protected internal PeaUnsupportedOperationException(string message, Exception innerException) : base(message, innerException) { }
    }


    // command exception
}
