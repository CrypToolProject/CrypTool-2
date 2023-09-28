/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using VoluntLib2.ManagementLayer.Messages;

namespace VoluntLib2.Tools
{
    public enum CertificateValidationState
    {
        Valid,
        Unknown,
        Invalid
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Assertion)]
    public class CertificateService
    {
        private const string NameOfHashAlgorithm = "SHA256";
        private readonly SHA256 HashAlgorithm = SHA256Managed.Create();

        private X509Certificate2 CaCertificate;
        private RSACryptoServiceProvider CryptoServiceProvider;
        public X509Certificate2 OwnCertificate { get; private set; }
        public string OwnName { get; private set; }
        public List<string> AdminCertificateList { get; set; }
        public List<string> BannedCertificateList { get; set; }

        /// <summary>
        /// Get the instance of the CertificateService
        /// </summary>
        private static CertificateService instance = null;
        public static CertificateService GetCertificateService()
        {
            if (instance == null)
            {
                instance = new CertificateService();
            }
            return instance;
        }

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private CertificateService()
        {
            BannedCertificateList = new List<string>();
            AdminCertificateList = new List<string>();
        }

        /// <summary>
        /// Init the service with CA and own certificate
        /// </summary>
        /// <param name="caCertificate"></param>
        /// <param name="ownCertificate"></param>
        public void Init(X509Certificate2 caCertificate, X509Certificate2 ownCertificate)
        {
            CaCertificate = caCertificate;
            OwnCertificate = ownCertificate;

            if (!IsValidCertificate(ownCertificate))
            {
                throw new InvalidDataException("Own Certificate is not valid!");
            }

            if (!ownCertificate.HasPrivateKey)
            {
                throw new KeyNotFoundException("Private Key in own certificate not found!");
            }

            //create the CryptoServiceProvider using the private key
            CryptoServiceProvider = (RSACryptoServiceProvider)ownCertificate.PrivateKey;
            if (CryptoServiceProvider.CspKeyContainerInfo.ProviderType == 1)
            {
                //if the Certificate's CryptoServiceProvider does not support SHA2 create a new CSP that does
                CryptoServiceProvider = new RSACryptoServiceProvider(new CspParameters
                {
                    KeyContainerName = CryptoServiceProvider.CspKeyContainerInfo.KeyContainerName,
                    KeyNumber = CryptoServiceProvider.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange ? 1 : 2
                })
                { PersistKeyInCsp = true };
            }
            OwnName = GetSubjectNameFromCertificate(ownCertificate);

            AdminCertificateList = new List<string>();
            BannedCertificateList = new List<string>();
        }

        public string GetSubjectNameFromCertificate(X509Certificate2 cert)
        {
            return cert.SubjectName.Name != null ? cert.SubjectName.Name.Split('=').Last() : "";
        }

        /// <summary>
        /// Verifies the signature of the given message.
        /// Also checks, if the certificate is banned
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CertificateValidationState VerifySignature(Message message)
        {
            //extract certificate 
            X509Certificate2 senderCertificate = ExtractValidCertificate(message);
            if (senderCertificate == null)
            {
                return CertificateValidationState.Invalid;
            }

            if (IsBannedCertificate(senderCertificate))
            {
                return CertificateValidationState.Invalid;
            }

            //extract signature and replace with empty signature
            byte[] originalSignature = message.MessageHeader.SignatureData;
            message.MessageHeader.SignatureData = new byte[0];
            byte[] data = message.Serialize(false);

            // Verify the signature with the hash
            RSACryptoServiceProvider provider = (RSACryptoServiceProvider)senderCertificate.PublicKey.Key;
            bool valid = provider.VerifyData(data, NameOfHashAlgorithm, originalSignature);

            message.MessageHeader.SignatureData = originalSignature;
            if (valid)
            {
                return CertificateValidationState.Valid;
            }
            return CertificateValidationState.Invalid;
        }

        /// <summary>
        /// Verifies if the given signature of the given data is valid.
        /// Also checks, if the certificate is banned or invalid
        /// </summary>
        /// <param name="data"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public CertificateValidationState VerifySignature(byte[] data, byte[] signature, X509Certificate2 certificate)
        {
            if (IsBannedCertificate(certificate))
            {
                return CertificateValidationState.Invalid;
            }
            if (!IsValidCertificate(certificate))
            {
                return CertificateValidationState.Invalid;
            }

            // Verify the signature with the hash
            RSACryptoServiceProvider provider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
            bool valid = provider.VerifyData(data, NameOfHashAlgorithm, signature);

            if (valid)
            {
                return CertificateValidationState.Valid;
            }
            return CertificateValidationState.Invalid;
        }


        /// <summary>
        ///  Signs the message, adds the sendername and the certificate
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns></returns>
        public byte[] SignData(byte[] data)
        {
            return CryptoServiceProvider.SignData(data, NameOfHashAlgorithm);
        }

        /// <summary>
        ///  Determines whether the specified message has been signed by an admin.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public bool IsAdmin(Message message)
        {
            X509Certificate2 senderCertificate = ExtractValidCertificate(message);
            if (senderCertificate == null)
            {
                return false;
            }
            return IsAdminCertificate(senderCertificate);
        }

        /// <summary>
        /// Checks, if the given certificate is an admin certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public bool IsAdminCertificate(X509Certificate2 certificate)
        {
            //by name
            string senderName = GetSubjectNameFromCertificate(certificate);
            if (AdminCertificateList.Contains("N:" + senderName) || AdminCertificateList.Contains(senderName))
            {
                return true;
            }

            //by serial number
            if (AdminCertificateList.Contains("SN:" + certificate.SerialNumber) || AdminCertificateList.Contains(certificate.SerialNumber))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks, if the given certificate is banned
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public bool IsBannedCertificate(X509Certificate2 certificate)
        {
            //by name
            string senderName = GetSubjectNameFromCertificate(certificate);
            if (BannedCertificateList.Contains("N:" + senderName) || BannedCertificateList.Contains(senderName))
            {
                return true;
            }

            //by serial number
            if (BannedCertificateList.Contains("SN:" + certificate.SerialNumber) || BannedCertificateList.Contains(certificate.SerialNumber))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns a valid certificate or null
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private X509Certificate2 ExtractValidCertificate(Message message)
        {
            X509Certificate2 senderCertificate;
            try
            {
                senderCertificate = new X509Certificate2(message.MessageHeader.CertificateData);
            }
            catch
            {
                return null;
            }

            //check if its valid
            if (IsValidCertificate(senderCertificate))
            {
                return senderCertificate;
            }
            return null;
        }

        /// <summary>
        ///  Determines whether the certificate is issued by our CA or not
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns></returns> 
        public bool IsValidCertificate(X509Certificate2 certificate)
        {
            try
            {
                X509Chain chain = new X509Chain(false);
                chain.ChainPolicy.ExtraStore.Add(CaCertificate);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                if (chain.Build(certificate) == false)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(certificate.SubjectName.Name))
                {
                    //We dont accept certificates with no subject name
                    return false;
                }
                if (!chain.ChainPolicy.ExtraStore.Contains(chain.ChainElements[chain.ChainElements.Count - 1].Certificate))
                {
                    //The certificate has to be signed by our CA certificate
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Computes the SHA256 hash of the given byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] ComputeHash(byte[] data)
        {
            return HashAlgorithm.ComputeHash(data);
        }
    }
}