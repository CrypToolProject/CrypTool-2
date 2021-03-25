namespace CrypTool.Plugins.T310
{

    /// <summary>
    /// This class encrypts characters by XORing and multiplying them with a matrix
    /// </summary>
    ///It represents the T310 encryption unit (Verschlüsselungseinheit)
    ///
    /// While usual stream ciphers only XOR the key stream with the data, the T310 uses a more sophisticated approach.
    /// It uses 2 registers SRV2 and SRV3 to calculate a matrix operation. After the matrix operation is done the character
    /// is XORed with the final bitstream value
    class EncryptionUnit
    {

        private byte srv2;
        private byte srv3;
        ComplexUnit complexUnit;

        /// <summary>
        /// Instantiate the T310 encryption unit
        /// </summary>
        /// <param name="complexUnit">a ready complex unit which provides the key stream</param>
        public EncryptionUnit(ComplexUnit complexUnit)
        {
            this.complexUnit = complexUnit;
        }

        /// <summary>
        /// Encrypt a 5 bit character
        /// </summary>
        /// 
        /// Instead of just XORing the keystream with the plaintext, the T310 has a more complex approach.
        /// It uses two registers srv2 and srv3, which get filled random bits and a fixed value. They then rotate
        /// within an linear feedback shift register until one register reaches the value 11111 (since it's only 5
        /// bits, it takes a maximum of 31 rounds). Then the registers switch and additional random bits get filled in.
        /// Also the plaintext character is XORed with srv3. Then it rotates a second loop, again until the value 11111
        /// is reached. After these two loops the encrypted value is taken from srv3.
        /// 
        /// <param name="c">a 5 bit character</param>
        /// <returns>the encrypted character</returns>
        public byte EncryptCharacter(byte c)
        {
            // Get 13 bits from the block cipher
            uint keystream = complexUnit.DeriveBitsFromKey();

            // fill the register srv2 with the bits 1-5 of the keystream and srv3 with 1s
            srv2 = (byte)(keystream & 0x1f); 
            srv3 = 0x01f;

            // the loop shifts both registers synchronous until srv2 has the value 11111 (or is 0)
            while ((srv2 != 0x1f) & (srv2 != 0))
            {
                srv2 = ShiftRegister(srv2);
                srv3 = ShiftRegister(srv3);
            }

            // After the first loop, the content of srv3 is moved to srv2
            // srv3 gets filled with the bits 6-11 XOR the plaintext character  
            srv2 = srv3;                           
            srv3 = (byte)((keystream >> 6) & 0x1f);       
            srv3 ^= c;

            // the loop again shifts both registers until srv2 has the value 11111 (or is 0)
            while ((srv2 != 0x1f) & (srv2 != 0))
            {
                srv2 = ShiftRegister(srv2);
                srv3 = ShiftRegister(srv3);
            }
            
            // after this process, srv3 contains the encrypted character and can be returned
            return srv3;
        }


        /// <summary>
        /// Dencrypt a 5 bit character
        /// </summary>
        /// 
        /// Decrypts a ciphertext character in similar loops as the encryption process.
        /// As the shift registers are complementary, only one loop needs to be run through.
        /// 
        /// For a more detailled description <see cref="EncryptCharacter"/>
        /// 
        /// <param name="c">a 5 bit ciphertext character</param>
        /// <returns>the decrypted character</returns>
        public byte DecryptCharacter(byte c)
        {
            // Get 13 bits from the block cipher
            uint keystream = complexUnit.DeriveBitsFromKey();

            // srv2 is filled with the bits 1-5 from the keystream; srv3 with the encrypted character 
            byte srv2 = (byte)(keystream & 0x1f);
            byte srv3 = c;

            // loop both register synchronous until srv2 reaches 11111 (or is 0)
            while ((srv2 != 0x1f) & (srv2 != 0))
            {
                srv2 = ShiftRegister(srv2);
                srv3 = ShiftRegister(srv3);
            }

            // since the values are complementary one loop and an XOR with the bits 7-11 is enough to retrieve
            // the plaintext character
            byte result = (byte)(srv3 ^ ((keystream >> 6) & 0x1f));
            return result;
        }

        /// <summary>
        /// Shift a 5 bit register with the polynom x5 = x3 + x1 + 1
        /// </summary>
        /// <param name="srv">a 5 bit register srv2 or srv3</param>
        /// <returns>the shifted register</returns>
        private byte ShiftRegister(byte srv)
        {
            int b = srv;
            return (byte)(((b << 1) | ((b >> 4) ^ (b >> 2)) & 1) & 0x1f);
        }

    }
}
