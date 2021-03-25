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
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;


using CrypTool.PluginBase.Utils.ObjectDeconstruct;
using CrypTool.PluginBase.Utils.Datatypes;
using static CrypTool.PluginBase.Utils.Datatypes.Datatypes;
using static CrypTool.LFSR.LFSRErrors;

namespace CrypTool.LFSR
{
    using CrypTool.LFSR.Implementation;
    using CrypTool.PluginBase.Utils;
    using CrypTool.PluginBase.Utils.Logging;
    using CrypTool.PluginBase.Utils.StandaloneComponent;
    using CrypTool.PluginBase.Utils.StandaloneComponent.Common;

    public class LFSRAPI : AbstractComponentAPI<LFSRParameters>
    {
        public event Action OnRoundStarting = () => { };
        public event Action OnRoundFinished = () => { };

        public LFSRRound currentRound;
        private bool isAlreadyBusy;

        // Upon construction, pass the parameters instance
        public LFSRAPI(LFSRParameters parameters) : base(parameters) {

            this.OnExecute += this.Execute;
            
            // Changing an input resets the component so it starts from scratch
            this.InputPoly.OnChange += (val) => this.currentRound = null;
            this.InputSeed.OnChange += (val) => this.currentRound = null;

            this.OnPreExecution += () => this.currentRound = null;
            this.isAlreadyBusy = false;

            this.InputClock.OnChange += OnInputClock;

            // push internal clock edge changes: (before, after)
            this.internalClk.OnChange += clk => OnInternalClock(
                this.internalClk.History[internalClk.History.Count - 2], 
                this.internalClk.History[internalClk.History.Count - 1]);

        }

        private void OnInputClock(bool inputClk)
        {
            if (Parameters.UseClock && inputClk != internalClk.Value)
            {
                this.internalClk.Value = inputClk;
            }
        }

        private void OnInternalClock(bool oldVal, bool newVal)
        {
            // after the predefined amount of rounds, the component is inactive
            if (this.currentRound != null && this.currentRound.RoundNumber > Parameters.Rounds) return;

            // Is the component still busy? Then, we can do nothing and it is an error from the clock input.
            if (this.isAlreadyBusy) throw new LFSRInputInvalidException(InputInvalidReason.ClockTooFast);

            if (!oldVal && newVal) // on rising edge of CLK
            {
                // Be patient (no error) when it's just about establishing inputs at the start. Clock cycle gets lost, tho.
                // TODO: may not be necessary; test!
                if (this.currentRound == null && (InputPoly.Value == null || InputSeed.Value == null)) return;

                Fun.InitialValidation(InputPoly.Value, InputSeed.Value); // throws Exceptions or logs warnings

                PerformRound();
            }
        }

        private void PerformRound()
        {
            OnRoundStarting();
            var poly = Fun.ToPoly(InputPoly.Value);
            var taps = Fun.ToSeed(InputSeed.Value, Parameters.SeedFlipped);
            Fun.ValidateNonrecurrentParams((poly, taps));

            this.isAlreadyBusy = true;
            try
            {
                this.currentRound = Fun.LFSRStep((poly, taps), this.currentRound, Parameters.MaxRecordedRounds);
                LFSRRound finishedRound = this.currentRound.ParentRound.Get();

                OutputAsBit.Value = Fun.OutputAsBool(finishedRound);
                OutputAsBits.Value = Fun.OutputAsBits(finishedRound);
                OutputAsString.Value = Fun.OutputAsString(finishedRound);
                OutputAsStatesString.Value = Fun.OutputAsStateSummary(finishedRound);

                ChangeProgress((double)finishedRound.RoundNumber/Parameters.Rounds);
            }
            finally
            {
                this.isAlreadyBusy = false;
            }
            OnRoundFinished();

            // if the component does not use an external CLK, self-propell by forcing a rising edge on the internal CLK
            if (! this.Parameters.UseClock)
            {
                this.internalClk.Value = false;
                this.internalClk.Value = true;
            }
        }

        private void Execute()
        {
            try
            {
                if (!this.Parameters.UseClock)
                {
                    // self-propell with a rising clock edge if no external clock is present
                    this.internalClk.Value = false;
                    this.internalClk.Value = true;
                }
            }
            catch (LFSRException e)
            {
                if (e.Kind == ErrorKinds.Unexpected)
                {
                    Log(CrypTool.LFSR.ConvertTo.String(e), LogLevels.Error);
                }
                else
                {
                    Log(ConvertTo.InputErrorToString(e), LogLevels.Warning);
                }
                this.ChangeStateToWontcomplete(e);
            }
        }

        private HistoryBox<bool> internalClk = new HistoryBox<bool>(false);

        public Box<String> InputPoly = new Box<string>(default);
        public Box<String> InputSeed = new Box<string>(default);
        public Box<bool> InputClock = new Box<bool>(default);
        public Box<string> OutputAsString = new Box<string>(default);
        public Box<bool> OutputAsBit = new Box<bool>(default);
        public Box<bool[]> OutputAsBits = new Box<bool[]>(default);
        public Box<string> OutputAsStatesString = new Box<string>(default);
    }

    public class LFSRParameters : AbstractParameters
    {
        public Parameter<int> PresentationShift = new Parameter<int>(0);
        public Parameter<int> Rounds = new Parameter<int>(10);
        public Parameter<int> MaxRecordedRounds = new Parameter<int>(100);
        public Parameter<bool> UseClock = new Parameter<bool>(false);
        public Parameter<bool> DisablePresentation = new Parameter<bool>(false);
        public Parameter<bool> SeedFlipped = new Parameter<bool>(true);
    }

    namespace Implementation
    {
        #region classes that map the roundwise transitions and history of an LFSR

        public class ShiftReg
        {
            public List<Boolean> Bits { get; private set; }

            public ShiftReg(List<bool> bits)
            {
                this.Bits = new List<bool>(bits);
            }

            public Boolean Shift(bool shiftIn)
            {
                Boolean outVal = Bits[Bits.Count - 1];
                Bits = Bits.GetRange(0, Bits.Count - 1);
                Bits.Insert(0, shiftIn);
                return outVal;
            }
            public Boolean GetShiftOutBit()
            {
                return this.Bits[this.Bits.Count - 1];
            }
            public ShiftReg Clone()
            {
                return new ShiftReg(new List<Boolean>(this.Bits));
            }
        }
        public class LFSRRound
        {
            public ShiftReg RegAfter = null;
            public ShiftReg RegInitial { get; private set; }
            public Polynom Polynom { get; }
            public Option<LFSRRound> ParentRound { get; private set; }
            // returns the rounds in order of occurrence
            public List<LFSRRound> History { get => flatRoundList(); }
            public int RoundNumber { get; }

            internal List<LFSRRound> flatRoundList()
            {
                if (this.ParentRound.IsNone)
                {
                    return Sequence<LFSRRound>(this);
                }
                else
                {
                    List<LFSRRound> parentList = this.ParentRound.Get().flatRoundList();
                    //TODO: this is not immutable. but is that bad here?
                    parentList.Add(this);
                    return parentList;
                }
            }
            public List<List<Boolean>> getRawHistory() => History.ConvertAll(round => round.RegInitial.Bits);
            public List<Boolean> getResultingSequence() => History.ConvertAll(round => round.RegInitial.GetShiftOutBit());
            public Boolean GetShiftOutBit() => RegInitial.GetShiftOutBit();

            public LFSRRound(LFSRRound parent, ShiftReg initial, int maxRoundsInMem, Polynom polynom)
            {
                this.ParentRound = Option<LFSRRound>.Some(parent);
                this.RoundNumber = parent.RoundNumber + 1;
                // for not keeping too many rounds in memory...
                parent.DetachAt(maxRoundsInMem - 1);
                this.RegInitial = initial;
                Polynom = polynom;
            }
            public LFSRRound(ShiftReg seed, Polynom polynom)
            {
                this.ParentRound = Option<LFSRRound>.None();
                this.RoundNumber = 1;
                this.RegInitial = seed;
                Polynom = polynom;
            }
            public void DetachAt(int depth)
            {
                if (depth == 0) this.ParentRound = None<LFSRRound>();
                if (ParentRound.IsSome)
                {
                    ParentRound.Get().DetachAt(depth - 1);
                }
            }
            public LFSRRound MakeNextRound(Polynom polynom, int maxRoundsInMem)
            {
                var resultRegister = this.RegInitial.Clone();
                var feedback = polynom.calculate(this.RegInitial.Bits);
                resultRegister.Shift(feedback);

                this.RegAfter = resultRegister.Clone();
                return new LFSRRound(this, resultRegister, maxRoundsInMem, polynom);
            }
        }
        public class Polynom
        {
            public List<Boolean> taps;
            public static Boolean ADD_XOR(List<Boolean> x)
            {
                var xor = false;
                for (int i = 0; i < x.Count; i++)
                {
                    xor = xor ^ x[i];
                }
                return xor;
            }
            public Polynom(List<bool> taps)
            {
                this.taps = new List<bool>(taps);
            }

            public Boolean calculate(List<Boolean> vs)
            {
                var toXOR = new List<Boolean>();
                for (int i = 0; i < taps.Count; i++)
                {
                    if (taps[i])
                    {
                        toXOR.Add(vs[i]);
                    }
                }
//                 toXOR.Add(vs[vs.Count - 1]);
                return ADD_XOR(toXOR);
            }
        }

        #endregion

        // pure functionality, what is done with the data (functions may be certain that their inputs are valid)
        public static class Fun
        {
            
            // Main function, the recurrent shift register operation
            // mind, that the recurrentRound is a nullable optional parameter by specification
            public static LFSRRound LFSRStep((Polynom poly, ShiftReg seed) input, LFSRRound recurrentRound, int maxInMem)
            {
                LFSRRound currentRound = recurrentRound;
                if (recurrentRound == null)
                {
                    currentRound = new LFSRRound(input.seed, input.poly);
                }
                return currentRound.MakeNextRound(input.poly, maxInMem);
            }

            #region Validation of function parameters 

            // perform null checks and other trivial validation that are necessary for the other functions to work correctly.
            // in turn, they may assume these conditions to always be met and therefore be more concise.
            public static void InitialValidation(string Poly, String Seed)
            {
                if (Poly == null) throw new LFSRInputInvalidException(InputInvalidReason.PolyIsUnset);
                if (Seed == null) throw new LFSRInputInvalidException(InputInvalidReason.SeedIsUnset);
                if (Fun.RemoveWhitespace(Poly).Length < 1) throw new LFSRInputInvalidException(InputInvalidReason.PolyIsEmpty);
                if (Fun.RemoveWhitespace(Seed).Length < 1) throw new LFSRInputInvalidException(InputInvalidReason.SeedIsEmpty);
            }

            // Validate the inputs of the linear feedback shift register that are not its own output
            public static void ValidateNonrecurrentParams((Polynom poly, ShiftReg reg) data)
            {
                if (data.poly.taps.Count != data.reg.Bits.Count)
                {
                    throw new LFSRInputInvalidException(InputInvalidReason.BitstringsNotSameLength);
                }
            }

            #endregion

            #region Parse string input to objects

            public static Polynom ToPoly(String rawPolynomString)
            {
                var polynom = Fun.RemoveWhitespace(rawPolynomString);
                List<(int, bool)> positions = new List<(int, bool)>();
                Regex mathPos = new Regex(@"^(x\^(\d+)|x|1)\+?");
                Regex boolPos = new Regex(@"^[01]");

                String remaining = polynom;
                bool hasMathMatch = false;
                int lastBoolMatchPos = 0;
                (int,bool) parsedPos = default;

                while(remaining.Length > 0)
                {
                    int hasParsed = -1;

                    //TODO: leading 1+ doesnt work yet
                    var matchMath = mathPos.Match(remaining);
                    if (matchMath.Success)
                    {
                        if (hasMathMatch && matchMath.Groups[1].Value.Equals("1"))
                        {
                            if (lastBoolMatchPos > 0) throw new LFSRInputInvalidException(InputInvalidReason.MixedPolyFormat);

                            parsedPos = (0, true);
                            hasParsed = matchMath.Value.Length;
                            hasMathMatch = true;
                        }
                        else if(matchMath.Groups[1].Value.Equals("x"))
                        {
                            if (lastBoolMatchPos > 0) throw new LFSRInputInvalidException(InputInvalidReason.MixedPolyFormat);
                            parsedPos = (1, true);
                            hasParsed = matchMath.Value.Length;
                            hasMathMatch = true;
                        }
                        else if(matchMath.Groups[0].Value.StartsWith("x"))
                        {
                            if (lastBoolMatchPos > 0) throw new LFSRInputInvalidException(InputInvalidReason.MixedPolyFormat);
                            parsedPos = (Convert.ToInt32(matchMath.Groups[2].Value), true);
                            hasParsed = matchMath.Value.Length;
                            hasMathMatch = true;
                        }
                    } 

                    var matchBool = boolPos.Match(remaining);
                    if ((hasParsed<0) && matchBool.Success)
                    {
                        if (hasMathMatch) throw new LFSRInputInvalidException(InputInvalidReason.MixedPolyFormat);
                        lastBoolMatchPos++;
                        parsedPos = (lastBoolMatchPos, matchBool.Groups[0].Value == "0" ? false : true);
                        hasParsed = matchBool.Value.Length;
                    }

                    if (hasParsed > -1)
                    {
                        positions.Add(parsedPos);
                        remaining = remaining.Substring(hasParsed);
                        continue;
                    }
                    throw new LFSRErrors.LFSRInputInvalidException(InputInvalidReason.IsMalformedPolynom, remaining);
                }

                HashSet<Int32> setPositions = new HashSet<int>();
                foreach (var pos in positions) // check for doubly-defined positions
                {
                    if (setPositions.Contains(pos.Item1)) throw new LFSRErrors.LFSRInputInvalidException(InputInvalidReason.PolyDoublePos, pos);
                    setPositions.Add(pos.Item1);
                }

                positions.Sort((pos1, pos2) => pos1.Item1 > pos2.Item1 ? 1 : -1);
                if (positions[0].Item1 == 0)
                {
                    positions.RemoveAt(0);
                }
                if (! positions[positions.Count-1].Item2)
                {
                    throw new LFSRErrors.LFSRInputInvalidException(InputInvalidReason.RightmostBitNotOne);
                }

                var result = new List<Boolean>(); 

                for (int i = 0; i < positions[positions.Count-1].Item1; i++) 
                    result.Add(false);
                foreach ((int ord, bool val) pos in positions) 
                    result[pos.ord-1] = pos.val;

                return new Polynom(result);
            }

            public static ShiftReg ToSeed(String seed, bool flipped)
            {
                var noWhitespace = RemoveWhitespace(seed);
                var result = new List<Boolean>();
                foreach (var c in noWhitespace)
                {
                    if (c == '0')
                    {
                        result.Add(false);
                    }
                    else if (c == '1')
                    {
                        result.Add(true);
                    }
                    else
                    {
                        throw new LFSRInputInvalidException(InputInvalidReason.IsNoBitstring);
                    }
                }
                // if the seed is read in flipped (in reverse direction of the screen)
                if (flipped)
                {
                    result.Reverse();
                }

                return new ShiftReg(result);
            }

            #endregion

            #region Output calculation (from LFSRRound to three different outputs)

            public static bool OutputAsBool(LFSRRound arg) => arg.GetShiftOutBit();
            public static string OutputAsString(LFSRRound arg) => BitsToString(arg.getResultingSequence());
            public static bool[] OutputAsBits(LFSRRound arg) => arg.getResultingSequence().ToArray();
            public static string OutputAsStateSummary(LFSRRound arg) => String.Join("\r\n", arg.getRawHistory().ConvertAll(BitsToString));

            #endregion

            #region Helper Methods

            public static String RemoveWhitespace(String s) => s.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
            public static String BitsToString(List<bool> bits) => String.Join("", bits.ConvertAll((bool b) => b ? "1" : "0"));
            private static char[] ReverseOrder(char[] tapSequence)
            {
                char[] tempCharArray = new char[tapSequence.Length];
                int temp;
                for (int j = tapSequence.Length - 1; j >= 0; j--)
                {
                    temp = (j - tapSequence.Length + 1) % (tapSequence.Length);
                    if (temp < 0) temp *= -1;
                    //GuiLogMessage("temp = " + temp, NotificationLevel.Info, true);
                    tempCharArray[j] = tapSequence[temp];
                }
                return tempCharArray;
            }

            #endregion

        }
    }

    public static class LFSRErrors
    {
        
        public enum ErrorKinds
        {
            Unexpected, 
            InputInvalid
        }
        public enum InputInvalidReason
        {
            IsMalformedPolynom, 
            IsNoBitstring, 
            RightmostBitNotOne, 
            BitstringsNotSameLength,
            ClockTooFast,
            PolyIsEmpty,
            SeedIsEmpty,
            PolyDoublePos,
            PolyIsUnset,
            SeedIsUnset,
            MixedPolyFormat
        }

        public class LFSRInputInvalidException : LFSRException
        {
            public InputInvalidReason Reason;
            public LFSRInputInvalidException(InputInvalidReason reason, params object[] context) : base(ErrorKinds.InputInvalid, context)
            {
                this.Reason = reason;
            }
        }
        public class LFSRException : Exception
        {
            public ErrorKinds Kind { get; private set; }

            public object[] context = new object[0];

            public LFSRException(ErrorKinds kind, params object[] context) : this(kind, null, context) { }
            public LFSRException(ErrorKinds kind, Exception innerException, params object[] context) : base(kind.ToString(), innerException) {
                this.Kind = kind;
                this.context = context;
            }
            public bool HasContext { get => context.Length > 0; }
        }
    }

    // Utilities to keep everything that is or generates a string together
    public class ConvertTo
    {
        private static AggregateStrConverterInc _ToString = null;
        public static void Log(object o, LogLevel level = null, Logger l = null)
        {
            Logger logger = (l ?? GlobalLog.CErr);
            LogLevel lvl = level ?? GlobalLog.defaultLevel;
            logger.Log(ConvertTo.String(o), lvl);
        }
        public static String String(object o, List<object> visitedList = null)
        {
            if (o is String s) return s;
            if (_ToString != null) return _ToString.Convert(o, visitedList == null ? new List<object>() : visitedList);

            var builder = new AggregateStrConverterInc();
            builder.WithType<String>(str => str);
            builder.WithRFormat<ComponentProgress>("{}, {}%", p => p.Kind, p => p.Ratio * 100);

            builder.WithType<LFSRRound>();
            builder.WithType<ShiftReg>();
            builder.WithType<Polynom>();
            builder.WithType<LFSRParameters>();
            builder.WithType<LFSRAPI>();
            builder.WithType<LFSRException>();

            builder.With(new ArrayToStringDeconstruction(builder.fallback.Convert, builder.Convert));
            builder.With(new SeqToStringDeconstruction(builder.fallback.Convert, builder.Convert));
            builder.WithType<Boolean>(b => b ? "1" : "0");

            _ToString = builder; return String(o, visitedList);
        }

        public static String InputErrorToString(LFSRErrors.LFSRException ex)
        {
            string inputName = "<not specified>";
            string poly = "Tap Sequence";
            string seed = "Seed";
            if (ex is LFSRInputInvalidException e)
            {
                switch (e.Reason)
                {
                    case InputInvalidReason.PolyIsUnset:
                        inputName = poly;
                        return string.Format("{0} Input is not assigned.", inputName);
                    case InputInvalidReason.SeedIsUnset:
                        inputName = seed;
                        return string.Format("{0} Input is not assigned.", inputName);
                    case InputInvalidReason.IsMalformedPolynom:
                        inputName = poly;
                        return string.Format("{0} Input is not well-formed.", inputName);
                    case InputInvalidReason.IsNoBitstring:
                        inputName = seed;
                        return string.Format("{0} Input is no bitstring.", inputName);
                    case InputInvalidReason.RightmostBitNotOne:
                        inputName = poly;
                        return string.Format("{0} Input is no valid tap sequence. The rightmost tap bit must be one.", inputName);
                    case InputInvalidReason.BitstringsNotSameLength:
                        return string.Format("The two inputs must have the same length.");
                    case InputInvalidReason.ClockTooFast:
                        return string.Format("The clock input changed too fast - the shift register was still calculating feedback.");
                    case InputInvalidReason.PolyIsEmpty:
                        inputName = poly;
                        return string.Format("{0} Input is empty.", inputName);
                    case InputInvalidReason.SeedIsEmpty:
                        inputName = seed;
                        return string.Format("{0} Input is empty.", inputName);
                    case InputInvalidReason.PolyDoublePos:
                        inputName = poly;
                        return string.Format("{0} Input is not well-formed. Reason: Doubly-defined tap position.", inputName);
                    case InputInvalidReason.MixedPolyFormat:
                        return string.Format("Polynom input has as well tap-sequence (0/1) as well as power-of-x notation parts which may not be mixed.");
                    default:
                        return string.Format("Some error with the Inputs of the Component occured.", e.context);
                }
            }
            else
            {
                return ex.Message;
            }
        }
    }
}
