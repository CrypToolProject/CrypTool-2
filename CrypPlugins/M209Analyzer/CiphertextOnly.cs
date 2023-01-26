using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.Utils;
using M209Analyzer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cryptool.Plugins.M209Analyzer
{
    internal class CiphertextOnly
    {
        public event EventHandler<OnLogEventEventArgs> OnLogEvent;
        public class OnLogEventEventArgs : EventArgs {
            public string Message;
            public NotificationLevel LogLevel;
            public OnLogEventEventArgs(string message, NotificationLevel logLevel)
            {
                Message = message;
                LogLevel = logLevel;
            }
        }
        private void LogMessage(string message, NotificationLevel logLevel)
        { 
            OnLogEvent?.Invoke(this, new OnLogEventEventArgs(message, logLevel));
        }
        //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
        private Grams grams;

        public bool Running = false;

        private Random Randomizer = new Random();

        private M209CipherMachine _m209 = new M209CipherMachine(new string[27] {
            "36","06","16","15","45","04","04","04","04",
            "20","20","20","20","20","20","20","20","20",
            "20","25","25","05","05","05","05","05","05"
        });

        public CiphertextOnly(Grams grams)
        {
            this.grams = grams;
        }

        public int [] GetCipherArray(string ciphertext)
        {
            int [] cipherArray = new int[ciphertext.Length];
            for (int i = 0; i < ciphertext.Length; i++)
            {
                cipherArray.Append(this._m209.LetterToInt(ciphertext[i]));
            }
            return cipherArray;
        }

        public string HCOuter(string ciphertext, string versionOfInstruction)
        {

            PinSettings BestPins = new PinSettings(versionOfInstruction);
            LugSettings BestLugs = new LugSettings(new string[27] {
                "36","06","16","15","45","04","04","04","04",
                "20","20","20","20","20","20","20","20","20",
                "20","25","25","05","05","05","05","05","05"
            });

            BestLugs.Randomize();
            BestPins.Randomize();

            this._m209.LugSettings = BestLugs;
            this._m209.PinSetting = BestPins;

            double bestScore = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));
            double score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

            bool improved = true;
            int localCounter = 100;
            do
            {
                improved = false;


                // Simple transformations
                for (int bar1 = 0; bar1 < 6; bar1++)
                {
                    for (int bar2 = 0; bar2 < 6; bar2++)
                    {
                        if ((bar1 < bar2) && (bar1 != bar2))
                        {
                            for (int i = 0; i < 2; i++)
                            {   
                                if(i == 0)
                                {
                                    this._m209.LugSettings.ApplySimpleTransformation(bar1, bar2);
                                } else
                                {
                                    this._m209.LugSettings.ApplySimpleTransformation(bar2, bar1);
                                }

                                //this._m209.PinSetting = SAInner(ciphertext, this._m209.LugSettings, "V");
                                this._m209.PinSetting = SA(ciphertext, this._m209.LugSettings, this._m209.PinSetting, "V", 1);

                                score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                                if (score > bestScore)
                                {
                                    LogMessage($"Improved: bestScore = {bestScore}, score = {score}", NotificationLevel.Info);
                                    bestScore = score;
                                    BestPins = this._m209.PinSetting;
                                    BestLugs = this._m209.LugSettings;
                                    improved = true;
                                }
                                else
                                {
                                    LogMessage($"No improvement ({localCounter}): bestScore = {bestScore}, score = {score}", NotificationLevel.Info);
                                    localCounter--;
                                }

                                if (!Running) return "Exit";
                            }
                        }

                    }
                }

                // Complex transformations
                if (!improved)
                {
                    for (int bar1 = 0; bar1 < 6 && !improved; bar1++)
                    {
                        for (int bar2 = bar1 + 1; bar2 < 6 && !improved; bar2++)
                        {
                            for (int bar3 = bar2 + 1; bar3 < 6 && !improved; bar3++)
                            {
                                for (int bar4 = bar3 + 1; bar4 < 6 && !improved; bar4++)
                                {
                                    for (int i = 0; i < 6; i++)
                                    {
                                        this._m209.LugSettings.ApplyTransformationComplex(bar1, bar2, bar3, bar4, i);

                                        //this._m209.PinSetting = SAInner(ciphertext, this._m209.LugSettings, "V");
                                        this._m209.PinSetting = SA(ciphertext, this._m209.LugSettings, this._m209.PinSetting, "V", 1);

                                        score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                                        if (score > bestScore)
                                        {
                                            LogMessage($"Improved: bestScore = {bestScore}, score = {score}", NotificationLevel.Info);
                                            bestScore = score;
                                            BestPins = this._m209.PinSetting;
                                            BestLugs = this._m209.LugSettings;
                                            improved = true;
                                        }
                                        else
                                        {
                                            LogMessage($"No improvement ({localCounter}): bestScore = {bestScore}, score = {score}", NotificationLevel.Info);
                                            localCounter--;
                                        }

                                        if (!Running) return "Exit";
                                    }
                                }
                            }
                        }
                    }
                }

            } while (!improved);
            return "BestLugs, BestPins";
        }

        /// <summary>
        /// From the java project
        /// </summary>
        /// <param name="roundLayers"></param>
        /// <param name="layers"></param>
        /// <param name="ciphertext"></param>
        /// <param name="lugSettings"></param>
        /// <param name="pinSetting"></param>
        /// <param name="versionOfInstruction"></param>
        /// <param name="cycles"></param>
        /// <returns></returns>
        /// private double SA(int[] roundLayers, int layers, string ciphertext, LugSettings lugSettings, PinSettings pinSetting, string versionOfInstruction, int cycles)
        private PinSettings SA(string ciphertext, LugSettings lugSettings, PinSettings pinSetting, string versionOfInstruction, int cycles)
        {
            double bestSAScore = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

            for (int i = 0; i < cycles; i++)
            {
                //roundLayers[layers] = cycles;
                pinSetting.Randomize();

                //roundLayers[layers + 1] = 0;
                for (double temperature = 1000.0; temperature >= 1.0; temperature /= 1.1)
                {
                    //bestSAScore = step(ciphertext, roundLayers, layers + 2, lugSettings, pinSetting, bestSAScore, temperature);
                    bestSAScore = step(ciphertext, lugSettings, pinSetting, bestSAScore, temperature);
                }
                double previous;
                do
                {
                    previous = bestSAScore;
                    //bestSAScore = step(ciphertext, roundLayers, layers + 2, lugSettings, pinSetting, bestSAScore, 0.0);
                    bestSAScore = step(ciphertext, lugSettings, pinSetting, bestSAScore, 0.0);
                } while (bestSAScore > previous);
            }

            //return bestSAScore;
            return pinSetting;
        }

        /// <summary>
        /// From the java project.
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundLayers"></param>
        /// <param name="layers"></param>
        /// <param name="lugSettings"></param>
        /// <param name="pinSettings"></param>
        /// <param name="bestSAScore"></param>
        /// <param name="temperature"></param>
        /// <returns></returns>
        /// private double step(string ciphertext, int[] roundLayers, int layers, LugSettings lugSettings, PinSettings pinSettings, double bestSAScore, double temperature)
        private double step(string ciphertext, LugSettings lugSettings, PinSettings pinSettings, double bestSAScore, double temperature)
        {
            double currLocalScore = 0;

            int MAX_COUNT = pinSettings.maxCount();
            int MIN_COUNT = pinSettings.minCount();

            double newScore;
            int count;


            bool changeAccepted = false;

            for (int w = 0; w < pinSettings.Wheels.Length; w++)
            {
                for (int p = 0; p < pinSettings.Wheels[w].Length; p++)
                {
                    if(!Running) return 0.0;

                    pinSettings.Wheels[w].TogglePinValue(p);
                    count = pinSettings.count();
                    if (count < MIN_COUNT || count > MAX_COUNT) // TODO: Add longSeq
                    {
                        pinSettings.Wheels[w].TogglePinValue(p);
                        continue;
                    }

                    this._m209.PinSetting = pinSettings;
                    this._m209.LugSettings = lugSettings;
                    newScore = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                    double D = newScore - currLocalScore;
                    if (D > 0 || this.Randomizer.NextDouble() < Math.Exp(-(Math.Abs(D) / temperature)))
                    {
                        currLocalScore = newScore;
                        changeAccepted = true;
                        if(newScore > bestSAScore)
                        {
                            bestSAScore = newScore;
                        }
                    } else
                    {
                        pinSettings.Wheels[w].TogglePinValue(p);
                    }


                }
            }
            return 0.0;
        }

        private PinSettings SAInner(string ciphertext, LugSettings lugSetting, string versionOfInstruction)
        {
            LogMessage($"SAInner \n", NotificationLevel.Info);
            double T = 0;
            double alpha = 0.9;

            PinSettings BestPins = new PinSettings(versionOfInstruction);
            BestPins.Randomize();

            PinSettings CurrentPins = BestPins;

            PinSettings[] neighbourPinSettings = new PinSettings[] { BestPins };

            this._m209.PinSetting = BestPins;
            this._m209.LugSettings = lugSetting;
            double bestScore = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

            for (int i = 0; i < 5; i++)
            {
                for (int k = 0; k < neighbourPinSettings.Length; k++)
                {
                    this._m209.PinSetting = neighbourPinSettings[k];
                    this._m209.LugSettings = lugSetting;
                    double score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                    this._m209.PinSetting = CurrentPins;
                    this._m209.LugSettings = lugSetting;
                    double currentScore = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                    LogMessage($"SA: currentScore = {bestScore}, score = {score}", NotificationLevel.Info);

                    double D = score - currentScore;
                    if (D > 0 || this.Randomizer.NextDouble() < Math.Exp(-(Math.Abs(D) / T)))
                    {
                        CurrentPins = neighbourPinSettings[k];
                        if (score > bestScore)
                        {
                            BestPins = neighbourPinSettings[k];
                        }
                        break;
                    }
                }
                T = alpha * T;
            }
            return BestPins;
        }
    }
}
