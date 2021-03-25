using System;

namespace CrypTool.CertificateLibrary.Network
{
    public enum ErrorType : byte
    {

        /// <summary>
        /// This error is a stub for invalid entries
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The additional fields are not valid
        /// </summary>
        AdditionalFieldsIncorrect,

        /// <summary>
        /// Verification process is already finished
        /// </summary>
        AlreadyVerified,

        /// <summary>
        /// The avatar name is already registered (CertificateResponse)
        /// </summary>
        AvatarAlreadyExists,

        /// <summary>
        /// The avatar format is invalid
        /// </summary>
        AvatarFormatIncorrect,

        /// <summary>
        /// Certificate request has not been authorized by an admin yet
        /// </summary>
        CertificateNotYetAuthorized,

        /// <summary>
        /// Certificate has been revoked
        /// </summary>
        CertificateRevoked,

        /// <summary>
        /// Could not deserialize the packet
        /// </summary>
        DeserializationFailed,

        /// <summary>
        /// The email is already registered (CertificateResponse)
        /// </summary>
        EmailAlreadyExists,

        /// <summary>
        /// The email address format is invalid
        /// </summary>
        EmailFormatIncorrect,

        /// <summary>
        /// The email address was not verified yet
        /// </summary>
        EmailNotYetVerified,

        /// <summary>
        /// The server could not find the corresponding certificate for the request (Wrong email/avatar)
        /// </summary>
        NoCertificateFound,

        /// <summary>
        /// The password format is invalid
        /// </summary>
        PasswordFormatIncorrect,

        /// <summary>
        /// Request successfully processed, but the SMTP server is down. Email will arrive with delay.
        /// </summary>
        SmtpServerDown,

        /// <summary>
        /// The world format is invalid
        /// </summary>
        WorldFormatIncorrect,

        /// <summary>
        /// The entered verification code is wrong
        /// </summary>
        WrongCode,

        /// <summary>
        /// The entered password is wrong
        /// </summary>
        WrongPassword,

        /// <summary>
        /// This means that several error sources may have cause the problem. 
        /// </summary>
        Unknown
    }


    public static class ErrorTypeCheck
    {
        /// <summary>
        /// Checks the byte for the corresponding ErrorType. Will return ErrorType.Unknown, if the byte is no known ErrorType.
        /// </summary>
        /// <param name="type">the byte to check</param>
        /// <returns>true if this is a known ErrorType, false otherwise</returns>
        public static ErrorType Parse(byte type)
        {
            try
            {
                return (ErrorType)Enum.Parse(typeof(ErrorType), type.ToString());
            }
            catch
            {
                return ErrorType.Invalid;
            }
        }

    }

}
