/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Christian Bender, Universität Siegen

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
using CrypTool.PluginBase.Miscellaneous;
using KKDFSHAKE256.Properties;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace CrypTool.Plugins.KKDFSHAKE256
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("KKDFSHAKE256.Properties.Resources", "PluginCaption", "KKDFSHAKE256Tooltip", "KKDFSHAKE256/userdoc.xml", new[] { "KKDFSHAKE256/images/icon.png" })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class KKDFSHAKE256 : ICrypComponent
    {
        #region Private Variables

        private readonly KKDFSHAKE256Settings settings = new KKDFSHAKE256Settings();
        private readonly KKDFSHAKE256Pres pres = new KKDFSHAKE256Pres();
        private byte[] _skm;
        private byte[] _key;
        private BigInteger _outputBytes;
        private byte[] _keyMaterial;
        private Thread workerThread;
        private int stepsToGo = 0;
        private int curStep = 0;

        #endregion

        #region Methods for calculation

        /// <summary>
        /// Computes the KKDFSHAKE256 function. Its like the traditional KDF described in https://eprint.iacr.org/2010/264.pdf This construction does not need a counter.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <param name="outputBytes"></param>
        /// <returns></returns>
        private static byte[] computeKKDFSHA256XOF(byte[] msg, byte[] key, int outputBytes)
        {
            //hash object
            ShakeDigest shake256 = new ShakeDigest(256);
            //output byte array
            byte[] result = new byte[outputBytes];
            //array for input of hashfunction
            byte[] input = new byte[key.Length + msg.Length];

            //prepare input
            System.Buffer.BlockCopy(key, 0, input, 0, key.Length);
            System.Buffer.BlockCopy(msg, 0, input, key.Length, msg.Length);

            //update internal state
            shake256.BlockUpdate(input, 0, input.Length);
            //finish the hash.
            shake256.DoFinal(result, 0, outputBytes);

            //DEBUG
            //Console.WriteLine("KM: " + BitConverter.ToString(result).Replace("-", ""));
            return result;
        }

        /// <summary>
        /// Method for refreshing the stepcounter in the presentation
        /// </summary>
        private void refreshStepState()
        {
            if (settings.DisplayPres)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Paragraph p = new Paragraph();

                    p.Inlines.Add(new Run(Resources.PresStepText.Replace("{0}", curStep.ToString()).Replace("{1}", stepsToGo.ToString())));
                    p.TextAlignment = TextAlignment.Right;
                    pres.txtStep.Document.Blocks.Add(p);
                    pres.txtStep.Document.Blocks.Remove(pres.txtStep.Document.Blocks.FirstBlock);

                }, null);
            }
        }

        /// <summary>
        ///  the method to be called in the workerthread
        /// </summary>
        private void tExecute()
        {
        //Label for restart
        Restart:

            //Progessbar adjusting
            ProgressChanged(0, 1);

            //Clean up outputs
            _keyMaterial = Encoding.UTF8.GetBytes("");
            //OnPropertyChanged("KeyMaterial");

            //clean steps
            curStep = 0;

            //Check for output length: max 5.242.880 byte = 5 Mb
            if (OutputBytes > 5242880)
            {
                GuiLogMessage(Resources.TooMuchOutputRequestedLogMSG.Replace("{0}", OutputBytes.ToString()), NotificationLevel.Warning);
                OutputBytes = 5242880;
                OnPropertyChanged("OutputBytes");
            }

            //twelve stepts takes the presentation
            double steps = 12;
            stepsToGo = (int)steps;
            double prgress_step = 1 / steps;

            refreshStepState();

            //Event for start
            AutoResetEvent buttonStartClickedEvent = pres.buttonStartClickedEvent;
            buttonStartClickedEvent.Reset();

            //Event for restart
            AutoResetEvent buttonRestartClickedEvent = pres.buttonRestartClickedEvent;
            buttonRestartClickedEvent.Reset();

            //Event for next
            AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
            buttonNextClickedEvent.Reset();

            //Event for prev
            AutoResetEvent buttonPrevClickedEvent = pres.buttonPrevClickedEvent;
            buttonPrevClickedEvent.Reset();



            int i = 0;

            if (settings.DisplayPres)
            {
                while (i < stepsToGo + 1)
                {
                    renderBlankView();
                    pres.Next = false;
                    pres.Prev = false;

                    WaitHandle[] waitHandles = new WaitHandle[]
                    {
                        buttonNextClickedEvent,
                        buttonPrevClickedEvent
                    };

                    switch (i)
                    {
                        case 0:
                            {
                                renderState0(prgress_step, i);
                                buttonStartClickedEvent.WaitOne();
                                i++;
                                break;
                            }
                        case 1:
                            {
                                renderState1(prgress_step, i);
                                int j = WaitHandle.WaitAny(waitHandles);
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 2:
                            {
                                renderState2(prgress_step, i);
                                WaitHandle.WaitAny(waitHandles);
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 3:
                            {
                                if (!pres.SkipChapter)
                                {
                                    renderState3(prgress_step, i);
                                    WaitHandle.WaitAny(waitHandles);
                                }
                                //Console.WriteLine("Handle fired: pres.Prev: " + pres.Prev + " pres.Next: " + pres.Next);
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 4:
                            {
                                pres.SkipChapter = false;
                                renderState4(prgress_step, i);
                                WaitHandle.WaitAny(waitHandles);
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 5:
                            {
                                if (!pres.SkipChapter)
                                {
                                    renderState5(prgress_step, i);
                                    WaitHandle.WaitAny(waitHandles);
                                }
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 6:
                            {
                                if (!pres.SkipChapter)
                                {
                                    renderState6(prgress_step, i);
                                    WaitHandle.WaitAny(waitHandles);
                                }
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 7:
                            {
                                if (!pres.SkipChapter)
                                {
                                    renderState7(prgress_step, i);
                                    WaitHandle.WaitAny(waitHandles);
                                }
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 8:
                            {
                                if (!pres.SkipChapter)
                                {
                                    renderState8(prgress_step, i);
                                    WaitHandle.WaitAny(waitHandles);
                                }
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;

                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 9:
                            {
                                pres.SkipChapter = false;
                                renderState9(prgress_step, i);
                                WaitHandle.WaitAny(waitHandles);
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }

                        case 10:
                            {
                                if (!pres.SkipChapter)
                                {
                                    renderState10(prgress_step, i);
                                    WaitHandle.WaitAny(waitHandles);
                                }
                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;
                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }
                                break;
                            }
                        case 11:
                            {
                                try
                                {
                                    byte[] result = computeKKDFSHA256XOF(_skm, _key, (int)_outputBytes);

                                    if (result == null)
                                    {
                                        return;
                                    }

                                    //Save to file if configured
                                    if (settings.SaveToFile && !string.IsNullOrEmpty(settings.FilePath))
                                    {
                                        System.IO.StreamWriter file = new System.IO.StreamWriter(settings.FilePath);
                                        int j = 0;
                                        foreach (byte b in result)
                                        {
                                            if (j % 31 == 0)
                                            {
                                                file.Write("\n");
                                            }
                                            file.Write(b.ToString("X2"));
                                            j++;
                                        }
                                        file.Close();
                                    }

                                    _keyMaterial = result;
                                    OnPropertyChanged("KeyMaterial");

                                }
                                catch (System.OutOfMemoryException ex)
                                {
                                    GuiLogMessage(ex.Message + " " + Resources.ExSystemOutOfMemory, NotificationLevel.Error);
                                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                                        pres.lblCalculationHeading.Visibility = Visibility.Hidden;
                                        pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                                        pres.imgCalculation.Visibility = Visibility.Hidden;
                                        pres.txtError.Visibility = Visibility.Visible;

                                    }, null);
                                    return;
                                }

                                if (!pres.SkipChapter)
                                {
                                    double val = prgress_step * i;
                                    ProgressChanged(val, 1);
                                    curStep = i;
                                    refreshStepState();

                                    //Block: Iteration section 
                                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                                        pres.lblCalculationHeading.Visibility = Visibility.Visible;
                                        pres.txtCalculationRounds.Visibility = Visibility.Visible;
                                        pres.imgCalculation.Visibility = Visibility.Visible;

                                        Paragraph p = new Paragraph();
                                        p.Inlines.Add(new Run(Resources.PresCalculationText.Replace("{1}", System.Text.Encoding.UTF8.GetString(_skm)).Replace("{2}", System.Text.Encoding.UTF8.GetString(_key))));
                                        p.TextAlignment = TextAlignment.Left;
                                        pres.txtCalculationRounds.Document.Blocks.Clear();
                                        pres.txtCalculationRounds.Document.Blocks.Add(p);

                                        //Buttons
                                        pres.spButtons.Visibility = Visibility.Visible;

                                        //progress counter
                                        pres.txtStep.Visibility = Visibility.Visible;

                                    }, null);
                                    WaitHandle.WaitAny(waitHandles);
                                }

                                if (pres.Prev)
                                {
                                    pres.Prev = false;
                                    i--;

                                }
                                else
                                {
                                    pres.Next = false;
                                    i++;
                                }

                                break;
                            }

                        case 12:
                            {
                                pres.SkipChapter = false;
                                renderState12(prgress_step, i);
                                buttonRestartClickedEvent.WaitOne();

                                if (pres.Restart)
                                {
                                    goto Restart;
                                }

                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            else
            {
                try
                {
                    byte[] result = computeKKDFSHA256XOF(_skm, _key, (int)_outputBytes);

                    if (result == null)
                    {
                        return;
                    }

                    //Save to file if configured
                    if (settings.SaveToFile && !string.IsNullOrEmpty(settings.FilePath))
                    {
                        System.IO.StreamWriter file = new System.IO.StreamWriter(settings.FilePath);
                        int j = 0;
                        foreach (byte b in result)
                        {
                            if (j % 31 == 0)
                            {
                                file.Write("\n");
                            }
                            file.Write(b.ToString("X2"));
                            j++;
                        }
                        file.Close();
                    }

                    _keyMaterial = result;
                    OnPropertyChanged("KeyMaterial");

                }
                catch (System.OutOfMemoryException ex)
                {
                    GuiLogMessage(ex.Message + " " + Resources.ExSystemOutOfMemory, NotificationLevel.Error);
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                        pres.lblCalculationHeading.Visibility = Visibility.Hidden;
                        pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                        pres.imgCalculation.Visibility = Visibility.Hidden;
                        pres.txtError.Visibility = Visibility.Visible;

                    }, null);
                    return;
                }

                ProgressChanged(1, 1);
            }
        }

        private void renderState12(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Hidden;
                pres.txtExplanationSectionText.Visibility = Visibility.Hidden;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;

                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Visible;
                pres.txtFinished.Visibility = Visibility.Visible;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = true;
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipChapter = false;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState10(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //Block: Iteration section 
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Visible;
                pres.txtCalculationRounds.Visibility = Visibility.Visible;
                pres.imgCalculation.Visibility = Visibility.Visible;

                Paragraph p = new Paragraph();
                p.Inlines.Add(new Run(Resources.PresCalculationText.Replace("{1}", System.Text.Encoding.UTF8.GetString(_skm)).Replace("{2}", System.Text.Encoding.UTF8.GetString(_key))));
                p.TextAlignment = TextAlignment.Left;
                pres.txtCalculationRounds.Document.Blocks.Clear();
                pres.txtCalculationRounds.Document.Blocks.Add(p);

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState9(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //Block: Calculation section heading  
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;
                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Visible;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState8(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //Block: Construction section part 4
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Visible;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Visible;
                pres.imgConstructionSponge.Visibility = Visibility.Visible;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState7(double inkrement, int stepNum)
        {

            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //Block: Construction section part 3
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Visible;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Visible;
                pres.imgConstructionSponge.Visibility = Visibility.Visible;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState6(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //Block: Construction section part 2
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Visible;
                pres.txtConstructionText1.Visibility = Visibility.Visible;
                pres.txtConstructionText2.Visibility = Visibility.Visible;
                pres.txtConstructionScheme.Visibility = Visibility.Visible;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState5(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //Block: Construction section part 1
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Visible;
                pres.txtConstructionText1.Visibility = Visibility.Visible;
                pres.txtConstructionScheme.Visibility = Visibility.Visible;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState4(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //Block: Construction section heading
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblTitleHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;
                pres.lblConstructionSectionHeading.Visibility = Visibility.Visible;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState3(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Visible;
                pres.txtIntroductionText.Visibility = Visibility.Visible;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState2(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            pres.SkipChapter = false;
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblTitleHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;

                //Buttons
                pres.spButtons.Visibility = Visibility.Visible;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState1(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Hidden;
                pres.txtExplanationSectionText.Visibility = Visibility.Hidden;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Visible;

                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipChapter = false;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);

            //Enable buttons
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;
                pres.buttonSkipIntro.IsEnabled = false;
            }, null);
        }

        /// <summary>
        /// renders a state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderState0(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            //clean up for starting
            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.buttonStart.IsEnabled = true;
                pres.buttonRestart.IsEnabled = false;

                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Visible;
                pres.txtExplanationSectionText.Visibility = Visibility.Visible;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;

                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipChapter = false;

            }, null);
        }

        /// <summary>
        /// renders the blank state
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderBlankView()
        {
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblExplanationSectionHeading.Visibility = Visibility.Hidden;
                pres.txtExplanationSectionText.Visibility = Visibility.Hidden;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;

                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;


                //Calculation finished
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;

                //progress counter
                pres.txtStep.Visibility = Visibility.Hidden;
            }, null);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Input for source key material
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputSKMCaption", "InputSKMToolTip", true)]
        public byte[] SKM
        {
            get => _skm;
            set => _skm = value;
        }

        /// <summary>
        /// Input for the key
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyToolTip", true)]
        public byte[] Key
        {
            get => _key;
            set => _key = value;
        }

        /// <summary>
        /// Input for outputlength
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputOutputLengthCaption", "InputOutputLengthToolTip", true)]
        public BigInteger OutputBytes
        {
            get => _outputBytes;
            set => _outputBytes = value;
        }

        /// <summary>
        /// Output for key material
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyMaterialCaption", "OutputKeyMaterialToolTip")]
        public byte[] KeyMaterial
        {
            get => _keyMaterial;
            set => _keyMaterial = value;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => pres;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            //Implementation with threads: this approach handles an inputchange in a better way
            if (workerThread == null)
            {
                workerThread = new Thread(new ThreadStart(tExecute))
                {
                    IsBackground = true
                };
                workerThread.Start();
            }
            else
            {
                if (workerThread.IsAlive)
                {
                    workerThread.Abort();
                    workerThread = new Thread(new ThreadStart(tExecute))
                    {
                        IsBackground = true
                    };
                    workerThread.Start();
                }
                else
                {
                    workerThread = new Thread(new ThreadStart(tExecute))
                    {
                        IsBackground = true
                    };
                    workerThread.Start();
                }
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            if (workerThread.IsAlive)
            {
                workerThread.Abort();
            }

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.Set();

                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Visible;
                pres.txtExplanationSectionText.Visibility = Visibility.Visible;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;


                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;


                //Calculation finished
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipChapter = false;
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;

                //progress counter
                pres.txtStep.Visibility = Visibility.Hidden;

            }, null);
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            Paragraph p = new Paragraph();

            //headline of lblExplanationSectionHeading
            p.Inlines.Add(new Run(Resources.PresExplanationSectionHeading));
            pres.lblExplanationSectionHeading.Document.Blocks.Add(p);
            pres.lblExplanationSectionHeading.Document.Blocks.Remove(pres.lblExplanationSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblTitleHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresTitleHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblTitleHeading.Document.Blocks.Add(p);
            pres.lblTitleHeading.Document.Blocks.Remove(pres.lblTitleHeading.Document.Blocks.FirstBlock);

            //headline of lblIntroductionSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIntroductionSectionHeadingNum));
            p.TextAlignment = TextAlignment.Center;
            pres.lblIntroductionSectionHeading.Document.Blocks.Add(p);
            pres.lblIntroductionSectionHeading.Document.Blocks.Remove(pres.lblIntroductionSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblIntroductionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIntroductionSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblIntroductionHeading.Document.Blocks.Add(p);
            pres.lblIntroductionHeading.Document.Blocks.Remove(pres.lblIntroductionHeading.Document.Blocks.FirstBlock);

            //headline of PresConstructionSectionHeadingNum
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionSectionHeadingNum));
            p.TextAlignment = TextAlignment.Center;
            pres.lblConstructionSectionHeading.Document.Blocks.Add(p);
            pres.lblConstructionSectionHeading.Document.Blocks.Remove(pres.lblConstructionSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblConstructionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblConstructionHeading.Document.Blocks.Add(p);
            pres.lblConstructionHeading.Document.Blocks.Remove(pres.lblConstructionHeading.Document.Blocks.FirstBlock);

            //headline of lblCalculationSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresCalculationSectionHeadingNum));
            p.TextAlignment = TextAlignment.Center;
            pres.lblCalculationSectionHeading.Document.Blocks.Add(p);
            pres.lblCalculationSectionHeading.Document.Blocks.Remove(pres.lblCalculationSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblCalculationHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresCalculationSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblCalculationHeading.Document.Blocks.Add(p);
            pres.lblCalculationHeading.Document.Blocks.Remove(pres.lblCalculationHeading.Document.Blocks.FirstBlock);

            //headline of lblFinishedSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresFinishedSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblFinishedSectionHeading.Document.Blocks.Add(p);
            pres.lblFinishedSectionHeading.Document.Blocks.Remove(pres.lblFinishedSectionHeading.Document.Blocks.FirstBlock);

            //text of txtIntroductionText
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIntroductionPart1Text));
            p.TextAlignment = TextAlignment.Left;
            pres.txtIntroductionText.Document.Blocks.Add(p);
            pres.txtIntroductionText.Document.Blocks.Remove(pres.txtIntroductionText.Document.Blocks.FirstBlock);

            //text of txtConstructionText1
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionPart1Text));
            p.TextAlignment = TextAlignment.Left;
            pres.txtConstructionText1.Document.Blocks.Add(p);
            pres.txtConstructionText1.Document.Blocks.Remove(pres.txtConstructionText1.Document.Blocks.FirstBlock);

            //text of txtConstructionScheme
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionScheme));
            p.TextAlignment = TextAlignment.Left;
            pres.txtConstructionScheme.Document.Blocks.Add(p);
            pres.txtConstructionScheme.Document.Blocks.Remove(pres.txtConstructionScheme.Document.Blocks.FirstBlock);

            //text of txtConstructionText2
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionPart2Text));
            p.TextAlignment = TextAlignment.Left;
            pres.txtConstructionText2.Document.Blocks.Add(p);
            pres.txtConstructionText2.Document.Blocks.Remove(pres.txtConstructionText2.Document.Blocks.FirstBlock);

            //text of txtError
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresErrorText));
            p.TextAlignment = TextAlignment.Left;
            pres.txtError.Document.Blocks.Add(p);
            pres.txtError.Document.Blocks.Remove(pres.txtError.Document.Blocks.FirstBlock);

            //text of txtFinished
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresFinishedText));
            p.TextAlignment = TextAlignment.Left;
            pres.txtFinished.Document.Blocks.Add(p);
            pres.txtFinished.Document.Blocks.Remove(pres.txtFinished.Document.Blocks.FirstBlock);

            //for formatting the text 
            string[] parts = Resources.PresSectionIntroductionText.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            p = new Paragraph();
            bool isBold = false;
            foreach (string part in parts)
            {
                if (part.Contains("<Underline>"))
                {
                    string[] underlinedParts = part.Split(new[] { "<Underline>", "</Underline>" }, StringSplitOptions.None);
                    bool isUnderlined = false;
                    foreach (string underlinePart in underlinedParts)
                    {
                        if (isUnderlined)
                        {
                            p.Inlines.Add(new Underline(new Bold((new Run(underlinePart)))));
                        }
                        else
                        {
                            p.Inlines.Add(new Run(underlinePart));
                        }
                        isUnderlined = !isUnderlined;
                        isBold = !isBold;
                    }
                    continue;
                }
                if (isBold)
                {
                    p.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    p.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
            pres.txtExplanationSectionText.Document.Blocks.Add(p);
            pres.txtExplanationSectionText.Document.Blocks.Remove(pres.txtExplanationSectionText.Document.Blocks.FirstBlock);

            //for formatting the text 
            parts = Resources.PresConstructionPart3Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            p = new Paragraph();
            isBold = false;
            foreach (string part in parts)
            {
                if (isBold)
                {
                    p.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    //pres.txtConstructionText3.Inlines.Add(new Run(part));
                    p.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
            pres.txtConstructionText3.Document.Blocks.Add(p);
            pres.txtConstructionText3.Document.Blocks.Remove(pres.txtConstructionText3.Document.Blocks.FirstBlock);

            //for formatting the text 
            parts = Resources.PresConstructionPart4Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            p = new Paragraph();
            isBold = false;
            foreach (string part in parts)
            {
                if (isBold)
                {
                    p.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    p.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
            pres.txtConstructionText4.Document.Blocks.Add(p);
            pres.txtConstructionText4.Document.Blocks.Remove(pres.txtConstructionText4.Document.Blocks.FirstBlock);
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

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
