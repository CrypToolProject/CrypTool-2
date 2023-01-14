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
        //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
        private Grams grams;

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

        public void Solve(int[] roundLayers, int layers, string ciphertext, int cycles, string versionOfInstruction)
        {
            LugSettings BestLugs = new LugSettings(new string[27] {
                "36","06","16","15","45","04","04","04","04",
                "20","20","20","20","20","20","20","20","20",
                "20","25","25","05","05","05","05","05","05"
            });
            LugSettings BestLocalLugs = BestLugs;

            PinSettings BestPins = new PinSettings(versionOfInstruction);
            PinSettings BestLocalPins = BestPins;

            for (int i = 0; i < cycles; i++)
            {
                roundLayers[layers] = cycles;
                double bestRandom = double.MinValue;
                double bestLocal = 0.0;

                int[] cipherArray = this.GetCipherArray(ciphertext);
                int phase1Trials = 200000 / cipherArray.Length;

                for (int r = 0; r < phase1Trials; r++)
                {
                    roundLayers[layers + 1] = r;
                    BestLugs.Randomize();

                    double newEval = SA(roundLayers, layers + 2, ciphertext, BestLugs, BestPins, versionOfInstruction, 1);
                    if (newEval > bestRandom)
                    {
                        bestRandom = newEval;
                        BestLocalLugs = BestLugs;
                        BestLocalPins = BestPins;
                    }
                    if (bestRandom > bestLocal)
                    {
                        bestLocal = bestRandom;
                        // ReportResult ...
                    }
                }
                // key.lugs.set(bestLocalTypeCount, false);
                //key.pins.set(bestLocalPins);
                //bestLocal = bestRandom;

                int round = 0;
                bool improved;
                do
                {
                    roundLayers[layers + 1] = round;
                    round++;
                    improved = false;

                } while (improved);

            }
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

        private string HCOuter(string ciphertext, string versionOfInstruction)
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

            bool stuck = false;
            int localCounter = 100;
            do
            {
                stuck = true;

                if (localCounter > 0)
                {
                    this._m209.LugSettings.ApplyTransformationSimple();
                }
                else
                {
                    this._m209.LugSettings.ApplyTransformationComplex();
                }
                this._m209.PinSetting = SAInner(ciphertext, this._m209.LugSettings, "V");

                double score = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

                if (score > bestScore)
                {
                    //GuiLogMessage($"Improved: bestScore = {bestScore}, score = {score}", NotificationLevel.Info);
                    bestScore = score;
                    BestPins = this._m209.PinSetting;
                    BestLugs = this._m209.LugSettings;
                    stuck = false;
                }
                else
                {
                    //GuiLogMessage($"No improvement ({localCounter}): bestScore = {bestScore}, score = {score}", NotificationLevel.Info);
                    localCounter--;
                }

            } while (stuck == true);
            return "BestLugs, BestPins";
        }

        private double SA(int[] roundLayers, int layers, string ciphertext, LugSettings lugSettings, PinSettings pinSetting, string versionOfInstruction, int cycles)
        {
            double bestSAScore = this.grams.CalculateCost(this._m209.Encrypt(ciphertext));

            for (int i = 0; i < cycles; i++)
            {
                roundLayers[layers] = cycles;
                pinSetting.Randomize();

                roundLayers[layers + 1] = 0;
                for (double temperature = 1000.0; temperature >= 1.0; temperature /= 1.1)
                {
                    bestSAScore = step(ciphertext, roundLayers, layers + 2, lugSettings, pinSetting, bestSAScore, temperature);
                }
                double previous;
                do
                {
                    previous = bestSAScore;
                    bestSAScore = step(ciphertext, roundLayers, layers + 2, lugSettings, pinSetting, bestSAScore, 0.0);
                } while (bestSAScore > previous);
            }

            return bestSAScore;
        }

        private double step(string ciphertext, int[] roundLayers, int layers, LugSettings lugSettings, PinSettings pinSettings, double bestSAScore, double temperature)
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
            //GuiLogMessage($"SAInner \n", NotificationLevel.Info);
            double T = 0;
            double alpha = 0.9;

            PinSettings BestPins = new PinSettings(versionOfInstruction);
            BestPins.Randomize();

            PinSettings CurrentPins = BestPins;

            PinSettings[] neighbourPinSettings = BestPins.GetNeighborPins();

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

                    //GuiLogMessage($"SA: currentScore = {bestScore}, score = {score}", NotificationLevel.Info);

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
