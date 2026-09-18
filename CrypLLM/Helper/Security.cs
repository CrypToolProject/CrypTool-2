/*                              
   Copyright 2026 Marc Philipp Kray, CrypTool Project

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

/// <summary>
/// This file implements cryptographic utility functions utilizing the Windows Data Protection API (DPAPI).
/// It provides secure, user-scoped encryption for sensitive configurational data,
/// such as LLM provider API keys, ensuring they are protected at rest within the host file system.
/// </summary>

using System;
using System.Security.Cryptography;
using System.Text;

namespace CrypTool.CrypLLM
{
    /// <summary>
    /// Provides abstracted encryption and decryption routines relying on DPAPI.
    /// By binding cryptographic secrets to the current operating system user profile, 
    /// it mitigates exposure risks without requiring manual, hardcoded key management.
    /// </summary>
    public static class SecretProtector
    {
        /// <summary>
        /// Encrypts plaintext data and encodes the resulting ciphertext into a Base64 string.
        /// </summary>
        /// <param name="plainText">The unencrypted sensitive data (e.g., an API token).</param>
        /// <returns>A Base64-encoded string representing the user-scoped ciphertext.</returns>
        public static string EncryptToBase64(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText ?? string.Empty);

            // DPAPI protection scoped to CurrentUser, integrating implicitly with the Windows logon credential.
            var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        /// <summary>
        /// Attempts to decrypt a Base64-encoded ciphertext bound to the current user.
        /// Configured to fail gracefully during initialization when the underlying payload 
        /// is missing, corrupted, or originating from an unauthorized user context.
        /// </summary>
        /// <param name="input">The Base64-encoded ciphertext string.</param>
        /// <returns>The decrypted plaintext string, or an empty string upon decryption failure.</returns>
        public static string TryDecryptOrReturn(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            try
            {
                var data = Convert.FromBase64String(input);
                var unprotected = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(unprotected);
            }
            catch
            {
                // Suppresses CryptographicException to prevent application hard-crashes on corrupted state.
                return string.Empty;
            }
        }
    }
}
