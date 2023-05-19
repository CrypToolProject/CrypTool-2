/*
   Copyright 2019 Simon Leischnig, based on the work of Soeren Rinne

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

using CrypTool.LFSR.Implementation;
using CrypTool.PluginBase;
using CrypTool.LFSR.Utils;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using static CrypTool.LFSR.Utils.Datatypes;

namespace CrypTool.LFSR
{

    public class LFSRSettings : AbstractComponentSettingsCompat<LFSRParameters>
    {
        protected LFSRParameters api;

        public LFSRSettings(LFSRParameters parameters)
        {
            api = parameters;
            Ids = new Dictionary<object, string>()
            {
                [api.SeedFlipped] = "SeedFlipped",
                [api.PresentationShift] = "PresentationShift",
                [api.Rounds] = "Rounds",
                [api.MaxRecordedRounds] = "MaxRecordedRounds",
                [api.UseClock] = "UseBoolClock",
                [api.DisablePresentation] = "NoQuickwatch"
            };

            api.PresentationShift.OnChange += (value) => RefreshVisibilities(); RaisePropertyChanged(Ids[api.PresentationShift]);
            api.Rounds.OnChange += (value) => RefreshVisibilities(); RaisePropertyChanged(Ids[api.Rounds]);
            api.MaxRecordedRounds.OnChange += (value) => RefreshVisibilities(); RaisePropertyChanged(Ids[api.MaxRecordedRounds]);
            api.UseClock.OnChange += (value) => RefreshVisibilities(); RaisePropertyChanged(Ids[api.UseClock]);
            api.DisablePresentation.OnChange += (value) => RefreshVisibilities(); RaisePropertyChanged(Ids[api.DisablePresentation]);

            //api.PresentationShift.OnChange += shift => HandlePresentationShift(shift);

            OnInitialization += RefreshVisibilities;
        }

        protected void RefreshVisibilities()
        {
        }

        #region UI binding: api.PresentationShift
        [TaskPane(
            "PresentationShiftCaption", "PresentationShiftTooltip",
            null,
            5,
            true,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger, 0, int.MaxValue
            )]
        public int PresentationShift { get => api.PresentationShift; set => api.PresentationShift.Set(value); }
        #endregion

        #region UI binding: api.Rounds <-> Rounds (Attributed property)
        [TaskPane(
            "RoundsCaption", "RoundsTooltip",
            null,
            3,
            false,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger, 1, int.MaxValue
            )]
        public int Rounds { get => api.Rounds; set => api.Rounds.Set(value); }
        #endregion

        #region UI binding: api.SeedFlipped <-> NoQuickwatch (attributed property)
        [TaskPane(
            "SeedFlippedCaption", "SeedFlippedTooltip",
            null,
            2,
            false,
            ControlType.CheckBox,
            "", null
            )]
        public bool SeedFlipped { get => api.SeedFlipped; set => api.SeedFlipped.Set(value); }
        #endregion

        #region UI binding: api.DisablePresentation <-> NoQuickwatch (attributed property)
        [TaskPane(
            "NoQuickwatchCaption", "NoQuickwatchTooltip",
            null,
            6,
            true,
            ControlType.CheckBox,
            "", null
            )]
        public bool NoQuickwatch { get => api.DisablePresentation; set => api.DisablePresentation.Set(value); }
        #endregion

        #region UI binding: api.UseClock <-> UseBoolClock (attributed property)
        [TaskPane(
            "UseBoolClockTPCaption", "UseBoolClockTPTooltip",
            "ClockGroup",
            0,
            false,
            ControlType.CheckBox,
            "", null
            )]
        public bool UseBoolClock { get => api.UseClock; set => api.UseClock.Set(value); }
        #endregion

        #region UI binding: api.MaxRecordedRounds <-> MaxRecordedRounds (attributed property)
        [TaskPane(
            "MaxRecordedRoundsCaption", "MaxRecordedRoundsTooltip",
            null,
            4,
            false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue
            )]
        public int MaxRecordedRounds { get => api.MaxRecordedRounds; set => api.MaxRecordedRounds.Set(value); }
        #endregion

    }

    [Author("Simon Leischnig", "leischnig@CrypTool.org", "At Home", "https://github.io/hi")]
    [PluginInfo("CrypTool.LFSR.Properties.Resources", "PluginCaption", "PluginTooltip", "LFSR/DetailedDescription/doc.xml", "LFSR/Images/LFSR.png", "LFSR/Images/encrypt.png", "LFSR/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.ToolsRandomNumbers)]
    public class LFSRComponent : AbstractStandaloneComponentCompat<LFSRAPI, LFSRParameters>
    {
        private readonly Dictionary<object, string> outputIds;

        public readonly LFSRPresentationHandler lfsrPresentationHandler;
        public override UserControl Presentation => lfsrPresentationHandler.Presentation;
        protected override ISettings CreateSettingsDescriptor(LFSRParameters parameters)
        {
            return new LFSRSettings(parameters);
        }

        // TODO: implement progress change in the API
        // TODO: implement max round restriction in the API
        // TODO: implement status change in the API
        // TODO: make presentation

        public LFSRComponent() : base(new LFSRAPI(new LFSRParameters()))
        {

            lfsrPresentationHandler = new LFSRPresentationHandler(api);
            outputIds = new Dictionary<object, string>()
            {
                [api.OutputAsString] = "OutputString",
                [api.OutputAsBit] = "OutputBool",
                [api.OutputAsBits] = "OutputBoolArray",
                [api.OutputAsStatesString] = "OutputStatesString"
            };

            api.OutputAsString.OnChange += (val) => RaisePropertyChanged(outputIds[api.OutputAsString]);
            api.OutputAsBit.OnChange += (val) => RaisePropertyChanged(outputIds[api.OutputAsBit]);
            api.OutputAsBits.OnChange += (val) => RaisePropertyChanged(outputIds[api.OutputAsBits]);
            api.OutputAsStatesString.OnChange += (val) => RaisePropertyChanged(outputIds[api.OutputAsStatesString]);
        }

        #region I/O Properties
        [PropertyInfo(Direction.InputData, "InputTapSequenceCaption", "InputTapSequenceTooltip", false)]
        public string InputTapSequence { set => SetTap(value); }

        private void SetTap(string value)
        {
            api.InputPoly.Value = value;
        }

        [PropertyInfo(Direction.InputData, "InputSeedCaption", "InputSeedTooltip", false)]
        public string InputSeed { set => api.InputSeed.Value = value; }

        [PropertyInfo(Direction.InputData, "InputClockBoolCaption", "InputClockBoolTooltip", false)]
        public bool InputClockBool { set => api.InputClock.Value = value; }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString => api.OutputAsString.Value;

        [PropertyInfo(Direction.OutputData, "OutputBoolCaption", "OutputBoolTooltip", false)]
        public bool OutputBool => api.OutputAsBit.Value;

        [PropertyInfo(Direction.OutputData, "OutputBoolArrayCaption", "OutputBoolArrayTooltip", false)]
        public bool[] OutputBoolArray => api.OutputAsBits.Value;

        [PropertyInfo(Direction.OutputData, "OutputStatesStringCaption", "OutputStatesStringTooltip", false)]
        public string OutputStatesString => api.OutputAsStatesString.Value;
        #endregion



        #region Image

        private enum LFSRImage
        {
            Default,
            Encode,
        }
        #endregion
    }

    public class LFSRPresentationHandler
    {
        private readonly LFSRPresentation lfsrPresentation;
        private readonly LFSRAPI api;
        private readonly bool isResettingShift = false;

        public UserControl Presentation => lfsrPresentation;

        public LFSRPresentationHandler(LFSRAPI api)
        {
            this.api = api;
            lfsrPresentation = new LFSRPresentation();
            api.Parameters.DisablePresentation.OnChange += val => updatePresentation();
            api.Parameters.MaxRecordedRounds.OnChange += val => updatePresentation();
            api.Parameters.PresentationShift.OnChange += val => { if (!isResettingShift) { updatePresentation(); } };
            api.OnRoundFinished += () => updatePresentation();

            api.OnDispose += () => deletePresentation();
            api.OnPreExecution += () => deletePresentation();
        }

        public void deletePresentation()
        {
            lfsrPresentation.DeleteAll(100);
        }

        public void updatePresentation()
        {
            int shift = api.Parameters.PresentationShift.Value;
            LFSRRound currentRound = api.currentRound;
            if (currentRound == null || currentRound.ParentRound.IsNone)
            {
                deletePresentation();
                return;
            }
            LFSRRound roundThatHasFinished = api.currentRound.ParentRound.Get();
            LFSRRound presentationRound = roundThatHasFinished;
            while (presentationRound.RoundNumber > roundThatHasFinished.RoundNumber - shift)
            {
                if (presentationRound.ParentRound.IsSome)
                {
                    presentationRound = presentationRound.ParentRound.Get();
                }
                else
                {
                    if (presentationRound.RoundNumber > 1)
                    {
                        api.Log(string.Format("Rounds earlier round number {0} are not kept in memory.", presentationRound.RoundNumber), LogLevels.Info);
                    }
                    //                     try
                    //                     {
                    //                         this.isResettingShift = true; // lock to prevent infinite recursion
                    //                         api.Parameters.PresentationShift.Value = Math.Max(0, roundThatHasFinished.RoundNumber - presentationRound.RoundNumber);
                    //                     }
                    //                     finally { this.isResettingShift = false; }
                    break;
                }
            }
            List<bool> registerContent = presentationRound.RegInitial.Bits;
            List<bool> tapSequence = presentationRound.Polynom.taps;
            Option<bool> output = Some(presentationRound.GetShiftOutBit());
            bool newBit = presentationRound.RegAfter.Bits[0];
            showPresentation(registerContent, tapSequence, output, newBit, presentationRound.RoundNumber);
        }

        public void showPresentation(List<bool> registerContent, List<bool> tapSequence, Option<bool> output, bool newBit, int roundNumber)
        {
            if (api.Parameters.DisablePresentation.Value)
            {
                deletePresentation();
                return;
            }
            lfsrPresentation.DeleteAll(100);
            char[] seedCharArray = new char[registerContent.Count];
            char[] tapSequenceCharArray = new char[tapSequence.Count];
            char[] tapSequenceCharArrayRev = new char[tapSequence.Count];
            int clocking = -1;
            char outputChar = output.Match(some => some ? '1' : '0', () => ' ');

            for (int i = 0; i < registerContent.Count; i++)
            {
                seedCharArray[i] = registerContent[i] ? '1' : '0';
            }
            for (int i = 0; i < registerContent.Count; i++)
            {
                tapSequenceCharArrayRev[tapSequenceCharArrayRev.Length - 1 - i] = tapSequence[i] ? '1' : '0';
            }
            for (int i = 0; i < registerContent.Count; i++)
            {
                tapSequenceCharArray[i] = tapSequence[i] ? '1' : '0';
            }

            lfsrPresentation.DrawLFSR(seedCharArray, tapSequenceCharArray, clocking);
            lfsrPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, outputChar, newBit ? '1' : '0', BuildPolynomialFromBinary(tapSequenceCharArrayRev, roundNumber));

        }

        private static string BuildPolynomialFromBinary(char[] tapSequence, int roundNumber)
        {
            string polyDescription = CrypTool.LFSR.Properties.Resources.RoundNumber + " " + roundNumber + "  ";
            polyDescription += CrypTool.LFSR.Properties.Resources.Feedback_polynomial + ": ";

            StringBuilder polynomialBuilder = new StringBuilder();
            polynomialBuilder.Append(polyDescription);

            // This part until the end of the function is the work of Soeren Rinne
            int power;

            //build polynomial
            for (int i = 0; i < tapSequence.Length; i++)
            {
                power = (i - tapSequence.Length + 1) * -1 % tapSequence.Length + 1;
                if (tapSequence[i] == '1')
                {
                    if (power == 1)
                    {
                        polynomialBuilder.Append("x + ");
                    }
                    else if (power != 0)
                    {
                        polynomialBuilder.Append("x^" + power + " + ");
                    }
                }
            }
            polynomialBuilder.Append("1");
            return polynomialBuilder.ToString();
        }


    }



}

