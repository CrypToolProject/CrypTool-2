using System.Security.Cryptography;

namespace CrypTool.PRESENT
{
    public abstract class PresentCipher : SymmetricAlgorithm
    {
        public PresentCipher()
        {
            KeySizeValue = 80;
            BlockSizeValue = 64;
            FeedbackSizeValue = 64;

            LegalKeySizesValue = new KeySizes[] { new KeySizes(80, 128, 48) };

            LegalBlockSizesValue = new KeySizes[] { new KeySizes(64, 64, 0) };
        }
    }
}