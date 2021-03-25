/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Numerics;

namespace CrypTool.Plugins.RSA
{
    [Author("Dennis Nolte, Raoul Falk, Sven Rech, Nils Kopal", null, "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("RSA.Properties.Resources", "PluginCaption", "PluginTooltip", "RSA/DetailedDescription/doc.xml", "RSA/iconrsa.png", "RSA/Images/encrypt.png", "RSA/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
     //<summary>
     //This plugin does a RSA encryption/decryption on a Message M / Ciphertext C
     //It also encrypts/decrypts text with RSA
     //</summary>
    class RSA : ICrypComponent
    {
        #region private members

        private RSASettings settings = new RSASettings();
        private BigInteger inputN = new BigInteger(1);
        private BigInteger inputmc = new BigInteger(1);
        private BigInteger inputed = new BigInteger(1);
        private BigInteger outputmc = new BigInteger(1);
        private byte[] inputText = null;
        private byte[] outputText = null;
        private int blocks_done = 0;
        private ArrayList threads;
        private bool stopped = true;

        #endregion

        #region events

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion

        #region public
        
        /// <summary>
        /// Notify that a property changed
        /// </summary>
        /// <param name="name">property name</param>
        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Gets/Sets the Settings of this plugin
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RSASettings)value; }
        }

        /// <summary>
        /// Get the Presentation of this plugin
        /// </summary>
        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called by the environment before execution
        /// </summary>
        public void PreExecution()
        {
            stopped = false;
        }

        /// <summary>
        /// Called by the environment to execute this plugin
        /// Does RSA on M/C and encrypt/decrypt the input Text
        /// This method starts threads to speed RSA up if the user switched on more than one
        /// thread
        /// </summary>
        public void Execute()
        {
            
            //calculate the BigIntegers
            try{
                if (this.InputN != 0 && this.InputED != 0 && this.InputMC != 0 && !stopped)
                    this.OutputMC = BigInteger.ModPow(InputMC, this.InputED, this.InputN);
            }
            catch (Exception ex)
            {
                GuiLogMessage("RSA could not work because of: " + ex.Message, NotificationLevel.Error);             
            }

            //
            // RSA on Texts
            //
            if (this.InputText is object && this.InputN != 0 && this.InputED != 0 && !stopped)
            {
                DateTime startTime = DateTime.Now;
                GuiLogMessage("starting RSA on texts", NotificationLevel.Info);

                threads = ArrayList.Synchronized(new ArrayList());

                int blocksize_input = 0;
                int blocksize_output = 0;

                if (settings.OverrideBlocksizes)
                {
                    blocksize_input = settings.InputBlocksize;
                    blocksize_output = settings.OutputBlocksize;
                }
                else
                {
                    //calculate block sizes from N          
                    //Encryption
                    if (settings.Action == 0)
                    {
                        blocksize_input = (int)Math.Floor(BigInteger.Log(InputN, 256));
                        blocksize_output = (int)Math.Ceiling(BigInteger.Log(InputN, 256));
                    }
                    //Decryption
                    else
                    {
                        blocksize_input = (int)Math.Ceiling(BigInteger.Log(InputN, 256));
                        blocksize_output = (int)Math.Floor(BigInteger.Log(InputN, 256));
                    }
                }

                GuiLogMessage("Input blocksize = " + blocksize_input, NotificationLevel.Debug);
                GuiLogMessage("Output blocksize = " + blocksize_output, NotificationLevel.Debug);
                
                if (blocksize_input == 0)
                {
                    GuiLogMessage("Input blocksize 0 - RSA cannot work", NotificationLevel.Error);
                    return;
                }

                if (blocksize_output == 0)
                {
                    GuiLogMessage("Output blocksize 0 - RSA cannot work", NotificationLevel.Error);
                    return;
                }

                //calculate amount of blocks and the difference between the input text
                //and the blocked input text
                int blockcount = (int)Math.Ceiling((double)this.InputText.Length / blocksize_input);                
                
                GuiLogMessage("Blockcount = " + blockcount, NotificationLevel.Debug);
                
                //Generate input and output array of correct block size
                byte[] output = new byte[blocksize_output * blockcount];
                blocks_done = 0;

                for (int i = 1; i < this.settings.CoresUsed + 1;i++ ) // CoresUsed starts with 0 (so 0 => use 1 Core)
                {
                    ParameterizedThreadStart pts = new ParameterizedThreadStart(this.crypt);
                    Thread thread = new Thread(pts);
                    thread.Name = "RSA worker thread " + i;
                    threads.Add(thread);
                    thread.Start(new Object[6] { output, blockcount, blocksize_input, blocksize_output, i, thread});
                    GuiLogMessage("started: " + thread.Name, NotificationLevel.Debug);

                    if (stopped)
                        return;

                }//end for

                //main thread should work also
                crypt(new Object[6] { output, blockcount, blocksize_input, blocksize_output, 0, null });

                //Wait for all worker threads to stop
                //Worker threads will be removed by themselves from the list
                //in finally block
                while (threads.Count != 0)
                {
                    if (stopped)
                        return;

                    Thread.Sleep(0);
                }
                
                output = removeZeros(output);
                this.OutputText = output;

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;

                GuiLogMessage(string.Format("Finished RSA on texts in {0} seconds!", duration.TotalSeconds), NotificationLevel.Info);
                //GuiLogMessage(string.Format(Resources.Finished_RSA_on_texts_in__0__seconds_, duration.TotalSeconds), NotificationLevel.Info);
            
            }//end if           
            ProgressChanged(1.0, 1.0);
        }//end Execute

        /// <summary>
        /// Called by the environment after execution of this plugin
        /// </summary>
        public void PostExecution()
        {
            this.stopped = true;
        }

        /// <summary>
        /// Called by the environment to stop this plugin
        /// </summary>
        public void Stop()
        {
            this.stopped = true;
        }

        /// <summary>
        /// Called by the environment to initialize this plugin
        /// </summary>
        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
            this.stopped = true;
        }

        /// <summary>
        /// Called by the environment to Dispose this plugin
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Gets/Sets the one part of the public/private key called N
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputNCaption", "InputNTooltip")]
        public BigInteger InputN
        {
            get
            {
                return inputN;
            }
            set
            {
                this.inputN = value;
                OnPropertyChanged("InputN");
            }
        }

        /// <summary>
        /// Gets/Sets a input message/ciphertext as BigInteger called M / C
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputMCCaption", "InputMCTooltip")]
        public BigInteger InputMC
        {
            get
            {
                return inputmc;
            }
            set
            {
                this.inputmc = value;
                OnPropertyChanged("InputMC");
            }
        }

        /// <summary>
        /// Gets/Sets the one part of the public/private key called E / D
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputEDCaption", "InputEDTooltip")]
        public BigInteger InputED
        {
            get
            {
                return inputed;
            }
            set
            {
                this.inputed = value;
                OnPropertyChanged("InputED");
            }
        }

        /// <summary>
        /// Gets/Sets a output message/ciphertext as BigInteger called C / M
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputMCCaption", "OutputMCTooltip")]
        public BigInteger OutputMC
        {
            get
            {
                return outputmc;
            }
            set
            {
                this.outputmc = value;
                OnPropertyChanged("OutputMC");
            }
        }

        /// <summary>
        /// Gets/Sets a text input for encryption/decryption
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip")]
        public byte[] InputText
        {
            get
            {
                return inputText;
            }
            set
            {
                this.inputText = value;
                //GuiLogMessage("InputText: " + (int)inputText[0] + " " + (int)inputText[1] + " " + (int)inputText[2] + " " + (int)inputText[3] + " ", NotificationLevel.Info);
                OnPropertyChanged("InputText");
            }
        }

        /// <summary>
        /// Gets/Sets a text output for encrypted/decrypted data
        /// </summary>       
        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public byte[] OutputText
        {
            get
            {
                return outputText;
            }
            set
            {
                this.outputText = value;
                //GuiLogMessage("OutputText: " + (int)outputText[0] + " " +(int)outputText[1] + " "+(int)outputText[2] + " "+(int)outputText[3] + " ", NotificationLevel.Info);
                OnPropertyChanged("OutputText");
            }
        }

        #endregion

        #region private

        /// <summary>
        /// Encrypts/Decrypts all blocks belonging to the thread nr
        /// </summary>
        /// <param name="parameters">parameters</param>
        private void crypt(Object parameters)
        {
            byte[] output = (byte[])((Object[])parameters)[0];
            int blockcount = (int)((Object[])parameters)[1];
            int blocksize_input = (int)((Object[])parameters)[2];
            int blocksize_output = (int)((Object[])parameters)[3];
            int threadnr = (int)((Object[])parameters)[4];
            Thread thread = (Thread)((Object[])parameters)[5];

            try
            {

                BigInteger bint = new BigInteger();
                
                //encrypt/decrypt each block
                for (int i = threadnr; i < blockcount; i += (this.settings.CoresUsed + 1)) //walk over the blocks
                // CoresUsed starts with 0 (so 0 => use 1 Core)
                {

                    //create a big integer from a block
                    byte[] help = new byte[blocksize_input+1];
                    for (int j = 0; j < blocksize_input; j++)
                    {
                        if (i * blocksize_input + j < InputText.Length)
                            help[j] = InputText[i * blocksize_input + j];
                        if (stopped)
                            return;
                    }
                    bint = new BigInteger(help);

                    //Check if the text could be encrypted/decrypted
                    //this is only possible if m < N
                    if (bint > this.InputN)
                    {
                        //Go out with an error because encryption/decryption is not possible
                        string mode = (settings.Action == 0) ? "encrypting" : "decrypting";
                        GuiLogMessage("N = " + this.InputN + " is not suitable for " + mode + " this text: M = " + new BigInteger(help) + " > N.", NotificationLevel.Error);
                        return;
                    }

                    //here we encrypt/decrypt with rsa algorithm
                    bint = BigInteger.ModPow(bint, this.InputED, this.InputN);

                    //create a block from the byte array of the BigInteger
                    byte[] bytes = removeZeros(bint.ToByteArray());
                    int diff = (blocksize_output - (bytes.Length % blocksize_output)) % blocksize_output;

                    if (bytes.Length > blocksize_output)
                    {
                        //Go out with an error because encryption/decryption is not possible
                        GuiLogMessage("Output blocksize = "+ blocksize_output + " is too small, must be at least " + bytes.Length + "!", NotificationLevel.Error);
                        return;
                    }

                    for (int j = 0; j < bytes.Length; j++)
                    {
                        output[i * blocksize_output + j/* + diff*/] = bytes[j];
                        if (stopped)
                            return;
                    }

                    if (stopped)
                        return;

                    blocks_done++;
                    ProgressChanged((double)blocks_done / blockcount, 1.0);

                }//end for i
            }
            finally
            {
                //remove thread from list so that main thread will stop
                //if all threads are removed
                if (this.threads != null && thread != null){
                    threads.Remove(thread);
                    GuiLogMessage("stopped: " + thread.Name, NotificationLevel.Debug);
                }
            }

        }//end crypt
        
        /// <summary>
        /// Remove all '0' from a byte arrays end
        /// example
        /// 
        /// { 'a','b','c',0,0 } => { 'a','b','c' }
        /// </summary>
        /// <param name="input">byte array</param>
        /// <returns>byte array</returns>
        private byte[] removeZeros(byte[] input)
        {
            //1. Count zeros
            int zeros = 0;
            for (int i=input.Length-1;i>0;i--){

                if (input[i] == 0)
                {
                    zeros++;
                }
                else
                {
                    break;
                }

            }

            //2. Create new smaller byte array with
            byte[] output = new byte[input.Length - zeros];

            //3. Copy from input array beginning at the first byte <> 0 to the output array
            for (int i = 0; i < input.Length - zeros; i++)
            {
                output[i] = input[i];
            }

            return output;
        }

        /// <summary>
        /// Change the progress of this plugin
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="max">max</param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Logg a message to CrypTool
        /// </summary>
        /// <param name="p">p</param>
        /// <param name="notificationLevel">notificationLevel</param>
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

    }//end rsa

}//end namespace
