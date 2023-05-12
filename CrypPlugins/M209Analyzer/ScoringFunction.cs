using CrypTool.PluginBase.Utils;
using M209AnalyzerLib.Enums;
using M209AnalyzerLib.M209;
using System;

namespace Cryptool.Plugins.M209Analyzer
{
    public class ScoringFunction : IScoring
    {
        //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
        private Grams grams;

        public ScoringFunction(int language, int gramsType, float normalizeValue)
        {
            //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
            grams = LanguageStatistics.CreateGrams(language, (LanguageStatistics.GramsType)(gramsType + 1), false);
            grams.Normalize(10_000_000);

        }
        double IScoring.Evaluate(EvalType evalType, int[] decryptedText, int[] crib)
        {
            double evalValue = 0.0;
            switch (evalType)
            {
                case EvalType.CRIB:
                    if (crib == null)
                    {
                        throw new Exception("Crib not given");
                    }
                    throw new System.NotImplementedException();
                    //evalValue = EvalCrib(decryptedText, crib);
                    break;
                case EvalType.MONO:
                    evalValue = grams.CalculateCost(decryptedText);
                    break;
                case EvalType.PINS_SA_CRIB:
                    break;
                default:
                    break;
            }
            return evalValue;
        }
    }
}
