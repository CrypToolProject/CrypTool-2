using System.Text.RegularExpressions;

namespace CrypTool.CertificateLibrary.Util
{
    public static class Verification
    {
        #region Methods to validate user input

        /// <summary>
        /// Checks if the entered value is a correct common name.
        /// Well, actually everything that is not null nor empty, is a common name.
        /// </summary>
        /// <param name="cn">The value to be checked</param>
        /// <returns>true, if the common name is correct, false otherwise</returns>
        public static bool IsValidCommonName(string cn)
        {
            if (cn == null || cn.Equals(string.Empty))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the entered value is a correct organisation.
        /// Well, actually everything that is not null nor empty, is an organisation.
        /// </summary>
        /// <param name="o">The organisation to be checked</param>
        /// <returns>true, if the organisation is correct, false otherwise</returns>
        public static bool IsValidOrganisation(string o)
        {
            if (o == null || o.Equals(string.Empty))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the entered value is a correct organisational unit.
        /// Well, actually everything that is not null nor empty, is an organisational unit.
        /// </summary>
        /// <param name="ou">The organisational unit to be checked</param>
        /// <returns>true, if the organisational unit is correct, false otherwise</returns>
        public static bool IsValidOrganisationalUnit(string ou)
        {
            if (ou == null || ou.Equals(string.Empty))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the entered value is a correct email address.  (Max string length is 60 characters)
        /// The address must match the format: foo@bar.doo
        /// </summary>
        /// <param name="email">The email address to be checked</param>
        /// <returns>true, if the email address is correct, false otherwise</returns>
        public static bool IsValidEmailAddress(string email)
        {
            if (email == null || email.Equals(string.Empty) || email.Length > 60)
            {
                return false;
            }
            Regex regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            if (!regex.IsMatch(email))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the entered value is a correct password.  (Max string length is 40 characters)
        /// Well, actually everything that is not null nor empty, is a password.
        /// </summary>
        /// <param name="password">The password to be checked</param>
        /// <returns>true, if the password is correct, false otherwise</returns>
        public static bool IsValidPassword(string password)
        {
            if (password == null || password.Equals(string.Empty) || password.Length > 40)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the entered value is a valid avatar name.  (Max string length is 40 characters)
        /// Well, actually everything that is not null nor empty, is a valid avatar.
        /// </summary>
        /// <param name="avatar">The avatar to be checked</param>
        /// <returns>true, if the avatar is correct, false otherwise</returns>
        public static bool IsValidAvatar(string avatar)
        {
            if (avatar == null || avatar.Equals(string.Empty) || avatar.Length > 40)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the entered value is a valid world name.  (Max string length is 40 characters)
        /// Well, actually everything that is not null nor empty, is a world name.
        /// </summary>
        /// <param name="world">The world name to be checked</param>
        /// <returns>true, if the world name is correct, false otherwise</returns>
        public static bool IsValidWorld(string world)
        {
            if (world == null || world.Equals(string.Empty) || world.Length > 40)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the entered value is a valid email verification code.  (Max string length is 40 characters)
        /// A valid verification code is anything that is not null or empty
        /// </summary>
        /// <param name="code">The code to be checked</param>
        /// <returns>true, if the code is not null or empty, false otherwise</returns>
        public static bool IsValidCode(string code)
        {
            if (code == null || code.Equals(string.Empty) || code.Length > 40)
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
