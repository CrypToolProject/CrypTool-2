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

        /// <summary>
        /// The outer hillclimbing algorithm responsible for the lug settings.
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="versionOfInstruction"></param>
        /// <returns></returns>
        public double HCOuter(string ciphertext, string versionOfInstruction)
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
            double score = bestScore;

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
                                LogMessage($"\t ApplySimpleTransformation", NotificationLevel.Info);

                                //this._m209.PinSetting = SAInner(ciphertext, this._m209.LugSettings, "V");
                                this._m209.PinSetting = SA(ciphertext, this._m209.LugSettings, this._m209.PinSetting, "V", 1);

                                score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                                if (score > bestScore)
                                {
                                    LogMessage($"Improved: bestScore = {bestScore}, score = {score} <{this._m209.Encrypt(ciphertext)}", NotificationLevel.Info);
                                    bestScore = score;
                                    BestPins = this._m209.PinSetting;
                                    BestLugs = this._m209.LugSettings;
                                    improved = true;
                                }
                                else
                                {
                                    LogMessage($"No improvement ({localCounter}): bestScore = {bestScore}, score = {score} <{this._m209.Encrypt(ciphertext)}", NotificationLevel.Info);
                                    localCounter--;
                                }

                                if (!Running) return 0.0;
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
                                        this._m209.LugSettings.ApplyComplexTransformation(bar1, bar2, bar3, bar4, i);
                                        LogMessage($"\t ApplyComplexTransformation", NotificationLevel.Info);

                                        //this._m209.PinSetting = SAInner(ciphertext, this._m209.LugSettings, "V");
                                        this._m209.PinSetting = SA(ciphertext, this._m209.LugSettings, this._m209.PinSetting, "V", 1);

                                        score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                                        if (score > bestScore)
                                        {
                                            LogMessage($"Improved: bestScore = {bestScore}, score = {score} {this._m209.Encrypt(ciphertext)}", NotificationLevel.Info);
                                            bestScore = score;
                                            BestPins = this._m209.PinSetting;
                                            BestLugs = this._m209.LugSettings;
                                            improved = true;
                                        }
                                        else
                                        {
                                            LogMessage($"No improvement ({localCounter}): bestScore = {bestScore}, score = {score} {this._m209.Encrypt(ciphertext)}", NotificationLevel.Info);
                                            localCounter--;
                                        }

                                        if (!Running) return 0.0;
                                    }
                                }
                            }
                        }
                    }
                }

            } while (!improved);
            return bestScore;
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

            // Transformation 1
            // toggle state of single pin on one wheel
            for (int w = 0; w < pinSettings.Wheels.Length; w++)
            {
                for (int p = 0; p < pinSettings.Wheels[w].Length; p++)
                {
                    if (!Running) return 0.0;

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
                    //LogMessage($"[SA-step] -> newScore:{newScore} \n", NotificationLevel.Info);

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

            // Transformation 4
            // toggle all pins on one wheel
            for (int w = 0; w < pinSettings.Wheels.Length; w++)
            {
                pinSettings.Wheels[w].ToggleAllPinValues();

                count = pinSettings.count();
                if (count < MIN_COUNT || count > MAX_COUNT) // TODO: Add longSeq
                {
                    pinSettings.Wheels[w].ToggleAllPinValues();
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
                    if (newScore > bestSAScore)
                    {
                        bestSAScore = newScore;
                    }
                }
                else
                {
                    pinSettings.Wheels[w].ToggleAllPinValues();
                }
            }

            if (changeAccepted)
            {
                return bestSAScore;
            }

            // Transformation 2
            // toggle 2 pins on one wheel having different states
            for (int w = 0; w < pinSettings.Wheels.Length; w++)
            {
                for (int p1 = 0; p1 < pinSettings.Wheels[w].Length; p1++)
                {
                    for (int p2 = p1 + 1; p2 < pinSettings.Wheels[w].Length; p2++)
                    {
                        if (pinSettings.Wheels[w].EvaluatePinAtPosition(p1) == pinSettings.Wheels[w].EvaluatePinAtPosition(p2))
                        {
                            continue;
                        }

                        pinSettings.Wheels[w].ToggleTwoPins(p1, p2);

                        count = pinSettings.count();
                        if (count < MIN_COUNT || count > MAX_COUNT || pinSettings.Wheels[w].longSeq(p1, p2))
                        {
                            pinSettings.Wheels[w].ToggleTwoPins(p1, p2);
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
                            if (newScore > bestSAScore)
                            {
                                bestSAScore = newScore;
                            }
                        }
                        else
                        {
                            pinSettings.Wheels[w].ToggleTwoPins(p1, p2);
                        }
                    }
                }
            }

            if (changeAccepted)
            {
                return bestSAScore;
            }

            // Transformation 3
            // Toggle the state of a pair aof two pins, from different wheels, wich have different states.
            for (int w1 = 0; w1 < pinSettings.Wheels.Length; w1++)
            {
                for (int w2 = w1 + 1; w2 < pinSettings.Wheels.Length; w2++)
                {
                    if (w1 == w2)
                    {
                        continue;
                    }

                    for (int p1 = 0; p1 < pinSettings.Wheels[w1].Length; p1++)
                    {
                        for (int p2 = 0; p2 < pinSettings.Wheels[w2].Length; p2++)
                        {
                            if (pinSettings.Wheels[w1].EvaluatePinAtPosition(p1) == pinSettings.Wheels[w2].EvaluatePinAtPosition(p2))
                            {
                                continue;
                            }

                            pinSettings.Wheels[w1].TogglePinValue(p1);
                            pinSettings.Wheels[w2].TogglePinValue(p2);

                            count = pinSettings.count();
                            if (count < MIN_COUNT || count > MAX_COUNT)
                            {
                                pinSettings.Wheels[w1].TogglePinValue(p1);
                                pinSettings.Wheels[w2].TogglePinValue(p2);
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
                                if (newScore > bestSAScore)
                                {
                                    bestSAScore = newScore;
                                }
                            }
                            else
                            {
                                pinSettings.Wheels[w1].TogglePinValue(p1);
                                pinSettings.Wheels[w2].TogglePinValue(p2);
                            }

                        }
                    }
                }
            }


            // inverseWheelBitmap Transformation from java application
            int bestV = -1;
            double bestVscore = 0;
            for (int v = 0; v < 64; v++)
            {
                pinSettings.InverseWheelBitmap(v);

                this._m209.PinSetting = pinSettings;
                this._m209.LugSettings = lugSettings;

                double score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));
                if (score > bestVscore)
                {
                    bestVscore = score;
                    bestV = v;
                }
                pinSettings.InverseWheelBitmap(v);

                newScore = bestVscore;
                double D = newScore - currLocalScore;
                if (D > 0 || this.Randomizer.NextDouble() < Math.Exp(-(Math.Abs(D) / temperature)))
                {
                    currLocalScore = newScore;
                    pinSettings.InverseWheelBitmap(bestV);
                    changeAccepted = true;
                    if (newScore > bestSAScore)
                    {
                        bestSAScore = newScore;
                    }
                }
            }

            return bestSAScore;
        }

        public void Solve(string cipherText, int cycles, string instructionVersion)
        {
            LugSettings bestLocalLugSettings = this._m209.LugSettings;
            PinSettings bestLocalPinSettings = this._m209.PinSetting;

            if (cycles == 0)
            {
                cycles = int.MaxValue;
            }

            for (int cycle = 0; cycle < cycles; cycle++)
            {
                double bestRandom = double.MinValue;
                double bestLocal = 0;

                int phase1Trials = 200000 / cipherText.Length;
                for (int r = 0; r < phase1Trials; r++)
                {
                    this._m209.LugSettings.Randomize();

                    double newEval = SA(cipherText, this._m209.LugSettings, this._m209.PinSetting, instructionVersion, cycles);
                    if (newEval > bestRandom)
                    {
                        bestRandom = newEval;
                        bestLocalLugSettings = this._m209.LugSettings;
                        bestLocalPinSettings = this._m209.PinSetting;
                    }
                    if (bestRandom > bestLocal)
                    {
                        bestLocal = bestRandom;
                    }
                }
                this._m209.LugSettings = bestLocalLugSettings;
                this._m209.PinSetting = bestLocalPinSettings;

                bestLocal = bestRandom;

                int round = 0;
                bool improved = false;

                do
                {

                } while (improved);
            }

        }
    }
}
