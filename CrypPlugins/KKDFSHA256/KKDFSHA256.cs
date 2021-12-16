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
using KKDFSHA256.Properties;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace CrypTool.Plugins.KKDFSHA256
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("KKDFSHA256.Properties.Resources", "PluginCaption", "KKDFSHA256Tooltip", "KKDFSHA256/userdoc.xml", new[] { "KKDFSHA256/images/icon.png" })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class KKDFSHA256 : ICrypComponent
    {
        #region Private Variables

        private readonly KKDFSHA256Settings settings = new KKDFSHA256Settings();
        private readonly KKDFSHA256Pres pres = new KKDFSHA256Pres();
        private byte[] _skm;
        private byte[] _key;
        private BigInteger _outputBytes;
        private byte[] _keyMaterial;
        private byte[] _keyMaterialDebug;
        private Thread workerThread;
        private int stepsToGo = 0;
        private int curStep = 0;

        #endregion

        #region Methods for calculation

        /// <summary>
        /// Computes the KKDFSHA256 function with a 32 bit counter. Its like the traditional KDF described in https://eprint.iacr.org/2010/264.pdf but has a 32-bit counter for longer outputs
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <param name="outputBytes"></param>
        /// <param name="show"></param>
        /// <param name="buttonEvent"></param>
        /// <returns></returns>
        private byte[] computeKKDFSHA256_IntCTR(byte[] msg, byte[] key, int outputBytes, bool show, AutoResetEvent buttonEvent, ref double curVal, double incVal)
        {
            //hash object
            Sha256Digest sha256 = new Sha256Digest();
            //calculates the ceil(iteration) rounds
            double N = Math.Ceiling(Convert.ToDouble(outputBytes) / sha256.GetDigestSize());

            if (N > 4294967296)
            {
                throw new TooMuchOutputRequestedException(Resources.ExToMuchOutputRequested.Replace("{0}", outputBytes.ToString()).Replace("{1}", (4294967295 * sha256.GetDigestSize()).ToString()));
            }

            //counter as hex beginning with zero byte. Counter has a size of 16 bits.
            int CTR = 0;
            //input byte array as: key || ctr || msg
            byte[] input = new byte[key.Length + sizeof(int) + msg.Length];
            //output byte array for all rounds of the iteration
            byte[] km = new byte[Convert.ToInt32(N) * sha256.GetDigestSize()];
            //output byte array for the function. in case of truncated output
            byte[] result = new byte[outputBytes];
            //output array for temp output for debug in the ui
            byte[] tmp_result = new byte[sha256.GetDigestSize()];

            //prepare input array
            System.Buffer.BlockCopy(key, 0, input, 0, key.Length);
            System.Buffer.BlockCopy(msg, 0, input, key.Length + sizeof(int), msg.Length);

            for (int i = 0; i < N; i++, CTR++)
            {
                //sets the counter
                System.Buffer.BlockCopy(BitConverter.GetBytes(CTR), 0, input, key.Length, sizeof(int));
                //update internal hash state
                sha256.BlockUpdate(input, 0, input.Length);
                //finish the hashing: leaves the state resetted for the next round
                sha256.DoFinal(km, i * sha256.GetDigestSize());

                System.Buffer.BlockCopy(km, i * sha256.GetDigestSize(), tmp_result, 0, tmp_result.Length);
                StringBuilder strBuilderDebug = new StringBuilder();
                StringBuilder strBuilderPres = new StringBuilder();
                strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()).Replace("{ord}", calcOrdinalNum(i + 1)));
                strBuilderPres.Append(Resources.PresKeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()).Replace("{ord}", calcOrdinalNum(i + 1)));

                //Generate formatted output for debug output textfield
                string tmp = "";
                for (int j = 1, k = 1; j <= sha256.GetDigestSize(); j++)
                {
                    tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                    if (j % 8 == 0)
                    {
                        strBuilderDebug.Replace("{" + k + "}", tmp);
                        strBuilderPres.Replace("{" + k + "}", tmp);
                        k++;
                        tmp = "";
                    }
                }

                _keyMaterialDebug = Encoding.UTF8.GetBytes(strBuilderDebug.ToString());
                OnPropertyChanged("KeyMaterialDebug");

                if (show && !pres.SkipChapter && !(i == (N - 1)))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        Paragraph p = new Paragraph();
                        p.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i + 1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationRounds.Document.Blocks.Clear();
                        pres.txtIterationRounds.Document.Blocks.Add(p);

                        p = new Paragraph();
                        p.Inlines.Add(new Run(strBuilderPres.ToString()));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationDebugOutput.Document.Blocks.Clear();
                        pres.txtIterationDebugOutput.Document.Blocks.Add(p);
                    }, null);

                    buttonEvent = pres.buttonNextClickedEvent;
                    buttonEvent.WaitOne();

                }
                else if (show && !pres.SkipChapter && !(i == N))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        Paragraph p = new Paragraph();
                        p.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i + 1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationRounds.Document.Blocks.Clear();
                        pres.txtIterationRounds.Document.Blocks.Add(p);

                        p = new Paragraph();
                        p.Inlines.Add(new Run(strBuilderPres.ToString()));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationDebugOutput.Document.Blocks.Clear();
                        pres.txtIterationDebugOutput.Document.Blocks.Add(p);
                    }, null);
                }

                if (pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.imgIteration.Visibility = Visibility.Hidden;
                        pres.txtIterationRounds.Visibility = Visibility.Hidden;
                        pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                    }, null);
                }

                if (!(i == (N - 1)))
                {
                    curVal += incVal;
                    ProgressChanged(curVal, 1);
                    curStep++;
                    refreshStepState();
                }
            }

            //truncated output
            System.Buffer.BlockCopy(km, 0, result, 0, outputBytes);
            return result;
        }


        /// <summary>
        /// Computes the KKDFSHA256 function with a 8 bit counter. Its like the traditional KDF described in https://eprint.iacr.org/2010/264.pdf
        /// Attention: Online hasher like https://hashgenerator.de/ are using for every input char a single byte. If you're trying to compare single inputs with a online hasher, you need to know, that they are using 
        /// ASCII encoding for the counter. That means, the string 0 is representated as 0x30, my representation starts with 0x00. Thats why you would need to adapt the input byte in my function to 0x30 if you like to compare.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <param name="outputBytes"></param>
        /// <returns>
        private byte[] computeKKDFSHA256_8BitCTR(byte[] msg, byte[] key, int outputBytes, bool show, AutoResetEvent buttonEvent, ref double curVal, double incVal)
        {
            //hash object
            Sha256Digest sha256 = new Sha256Digest();
            //calculates the ceil(iteration) rounds
            double N = Math.Ceiling(Convert.ToDouble(outputBytes) / sha256.GetDigestSize());
            //counter as hex beginning with zero byte
            byte CTR = 0x00;
            //input byte array as: key || ctr || msg
            byte[] input = new byte[key.Length + 1 + msg.Length];
            //output byte array for all rounds of the iteration
            byte[] km = new byte[Convert.ToInt32(N) * sha256.GetDigestSize()];
            //output byte array for the function. in case of truncated output
            byte[] result = new byte[outputBytes];
            //output array for temp output for debug in the ui
            byte[] tmp_result = new byte[sha256.GetDigestSize()];

            //prepare input array
            System.Buffer.BlockCopy(key, 0, input, 0, key.Length);
            System.Buffer.BlockCopy(msg, 0, input, key.Length + 1, msg.Length);

            if (N > 255)
            {
                throw new TooMuchOutputRequestedException(Resources.ExToMuchOutputRequested.Replace("{0}", outputBytes.ToString()).Replace("{1}", (255 * sha256.GetDigestSize()).ToString()));
            }

            for (int i = 0; i < N; i++, CTR++)
            {
                //sets the counter
                input[key.Length] = CTR;
                //update internal hash state
                sha256.BlockUpdate(input, 0, input.Length);
                //finish the hashing: leaves the state resetted for the next round
                sha256.DoFinal(km, i * sha256.GetDigestSize());

                System.Buffer.BlockCopy(km, i * sha256.GetDigestSize(), tmp_result, 0, tmp_result.Length);
                StringBuilder strBuilderDebug = new StringBuilder();
                StringBuilder strBuilderPres = new StringBuilder();
                strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()).Replace("{ord}", calcOrdinalNum(i + 1)));
                strBuilderPres.Append(Resources.PresKeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()).Replace("{ord}", calcOrdinalNum(i + 1)));

                //Generate formatted output for debug output textfield
                string tmp = "";
                for (int j = 1, k = 1; j <= sha256.GetDigestSize(); j++)
                {
                    tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                    if (j % 8 == 0)
                    {
                        strBuilderDebug.Replace("{" + k + "}", tmp);
                        strBuilderPres.Replace("{" + k + "}", tmp);
                        k++;
                        tmp = "";
                    }
                }

                _keyMaterialDebug = Encoding.UTF8.GetBytes(strBuilderDebug.ToString());
                OnPropertyChanged("KeyMaterialDebug");

                if (show && !pres.SkipChapter && !(i == (N - 1)))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        Paragraph p = new Paragraph();
                        p.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i + 1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationRounds.Document.Blocks.Clear();
                        pres.txtIterationRounds.Document.Blocks.Add(p);

                        p = new Paragraph();
                        p.Inlines.Add(new Run(strBuilderPres.ToString()));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationDebugOutput.Document.Blocks.Clear();
                        pres.txtIterationDebugOutput.Document.Blocks.Add(p);
                    }, null);

                    buttonEvent = pres.buttonNextClickedEvent;
                    buttonEvent.WaitOne();

                }
                else if (show && !pres.SkipChapter && !(i == N))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        Paragraph p = new Paragraph();
                        p.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i + 1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationRounds.Document.Blocks.Clear();
                        pres.txtIterationRounds.Document.Blocks.Add(p);

                        p = new Paragraph();
                        p.Inlines.Add(new Run(strBuilderPres.ToString()));
                        p.TextAlignment = TextAlignment.Left;
                        pres.txtIterationDebugOutput.Document.Blocks.Clear();
                        pres.txtIterationDebugOutput.Document.Blocks.Add(p);
                    }, null);

                }

                if (pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.imgIteration.Visibility = Visibility.Hidden;
                        pres.txtIterationRounds.Visibility = Visibility.Hidden;
                        pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                    }, null);
                }

                if (!(i == (N - 1)))
                {
                    curVal += incVal;
                    ProgressChanged(curVal, 1);
                    curStep++;
                    refreshStepState();
                }
            }

            //truncat output
            System.Buffer.BlockCopy(km, 0, result, 0, outputBytes);
            return result;
        }

        /// <summary>
        /// Calculates the ordinal numbers
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private string calcOrdinalNum(int num)
        {
            if (CultureInfo.CurrentUICulture.Name == "en-US")
            {
                if (num <= 0)
                {
                    return num.ToString();
                }

                switch (num % 100)
                {
                    case 11:
                    case 12:
                    case 13:
                        return "th";
                }
                switch (num % 10)
                {
                    case 1:
                        return "st";
                    case 2:
                        return "nd";
                    case 3:
                        return "rd";
                    default:
                        return "th";
                }
            }
            else
            {
                return ".";
            }
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

                    //headline of lblExplanationSectionHeading
                    p.Inlines.Add(new Run(Resources.PresStepText.Replace("{0}", curStep.ToString()).Replace("{1}", stepsToGo.ToString())));
                    p.TextAlignment = TextAlignment.Right;
                    pres.txtStep.Document.Blocks.Add(p);
                    pres.txtStep.Document.Blocks.Remove(pres.txtStep.Document.Blocks.FirstBlock);
                }, null);
            }
        }

        /// <summary>
        /// the method to be called in the workerthread
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

            _keyMaterialDebug = Encoding.UTF8.GetBytes("");
            OnPropertyChanged("KeyMaterialDebug");

            //clean steps
            curStep = 0;

            //Check for output length: max 5.242.880 byte = 5 Mb
            if (!settings.InfinityOutput && OutputBytes > 5242880)
            {
                GuiLogMessage(Resources.TooMuchOutputRequestedLogMSG.Replace("{0}", OutputBytes.ToString()), NotificationLevel.Warning);
                OutputBytes = 5242880;
                OnPropertyChanged("OutputBytes");
            }
            if (settings.InfinityOutput && OutputBytes > 255 * (new Sha256Digest()).GetDigestSize())
            {
                GuiLogMessage(Resources.TooMuchOutputRequestedLogForKKDFStd.Replace("{0}", OutputBytes.ToString()), NotificationLevel.Warning);
                OutputBytes = 8160;
                OnPropertyChanged("OutputBytes");
            }

            //eight steps takes the presentation
            double steps = (Math.Ceiling(Convert.ToDouble((int)OutputBytes) / (new Sha256Digest()).GetDigestSize())) + 9;
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
                while (i < 11)
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
                                pres.SkipChapter = false;
                                renderState8(prgress_step, i);
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
                        case 9:
                            {
                                double val = (prgress_step * 9);
                                ProgressChanged(val, 1);
                                curStep = 9;
                                refreshStepState();

                                //Clean up outputs
                                _keyMaterial = Encoding.UTF8.GetBytes("");
                                OnPropertyChanged("KeyMaterial");

                                _keyMaterialDebug = Encoding.UTF8.GetBytes("");
                                OnPropertyChanged("KeyMaterialDebug");

                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                                    pres.lblIterationHeading.Visibility = Visibility.Visible;
                                    pres.txtIterationRounds.Visibility = Visibility.Visible;
                                    pres.imgIteration.Visibility = Visibility.Visible;
                                    pres.txtIterationDebugOutput.Visibility = Visibility.Visible;

                                    //Buttons
                                    pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                                    pres.buttonStart.IsEnabled = false;
                                    pres.buttonRestart.IsEnabled = false;
                                    pres.spButtons.Visibility = Visibility.Visible;
                                    pres.buttonSkipIntro.IsEnabled = true;
                                    pres.buttonNext.IsEnabled = true;
                                    pres.buttonPrev.IsEnabled = false;

                                    //progress counter
                                    pres.txtStep.Visibility = Visibility.Visible;
                                }, null);

                                try
                                {
                                    byte[] result;
                                    if (settings.InfinityOutput)
                                    {
                                        result = computeKKDFSHA256_8BitCTR(_skm, _key, (int)_outputBytes, settings.DisplayPres, buttonNextClickedEvent, ref val, prgress_step);
                                    }
                                    else
                                    {
                                        result = computeKKDFSHA256_IntCTR(_skm, _key, (int)_outputBytes, settings.DisplayPres, buttonNextClickedEvent, ref val, prgress_step);
                                    }

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

                                    if (!pres.SkipChapter && settings.DisplayPres)
                                    {
                                        buttonNextClickedEvent = pres.buttonNextClickedEvent;
                                        buttonNextClickedEvent.WaitOne();
                                    }

                                    val += prgress_step;
                                    ProgressChanged(val, 1);
                                    curStep++;
                                    refreshStepState();

                                    _keyMaterial = result;
                                    OnPropertyChanged("KeyMaterial");
                                }
                                //in case of too long outputs specified
                                catch (TooMuchOutputRequestedException ex)
                                {
                                    GuiLogMessage(ex.Message, NotificationLevel.Error);
                                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                                        pres.lblIterationHeading.Visibility = Visibility.Hidden;
                                        pres.txtIterationRounds.Visibility = Visibility.Hidden;
                                        pres.imgIteration.Visibility = Visibility.Hidden;
                                        pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                                        pres.txtError.Visibility = Visibility.Visible;
                                    }, null);
                                    return;
                                }
                                catch (System.OutOfMemoryException ex)
                                {
                                    GuiLogMessage(ex.Message + " " + Resources.ExSystemOutOfMemory, NotificationLevel.Error);
                                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                                        pres.lblIterationHeading.Visibility = Visibility.Hidden;
                                        pres.txtIterationRounds.Visibility = Visibility.Hidden;
                                        pres.imgIteration.Visibility = Visibility.Hidden;
                                        pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                                        pres.txtError.Visibility = Visibility.Visible;
                                    }, null);
                                    return;
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
                        case 10:
                            {
                                pres.SkipChapter = false;
                                renderState10(prgress_step, (int)steps);
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
                double val = (prgress_step * 9);
                ProgressChanged(val, 1);
                curStep = 9;
                refreshStepState();

                try
                {
                    byte[] result;
                    if (settings.InfinityOutput)
                    {
                        result = computeKKDFSHA256_8BitCTR(_skm, _key, (int)_outputBytes, settings.DisplayPres, buttonNextClickedEvent, ref val, prgress_step);
                    }
                    else
                    {
                        result = computeKKDFSHA256_IntCTR(_skm, _key, (int)_outputBytes, settings.DisplayPres, buttonNextClickedEvent, ref val, prgress_step);
                    }

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

                    if (!pres.SkipChapter && settings.DisplayPres)
                    {
                        buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }

                    val += prgress_step;
                    ProgressChanged(val, 1);
                    curStep++;
                    refreshStepState();

                    _keyMaterial = result;
                    OnPropertyChanged("KeyMaterial");
                }
                //in case of too long outputs specified
                catch (TooMuchOutputRequestedException ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Error);
                    return;
                }
                catch (System.OutOfMemoryException ex)
                {
                    GuiLogMessage(ex.Message + " " + Resources.ExSystemOutOfMemory, NotificationLevel.Error);
                    return;
                }
            }
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
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgIteration.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Visible;
                pres.txtFinished.Visibility = Visibility.Visible;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = true;

                pres.spButtons.Visibility = Visibility.Hidden;

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
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Hidden;

                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Hidden;
                pres.lblTitleHeading.Visibility = Visibility.Hidden;

                pres.lblIterationSectionHeading.Visibility = Visibility.Visible;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

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

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Visible;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Visible;
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Visible;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

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

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Visible;
                pres.txtConstructionText1.Visibility = Visibility.Visible;
                pres.txtConstructionText2.Visibility = Visibility.Visible;
                pres.txtConstructionScheme.Visibility = Visibility.Visible;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

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

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Visible;
                pres.txtConstructionText1.Visibility = Visibility.Visible;
                pres.txtConstructionScheme.Visibility = Visibility.Visible;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

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

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblTitleHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;
                pres.lblConstructionSectionHeading.Visibility = Visibility.Visible;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

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
                pres.lblTitleHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Visible;
                pres.txtIntroductionText.Visibility = Visibility.Visible;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

            }, null);
        }

        private void renderState2(double inkrement, int stepNum)
        {
            double val = stepNum * inkrement;
            ProgressChanged(val, 1);
            curStep = stepNum;
            refreshStepState();

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.lblTitleHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Visible;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = true;
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

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
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.imgIteration.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;

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
                pres.buttonNext.IsEnabled = true;
                pres.buttonPrev.IsEnabled = true;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

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
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgIteration.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.buttonRestart.IsEnabled = false;
                pres.buttonStart.IsEnabled = true;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;

            }, null);
        }

        /// <summary>
        /// hides all elements
        /// </summary>
        /// <param name="inkrement"></param>
        /// <param name="stepNum"></param>
        private void renderBlankView()
        {
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;

                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Hidden;
                pres.txtExplanationSectionText.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

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
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgIteration.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.spButtons.Visibility = Visibility.Hidden;

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

        /// <summary>
        /// Output for debug
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyMaterialDebugCaption", "OutputKeyMaterialDebugToolTip")]
        public byte[] KeyMaterialDebug
        {
            get => _keyMaterialDebug;
            set => _keyMaterialDebug = value;
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
                pres.imgConstructionKKDFSHA256.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgIteration.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
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

            refreshStepState();

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

            //headline of lblConstructionSectionHeading
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

            //text of txtConstructionText3
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionPart3Text));
            p.TextAlignment = TextAlignment.Left;
            pres.txtConstructionText3.Document.Blocks.Add(p);
            pres.txtConstructionText3.Document.Blocks.Remove(pres.txtConstructionText3.Document.Blocks.FirstBlock);

            //headline of lblIterationSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIterationSectionHeadingNum));
            p.TextAlignment = TextAlignment.Center;
            pres.lblIterationSectionHeading.Document.Blocks.Add(p);
            pres.lblIterationSectionHeading.Document.Blocks.Remove(pres.lblIterationSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblIterationHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIterationSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblIterationHeading.Document.Blocks.Add(p);
            pres.lblIterationHeading.Document.Blocks.Remove(pres.lblIterationHeading.Document.Blocks.FirstBlock);

            //headline of lblFinishedSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresFinishedSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblFinishedSectionHeading.Document.Blocks.Add(p);
            pres.lblFinishedSectionHeading.Document.Blocks.Remove(pres.lblFinishedSectionHeading.Document.Blocks.FirstBlock);

            //text of txtFinished
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresFinishedText));
            p.TextAlignment = TextAlignment.Left;
            pres.txtFinished.Document.Blocks.Add(p);
            pres.txtFinished.Document.Blocks.Remove(pres.txtFinished.Document.Blocks.FirstBlock);

            //text of txtError
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresErrorText));
            p.TextAlignment = TextAlignment.Left;
            pres.txtError.Document.Blocks.Add(p);
            pres.txtError.Document.Blocks.Remove(pres.txtError.Document.Blocks.FirstBlock);

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
            parts = Resources.PresIntroductionPart1Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
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
            pres.txtIntroductionText.Document.Blocks.Add(p);
            pres.txtIntroductionText.Document.Blocks.Remove(pres.txtIntroductionText.Document.Blocks.FirstBlock);

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
