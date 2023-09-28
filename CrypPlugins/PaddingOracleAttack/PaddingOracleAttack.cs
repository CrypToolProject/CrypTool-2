/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
//using CrypTool.Plugins.PaddingOracleAttack.Properties;
using PaddingOracleAttack.Properties;
//using PaddingOracleAttack.Properties;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace CrypTool.Plugins.PaddingOracleAttack
{
    [Author("Alexander Juettner", "alex@juettner-online.com", "CrypTool 2 Team", "http://CrypTool2.vs.uni-due.de")]
    [PluginInfo("PaddingOracleAttack.Properties.Resources", "PluginCaption", "PluginTooltip", "PaddingOracleAttack/Documentation/doc.xml", new[] { "PaddingOracleAttack/img/icon.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class PaddingOracleAttack : ICrypComponent
    {
        #region Private Variables

        //settings
        private readonly PaddingOracleAttackSettings settings = new PaddingOracleAttackSettings();
        private readonly AttackPresentation pres = new AttackPresentation();
        private CStreamWriter padStreamWriter;
        private RoutedEventHandler reventHandler;
        private RoutedPropertyChangedEventHandler<double> rviewEventHandler;
        //private PropertyChangedEventHandler seventHandler;

        //states
        private STATES curState = STATES.END;
        private RETSTATES retState;

        //calculation variables
        private int blockSize;
        private int bytePointer;
        private byte curPadLen;
        private byte curPadLenSave;
        private bool runToNextPhase;
        private bool finishAll;
        private int bytesDecrypted;
        private bool paddingDecrypted;
        private int requestsSent;
        private int requestsSentSave;
        private bool idle;
        private bool isClickActive;
        private int firstViewBytePos;
        private bool isInitialized = false;

        //byte blocks
        private byte[] cipherBlock;
        private byte[] prelBlock;
        private byte[] corruptedBlock;
        private byte[] overlayBlock;
        private byte[] overlayBlockOld;
        private byte[] decryptedBlock;
        private byte[] plaintextBlock;


        private enum STATES
        {
            INITP1, PHASE1, INITP2, PHASE2, INITP3, PHASE3FIND, PHASE3DEC, PHASE3NEXT, PHASE3PLAIN, END, ERROR
        }

        private enum RETSTATES
        {
            PHASE1, PHASE1END, PHASE2, PHASE2END, PHASE3INCPADDING, PHASE3FIND, PHASE3FINDEND, NR
        }


        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PaddingOracleInputCaption", "PaddingOracleInputTooltip", false)]
        public bool PaddingOracleInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "CipherInputCaption", "CipherInputTooltip", true)]
        public ICrypToolStream CipherInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "ResultOutputCaption", "ResultOutputTooltip")]
        public ICrypToolStream PaddingOracleOutput => padStreamWriter;


        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => pres;

        public void PreExecution()
        {

            //init variables
            blockSize = settings.BlockSize;
            curPadLen = 0;
            bytesDecrypted = 0;
            setBytePointer(blockSize - 1, false);
            PaddingOracleInput = false;
            paddingDecrypted = false;
            requestsSent = 0;
            requestsSentSave = 0;
            idle = false;
            isClickActive = true;
            //firstViewBytePos = settings.ViewByte;
            firstViewBytePos = 1;

            //init blocks
            cipherBlock = new byte[blockSize];
            prelBlock = new byte[blockSize];
            corruptedBlock = new byte[blockSize];
            overlayBlock = new byte[blockSize];
            overlayBlockOld = new byte[blockSize];
            decryptedBlock = new byte[blockSize];
            plaintextBlock = new byte[blockSize];

            for (int i = 0; i < blockSize; i++)
            {
                decryptedBlock[i] = 0;
                plaintextBlock[i] = 0;
                overlayBlock[i] = 0;
                overlayBlockOld[i] = 0;
            }

            changePres(pres.outCipherBlock, "");
            changePres(pres.outCorruptedBlock, "");
            changePres(pres.attDecBlock, "");
            changePres(pres.attCorruptedBlock, "");
            changePres(pres.attOverlayBlock, "");
            changePres(pres.attPlainBlock, "");

            //set workflow to single step
            runToNextPhase = false;
            finishAll = false;

            //min value for viewbyte
            double minVal = Math.Min(18 - blockSize, 10);
            minVal /= 10;

            //change presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.btnNext.IsEnabled = true;
                pres.btnRun.IsEnabled = false;
                pres.btnFinish.IsEnabled = false;
                pres.btnReturn.IsEnabled = false;
                pres.btnReturnPhase.IsEnabled = false;
                pres.imgComplete.Visibility = Visibility.Hidden;

                pres.outCounter.Content = "0";

                pres.viewByteScroller.Minimum = minVal;
                pres.viewByteScroller.Value = 1.0;
                pres.descShownBytes.Content = "1...8";

                if (minVal == 1)
                {
                    pres.viewByteScroller.Visibility = Visibility.Hidden;
                    pres.viewBytePanel.Visibility = Visibility.Hidden;
                }
            }, null);



            //init start state
            isInitialized = true;

            curState = STATES.INITP1;

            retState = RETSTATES.NR;

        }

        public void Execute()
        {
            bool paddingOracleInput = PaddingOracleInput;

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.padInput(paddingOracleInput);
            }, paddingOracleInput);


            switch (curState)
            {
                case STATES.INITP1:
                    //Change Description
                    changePres(pres.descTask, Resources.descP1Init);

                    //change btn description
                    changePres(pres.btnNextLbl, Resources.btnLblP1Init);

                    //Set Phase Header
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.setPhase(1);
                    }, null);

                    //no real padding input = no picture!
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.inPadValid.Visibility = Visibility.Hidden;
                        pres.inPadInvalid.Visibility = Visibility.Hidden;
                    }, null);

                    //init padding length
                    curPadLen = 0;

                    //init cipher block input
                    initCipherInput();

                    //init presentation output
                    string prelBlockValue = arrToString(prelBlock);
                    string cipherBlockValue = arrToString(cipherBlock);

                    changePres(pres.inPrelBlock, prelBlockValue);
                    changePres(pres.inCipherBlock, cipherBlockValue);

                    changePres(pres.attDecBlock, getMultipleValue("??"));
                    changePres(pres.attCorruptedBlock, prelBlockValue);
                    changePres(pres.attOverlayBlock, getMultipleValue("00"));
                    changePres(pres.attPlainBlock, getMultipleValue("??"));

                    break;
                case STATES.PHASE1: //find valid padding
                    if (paddingOracleInput) //valid padding found
                    {
                        //Change Description
                        changePres(pres.descTask, Resources.descP1Done);

                        //Change State
                        curState = STATES.INITP2;
                        retState = RETSTATES.PHASE1END;

                        //Switch to single clicks
                        runToNextPhase = false;


                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            //deactivate run button
                            pres.btnRun.IsEnabled = false;

                            //change bordercolor of description
                            pres.changeBorderColor(true);

                        }, null);

                        //change btn description
                        changePres(pres.btnNextLbl, Resources.btnLblP1End);
                    }
                    else //padding still invalid
                    {
                        //change btn description
                        changePres(pres.descTask, Resources.descP1Task);

                        //change btn description
                        changePres(pres.btnNextLbl, Resources.btnLblP1);

                    }

                    break;
                case STATES.PHASE2: //find padding length
                    bool isFinished = false;

                    if (bytePointer == blockSize - 2 && paddingOracleInput) //if padding length is '01'
                    {
                        isFinished = true;

                        //Set Padding Length
                        curPadLen = 1;
                        curPadLenSave = curPadLen;

                        //update viewbytes
                        if (blockSize > 8)
                        {
                            firstViewBytePos = blockSize - 7;
                            updateScrollValue();
                        }

                        //Change Description
                        changePres(pres.descTask, Resources.descP2DoneSpecial);


                    }
                    else if (!paddingOracleInput) //padding byte changed
                    {
                        isFinished = true;

                        //Set Padding Length
                        curPadLen = Convert.ToByte(blockSize - bytePointer);
                        curPadLenSave = curPadLen;

                        //Change Description
                        changePres(pres.descTask, Resources.descP2Done + curPadLen);
                    }


                    if (isFinished) //(first) padding byte found
                    {
                        //Change State
                        curState = STATES.INITP3;
                        retState = RETSTATES.PHASE2END;

                        //Switch to single clicks
                        runToNextPhase = false;


                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            //deactivate run button
                            pres.btnRun.IsEnabled = false;

                            //change bordercolor of description
                            pres.changeBorderColor(true);
                        }, null);

                        //change btn description
                        changePres(pres.btnNextLbl, Resources.btnLblP2End);
                    }
                    else //padding still invalid
                    {
                        //activate run button
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.btnRun.IsEnabled = true;
                        }, null);

                        //Change Description
                        changePres(pres.descTask, Resources.descP2Task);

                        //change btn description
                        changePres(pres.btnNextLbl, Resources.btnLblP2);

                        //ret state
                        retState = RETSTATES.PHASE2;
                    }
                    break;
                case STATES.PHASE3FIND: //decrypt
                    if (paddingOracleInput) //valid padding found
                    {
                        //Change State
                        curState = STATES.PHASE3DEC;
                        retState = RETSTATES.PHASE3FINDEND;

                        if (!finishAll)
                        {
                            //Change Description
                            changePres(pres.descTask, Resources.descP3FindDone);

                            //deactivate run button
                            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.btnRun.IsEnabled = false;
                                pres.btnReturn.IsEnabled = false;
                            }, null);

                            //change btn description
                            changePres(pres.btnNextLbl, Resources.btnLblP3Decrypt);
                        }
                    }
                    else //still looking for valid padding
                    {
                        bool activateReturnBtn = true;
                        if (retState == RETSTATES.NR)
                        {
                            retState = RETSTATES.PHASE3INCPADDING;
                        }
                        else
                        {
                            retState = RETSTATES.PHASE3FIND;
                            activateReturnBtn = false;
                        }

                        if (!finishAll)
                        {
                            //Change Description
                            changePres(pres.descTask, Resources.descP3FindTask);

                            //change btn description
                            changePres(pres.btnNextLbl, Resources.btnLblP3Find);

                            //activate run button
                            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.btnRun.IsEnabled = true;
                                if (activateReturnBtn)
                                {
                                    pres.btnReturn.IsEnabled = true;
                                }
                            }, null);
                        }
                    }

                    break;
                case STATES.ERROR:
                    GuiLogMessage("An Error Occurred!", NotificationLevel.Error);
                    break;
            }


            idle = false;
            if (runToNextPhase || finishAll)
            {
                ExecuteRound();
            }
            else
            {
                isClickActive = true;
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            curState = STATES.END;
        }

        public void Initialize()
        {
            //add button event handler to presentation
            reventHandler = new RoutedEventHandler(PresEventHandler);

            //required, because handler is added twice... therefore, remove it and add it again!
            try { pres.buttonPanel.RemoveHandler(Button.ClickEvent, reventHandler); }
            catch (Exception e) { GuiLogMessage("Error!! " + e.Message, NotificationLevel.Info); }

            pres.buttonPanel.AddHandler(Button.ClickEvent, reventHandler);


            //add byteview event handler to presentation
            rviewEventHandler = new RoutedPropertyChangedEventHandler<double>(PresViewEventHandler);

            try { pres.viewBytePanel.RemoveHandler(System.Windows.Controls.Primitives.RangeBase.ValueChangedEvent, rviewEventHandler); }
            catch (Exception e) { GuiLogMessage("Error!! " + e.Message, NotificationLevel.Info); }

            pres.viewBytePanel.AddHandler(System.Windows.Controls.Primitives.RangeBase.ValueChangedEvent, rviewEventHandler);


            //settings event handler
            /*seventHandler = new PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
            
            try { this.settings.PropertyChanged -= seventHandler;}
            catch (Exception e) { GuiLogMessage("Error!! " + e.Message, NotificationLevel.Info); }
            
            this.settings.PropertyChanged += seventHandler;*/
        }

        public void Dispose()
        {
        }

        #endregion

        #region Methods

        private void ExecuteRound() //invoked by click or Execute()
        {
            bool reRun = false; //required for finish all

            do
            {
                reRun = false;
                switch (curState)
                {
                    case STATES.INITP1:
                        //Set Byte Pointer
                        setBytePointer(blockSize - 1, true);

                        //Change Output
                        changePres(pres.outCorruptedBlock, arrToString(prelBlock));
                        changePres(pres.outCipherBlock, arrToString(cipherBlock));

                        //Change State 
                        curState = STATES.PHASE1;
                        pres.btnReturn.IsEnabled = false;

                        //activate run button
                        pres.btnRun.IsEnabled = true;

                        //change decription border
                        pres.changeBorderColor(false);

                        //update viewbyte
                        firstViewBytePos = blockSize - 7;
                        updateScrollValue();

                        //Send Request
                        sendOracleRequest();

                        break;
                    case STATES.PHASE1: //find valid padding
                        setOverlayOld();
                        changeCurByte();
                        sendOracleRequest();
                        break;
                    case STATES.INITP2:
                        //Change Description
                        changePres(pres.descTask, Resources.descP2Init);

                        //Set Phase Header
                        pres.setPhase(2);

                        //change decription border
                        pres.changeBorderColor(false);

                        //Set Byte Pointer
                        setBytePointer(-1, false);

                        //Change State 
                        curState = STATES.PHASE2;
                        retState = RETSTATES.NR;
                        pres.btnReturn.IsEnabled = false;

                        //change btn description
                        changePres(pres.btnNextLbl, Resources.btnLblP2Init);

                        //update viewbyte
                        firstViewBytePos = 1;
                        updateScrollValue();

                        //no new padding input = no picture!
                        pres.inPadValid.Visibility = Visibility.Hidden;

                        requestsSentSave = requestsSent;

                        isClickActive = true;

                        break;
                    case STATES.PHASE2: //find padding length
                        //restore value of changed byte
                        if (bytePointer >= 0)
                        {
                            corruptedBlock[bytePointer] = prelBlock[bytePointer];
                            setOverlayOld();
                            overlayBlock[bytePointer] = 0;
                            changePres(pres.attOverlayBlock, arrToString(overlayBlock));
                        }
                        //Set Byte Pointer
                        setBytePointer(bytePointer + 1, true);

                        //update viewbyte
                        if (firstViewBytePos < bytePointer - 6)
                        {
                            firstViewBytePos = bytePointer - 6;
                            updateScrollValue();
                        }

                        //Send Request
                        setOverlayOld();
                        changeCurByte();
                        sendOracleRequest();

                        break;
                    case STATES.INITP3:
                        //Change Description
                        changePres(pres.descTask, Resources.descP3Init);

                        //Set Phase Header
                        pres.setPhase(3);

                        //change decription border
                        pres.changeBorderColor(false);

                        //set padding in plaintext block
                        for (int padCounter = 0; padCounter < blockSize - 1; padCounter++)
                        {
                            if (padCounter < curPadLen)
                            {
                                plaintextBlock[blockSize - 1 - padCounter] = curPadLen;
                            }
                            else
                            {
                                plaintextBlock[blockSize - 1 - padCounter] = 0;
                            }
                        }
                        changePres(pres.attPlainBlock, arrToIncompleteString(plaintextBlock));

                        //restore value of prel byte where first pad byte was found
                        setOverlayOld();
                        overlayBlock[bytePointer]--;
                        overlayBlock[bytePointer]--;
                        changeCurByte();

                        //Set Byte Pointer; set to 7, because the last (padding) byte is decrypted first
                        setBytePointer(blockSize - 1, true);

                        //Change State 
                        curState = STATES.PHASE3DEC;
                        pres.btnReturn.IsEnabled = false;
                        pres.btnReturnPhase.IsEnabled = false;

                        //change btn description
                        changePres(pres.btnNextLbl, Resources.btnLblP3Decrypt);

                        //no new padding input = no picture!
                        pres.inPadInvalid.Visibility = Visibility.Hidden;
                        requestsSentSave = requestsSent;

                        isClickActive = true;

                        break;
                    case STATES.PHASE3DEC:
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.btnReturn.IsEnabled = false;
                        }, null);

                        //Decrypt
                        decryptByte();

                        if (!finishAll)
                        {
                            //Change Description
                            changePres(pres.descTask, Resources.descP3Dec);

                            //Switch to single steps
                            runToNextPhase = false;
                        }

                        //Set Byte Pointer
                        setBytePointer(bytePointer - 1, true);

                        //update viewbyte
                        if (bytePointer < firstViewBytePos - 1)
                        {
                            if (bytePointer >= 1)
                            {
                                firstViewBytePos = bytePointer + 1;
                            }

                            updateScrollValue();
                        }

                        //Decide what to do next (end or go to next byte?)
                        if (bytePointer < 0) //everything decrypted
                        {
                            changePres(pres.descTask, Resources.descP3Done);

                            setBytePointer(0, false);

                            //change btn description
                            changePres(pres.btnNextLbl, Resources.btnLblP3End);

                            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.btnNext.IsEnabled = true;
                                pres.btnRun.IsEnabled = false;
                                pres.btnFinish.IsEnabled = false;
                            }, null);

                            curState = STATES.PHASE3PLAIN;
                        }
                        else if (bytesDecrypted >= curPadLen) //if all padding bytes have been decrypted
                        {
                            if (!paddingDecrypted) //is run once, when padding is completely decrypted
                            {
                                changePres(pres.descTask, Resources.descP3DecPadDone);
                                paddingDecrypted = true;
                            }

                            if (finishAll)
                            {
                                reRun = true;
                            }
                            else
                            {
                                //change task description
                                changePres(pres.descTask, Resources.descP3DecDone);

                                //change btn description
                                changePres(pres.btnNextLbl, Resources.btnLblP3IncPad);
                            }


                            curState = STATES.PHASE3NEXT;
                        }

                        //no new padding input = no picture!
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.inPadValid.Visibility = Visibility.Hidden;
                            isClickActive = true;
                        }, null);

                        break;
                    case STATES.PHASE3NEXT: //when padding is valid (new byte found)
                        //return state
                        retState = RETSTATES.NR;
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.btnReturn.IsEnabled = true;
                        }, null);

                        increasePadding();
                        if (!finishAll)
                        {
                            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.btnFinish.IsEnabled = true;
                            }, null);
                        }
                        curState = STATES.PHASE3FIND;
                        sendOracleRequest();

                        break;
                    case STATES.PHASE3FIND: //find valid padding
                        setOverlayOld();
                        changeCurByte();
                        sendOracleRequest();
                        break;
                    case STATES.PHASE3PLAIN:
                        //changePres(pres.attCorruptedBlock, arrToString(prelBlock));
                        changePres(pres.attOverlayBlock, getMultipleValue("00"));

                        for (int byteCounter = 0; byteCounter < blockSize; byteCounter++)
                        {
                            plaintextBlock[byteCounter] = Convert.ToByte(prelBlock[byteCounter] ^ decryptedBlock[byteCounter]);
                            overlayBlock[byteCounter] = 0;
                        }

                        changePres(pres.attPlainBlock, arrToString(plaintextBlock));

                        changePres(pres.descTask, Resources.descDone);
                        pres.btnNext.IsEnabled = false;
                        pres.btnReturn.IsEnabled = false;
                        pres.btnReturnPhase.IsEnabled = false;
                        pres.btnNextLbl.Content = "";
                        pres.imgComplete.Visibility = Visibility.Visible;

                        curState = STATES.END;

                        break;
                    case STATES.ERROR:
                        pres.descTask.Text = "An error occurred. Any processing has been aborted.";
                        break;
                }
            } while (reRun);
        }

        private void initCipherInput()
        {
            long inputLength;
            using (CStreamReader reader = CipherInput.CreateReader())
            {
                inputLength = reader.Length;

                reader.Read(prelBlock);
                reader.Read(cipherBlock);

                reader.Close();
            }

            for (int byteCounter = 0; byteCounter < blockSize; byteCounter++)
            {
                corruptedBlock[byteCounter] = prelBlock[byteCounter];
            }

            if (inputLength < 2 * blockSize)
            {
                curState = STATES.ERROR;
                GuiLogMessage("Input Message too short. Please check if the Block Size is correct (in the Plugin Settings).", NotificationLevel.Error);
            }
            else if (inputLength > 2 * blockSize)
            {
                GuiLogMessage("The input message is longer than the expected length. Please check if the Block Size is correct (in the Plugin Settings).", NotificationLevel.Warning);
            }

        }

        private void sendOracleRequest()
        {
            if (!idle)
            {
                byte[] outputData = new byte[2 * blockSize];

                for (int byteCounter = 0; byteCounter < blockSize; byteCounter++)
                {
                    //corrupted preliminary/iv block
                    outputData[byteCounter] = corruptedBlock[byteCounter];

                    //cipher block
                    outputData[byteCounter + blockSize] = cipherBlock[byteCounter];
                }

                padStreamWriter = new CStreamWriter(outputData);

                requestsSent++;
                changePres(pres.outCounter, requestsSent.ToString());

                //GuiLogMessage("SendRequest", NotificationLevel.Info); 

                idle = true;

                OnPropertyChanged("PaddingOracleOutput");
            }
        }

        private void changeCurByte()
        {
            //corruptedBlock[bytePointer]++;

            overlayBlock[bytePointer]++;

            corruptedBlock[bytePointer] = Convert.ToByte(overlayBlock[bytePointer] ^ prelBlock[bytePointer]);

            changePres(pres.attOverlayBlock, arrToString(overlayBlock));
            //changePres(pres.attCorruptedBlock, arrToString(corruptedBlock));
            changePres(pres.outCorruptedBlock, arrToString(corruptedBlock));
        }

        private void increasePadding()
        {

            int lastBytePos = blockSize - 1;
            curPadLen++;
            setOverlayOld();

            if (blockSize > 8)
            {
                //GuiLogMessage("test: " + firstViewBytePos + ", " + bytePointer, NotificationLevel.Info);
                updateScrollValue();
            }


            for (int i = 0; i < curPadLen - 1; i++) //last bytes should decrypt to curPadLen after xor
            {
                corruptedBlock[lastBytePos - i] = Convert.ToByte(decryptedBlock[lastBytePos - i] ^ curPadLen);
                overlayBlock[lastBytePos - i] = Convert.ToByte(corruptedBlock[lastBytePos - i] ^ prelBlock[lastBytePos - i]);
                plaintextBlock[lastBytePos - i] = curPadLen;
            }
            plaintextBlock[blockSize - curPadLen] = curPadLen;

            //changePres(pres.attCorruptedBlock, arrToString(corruptedBlock));
            changePres(pres.attOverlayBlock, arrToString(overlayBlock));
            changePres(pres.outCorruptedBlock, arrToString(corruptedBlock));
            changePres(pres.attPlainBlock, arrToIncompleteString(plaintextBlock));

        }

        private void decryptByte() //phase 3
        {
            //change bytes[]
            decryptedBlock[bytePointer] = Convert.ToByte(curPadLen ^ corruptedBlock[bytePointer]);
            plaintextBlock[bytePointer] = Convert.ToByte(curPadLen);

            //change description/output
            changePres(pres.attDecBlock, arrToIncompleteString(decryptedBlock));
            changePres(pres.attPlainBlock, arrToIncompleteString(plaintextBlock));

            //increase counter
            bytesDecrypted++;
        }

        private string arrToString(byte[] arrInput) //returns the hex values of a byte[] in string format
        {
            string result = System.BitConverter.ToString(arrInput);
            result = result.Replace("-", " ");

            result = result.Substring(firstViewBytePos * 3 - 3);


            return result;
        }

        private string arrToIncompleteString(byte[] arrInput)
        {

            int maxValue = blockSize - 1;
            if (arrInput == decryptedBlock)
            {
                maxValue = blockSize - 1 - bytesDecrypted;
            }

            if (arrInput == plaintextBlock)
            {
                maxValue = blockSize - curPadLen;
            }

            if (maxValue == -1)
            {
                maxValue = 0;
            }

            string result = "";

            //for the bytes yet to discover
            for (int counter = 0; counter < maxValue; counter++)
            {
                result += "?? ";
            }

            //for the discovered bytes
            if (maxValue < blockSize)
            {
                result += System.BitConverter.ToString(arrInput, maxValue);
                result = result.Replace("-", " ");
            }

            result = result.Substring(firstViewBytePos * 3 - 3);

            return result;
        }

        private string getMultipleValue(string val)
        {
            string returnString = "";
            for (int i = 0; i < blockSize; i++)
            {
                returnString += val;
                if (i < blockSize - 1)
                {
                    returnString += " ";
                }
            }
            return returnString;
        }



        #region Return Methods

        private void StepBack()
        {
            for (int byteCounter = 0; byteCounter < blockSize; byteCounter++)
            {
                //restoreOverlay
                overlayBlock[byteCounter] = overlayBlockOld[byteCounter];

                //recalculate corrupted block
                corruptedBlock[byteCounter] = Convert.ToByte(overlayBlock[byteCounter] ^ prelBlock[byteCounter]);
            }
            changePres(pres.attOverlayBlock, arrToString(overlayBlock));
            changePres(pres.outCorruptedBlock, arrToString(corruptedBlock));
            //GuiLogMessage("curret: " + retState, NotificationLevel.Info);
            switch (retState)
            {
                case RETSTATES.PHASE1END:
                    curState = STATES.PHASE1;
                    changePres(pres.descTask, Resources.descP1Task);
                    changePres(pres.btnNextLbl, Resources.btnLblP1);
                    pres.padInput(false);
                    pres.changeBorderColor(false);
                    break;
                case RETSTATES.PHASE2:
                    curState = STATES.PHASE2;
                    bytePointer--;
                    break;
                case RETSTATES.PHASE2END:
                    curState = STATES.PHASE2;
                    bytePointer--;
                    changePres(pres.descTask, Resources.descP2Task);
                    changePres(pres.btnNextLbl, Resources.btnLblP2);
                    pres.padInput(true);
                    pres.changeBorderColor(false);
                    break;
                case RETSTATES.PHASE3INCPADDING:
                    curPadLen--;
                    for (int i = 0; i < curPadLen; i++)
                    {
                        plaintextBlock[blockSize - 1 - i] = curPadLen;
                    }
                    changePres(pres.attPlainBlock, arrToIncompleteString(plaintextBlock));

                    changePres(pres.descTask, Resources.descP3DecDone);
                    changePres(pres.btnNextLbl, Resources.btnLblP3IncPad);
                    pres.padInput(false);
                    //bytePointer++;
                    curState = STATES.PHASE3NEXT;
                    break;
                case RETSTATES.PHASE3FINDEND:
                    curState = STATES.PHASE3FIND;
                    changePres(pres.descTask, Resources.descP3FindTask);
                    changePres(pres.btnNextLbl, Resources.btnLblP3Find);
                    pres.padInput(false);
                    break;
            }
            requestsSent = requestsSent - 2; //the amount of requests caused during step backs should not be counted
            changePres(pres.outCounter, requestsSent.ToString());

            sendOracleRequest();

        }

        private void PhaseBack()
        {
            requestsSent = requestsSentSave;
            pres.outCounter.Content = requestsSent.ToString();

            if (curState == STATES.PHASE1 || curState == STATES.INITP2)
            {
                //change state
                curState = STATES.INITP1;

                //Change Description
                changePres(pres.descTask, Resources.descP1Init);

                //change btn description
                changePres(pres.btnNextLbl, Resources.btnLblP1Init);

                //no real padding input = no picture!
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.inPadValid.Visibility = Visibility.Hidden;
                    pres.inPadInvalid.Visibility = Visibility.Hidden;
                }, null);

                //restore overlay
                overlayBlock[blockSize - 1] = 0;
                overlayBlockOld[blockSize - 1] = 0;
                corruptedBlock[blockSize - 1] = prelBlock[blockSize - 1];

                changePres(pres.attOverlayBlock, getMultipleValue("00"));
                changePres(pres.outCorruptedBlock, arrToString(prelBlock));

                //hide bytepointer
                setBytePointer(blockSize - 1, false);

                //set description border to black
                pres.changeBorderColor(false);
            }
            else if (curState == STATES.PHASE2 || curState == STATES.INITP3)
            {
                //change state
                curState = STATES.INITP2;

                //restore overlay
                if (bytePointer > -1)
                {
                    overlayBlock[bytePointer] = 0;
                    overlayBlockOld[bytePointer] = 0;
                    corruptedBlock[bytePointer] = prelBlock[bytePointer];

                    changePres(pres.attOverlayBlock, arrToString(overlayBlock));
                    changePres(pres.outCorruptedBlock, arrToString(prelBlock));
                }

                //set description border to black
                if (curState == STATES.INITP3)
                {
                    pres.changeBorderColor(false);
                }

                ExecuteRound();
            }
            else if (curState == STATES.PHASE3FIND || curState == STATES.PHASE3DEC || curState == STATES.PHASE3NEXT)
            {
                int lastBytePos = blockSize - 1;

                //restore original padding
                curPadLen = curPadLenSave;

                for (int i = 0; i < lastBytePos; i++)
                {
                    overlayBlock[i] = 0;
                    corruptedBlock[i] = prelBlock[i];
                }
                overlayBlock[lastBytePos] = Convert.ToByte(Convert.ToByte(curPadLen ^ prelBlock[lastBytePos]) ^ decryptedBlock[lastBytePos]);
                decryptedBlock[lastBytePos] = 0;
                corruptedBlock[lastBytePos] = Convert.ToByte(prelBlock[lastBytePos] ^ overlayBlock[lastBytePos]);

                bytesDecrypted = 0;

                if (curPadLen == 1)
                {
                    bytePointer = blockSize - 2;
                }
                else
                {
                    bytePointer = blockSize - curPadLen;
                }
                overlayBlock[bytePointer] = 1;

                changePres(pres.attDecBlock, getMultipleValue("??"));
                changePres(pres.attOverlayBlock, arrToString(overlayBlock));


                curState = STATES.INITP3;

                ExecuteRound();
            }
        }

        #endregion

        /**********************************
         * 
         * Change Presentation
         * 
         **********************************/
        private void changePres(System.Windows.Controls.Label label, string fillValue)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                label.Content = fillValue;
            }, fillValue);
        }

        private void changePres(System.Windows.Controls.TextBlock block, string fillValue)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                block.Text = fillValue;
            }, fillValue);
        }

        private void changePres(System.Windows.Controls.TextBox box, string fillValue)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                box.Text = fillValue;
            }, fillValue);
        }

        private void changePres(System.Windows.Controls.Button btn, string fillValue)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                btn.Content = fillValue;
            }, fillValue);
        }

        private void setBytePointer(int position, bool visible)
        {
            int viewPos;
            bytePointer = position;

            if (position < firstViewBytePos - 1)
            {
                viewPos = -1;
            }
            else
            {
                viewPos = position - firstViewBytePos + 1;
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.setBytePointer(viewPos, visible);
            }, new object[] { viewPos, visible });
        }

        private void setOverlayOld()
        {
            for (int byteCounter = 0; byteCounter < blockSize; byteCounter++)
            {
                overlayBlockOld[byteCounter] = overlayBlock[byteCounter];
            }
        }

        private void updateBlockView()
        {

            changePres(pres.inCipherBlock, arrToString(cipherBlock));
            changePres(pres.inPrelBlock, arrToString(prelBlock));
            changePres(pres.attDecBlock, arrToIncompleteString(decryptedBlock));
            changePres(pres.attCorruptedBlock, arrToString(prelBlock));
            changePres(pres.attOverlayBlock, arrToString(overlayBlock));
            changePres(pres.attPlainBlock, arrToIncompleteString(plaintextBlock));
            changePres(pres.outCorruptedBlock, arrToString(corruptedBlock));
            changePres(pres.outCipherBlock, arrToString(cipherBlock));

            bool showPointer = true;
            if (curState == STATES.END || curState == STATES.PHASE3PLAIN)
            {
                showPointer = false;
            }

            setBytePointer(bytePointer, showPointer);
        }

        private void updateScrollValue()
        {
            double newValue = (11 - firstViewBytePos);
            newValue /= 10;

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.viewByteScroller.Value = newValue;
            }, null);

            updateViewRangeLabel(newValue);
        }

        private void updateViewRangeLabel(double viewBytePos)
        {
            if (viewBytePos < 1)
            {
                firstViewBytePos = Convert.ToInt32((1.1 - viewBytePos) * 10);
            }
            else
            {
                firstViewBytePos = Convert.ToInt32(viewBytePos);

                //value of scroller has to be adjusted if the viewed bytes are changed without using scroller
                int newValue = (11 - firstViewBytePos) / 10;
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.viewByteScroller.Value = newValue;
                }, null);
            }

            int lastViewBytePos = firstViewBytePos + 7;

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.descShownBytes.Content = firstViewBytePos + "..." + lastViewBytePos;
            }, null);

            updateBlockView();
        }

        #endregion



        #region Event Handling

        private void PresEventHandler(object sender, RoutedEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            //string test = "test";
            //GuiLogMessage("click "+ test.Substring(1), NotificationLevel.Info);

            if (curState != STATES.END)
            {
                if (feSource.Name == "btnNext")
                {
                    if (isClickActive)
                    {
                        isClickActive = false;
                        pres.btnReturn.IsEnabled = true;
                        pres.btnReturnPhase.IsEnabled = true;
                        ExecuteRound();
                    }
                }
                else if (feSource.Name == "btnRun")
                {
                    pres.btnReturn.IsEnabled = false;
                    pres.btnReturnPhase.IsEnabled = true;
                    runToNextPhase = true;
                    ExecuteRound();
                }
                else if (feSource.Name == "btnFinish")
                {
                    finishAll = true;
                    pres.descTask.Text = Resources.descFinishAll;
                    pres.btnReturn.IsEnabled = false;
                    pres.btnReturnPhase.IsEnabled = false;
                    pres.btnNext.IsEnabled = false;
                    pres.btnRun.IsEnabled = false;
                    pres.btnFinish.IsEnabled = false;
                    ExecuteRound();
                }
                else if (feSource.Name == "btnReturn")
                {
                    pres.btnReturn.IsEnabled = false;
                    StepBack();
                }
                else if (feSource.Name == "btnReturnPhase")
                {
                    pres.btnReturnPhase.IsEnabled = false;
                    pres.btnReturn.IsEnabled = false;
                    PhaseBack();
                }
            }
        }

        private void PresViewEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //FrameworkElement feSource = e.Source as FrameworkElement;
            if (isInitialized)
            {
                double viewBytePos = pres.viewByteScroller.Value;

                updateViewRangeLabel(viewBytePos);
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
