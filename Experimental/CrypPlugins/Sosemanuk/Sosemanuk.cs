/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, 
   software distributed under the License is distributed on an 
   "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
   either express or implied. See the License for the specific 
   language governing permissions and limitations under the License.
*/
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;

namespace CrypTool.Plugins.Sosemanuk
{
    [Author("Robin Nelle", "rnelle@mail.uni-mannheim.de", "Uni Mannheim - Lehrstuhl Prof. Dr. Armknecht", "http://ls.wim.uni-mannheim.de/")]
    [PluginInfo("Sosemanuk.Properties.Resources", "PluginCaption", "PluginTooltip", "Sosemanuk/DetailedDescription/doc.xml", "Sosemanuk/Images/icon.jpg")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]

    public class Sosemanuk : ICrypComponent
    {
        #region Private Variables
        private readonly SosemanukSettings settings
            = new SosemanukSettings();

        // Subkeys for Serpent24: 100 32-bit words
        private uint[] serpent24SubKeys = new uint[100];

        //Internal cipher state
        private uint[] lfsr = new uint[10];
        private uint fsmR1, fsmR2;

        //Input
        private byte[] inputKey;
        private byte[] inputIV;
        private byte[] inputData;

        //Output
        private byte[] outputData;

        /*
         * mulAlpha[] is used to multiply a word by alpha; 
         * mulAlpha[x] is equal to x * alpha^4.
         *
         * divAlpha[] is used to divide a word by alpha; 
         * divAlpha[x] is equal to x / alpha.
         */
        private static readonly uint[] mulAlpha = new uint[256];
        private static readonly uint[] divAlpha = new uint[256];

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public byte[] InputData
        {
            get { return this.inputData; }
            set
            {
                this.inputData = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", true)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public byte[] OutputData
        {
            get { return this.outputData; }
            set
            {
                this.outputData = value;
                OnPropertyChanged("OutputData");
            }
        }

        #endregion

        public ISettings Settings
        {
            get { return settings; } 
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public Sosemanuk()
        {
            /*
             * We first build exponential and logarithm tables
             * relatively to beta in F_{2^8}. We set 
             * log(0x00) = 0xFF conventionaly, but this is 
             * actually not used in our computations.
             */
            uint[] expb = new uint[256];
            for (uint i = 0, x = 0x01; i < 0xFF; i++)
            {
                expb[i] = x;
                x <<= 1;
                if (x > 0xFF)
                    x ^= 0x1A9;
            }
            expb[0xFF] = 0x00;
            uint[] logb = new uint[256];
            for (uint i = 0; i < 0x100; i++)
                logb[expb[i]] = i;

            /*
             * We now compute mulAlpha[] and divAlpha[]. For all
             * x != 0, we work with invertible numbers, which are
             * as such powers of beta. Multiplication (in F_{2^8})
             * is then implemented as integer addition modulo 255,
             * over the exponents computed by the logb[] table.
             *
             * We have the following equations:
             * alpha^4 = beta^23 * alpha^3 + beta^245 * alpha^2
             *           + beta^48 * alpha + beta^239
             * 1/alpha = beta^16 * alpha^3 + beta^39 * alpha^2
             *           + beta^6 * alpha + beta^64
             */
            mulAlpha[0x00] = 0x00000000;
            divAlpha[0x00] = 0x00000000;
            for (int x = 1; x < 0x100; x++)
            {
                uint ex = logb[x];
                mulAlpha[x] = (expb[(ex + 23) % 255] << 24)
                    | (expb[(ex + 245) % 255] << 16)
                    | (expb[(ex + 48) % 255] << 8)
                    | expb[(ex + 239) % 255];
                divAlpha[x] = (expb[(ex + 16) % 255] << 24)
                    | (expb[(ex + 39) % 255] << 16)
                    | (expb[(ex + 6) % 255] << 8)
                    | expb[(ex + 64) % 255];
            }
        }

        /**
	    * Decode a 32-bit value from a buffer (little-endian).
	    *
	    * @param buf   the input buffer
	    * @param off   the input offset
	    * @return  the decoded value
	    */
        private static uint decode32le(byte[] buf, int off)
        {
            return (uint)(buf[off] | (buf[off + 1] << 8) | (buf[off + 2] << 16) | (buf[off + 3] << 24));
        }

        /**
	    * Encode a 32-bit value into a buffer (little-endian).
	    *
	    * @param val   the value to encode
	    * @param buf   the output buffer
	    * @param off   the output offset
	    */
        private static void encode32le(uint val, byte[] buf, int off)
        {
            buf[off] = (byte)val;
            buf[off + 1] = (byte)(val >> 8);
            buf[off + 2] = (byte)(val >> 16);
            buf[off + 3] = (byte)(val >> 24);
        }

        /**
        * Left-rotate a 32-bit value by some bit.
        *
        * @param val   the value to rotate
        * @param n     the rotation count (between 1 and 31)
           */
        private static uint rotateLeft(uint val, int n)
        {
            return ((val << n) | (val >> (32 - n)));
        }

        /*
         * Definition of SBoxes
         */
         
        private delegate void SBox(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4);

        private void S0(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r3 ^= r0; r4 = r1; r1 &= r3; r4 ^= r2; r1 ^= r0; r0 |= r3; r0 ^= r4; r4 ^= r3; r3 ^= r2;
            r2 |= r1; r2 ^= r4; r4 = ~r4; r4 |= r1; r1 ^= r3; r1 ^= r4; r3 |= r0; r1 ^= r3; r4 ^= r3;
        }

        private void S1(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r0 = ~r0; r2 = ~r2; r4 = r0; r0 &= r1; r2 ^= r0; r0 |= r3; r3 ^= r2; r1 ^= r0; r0 ^= r4;
            r4 |= r1; r1 ^= r3; r2 |= r0; r2 &= r4; r0 ^= r1; r1 &= r2; r1 ^= r0; r0 &= r2; r0 ^= r4;
        }

        private void S2(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r4 = r0; r0 &= r2; r0 ^= r3; r2 ^= r1; r2 ^= r0; r3 |= r4; r3 ^= r1; r4 ^= r2; r1 = r3;
            r3 |= r4; r3 ^= r0; r0 &= r1; r4 ^= r0; r1 ^= r3; r1 ^= r4; r4 = ~r4;
        }

        private void S3(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r4 = r0; r0 |= r3; r3 ^= r1; r1 &= r4; r4 ^= r2; r2 ^= r3; r3 &= r0; r4 |= r1; r3 ^= r4;
            r0 ^= r1; r4 &= r0; r1 ^= r3; r4 ^= r2; r1 |= r0; r1 ^= r2; r0 ^= r3; r2 = r1; r1 |= r3; r1 ^= r0;
        }

        private void S4(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r1 ^= r3; r3 = ~r3; r2 ^= r3; r3 ^= r0; r4 = r1; r1 &= r3; r1 ^= r2; r4 ^= r3; r0 ^= r4; r2 &= r4;
            r2 ^= r0; r0 &= r1; r3 ^= r0; r4 |= r1; r4 ^= r0; r0 |= r3; r0 ^= r2; r2 &= r3; r0 = ~r0; r4 ^= r2;
        }

        private void S5(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r0 ^= r1; r1 ^= r3; r3 = ~r3; r4 = r1; r1 &= r0; r2 ^= r3; r1 ^= r2; r2 |= r4; r4 ^= r3; r3 &= r1;
            r3 ^= r0; r4 ^= r1; r4 ^= r2; r2 ^= r0; r0 &= r3; r2 = ~r2; r0 ^= r4; r4 |= r3; r2 ^= r4;
        }

        private void S6(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r2 = ~r2; r4 = r3; r3 &= r0; r0 ^= r4; r3 ^= r2; r2 |= r4; r1 ^= r3; r2 ^= r0; r0 |= r1; r2 ^= r1;
            r4 ^= r0; r0 |= r3; r0 ^= r2; r4 ^= r3; r4 ^= r0; r3 = ~r3; r2 &= r4; r2 ^= r3;
        }

        private void S7(ref uint r0, ref uint r1, ref uint r2, ref uint r3, ref uint r4)
        {
            r4 = r1; r1 |= r2; r1 ^= r3; r4 ^= r2; r2 ^= r1; r3 |= r4; r3 &= r0; r4 ^= r2; r3 ^= r1; r1 |= r4;
            r1 ^= r0; r0 |= r4; r0 ^= r2; r1 ^= r4; r2 ^= r1; r1 &= r0; r1 ^= r4; r2 = ~r2; r2 |= r0; r4 ^= r2;
        }

        private void KeyAddition(ref uint r0, ref uint r1, ref uint r2, ref uint r3, int ofs)
        {
            r0 ^= serpent24SubKeys[ofs];
            r1 ^= serpent24SubKeys[ofs + 1];
            r2 ^= serpent24SubKeys[ofs + 2];
            r3 ^= serpent24SubKeys[ofs + 3];
        }

        private void Serpent_LinearTransform(ref uint r0, ref uint r1, ref uint r2, ref uint r3)
        {
            r0 = rotateLeft(r0, 13);
            r2 = rotateLeft(r2, 3);
            r1 = r1 ^ r0 ^ r2;
            r3 = r3 ^ r2 ^ (r0 << 3);
            r1 = rotateLeft(r1, 1);
            r3 = rotateLeft(r3, 7);
            r0 = r0 ^ r1 ^ r3;
            r2 = r2 ^ r3 ^ (r1 << 7);
            r0 = rotateLeft(r0, 5);
            r2 = rotateLeft(r2, 22);
        }

        /*
         * One Serpent round.
         *   ofs = current subkey counter
         *   S = S-box for this round
         *   i0 to i4 = input register numbers (the fifth is a scratch register)
         *   o0 to o3 = output register numbers
         */
        private void Serpent_Round(int ofs, SBox S, uint[] r, int i0, int i1, int i2, int i3, int i4, int o0, int o1, int o2, int o3)
        {
            KeyAddition(ref r[i0], ref r[i1], ref r[i2], ref r[i3], ofs);
            S(ref r[i0], ref r[i1], ref r[i2], ref r[i3], ref r[i4]);
            Serpent_LinearTransform(ref r[o0], ref r[o1], ref r[o2], ref r[o3]);
        }

        private uint WUP(uint w0, uint w1, uint w2, uint w3, int cc)
        {
            return rotateLeft( w0 ^ w1 ^ w2 ^ w3 ^ (0x9E3779B9 ^ (uint)cc), 11 );
	    }

        private delegate void WUPfunc(uint[] w, int cc);

        private void WUP0(uint[] w, int cc)
        {
		    w[0] = WUP(w[0], w[3], w[5], w[7], cc);
		    w[1] = WUP(w[1], w[4], w[6], w[0], cc + 1);
		    w[2] = WUP(w[2], w[5], w[7], w[1], cc + 2);
		    w[3] = WUP(w[3], w[6], w[0], w[2], cc + 3);
	    }

        private void WUP1(uint[] w, int cc)
        {
            w[4] = WUP(w[4], w[7], w[1], w[3], cc);
            w[5] = WUP(w[5], w[0], w[2], w[4], cc + 1);
            w[6] = WUP(w[6], w[1], w[3], w[5], cc + 2);
            w[7] = WUP(w[7], w[2], w[4], w[6], cc + 3);
	    }
        
        private void SKS(WUPfunc wup, SBox S, uint[] w, uint w0, uint w1, uint w2, uint w3, int i0, int i1, int i2, int i3, int ofs)
        {
            wup(w, ofs);

            uint[] r = new uint[5];

            r[0] = w[w0];
            r[1] = w[w1];
            r[2] = w[w2];
            r[3] = w[w3];
            r[4] = 0;

            S(ref r[0], ref r[1], ref r[2], ref r[3], ref r[4]);

            serpent24SubKeys[ofs] = r[i0];
            serpent24SubKeys[ofs + 1] = r[i1];
            serpent24SubKeys[ofs + 2] = r[i2];
            serpent24SubKeys[ofs + 3] = r[i3];
        }

        /**
        * Set the private key. The key length must be between 1
        * and 32 bytes.
         *
         * @param key   the private key
         */
        public void setKey(byte[] key)
        {
            if (key.Length < 16)
            {
                GuiLogMessage("The provided key is too short. It must be between 128 and 256 bits (16 and 32 bytes) long. Padding it with zero bytes...", NotificationLevel.Warning);
            } 
            if (key.Length > 32)
            {
                GuiLogMessage("The provided key is too long. It must be between 128 and 256 bits (16 and 32 bytes) long. Exceeding bytes will be ignored...", NotificationLevel.Warning);
            }

            byte[] lkey = new byte[32];
            System.Array.Copy(key, 0, lkey, 0, Math.Min(key.Length,lkey.Length));

            if (key.Length < lkey.Length)
            {
                lkey[key.Length] = 0x01;

                for (int j = key.Length + 1; j < lkey.Length; j++)
                    lkey[j] = 0x00;
            }

            uint[] w = new uint[8];
            for(int i=0;i<8;i++) w[i] = decode32le(lkey, 4*i);

            SKS(WUP0, S3, w, 0, 1, 2, 3, 1, 2, 3, 4,  0); 
            SKS(WUP1, S2, w, 4, 5, 6, 7, 2, 3, 1, 4,  4); 
            SKS(WUP0, S1, w, 0, 1, 2, 3, 2, 0, 3, 1,  8); 
            SKS(WUP1, S0, w, 4, 5, 6, 7, 1, 4, 2, 0, 12); 
            SKS(WUP0, S7, w, 0, 1, 2, 3, 4, 3, 1, 0, 16); 
            SKS(WUP1, S6, w, 4, 5, 6, 7, 0, 1, 4, 2, 20); 
            SKS(WUP0, S5, w, 0, 1, 2, 3, 1, 3, 0, 2, 24); 
            SKS(WUP1, S4, w, 4, 5, 6, 7, 1, 4, 0, 3, 28); 
            SKS(WUP0, S3, w, 0, 1, 2, 3, 1, 2, 3, 4, 32); 
            SKS(WUP1, S2, w, 4, 5, 6, 7, 2, 3, 1, 4, 36); 
            SKS(WUP0, S1, w, 0, 1, 2, 3, 2, 0, 3, 1, 40); 
            SKS(WUP1, S0, w, 4, 5, 6, 7, 1, 4, 2, 0, 44); 
            SKS(WUP0, S7, w, 0, 1, 2, 3, 4, 3, 1, 0, 48); 
            SKS(WUP1, S6, w, 4, 5, 6, 7, 0, 1, 4, 2, 52); 
            SKS(WUP0, S5, w, 0, 1, 2, 3, 1, 3, 0, 2, 56); 
            SKS(WUP1, S4, w, 4, 5, 6, 7, 1, 4, 0, 3, 60); 
            SKS(WUP0, S3, w, 0, 1, 2, 3, 1, 2, 3, 4, 64); 
            SKS(WUP1, S2, w, 4, 5, 6, 7, 2, 3, 1, 4, 68); 
            SKS(WUP0, S1, w, 0, 1, 2, 3, 2, 0, 3, 1, 72); 
            SKS(WUP1, S0, w, 4, 5, 6, 7, 1, 4, 2, 0, 76); 
            SKS(WUP0, S7, w, 0, 1, 2, 3, 4, 3, 1, 0, 80); 
            SKS(WUP1, S6, w, 4, 5, 6, 7, 0, 1, 4, 2, 84); 
            SKS(WUP0, S5, w, 0, 1, 2, 3, 1, 3, 0, 2, 88); 
            SKS(WUP1, S4, w, 4, 5, 6, 7, 1, 4, 0, 3, 92); 
            SKS(WUP0, S3, w, 0, 1, 2, 3, 1, 2, 3, 4, 96);
        }

        /**
         * Set the IV. 
         *
         * @param iv   the IV 
         */
        public void setIV(byte[] iv)
        {
            if (iv.Length < 16)
            {
                GuiLogMessage("The provided IV is too short. It must be 128 bits (16 bytes) long. Padding it with zero bytes...", NotificationLevel.Warning);
            }
            else if (iv.Length > 16)
            {
                GuiLogMessage("The provided IV is too long. It must be 128 bits (16 bytes) long. Exceeding bytes will be ignored...", NotificationLevel.Warning);
            }

            byte[] piv = new byte[16];
            System.Array.Copy(iv, 0, piv, 0, Math.Min(iv.Length,piv.Length) );
            for (int i = iv.Length; i < piv.Length; i++)
                piv[i] = 0x00;

            uint[] r = new uint[5];

            for(int i=0;i<4;i++)
                r[i] = decode32le(piv, 4*i);

            Serpent_Round(  0, S0, r, 0, 1, 2, 3, 4, 1, 4, 2, 0 );
            Serpent_Round(  4, S1, r, 1, 4, 2, 0, 3, 2, 1, 0, 4 );
            Serpent_Round(  8, S2, r, 2, 1, 0, 4, 3, 0, 4, 1, 3 );
            Serpent_Round( 12, S3, r, 0, 4, 1, 3, 2, 4, 1, 3, 2 );
            Serpent_Round( 16, S4, r, 4, 1, 3, 2, 0, 1, 0, 4, 2 );
            Serpent_Round( 20, S5, r, 1, 0, 4, 2, 3, 0, 2, 1, 4 );
            Serpent_Round( 24, S6, r, 0, 2, 1, 4, 3, 0, 2, 3, 1 );
            Serpent_Round( 28, S7, r, 0, 2, 3, 1, 4, 4, 1, 2, 0 );
            Serpent_Round( 32, S0, r, 4, 1, 2, 0, 3, 1, 3, 2, 4 );
            Serpent_Round( 36, S1, r, 1, 3, 2, 4, 0, 2, 1, 4, 3 );
            Serpent_Round( 40, S2, r, 2, 1, 4, 3, 0, 4, 3, 1, 0 );
            Serpent_Round( 44, S3, r, 4, 3, 1, 0, 2, 3, 1, 0, 2 );

            lfsr[9] = r[3];
            lfsr[8] = r[1];
            lfsr[7] = r[0];
            lfsr[6] = r[2];

            Serpent_Round( 48, S4, r, 3, 1, 0, 2, 4, 1, 4, 3, 2 );
            Serpent_Round( 52, S5, r, 1, 4, 3, 2, 0, 4, 2, 1, 3 );
            Serpent_Round( 56, S6, r, 4, 2, 1, 3, 0, 4, 2, 0, 1 );
            Serpent_Round( 60, S7, r, 4, 2, 0, 1, 3, 3, 1, 2, 4 );
            Serpent_Round( 64, S0, r, 3, 1, 2, 4, 0, 1, 0, 2, 3 );
            Serpent_Round( 68, S1, r, 1, 0, 2, 3, 4, 2, 1, 3, 0 );

            fsmR1 = r[2];
            lfsr[4] = r[1];
            fsmR2 = r[3];
            lfsr[5] = r[0];

            Serpent_Round( 72, S2, r, 2, 1, 3, 0, 4, 3, 0, 1, 4 );
            Serpent_Round( 76, S3, r, 3, 0, 1, 4, 2, 0, 1, 4, 2 );
            Serpent_Round( 80, S4, r, 0, 1, 4, 2, 3, 1, 3, 0, 2 );
            Serpent_Round( 84, S5, r, 1, 3, 0, 2, 4, 3, 2, 1, 0 );
            Serpent_Round( 88, S6, r, 3, 2, 1, 0, 4, 3, 2, 4, 1 );
            Serpent_Round( 92, S7, r, 3, 2, 4, 1, 0, 0, 1, 2, 3 );

            KeyAddition(ref r[0], ref r[1], ref r[2], ref r[3], 96);

            lfsr[3] = r[0];
            lfsr[2] = r[1];
            lfsr[1] = r[2];
            lfsr[0] = r[3];
        }

        /**
        * FSM update.
        */
        private void updateFSM()
        {
            uint oldR1 = fsmR1;
            fsmR1 = fsmR2 + (lfsr[1] ^ ((fsmR1 & 0x01) != 0 ? lfsr[8] : 0));
            fsmR2 = rotateLeft(oldR1 * 0x54655307, 7);
        }

        /**
	     * LFSR update. The "dropped" value (s_t) is returned.
	     *
	     * @return  s_t
	     */
        private uint updateLFSR()
        {
            uint v1 = lfsr[9];

            uint changeBitLFSR = (lfsr[3] >> 8);
            if (lfsr[3] < 0)
            {
                changeBitLFSR = lfsr[3] >> 8;
            }
            uint v2 = changeBitLFSR ^ divAlpha[lfsr[3] & 0xFF];

            uint changeBitMulAlpha = (lfsr[0] >> 24);
            if (lfsr[0] < 0)
            {
                changeBitMulAlpha = lfsr[0] >> 24;
            }
            uint v3 = (lfsr[0] << 8) ^ mulAlpha[changeBitMulAlpha];
            uint dropped = lfsr[0];

            for (int i = 0; i < 9; i++)
                lfsr[i] = lfsr[i + 1];
            lfsr[9] = v1 ^ v2 ^ v3;
            return dropped;
        }

        /**
        * Intermediate value computation. Note: this method is 
        * called before the LFSR update, and hence uses lfsr[9].
        *
        * @return  f_t
        */
        private uint computeIntermediate()
        {
            return (lfsr[9] + fsmR1) ^ fsmR2;
        }

        /**
         * Produce 16 bytes of output stream into the provided 
         * buffer.
         *
         * @param buf   the output buffer
         * @param off   the output offset
         */
        private void makeStreamBlock(byte[] buf, int off)
        {
            updateFSM();
            uint f0 = computeIntermediate();
            uint s0 = updateLFSR();

            updateFSM();
            uint f1 = computeIntermediate();
            uint s1 = updateLFSR();

            updateFSM();
            uint f2 = computeIntermediate();
            uint s2 = updateLFSR();

            updateFSM();
            uint f3 = computeIntermediate();
            uint s3 = updateLFSR();

            /*
            * Apply the third S-box (number 2) on (f3, f2, f1, f0).
            */
            uint f4 = f0;
            S2(ref f0, ref f1, ref f2, ref f3, ref f4);

            /*
             * S-box result is in (f2, f3, f1, f4).
             */
            encode32le(f2 ^ s0, buf, off);
            encode32le(f3 ^ s1, buf, off + 4);
            encode32le(f1 ^ s2, buf, off + 8);
            encode32le(f4 ^ s3, buf, off + 12);
        }

        /*
         * Internal buffer for partial blocks. "streamPtr" points 
         * to the first stream byte which has been computed but 
         * not output.
         */
        private static readonly int BUFFERLEN = 16;
        private readonly byte[] streamBuf = new byte[BUFFERLEN];
        private int streamPtr = BUFFERLEN;

        /**
         * Produce the required number of stream bytes.
         *
         * @param buf   the destination buffer
         * @param off   the destination offset
         * @param len   the required stream length (in bytes)
         */
        public void makeStream(byte[] buf, int off, int len)
        {
            if (streamPtr < BUFFERLEN)
            {
                int blen = BUFFERLEN - streamPtr;
                if (blen > len)
                    blen = len;
                Array.Copy(streamBuf, streamPtr, buf, off, blen);
                streamPtr += blen;
                off += blen;
                len -= blen;
            }
            while (len > 0)
            {
                if (len >= BUFFERLEN)
                {
                    makeStreamBlock(buf, off);
                    off += BUFFERLEN;
                    len -= BUFFERLEN;
                }
                else
                {
                    makeStreamBlock(streamBuf, 0);
                    Array.Copy(streamBuf, 0, buf, off, len);
                    streamPtr = len;
                    len = 0;
                }
            }
        }

        /* Generate key stream byte */
        public byte getKeyStreamByte()
        {
            if (streamPtr == BUFFERLEN)
            {
                makeStreamBlock(streamBuf, 0);
                streamPtr = 0;
            }

            return streamBuf[streamPtr++];
        }

        /* Generate ciphertext */
        public byte[] encrypt(byte[] src)
        {
            byte[] dst = new byte[src.Length];

            for (int i = 0; i < src.Length; i++)
                dst[i] = (byte)(src[i] ^ getKeyStreamByte());

            return dst;
        }

        public void init()
        {
            streamPtr = BUFFERLEN;

            setKey(InputKey);
            setIV(InputIV);
        }

        public void PreExecution()
        {
            Dispose();
        }

        private bool checkParameters()
        {
            if (inputData == null)
            {
                GuiLogMessage("No input given. Aborting.", NotificationLevel.Error);
                return false;
            }

            if (inputKey == null)
            {
                GuiLogMessage("No key given. Aborting.", NotificationLevel.Error);
                return false;
            }

            if (inputIV == null)
            {
                GuiLogMessage("No IV given. Aborting.", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!checkParameters()) return;

            init();

            OutputData = encrypt(inputData);

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            inputData = null;
            inputKey = null;
            inputIV = null;
            outputData = null;
        }

        public void Stop()
        {
        }

        #region Event Handling

        public event StatusChangedEventHandler 
            OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler 
            OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler 
            OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, 
            NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, 
                this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, 
                new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, 
                this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
