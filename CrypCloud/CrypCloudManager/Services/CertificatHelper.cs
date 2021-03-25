using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates; 

namespace CrypCloud.Manager
{
    public class CertificatHelper
    {
        public static readonly string DefaultUserCertificatDir = CreatePathInAppdata("CryptCloud", "Certificates");

        private const string CertFileExtention = ".pfx";

        public static void CreateDirectory()
        {
            Directory.CreateDirectory(DefaultUserCertificatDir);
        }

        public static bool DoesDirectoryExists()
        {
            return Directory.Exists(DefaultUserCertificatDir);
        }


        public static List<string> GetNamesOfKnownCertificats()
        {
            if ( ! DoesDirectoryExists())
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(DefaultUserCertificatDir, "*" + CertFileExtention);
            var usernames = files.Select(FullPathToFilename);
            return usernames.ToList(); 
        } 

        public static X509Certificate2 LoadPrivateCertificat(string name, SecureString password)
        {
            try
            {
                var fromCertFile = new X509Certificate2(CreatePathToUserCertificat(name), password);
                return fromCertFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool UserCertificatIsUnknown(string username)
        {
            return ! File.Exists(CreatePathToUserCertificat(username));
        }

        #region pathHelper

        private static string FullPathToFilename(string fullPath)
        {
            return fullPath.Replace(DefaultUserCertificatDir, "").Replace(CertFileExtention, "").Replace("\\", "");
        }

        public static string CreatePathInAppdata(params string[] folders)
        {
            var pathToAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var pathInAppdata = "";
            foreach (var folder in folders)
            {
                pathInAppdata += folder + Path.DirectorySeparatorChar;
            }

            return Path.Combine(pathToAppdata, pathInAppdata);
        }

        private static string CreatePathToUserCertificat(string username)
        {
           
            return DefaultUserCertificatDir + Path.DirectorySeparatorChar + username + CertFileExtention;
        }

        #endregion

       
    }
}
