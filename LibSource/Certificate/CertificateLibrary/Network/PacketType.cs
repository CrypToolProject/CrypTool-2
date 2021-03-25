using System;

namespace CrypTool.CertificateLibrary.Network
{
    /// <summary>
    /// This enum represents the network packet types.
    /// </summary>
    public enum PacketType : byte
    {

        #region Client and server

        /// <summary>
        /// Invalid packet
        /// </summary>
        Invalid,

        /// <summary>
        /// Host forces a disconnect
        /// </summary>
        Disconnect,

        #endregion


        #region Packets from client

        /// <summary>
        /// Client requests the additional values required to register a new certificate
        /// </summary>
        RegistrationFormularRequest,

        /// <summary>
        /// Client wants to register a new certificate
        /// </summary>
        CertificateRegistration,

        /// <summary>
        /// Client verfies that the email address is correct
        /// </summary>
        EmailVerification,

        /// <summary>
        /// Client requests an existing certificate
        /// </summary>
        CertificateRequest,

        /// <summary>
        /// Client wants to reset the password of an existing certificate
        /// </summary>
        PasswordReset,

        /// <summary>
        /// Client verfies the password reset
        /// </summary>
        PasswordResetVerification,

        /// <summary>
        /// Client wants to change the password
        /// </summary>
        PasswordChange,

        #endregion


        #region Packets from server

        /// <summary>
        /// Server sends the additional values required to register a new certificate
        /// </summary>
        RegistrationFormular,

        /// <summary>
        /// Server sends a certificate to the client
        /// </summary>
        CertificateResponse,

        /// <summary>
        /// Server signals that the email address needs to be verfied (Verification code sent per email)
        /// </summary>
        EmailVerificationRequired,

        /// <summary>
        /// Server informs client that manual certificate authorization is enabled
        /// </summary>
        CertificateAuthorizationRequired,

        /// <summary>
        /// Server signals that the password reset needs to be validated (Verification code sent per email)
        /// </summary>
        PasswordResetVerificationRequired,

        /// <summary>
        /// Server informs client that an error occurred while processing the request (Error code supplies further details)
        /// </summary>
        ProcessingError,

        /// <summary>
        /// The server successfully deleted the registration
        /// </summary>
        RegistrationDeleted,

        /// <summary>
        /// The server informs the client that the email verification was successful
        /// </summary>
        EmailVerified

        #endregion

    }

    public static class PacketTypeCheck
    {

        /// <summary>
        /// Checks the byte for the corresponding PacketType. Will return PacketType.Invalid, if the byte is no real PacketType.
        /// </summary>
        /// <param name="type">the byte to check</param>
        /// <returns>true if this is a valid PacketType, false otherwise</returns>
        public static PacketType Parse(byte type)
        {
            try
            {
                return (PacketType)Enum.Parse(typeof(PacketType), type.ToString());
            }
            catch
            {
                return PacketType.Invalid;
            }
        }
    }
}
