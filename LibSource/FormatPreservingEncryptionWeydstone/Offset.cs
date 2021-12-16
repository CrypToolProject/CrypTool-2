/*
 * WORK IN PROGRESS 
 */

namespace FormatPreservingEncryptionWeydstone
{

    public interface Offset
    {
        byte[] Off(byte[] K, byte[] T);
    }

    public class OFF1 : Offset
    {
        public byte[] Off(byte[] K, byte[] T)
        {
            return new byte[16];
        }
    }

    public class OFF2 : Offset
    {
        public OFF2(Ciphers ciphers)
        {
            this.ciphers = ciphers;
        }

        public OFF2()
        {
            ciphers = new Ciphers();
        }

        private readonly Ciphers ciphers;

        public byte[] Off(byte[] K, byte[] T_)
        {
            return ciphers.ciph(K, T_);
        }
    }
}
