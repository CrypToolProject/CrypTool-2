using System;
using System.Xml.Serialization;
using System.IO;

namespace CrypTool.CertificateLibrary.Network
{
    public abstract class Message
    {
        /// <summary>
        /// Serializes the xml message to packet data.
        /// </summary>
        /// <returns>the byte array</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte[] Serialize()
        {
            byte[] bytes = null;
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            using (MemoryStream mstream = new MemoryStream())
            {
                serializer.Serialize(mstream, this);
                bytes = mstream.ToArray();
                mstream.Close();
            }
            return bytes;
        }

        public abstract bool Deserialize(byte[] bytes);
    }

    public abstract class ClientMessage : Message
    {
        public string ProgramName { get; set; }
        public string ProgramVersion { get; set; }
        public string ProgramLocale { get; set; }
        public string OptionalInfo { get; set; }
    }


    #region Certificate registration

    /// <summary>
    /// Certificate registration payload (client packet)
    /// </summary>
    [Serializable]
    public class CertificateRegistration : ClientMessage
    {
        public CertificateRegistration()
        {
        }

        public CertificateRegistration(string avatar, string email, string world, string password)
        {
            this.Avatar = avatar;
            this.Email = email;
            this.World = world;
            this.Password = password;
        }

        public string Avatar;
        public string Email;
        public string World;
        public string Password;

        /// <summary>
        /// Deserializes the certificate registration packet data.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>true if the deserialization succeeded</returns>
        public override bool Deserialize(byte[] bytes)
        {
            CertificateRegistration content = null;
            try
            {
                using (MemoryStream mstream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CertificateRegistration));
                    content = (CertificateRegistration)serializer.Deserialize(mstream);
                    mstream.Close();
                }
                this.Avatar = content.Avatar;
                this.Email = content.Email;
                this.World = content.World;
                this.Password = content.Password;
                this.ProgramName = content.ProgramName;
                this.ProgramVersion = content.ProgramVersion;
                this.ProgramLocale = content.ProgramLocale;
                this.OptionalInfo = content.OptionalInfo;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion


    #region Email verification

    /// <summary>
    /// Email verification payload (client packet)
    /// </summary>
    [Serializable]
    public class EmailVerification : ClientMessage
    {
        public EmailVerification()
        {
        }

        public EmailVerification(string code, bool delete)
        {
            this.Code = code;
            this.Delete = delete;
        }

        // Password just for server backward compatibility. May be removed in the future.
        public string Password;
        public string Code;
        public bool Delete;

        /// <summary>
        /// Deserializes the email verification packet data.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>true if the deserialization succeeded</returns>
        public override bool Deserialize(byte[] bytes)
        {
            try
            {
                EmailVerification content = null;
                using (MemoryStream mstream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EmailVerification));
                    content = (EmailVerification)serializer.Deserialize(mstream);
                    mstream.Close();
                }
                this.Password = content.Password;
                this.Code = content.Code;
                this.Delete = content.Delete;
                this.ProgramName = content.ProgramName;
                this.ProgramVersion = content.ProgramVersion;
                this.ProgramLocale = content.ProgramLocale;
                this.OptionalInfo = content.OptionalInfo;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion


    #region Certificate request

    /// <summary>
    /// Certificate request payload (client packet)
    /// </summary>
    [Serializable]
    public class CT2CertificateRequest : ClientMessage
    {
        public CT2CertificateRequest()
        {
        }

        public CT2CertificateRequest(string avatar, string email, string password)
        {
            this.Avatar = avatar;
            this.Email = email;
            this.Password = password;
        }

        public string Avatar;
        public string Email;
        public string Password;

        /// <summary>
        /// Deserializes the certificate request packet data.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>true if the deserialization succeeded</returns>
        public override bool Deserialize(byte[] bytes)
        {
            try
            {
                CT2CertificateRequest content = null;
                using (MemoryStream mstream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CT2CertificateRequest));
                    content = (CT2CertificateRequest)serializer.Deserialize(mstream);
                    mstream.Close();
                }
                this.Avatar = content.Avatar;
                this.Email = content.Email;
                this.Password = content.Password;
                this.ProgramName = content.ProgramName;
                this.ProgramVersion = content.ProgramVersion;
                this.ProgramLocale = content.ProgramLocale;
                this.OptionalInfo = content.OptionalInfo;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion


    #region Password reset

    /// <summary>
    /// Password reset payload (client packet)
    /// </summary>
    [Serializable]
    public class PasswordReset : ClientMessage
    {
        public PasswordReset()
        {
        }

        public PasswordReset(string avatar, string email)
        {
            this.Avatar = avatar;
            this.Email = email;
        }

        public string Avatar;
        public string Email;

        /// <summary>
        /// Deserializes the password reset packet data.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>true if the deserialization succeeded</returns>
        public override bool Deserialize(byte[] bytes)
        {
            try
            {
                PasswordReset content = null;
                using (MemoryStream mstream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PasswordReset));
                    content = (PasswordReset)serializer.Deserialize(mstream);
                    mstream.Close();
                }
                this.Avatar = content.Avatar;
                this.Email = content.Email;
                this.ProgramName = content.ProgramName;
                this.ProgramVersion = content.ProgramVersion;
                this.ProgramLocale = content.ProgramLocale;
                this.OptionalInfo = content.OptionalInfo;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion


    #region Password reset verification

    /// <summary>
    /// Password reset verification payload (client packet)
    /// </summary>
    [Serializable]
    public class PasswordResetVerification : ClientMessage
    {
        public PasswordResetVerification()
        {
        }

        public PasswordResetVerification(string newPassword, string code)
        {
            this.NewPassword = newPassword;
            this.Code = code;
        }

        public string NewPassword;
        public string Code;

        /// <summary>
        /// Deserializes the password reset verification packet data.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>true if the deserialization succeeded</returns>
        public override bool Deserialize(byte[] bytes)
        {
            try
            {
                PasswordResetVerification content = null;
                using (MemoryStream mstream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PasswordResetVerification));
                    content = (PasswordResetVerification)serializer.Deserialize(mstream);
                    mstream.Close();
                }
                this.NewPassword = content.NewPassword;
                this.Code = content.Code;
                this.ProgramName = content.ProgramName;
                this.ProgramVersion = content.ProgramVersion;
                this.ProgramLocale = content.ProgramLocale;
                this.OptionalInfo = content.OptionalInfo;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion


    #region Password change

    /// <summary>
    /// Password change payload (client packet)
    /// </summary>
    [Serializable]
    public class PasswordChange : ClientMessage
    {
        public PasswordChange()
        {
        }

        public PasswordChange(string avatar, string email, string oldPassword, string newPassword)
        {
            this.Avatar = avatar;
            this.Email = email;
            this.OldPassword = oldPassword;
            this.NewPassword = newPassword;
        }

        public string Avatar;
        public string Email;
        public string OldPassword;
        public string NewPassword;

        /// <summary>
        /// Deserializes the password change packet data.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>true if the deserialization succeeded</returns>
        public override bool Deserialize(byte[] bytes)
        {
            try
            {
                PasswordChange content = null;
                using (MemoryStream mstream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PasswordChange));
                    content = (PasswordChange)serializer.Deserialize(mstream);
                    mstream.Close();
                }
                this.Avatar = content.Avatar;
                this.Email = content.Email;
                this.OldPassword = content.OldPassword;
                this.NewPassword = content.NewPassword;
                this.ProgramName = content.ProgramName;
                this.ProgramVersion = content.ProgramVersion;
                this.ProgramLocale = content.ProgramLocale;
                this.OptionalInfo = content.OptionalInfo;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion


    #region Invalid data

    /// <summary>
    /// Processing error payload (Server packet)
    /// </summary>
    [Serializable]
    public class ProcessingError : Message
    {
        public ProcessingError()
        {
            this.Type = ErrorType.Invalid;
        }

        public ProcessingError(ErrorType type, string message = null)
        {
            this.Type = type;
            this.Message = message;
        }

        public ErrorType Type;
        public string Message;

        /// <summary>
        /// Deserializes the processing error data of the packet.
        /// </summary>
        /// <param name="bytes">The input byte array</param>
        /// <returns>true if the deserialization succeeded</returns>
        public override bool Deserialize(byte[] bytes)
        {
            try
            {
                ProcessingError error = null;
                using (MemoryStream mstream = new MemoryStream(bytes))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ProcessingError));
                    error = (ProcessingError)serializer.Deserialize(mstream);
                    mstream.Close();
                }
                this.Type = error.Type;
                this.Message = error.Message;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion

}
