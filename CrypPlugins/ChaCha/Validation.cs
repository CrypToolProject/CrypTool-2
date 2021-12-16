using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace CrypTool.Plugins.ChaCha
{
    internal sealed class KeyValidator : ValidationAttribute
    {
        public KeyValidator(string errorMessage) : base(errorMessage)
        {
        }

        public override bool IsValid(object value)
        {
            // A key is valid if it is *exactly* 128-bit or *exactly* 256-bit.
            // We do no padding ourselves, we want the user to explicitly set his key to 128-bit or 256-bit.
            // There are multiple reasons for this:
            //   1. We could possibly confuse the user by adding zero-padding to the wrong side he expected.
            //   2. The user would maybe even not expect that a key not exactly 128-bit or 256-bit does work.
            //   3. We would still need to enforce a strict size of 256-bit for keys larger than 128-bit because
            //      we cannot expand a key larger than 128-bit to 256-bit without cropping. Again, we could crop
            //      the key in ways the user may not expect.
            byte[] inputKey = value as byte[];
            string hexKey = string.Join("", inputKey.Select(b => b.ToString("X2")));

            // Because one byte consists of 2 hexadecimal letters, we multiply by 2.
            StringLengthAttribute check128Bit = new StringLengthAttribute(16 * 2) { MinimumLength = 16 * 2 };
            StringLengthAttribute check256Bit = new StringLengthAttribute(32 * 2) { MinimumLength = 32 * 2 };
            return check128Bit.IsValid(hexKey) || check256Bit.IsValid(hexKey);
        }
    }

    internal sealed class IVValidator : ValidationAttribute
    {
        public IVValidator(string errorMessage) : base(errorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            // The initialization vector must be of the size specified by the version.
            // DJB version:     64-bit
            // IETF version:    96-bit
            // For the same reasons as with the key, we will do no padding,
            // so the IV must be already in the correct size.
            ChaCha chacha = context.ObjectInstance as ChaCha;
            Version currentVersion = ((ChaChaSettings)chacha.Settings).Version;
            byte[] inputIV = value as byte[];
            string hexIV = string.Join("", inputIV.Select(b => b.ToString("X2")));
            int maxBits = (int)currentVersion.IVBits;
            int maxBytes = maxBits / 8;

            StringLengthAttribute required = new StringLengthAttribute(maxBytes * 2) { MinimumLength = maxBytes * 2 };
            return required.IsValid(hexIV) ?
                ValidationResult.Success :
                new ValidationResult(ErrorMessage);
        }
    }

    internal sealed class InitialCounterValidator : ValidationAttribute
    {
        public InitialCounterValidator(string errorMessage) : base(errorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            // The initial counter can be any value within the limits given by the version.
            // DJB version:     64-bit
            // IETF version:    32-bit
            ChaCha chacha = context.ObjectInstance as ChaCha;
            Version currentVersion = ((ChaChaSettings)chacha.Settings).Version;
            ulong maxCounter = currentVersion.CounterBits == 32 ? uint.MaxValue : ulong.MaxValue;
            BigInteger initialCounter = (BigInteger)value;

            return initialCounter <= maxCounter ?
                ValidationResult.Success :
                new ValidationResult(ErrorMessage);
        }
    }
}