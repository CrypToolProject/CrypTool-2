/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.IO;
using System.Windows.Controls;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.HIGHT
{
    [Author("Soeren Rinne", "soeren.rinne@CrypTool.de", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.crypto.ruhr-uni-bochum.de/")]
    [PluginInfo("HIGHT.Properties.Resources", "PluginCaption", "PluginTooltip", "HIGHT/DetailedDescription/doc.xml", "HIGHT/Images/HIGHT.png", "HIGHT/Images/encrypt.png", "HIGHT/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class HIGHT : ICrypComponent
    {
        #region IPlugin Members

        private HIGHTSettings settings;
        private ICrypToolStream inputStream;
        private CStreamWriter outputStreamWriter;
        private byte[] inputKey;
        private bool stop = false;

        #endregion

        public HIGHT()
        {
            this.settings = new HIGHTSettings();
            //((HIGHTSettings)(this.settings)).LogMessage += HIGHT_LogMessage;
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (HIGHTSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICrypToolStream InputStream
        {
            get 
            {
                return inputStream;
              }
            set 
            { 
              this.inputStream = value;
              
              // wander 20100208: unnecessary event, should be propagated by editor
              //OnPropertyChanged("InputStream");
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

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get
            {
                return outputStreamWriter;
                }
            set
            {
            }
        }

        public void Dispose()
        {
            stop = false;
            inputKey = null;
            outputStreamWriter = null;
            inputStream = null;
        }

        public void Execute()
        {
            process(settings.Action, settings.Padding);
        }

        private void process(int action, int padding)
        {
            //Encrypt/Decrypt Stream
            try
            {                
                if (inputStream == null || inputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }

                outputStreamWriter = new CStreamWriter();

                long inputbytes = inputStream.Length;
                GuiLogMessage("inputStream length [bytes]: " + inputStream.Length.ToString(), NotificationLevel.Debug);
                
                int bytesRead = 0;
                int blocksRead = 0;
                int position;
                int blocks;

                // get number of blocks
                if (((int)inputbytes % 8) == 0)
                {
                    blocks = (int)inputbytes / 8;
                }
                else
                {
                    blocks = (int)Math.Round(inputbytes / 8 + 0.4, 0) + 1;
                }

                byte[] inputbuffer = new byte[8 * blocks];
                byte[] outputbuffer = new byte[4];
                GuiLogMessage("# of blocks: " + blocks.ToString(), NotificationLevel.Debug);

                using (CStreamReader reader = inputStream.CreateReader())
                {

                //read input
                //GuiLogMessage("Current position: " + inputStream.Position.ToString(), NotificationLevel.Debug);
                for (blocksRead = 0; blocksRead <= blocks - 1; blocksRead++)
                {
                    for (position = bytesRead; position <= (blocksRead * 8 + 7); position++)
                    {
                        // no padding to do
                        if (position < inputbytes)
                        {
                                inputbuffer[position] = (byte)reader.ReadByte();
                        }
                        else // padding to do!
                        {
                            if (padding == 0)
                            {
                                // padding with zeros
                                inputbuffer[position] = 48; 
                            }
                            else if (padding == 2)
                            {
                                // padding with PKCS7
                                int temp = 8 - (int)inputbytes % 8 + 48;
                                inputbuffer[position] = (byte)temp;
                            }
                            else
                            {
                                // no padding
                                    inputbuffer[position] = (byte)reader.ReadByte();
                                GuiLogMessage("Byte is: " + inputbuffer[position].ToString(), NotificationLevel.Info);
                            }
                        }
                        bytesRead++;
                        //GuiLogMessage("Current position: " + inputStream.Position.ToString(), NotificationLevel.Debug);
                        //GuiLogMessage("Content of buffer[" + position + "]: " + buffer[position].ToString(), NotificationLevel.Debug);
                    }
                }

                //GuiLogMessage("vector[0] before coding: " + vector[0].ToString(), NotificationLevel.Debug);
                //GuiLogMessage("vector[1] before coding: " + vector[1].ToString(), NotificationLevel.Debug);

                uint[] key = new uint[4];
                long keybytes = inputKey.Length;
                GuiLogMessage("inputKey length [byte]: " + keybytes.ToString(), NotificationLevel.Debug);

                if (keybytes != 16)
                {
                    GuiLogMessage("Given key has false length. Please provide a key with 128 Bits length. Aborting now.", NotificationLevel.Error);
                    return;
                }
                else
                {
                    key[0] = BitConverter.ToUInt32(inputKey, 0);
                    key[1] = BitConverter.ToUInt32(inputKey, 4);
                    key[2] = BitConverter.ToUInt32(inputKey, 8);
                    key[3] = BitConverter.ToUInt32(inputKey, 12);
                }

                //encryption or decryption
                GuiLogMessage("Action is: " + action, NotificationLevel.Debug);
                DateTime startTime = DateTime.Now;
                
                uint[] vector = new uint[2];

                if (action == 0)
                {
                    StatusChanged((int)HIGHTImage.Encode);
                    GuiLogMessage("Starting encryption [Keysize=128 Bits, Blocksize=64 Bits]", NotificationLevel.Info);
                    for (int i = 0; i <= blocks - 1; i++)
                    {
                        vector[0] = BitConverter.ToUInt32(inputbuffer, (i * 8));
                        vector[1] = BitConverter.ToUInt32(inputbuffer, (i * 8 + 4));

                        //GuiLogMessage("vector[0]: " + vector[0].ToString("X"), NotificationLevel.Info);
                        //GuiLogMessage("vector[1]: " + vector[1].ToString("X"), NotificationLevel.Info);

                        vector = general_test(vector, key, 0);

                        //write buffer to output stream
                        outputbuffer = BitConverter.GetBytes(vector[0]);
                            outputStreamWriter.Write(outputbuffer, 0, 4);
                        outputbuffer = BitConverter.GetBytes(vector[1]);
                            outputStreamWriter.Write(outputbuffer, 0, 4);
                    }
                    }
                    else if (action == 1)
                    {
                    StatusChanged((int)HIGHTImage.Decode);
                    GuiLogMessage("Starting decryption [Keysize=128 Bits, Blocksize=64 Bits]", NotificationLevel.Info);
                    for (int i = 0; i <= blocks - 1; i++)
                    {
                        vector[0] = BitConverter.ToUInt32(inputbuffer, i * 8);
                        vector[1] = BitConverter.ToUInt32(inputbuffer, i * 8 + 4);

                        vector = general_test(vector, key, 1);

                        //write buffer to output stream
                        outputbuffer = BitConverter.GetBytes(vector[0]);
                            outputStreamWriter.Write(outputbuffer, 0, 4);
                        outputbuffer = BitConverter.GetBytes(vector[1]);
                            outputStreamWriter.Write(outputbuffer, 0, 4);
                    }
                }

                /*while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                    if ((int)(inputStream.Position * 100 / inputStream.Length) > position)
                    {
                        position = (int)(inputStream.Position * 100 / inputStream.Length);
                        //ProgressChanged(inputStream.Position, inputStream.Length);
                    }
                }*/

                    long outbytes = outputStreamWriter.Length;
                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                //(outputStream as CrypToolStream).FinishWrite();

                if (!stop)
                {
                    if (action == 0)
                    {
                        GuiLogMessage("Encryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outbytes.ToString() + " bytes)", NotificationLevel.Info);
                    }
                    else
                    {
                        GuiLogMessage("Decryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outbytes.ToString() + " bytes)", NotificationLevel.Info);
                    }
                    GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                        outputStreamWriter.Close();
                    OnPropertyChanged("OutputStream");
                }

                if (stop)
                {
                        outputStreamWriter.Close();
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }
            }
            }
            /*catch (CryptographicException cryptographicException)
            {
                // TODO: For an unknown reason p_crypto_stream cannot be closed after exception.
                // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
                // the error messages will be doubled. 
                // As a workaround we set p_crypto_stream to null here.
                p_crypto_stream = null;
                //GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
            }*/
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        public uint[] F0 = new uint[256] {
		        0, 134, 13, 139, 26, 156, 23, 145, 52, 178, 57, 191, 46, 168, 35, 165, 
		        104, 238, 101, 227, 114, 244, 127, 249, 92, 218, 81, 215, 70, 192, 75, 205, 
		        208, 86, 221, 91, 202, 76, 199, 65, 228, 98, 233, 111, 254, 120, 243, 117, 
		        184, 62, 181, 51, 162, 36, 175, 41, 140, 10, 129, 7, 150, 16, 155, 29, 
		        161, 39, 172, 42, 187, 61, 182, 48, 149, 19, 152, 30, 143, 9, 130, 4, 
		        201, 79, 196, 66, 211, 85, 222, 88, 253, 123, 240, 118, 231, 97, 234, 108, 
		        113, 247, 124, 250, 107, 237, 102, 224, 69, 195, 72, 206, 95, 217, 82, 212, 
		        25, 159, 20, 146, 3, 133, 14, 136, 45, 171, 32, 166, 55, 177, 58, 188, 
		        67, 197, 78, 200, 89, 223, 84, 210, 119, 241, 122, 252, 109, 235, 96, 230, 
		        43, 173, 38, 160, 49, 183, 60, 186, 31, 153, 18, 148, 5, 131, 8, 142, 
		        147, 21, 158, 24, 137, 15, 132, 2, 167, 33, 170, 44, 189, 59, 176, 54, 
		        251, 125, 246, 112, 225, 103, 236, 106, 207, 73, 194, 68, 213, 83, 216, 94, 
		        226, 100, 239, 105, 248, 126, 245, 115, 214, 80, 219, 93, 204, 74, 193, 71, 
		        138, 12, 135, 1, 144, 22, 157, 27, 190, 56, 179, 53, 164, 34, 169, 47, 
		        50, 180, 63, 185, 40, 174, 37, 163, 6, 128, 11, 141, 28, 154, 17, 151, 
		        90, 220, 87, 209, 64, 198, 77, 203, 110, 232, 99, 229, 116, 242, 121, 255};

        public uint[] F1 = new uint[256] {
		        0, 88, 176, 232, 97, 57, 209, 137, 194, 154, 114, 42, 163, 251, 19, 75, 
		        133, 221, 53, 109, 228, 188, 84, 12, 71, 31, 247, 175, 38, 126, 150, 206, 
		        11, 83, 187, 227, 106, 50, 218, 130, 201, 145, 121, 33, 168, 240, 24, 64, 
		        142, 214, 62, 102, 239, 183, 95, 7, 76, 20, 252, 164, 45, 117, 157, 197, 
		        22, 78, 166, 254, 119, 47, 199, 159, 212, 140, 100, 60, 181, 237, 5, 93, 
		        147, 203, 35, 123, 242, 170, 66, 26, 81, 9, 225, 185, 48, 104, 128, 216, 
		        29, 69, 173, 245, 124, 36, 204, 148, 223, 135, 111, 55, 190, 230, 14, 86, 
		        152, 192, 40, 112, 249, 161, 73, 17, 90, 2, 234, 178, 59, 99, 139, 211, 
		        44, 116, 156, 196, 77, 21, 253, 165, 238, 182, 94, 6, 143, 215, 63, 103, 
		        169, 241, 25, 65, 200, 144, 120, 32, 107, 51, 219, 131, 10, 82, 186, 226, 
		        39, 127, 151, 207, 70, 30, 246, 174, 229, 189, 85, 13, 132, 220, 52, 108, 
		        162, 250, 18, 74, 195, 155, 115, 43, 96, 56, 208, 136, 1, 89, 177, 233, 
		        58, 98, 138, 210, 91, 3, 235, 179, 248, 160, 72, 16, 153, 193, 41, 113, 
		        191, 231, 15, 87, 222, 134, 110, 54, 125, 37, 205, 149, 28, 68, 172, 244, 
		        49, 105, 129, 217, 80, 8, 224, 184, 243, 171, 67, 27, 146, 202, 34, 122, 
		        180, 236, 4, 92, 213, 141, 101, 61, 118, 46, 198, 158, 23, 79, 167, 255};

        public uint Delta0 = 0x5a;

        private uint LFSR_h(uint state) {
            uint temp = state & 0x9;							
            if (temp == 0 || temp == 9) temp = 0;		
            else temp = 0x40;							
            state = temp | (state>>1);

            return state;
        }

        private void HIGHT_Keyschedule(uint[] mk, ref uint[] sk)
        {
            int i, j;

            for(i=0 ; i < 128 ; i++)
            {
	            Delta[i] = Delta0;
	            Delta0 = LFSR_h(Delta0);
                //GuiLogMessage("Delta0: " + Delta0.ToString("X1"), NotificationLevel.Info);
            }
        	
            for(i=0 ; i < 8 ; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    sk[16 * i + j] = mk[(j - i) < 0 ? (j - i + 8) : (j - i)] + Delta[16 * i + j];
                }

                for (j = 0; j < 8; j++)
                {
                    sk[16 * i + j + 8] = mk[(j - i) < 0 ? (j - i + 16) : (j - i + 8)] + Delta[16 * i + j + 8];
                }
            }
        }
        
        private void Transformation(ref uint x6, ref uint x4, ref uint x2, ref uint x0, uint mk3, uint mk2, uint mk1, uint mk0)
        {
	        x0 += mk0;
	        x2 ^= mk1;
	        x4 += mk2;
	        x6 ^= mk3;
        }

        private void Round(ref uint x7, uint x6, ref uint x5, uint x4, ref uint x3, uint x2, ref uint x1, uint x0, uint i, uint[] sk)
        {
            x1 = (x1 + (F1[x0] ^ sk[i]))%256;
            x3 = (x3 ^ (F0[x2] + sk[i + 1]))%256;
            x5 = (x5 + (F1[x4] ^ sk[i + 2]))%256;
            x7 = (x7 ^ (F0[x6] + sk[i + 3]))%256;
        }

        public static uint[] Delta = new uint[128];

        private void HIGHT_Enc(uint[] x, ref uint[] y, uint[] mk, uint[] sk) {

	        Transformation(ref x[6], ref x[4], ref x[2], ref x[0], mk[3], mk[2], mk[1], mk[0]);	// Initial Transformation

            Round(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 0, sk);		//Round1
            Round(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 4, sk);		//Round2
            Round(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 8, sk);		//Round3
            Round(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 12, sk);		//Round4
            Round(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 16, sk);		//Round5
            Round(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 20, sk);		//Round6
            Round(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 24, sk);		//Round7
            Round(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 28, sk);		//Round8
            Round(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 32, sk);		//Round9
            Round(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 36, sk);		//Round10
            Round(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 40, sk);		//Round11
            Round(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 44, sk);		//Round12
            Round(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 48, sk);		//Round13
            Round(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 52, sk);		//Round14
            Round(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 56, sk);		//Round15
            Round(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 60, sk);		//Round16
            Round(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 64, sk);		//Round17
            Round(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 68, sk);		//Round18
            Round(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 72, sk);		//Round19
            Round(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 76, sk);		//Round20
            Round(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 80, sk);		//Round21
            Round(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 84, sk);		//Round22
            Round(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 88, sk);		//Round23
            Round(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 92, sk);		//Round24
            Round(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 96, sk);		//Round25
            Round(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 100, sk);		//Round26
            Round(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 104, sk);		//Round27
            Round(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 108, sk);		//Round28
            Round(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 112, sk);		//Round29
            Round(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 116, sk);		//Round30
            Round(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 120, sk);		//Round31
            Round(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 124, sk);		//Round32

	        Transformation(ref x[7],ref x[5],ref x[3],ref x[1],mk[15],mk[14],mk[13],mk[12]);	// Final Transformation

	        y[7] = x[0]; y[6] = x[7]; y[5] = x[6]; y[4] = x[5];
	        y[3] = x[4]; y[2] = x[3]; y[1] = x[2]; y[0] = x[1];

        }

        private void DTransformation(ref uint x6, ref uint x4, ref uint x2, ref uint x0, uint mk3, uint mk2, uint mk1, uint mk0)
        {
            x0 = (x0 - mk0)%256;
	        x2 ^= mk1;
	        x4 = (x4 - mk2)%256;
	        x6 ^= mk3;
        }

        private void DRound( ref uint x7, uint x6, ref uint x5, uint x4, ref uint x3, uint x2, ref uint x1, uint x0, uint i, uint[] sk) 
        {
            x1 = (x1 - (F1[x0] ^ sk[i]))%256;
            x3 = (x3 ^ (F0[x2] + sk[i+1]))%256;
            x5 = (x5 - (F1[x4] ^ sk[i+2]))%256;
            x7 = (x7 ^ (F0[x6] + sk[i+3]))%256;
        }

        private void HIGHT_Dec(uint[] x, ref uint[] y, uint[] mk, uint[] sk) {
        	
            DTransformation(ref x[6],ref x[4],ref x[2],ref x[0],mk[15],mk[14],mk[13],mk[12]);	// Initial Transformation

            DRound(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 124, sk);		//Round1
            DRound(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 120, sk);		//Round2
            DRound(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 116, sk);		//Round3
            DRound(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 112, sk);		//Round4
            DRound(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 108, sk);		//Round5
            DRound(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 104, sk);		//Round6
            DRound(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 100, sk);		//Round7
            DRound(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 96, sk);			//Round8
            DRound(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 92, sk);			//Round9
            DRound(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 88, sk);			//Round10
            DRound(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 84, sk);			//Round11
            DRound(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 80, sk);			//Round12
            DRound(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 76, sk);			//Round13
            DRound(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 72, sk);			//Round14
            DRound(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 68, sk);			//Round15
            DRound(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 64, sk);			//Round16
            DRound(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 60, sk);			//Round17
            DRound(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 56, sk);			//Round18
            DRound(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 52, sk);			//Round19
            DRound(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 48, sk);			//Round20
            DRound(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 44, sk);			//Round21
            DRound(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 40, sk);			//Round22
            DRound(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 36, sk);			//Round23
            DRound(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 32, sk);			//Round24
            DRound(ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], 28, sk);			//Round25
            DRound(ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], 24, sk);			//Round26
            DRound(ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], ref x[3], x[2], 20, sk);			//Round27
            DRound(ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], ref x[4], x[3], 16, sk);			//Round28
            DRound(ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], ref x[5], x[4], 12, sk);			//Round29
            DRound(ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], ref x[6], x[5], 8, sk);			//Round30
            DRound(ref x[5], x[4], ref x[3], x[2], ref x[1], x[0], ref x[7], x[6], 4, sk);			//Round31
            DRound(ref x[6], x[5], ref x[4], x[3], ref x[2], x[1], ref x[0], x[7], 0, sk);			//Round32

            DTransformation(ref x[5],ref x[3],ref x[1],ref x[7],mk[3],mk[2],mk[1],mk[0]);	// Final Transformation

            y[7] = x[6]; y[6] = x[5]; y[5] = x[4]; y[4] = x[3];
            y[3] = x[2]; y[2] = x[1]; y[1] = x[0]; y[0] = x[7];
        	
        }

        private uint[] general_test(uint[] text, uint[] key, uint mode){

            //GuiLogMessage("HIGHT Test Plaintext Original: " + plaintext[0].ToString("X") + " " + plaintext[1].ToString("X"), NotificationLevel.Info);

            uint[] MK = new uint[16];// {0x0f, 0x0e, 0x0d, 0x0c, 0x0b, 0x0a, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00};
            uint[] Plain = new uint[8];// { 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01 };
            uint[] SK = new uint[128];
            uint[] Cipher = new uint[8];
            uint[] temp, Plain_tmp, Cipher_tmp = new uint[8];
            uint[] key_temp = new uint[16];

            //MK = Plain = SK = Cipher = temp = Plain_tmp = Cipher_tmp = key_temp = null;

            // convert key into 8 bit values
            key_temp = key;
            MK[12] = key_temp[0] >> 24;
            key_temp = key;
            MK[13] = key_temp[0] << 8;
            MK[13] = MK[13] >> 24;
            key_temp = key;
            MK[14] = key_temp[0] << 16;
            MK[14] = MK[14] >> 24;
            key_temp = key;
            MK[15] = key_temp[0] << 24;
            MK[15] = MK[15] >> 24;

            key_temp = key;
            MK[8] = key_temp[1] >> 24;
            key_temp = key;
            MK[9] = key_temp[1] << 8;
            MK[9] = MK[9] >> 24;
            key_temp = key;
            MK[10] = key_temp[1] << 16;
            MK[10] = MK[10] >> 24;
            key_temp = key;
            MK[11] = key_temp[1] << 24;
            MK[11] = MK[11] >> 24;

            key_temp = key;
            MK[4] = key_temp[2] >> 24;
            key_temp = key;
            MK[5] = key_temp[2] << 8;
            MK[5] = MK[5] >> 24;
            key_temp = key;
            MK[6] = key_temp[2] << 16;
            MK[6] = MK[6] >> 24;
            key_temp = key;
            MK[7] = key_temp[2] << 24;
            MK[7] = MK[7] >> 24;

            key_temp = key;
            MK[0] = key_temp[3] >> 24;
            key_temp = key;
            MK[1] = key_temp[3] << 8;
            MK[1] = MK[1] >> 24;
            key_temp = key;
            MK[2] = key_temp[3] << 16;
            MK[2] = MK[2] >> 24;
            key_temp = key;
            MK[3] = key_temp[3] << 24;
            MK[3] = MK[3] >> 24;

            // run keyschedule of HIGHT
	        HIGHT_Keyschedule(MK, ref SK);

            // choose encryption/decryption
            if (mode == 0)
            {
                //convert 32 bit entries into 8 bit/1 byte entries
                temp = text;

                Plain[4] = temp[0] >> 24;
                temp = text;
                Plain[5] = temp[0] << 8;
                Plain[5] = Plain[5] >> 24;
                temp = text;
                Plain[6] = temp[0] << 16;
                Plain[6] = Plain[6] >> 24;
                temp = text;
                Plain[7] = temp[0] << 24;
                Plain[7] = Plain[7] >> 24;
                temp = text;

                Plain[0] = temp[1] >> 24;
                temp = text;
                Plain[1] = temp[1] << 8;
                Plain[1] = Plain[1] >> 24;
                temp = text;
                Plain[2] = temp[1] << 16;
                Plain[2] = Plain[2] >> 24;
                temp = text;
                Plain[3] = temp[1] << 24;
                Plain[3] = Plain[3] >> 24;
                temp = text;

                //GuiLogMessage("HIGHT Cipher before: " + Cipher[0].ToString("X1") + " " + Cipher[1].ToString("X1") + " " + Cipher[2].ToString("X1") + " " + Cipher[3].ToString("X1") + " " + Cipher[4].ToString("X1") + " " + Cipher[5].ToString("X1") + " " + Cipher[6].ToString("X1") + " " + Cipher[7].ToString("X1") + " ", NotificationLevel.Info);
                //GuiLogMessage("HIGHT Plain before: " + Plain[0].ToString("X1") + " " + Plain[1].ToString("X1") + " " + Plain[2].ToString("X1") + " " + Plain[3].ToString("X1") + " " + Plain[4].ToString("X1") + " " + Plain[5].ToString("X1") + " " + Plain[6].ToString("X1") + " " + Plain[7].ToString("X1") + " ", NotificationLevel.Info);

                Plain_tmp = Plain;
                HIGHT_Enc(Plain_tmp, ref Cipher, MK, SK);

                Cipher_tmp = Cipher;

                //reconvert 8 bit entries into 32 bit entries
                text[0] = (Cipher_tmp[0] << 24) + (Cipher_tmp[1] << 16) + (Cipher_tmp[2] << 8) + Cipher_tmp[3];
                text[1] = (Cipher_tmp[4] << 24) + (Cipher_tmp[5] << 16) + (Cipher_tmp[6] << 8) + Cipher_tmp[7];

                //GuiLogMessage("HIGHT Cipher after Enc: " + Cipher[0].ToString("X1") + " " + Cipher[1].ToString("X1") + " " + Cipher[2].ToString("X1") + " " + Cipher[3].ToString("X1") + " " + Cipher[4].ToString("X1") + " " + Cipher[5].ToString("X1") + " " + Cipher[6].ToString("X1") + " " + Cipher[7].ToString("X1") + " ", NotificationLevel.Info);
                //GuiLogMessage("HIGHT Plain after Enc: " + Plain[0].ToString("X1") + " " + Plain[1].ToString("X1") + " " + Plain[2].ToString("X1") + " " + Plain[3].ToString("X1") + " " + Plain[4].ToString("X1") + " " + Plain[5].ToString("X1") + " " + Plain[6].ToString("X1") + " " + Plain[7].ToString("X1") + " ", NotificationLevel.Info);
            }
            else
            {
                //convert 32 bit entries into 8 bit entries
                temp = text;

                Cipher[0] = temp[0] >> 24;
                temp = text;
                Cipher[1] = temp[0] << 8;
                Cipher[1] = Cipher[1] >> 24;
                temp = text;
                Cipher[2] = temp[0] << 16;
                Cipher[2] = Cipher[2] >> 24;
                temp = text;
                Cipher[3] = temp[0] << 24;
                Cipher[3] = Cipher[3] >> 24;
                temp = text;

                Cipher[4] = temp[1] >> 24;
                temp = text;
                Cipher[5] = temp[1] << 8;
                Cipher[5] = Cipher[5] >> 24;
                temp = text;
                Cipher[6] = temp[1] << 16;
                Cipher[6] = Cipher[6] >> 24;
                temp = text;
                Cipher[7] = temp[1] << 24;
                Cipher[7] = Cipher[7] >> 24;
                temp = text;

                Cipher_tmp = Cipher;

                //GuiLogMessage("HIGHT Cipher before: " + Cipher[0].ToString("X1") + " " + Cipher[1].ToString("X1") + " " + Cipher[2].ToString("X1") + " " + Cipher[3].ToString("X1") + " " + Cipher[4].ToString("X1") + " " + Cipher[5].ToString("X1") + " " + Cipher[6].ToString("X1") + " " + Cipher[7].ToString("X1") + " ", NotificationLevel.Info);
                //GuiLogMessage("HIGHT Plain before: " + Plain[0].ToString("X1") + " " + Plain[1].ToString("X1") + " " + Plain[2].ToString("X1") + " " + Plain[3].ToString("X1") + " " + Plain[4].ToString("X1") + " " + Plain[5].ToString("X1") + " " + Plain[6].ToString("X1") + " " + Plain[7].ToString("X1") + " ", NotificationLevel.Info);
                
                HIGHT_Dec(Cipher_tmp, ref Plain, MK, SK);

                Plain_tmp = Plain;
                //reconvert 8 bit entries into 32 bit entries
                text[1] = (Plain[0] << 24) + (Plain[1] << 16) + (Plain[2] << 8) + Plain[3];
                text[0] = (Plain[4] << 24) + (Plain[5] << 16) + (Plain[6] << 8) + Plain[7];

                //GuiLogMessage("HIGHT Cipher after Dec: " + Cipher[0].ToString("X1") + " " + Cipher[1].ToString("X1") + " " + Cipher[2].ToString("X1") + " " + Cipher[3].ToString("X1") + " " + Cipher[4].ToString("X1") + " " + Cipher[5].ToString("X1") + " " + Cipher[6].ToString("X1") + " " + Cipher[7].ToString("X1") + " ", NotificationLevel.Info);
                //GuiLogMessage("HIGHT Plain after Dec: " + Plain[0].ToString("X1") + " " + Plain[1].ToString("X1") + " " + Plain[2].ToString("X1") + " " + Plain[3].ToString("X1") + " " + Plain[4].ToString("X1") + " " + Plain[5].ToString("X1") + " " + Plain[6].ToString("X1") + " " + Plain[7].ToString("X1") + " ", NotificationLevel.Info);
            }

            return text;
        }

        public void Encrypt()
        {
            //Encrypt Stream
            process(0, settings.Padding);
        }

        public void Decrypt()
        {
            //Decrypt Stream
            process(1, settings.Padding);
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Stop()
        {
            this.stop = true;
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            /*if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
            }*/
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion
    }

    enum HIGHTImage
    {
        Default,
        Encode,
        Decode
    }
}
