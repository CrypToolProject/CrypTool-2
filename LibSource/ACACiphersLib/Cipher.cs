using System;
using System.Collections.Generic;

namespace CrypTool.ACACiphersLib
{
    abstract public class Cipher
    {
        public const string LATIN_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string LATIN_ALPHABET_WITH_BLANKSPACE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        public const string LATIN_ALPHABET_WITH_EQUAL_I_AND_J = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        public enum CipherType
        {
            AMSCO,
            AUTOKEY,
            BACONIAN,
            BAZERIES,
            BEAUFORT,
            BIFID,
            CADENUS,
            CHECKERBOARD,
            COMPLETE_COLUMNAR_TRANSPOSITION,
            CONDI,
            CM_BIFID,
            DIGRAFID,
            FOURSQUARE,
            FRANCTIONATED_MORSE,
            GRANDPRE,
            GRILLE,
            GROMARK,
            GRONSFELD,
            HEADLINES,
            HOMOPHONIC,
            INCOMPLETE_COLUMNAR,
            INTERRUPTED_KEY,
            KEY_PHRASE,
            MONOME_DINOME,
            MORBIT,
            MYSZKOWSKI,
            NICODEMUS,
            NIHILIST_SUBSTITUTION,
            NIHILIST_TRANSPOSITION,
            NULL,
            NUMBERED_KEY,
            PERIODIC_GROMARK,
            PHILLIPS,
            PHILLIPS_RC,
            PLAYFAIR,
            POLLUX,
            PORTA,
            PORTAX,
            PROGRESSIVE_KEY,
            QUAGMIRE_I,
            QUAGMIRE_II,
            QUAGMIRE_III,
            QUAGMIRE_VI,
            RAGBABY,
            RAILFENCE,
            REDEFENSE,
            ROUTE_TRANSPOSITION,
            RUNNING_KEY,
            SERIATED_PLAYFAIR,
            SLIDEFAIR,
            SWAGMAN,
            SYLLABARY,
            TRIDIGITAL,
            TRIFID,
            TRI_SQUARE,
            TWIN_BIFID,
            TWIN_TRIFID,
            TWO_SQUARE,
            VARIANT,
            VIGENERE
        }
 
        public abstract int[] Encrypt(int[] plaintext, int[] key);
        public abstract int[] Decrypt(int[] ciphertext, int[] key);

        public static Cipher CreateCipher(CipherType cipherType, string text, string key, string[] additional_parameters, string[] dictionary, List<string> parameters)
        {
            switch (cipherType)
            {
                default:
                    throw new NotImplementedException(string.Format("Cipher {0} not implemented", cipherType));
                    
                case CipherType.AMSCO:
                    return new AmscoAlgorithm();

                case CipherType.AUTOKEY:
                    return new AutokeyAlgorithm();

                case CipherType.BACONIAN:
                    return new BaconianAlgorithm();

                case CipherType.BEAUFORT:
                    return new BeaufortAlgorithm();

                case CipherType.COMPLETE_COLUMNAR_TRANSPOSITION:
                    return new CompleteColumnarTranspositionAlgorithm();

                case CipherType.GROMARK:
                    return new GromarkAlgorithm();

                case CipherType.GRONSFELD:
                    return new GronsfeldAlgorithm();

                case CipherType.HOMOPHONIC:
                    return new HomophonicAlgorithm();

                case CipherType.KEY_PHRASE:
                    return new KeyphraseAlgorithm();

                case CipherType.MYSZKOWSKI:
                    return new MyszkowskiAlgorithm();

                case CipherType.NUMBERED_KEY:
                    return new NumberedkeyAlgorithm();

                case CipherType.NULL:
                    return new NullAlgorithm(dictionary,parameters);

                case CipherType.PHILLIPS:
                    return new PhillipsAlgorithm();

                case CipherType.PLAYFAIR:
                    return new PlayfairAlgorithm();

                case CipherType.PORTA:
                    return new PortaAlgorithm();

                case CipherType.PROGRESSIVE_KEY:
                    return new ProgressiveKeyAlgorithm();

                case CipherType.QUAGMIRE_I:
                    return new QuagmireOneAlgorithm(additional_parameters);

                case CipherType.RUNNING_KEY:
                    return new RunningkeyAlgorithm();

                case CipherType.SLIDEFAIR:
                    return new SlidefairAlgorithm();

                case CipherType.VARIANT:
                    return new VariantAlgorithm();

                case CipherType.VIGENERE:
                    return new VigenereAlgorithm();

            }
        }
    }
}
