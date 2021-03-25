using System;

namespace CrypTool.RPNExpression
{
    public static class RPNFunctionUtils
    {
        public const string cFuncGroupLogical = "Logical";
        public const string cFuncGroupDateTime = "Date & Time";
        public const string cFuncGroupConvert = "Convert";
        public const string cFuncGroupString = "String";
        public const string cFuncGroupMath = "Math";

        public static void RegisterFunctions(ExprEnvironment pEnvironment)
        {
            pEnvironment.RegisterFunction(typeof(IfFunction));
            pEnvironment.RegisterFunction(typeof(NowFunction));
            pEnvironment.RegisterFunction(typeof(DateFunction));
            pEnvironment.RegisterFunction(typeof(TimeFunction));
            pEnvironment.RegisterFunction(typeof(DayFunction));
            pEnvironment.RegisterFunction(typeof(MonthFunction));
            pEnvironment.RegisterFunction(typeof(YearFunction));
            pEnvironment.RegisterFunction(typeof(StrToNumFunction));
            pEnvironment.RegisterFunction(typeof(StrToDateFunction));
            pEnvironment.RegisterFunction(typeof(StrToTimeFunction));
            pEnvironment.RegisterFunction(typeof(NumToStrFunction));
            pEnvironment.RegisterFunction(typeof(DateToStrFunction));
            pEnvironment.RegisterFunction(typeof(TimeToStrFunction));
            pEnvironment.RegisterFunction(typeof(UpperFunction));
            pEnvironment.RegisterFunction(typeof(LowerFunction));
            pEnvironment.RegisterFunction(typeof(CopyFunction));
            pEnvironment.RegisterFunction(typeof(TrimFunction));
            pEnvironment.RegisterFunction(typeof(LenFunction));
            pEnvironment.RegisterFunction(typeof(ReplaceFunction));
            pEnvironment.RegisterFunction(typeof(AbsFunction));
            pEnvironment.RegisterFunction(typeof(RoundFunction));
            pEnvironment.RegisterFunction(typeof(RandomFunction));
            pEnvironment.RegisterFunction(typeof(SumFunction));
            pEnvironment.RegisterFunction(typeof(MaxFunction));
            pEnvironment.RegisterFunction(typeof(MinFunction));
            pEnvironment.RegisterFunction(typeof(AverageFunction));
            pEnvironment.RegisterFunction(typeof(CountFunction));

            pEnvironment.RegisterFunction(typeof(ACosFunction));
            pEnvironment.RegisterFunction(typeof(ASinFunction));
            pEnvironment.RegisterFunction(typeof(ATanFunction));
            pEnvironment.RegisterFunction(typeof(CosFunction));
            pEnvironment.RegisterFunction(typeof(CoshFunction));
            pEnvironment.RegisterFunction(typeof(ExpFunction));
            pEnvironment.RegisterFunction(typeof(LogFunction));
            pEnvironment.RegisterFunction(typeof(Log10Function));
            pEnvironment.RegisterFunction(typeof(SinFunction));
            pEnvironment.RegisterFunction(typeof(SinhFunction));
            pEnvironment.RegisterFunction(typeof(SqrtFunction));
            pEnvironment.RegisterFunction(typeof(TanFunction));
            pEnvironment.RegisterFunction(typeof(TanhFunction));
            pEnvironment.RegisterFunction(typeof(PIFunction));
        }
    }

    [Function("IF", ParamNames="logicaltest; value if true; value if false", ParamTypes="BAA", Group=RPNFunctionUtils.cFuncGroupLogical)]
    public class IfFunction : RPNFunction
    {
        public override object Calc()
        {
            if ((bool)Params[0])
                return Params[1];
            else
                return Params[2]; 
        }
    }

    [Function("NOW", Group = RPNFunctionUtils.cFuncGroupDateTime)]
    public class NowFunction : RPNFunction
    {
        public override object Calc()
        {
            return DateTime.Now;
        }
    }

    [Function("DATE", Group = RPNFunctionUtils.cFuncGroupDateTime)]
    public class DateFunction : RPNFunction
    {
        public override object Calc()
        {
            return DateTime.Today;
        }
    }

    [Function("TIME", Group = RPNFunctionUtils.cFuncGroupDateTime)]
    public class TimeFunction : RPNFunction
    {
        public override object Calc()
        {
            return DateTime.Now.TimeOfDay;
        }
    }

    [Function("DAY", ParamTypes = "D", Group = RPNFunctionUtils.cFuncGroupDateTime)]
    public class DayFunction : RPNFunction
    {
        public override object Calc()
        {
            return ((DateTime)Params[0]).Day;
        }
    }

    [Function("MONTH", ParamTypes = "D", Group = RPNFunctionUtils.cFuncGroupDateTime)]
    public class MonthFunction : RPNFunction
    {
        public override object Calc()
        {
            return ((DateTime)Params[0]).Month;
        }
    }

    [Function("YEAR", ParamTypes = "D", Group = RPNFunctionUtils.cFuncGroupDateTime)]
    public class YearFunction : RPNFunction
    {
        public override object Calc()
        {
            return ((DateTime)Params[0]).Year;
        }
    }

    [Function("STRTONUM", ParamTypes = "S", Group = RPNFunctionUtils.cFuncGroupConvert)]
    public class StrToNumFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]);
        }
    }

    [Function("STRTODATE", ParamTypes = "S", Group = RPNFunctionUtils.cFuncGroupConvert)]
    public class StrToDateFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToDateTime(Params[0]);
        }
    }

    [Function("STRTOTIME", ParamTypes = "S", Group = RPNFunctionUtils.cFuncGroupConvert)]
    public class StrToTimeFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToDateTime(Params[0]);
        }
    }

    [Function("NUMTOSTR", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupConvert)]
    public class NumToStrFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]);
        }
    }

    [Function("DATETOSTR", ParamTypes = "D", Group = RPNFunctionUtils.cFuncGroupConvert)]
    public class DateToStrFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]);
        }
    }

    [Function("TIMETOSTR", ParamTypes = "D", Group = RPNFunctionUtils.cFuncGroupConvert)]
    public class TimeToStrFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]);
        }
    }

    [Function("UPPER", ParamTypes = "S", Group = RPNFunctionUtils.cFuncGroupString)]
    public class UpperFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]).ToUpper();
        }
    }

    [Function("LOWER", ParamTypes = "S", Group = RPNFunctionUtils.cFuncGroupString)]
    public class LowerFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]).ToLower();
        }
    }

    [Function("COPY", ParamTypes = "SNN", Group = RPNFunctionUtils.cFuncGroupString)]
    public class CopyFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]).Substring(Convert.ToInt32(Params[1]), Convert.ToInt32(Params[2]));
        }
    }

    [Function("TRIM", ParamTypes = "S", Group = RPNFunctionUtils.cFuncGroupString)]
    public class TrimFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]).Trim();
        }
    }

    [Function("LEN", ParamTypes = "S", Group = RPNFunctionUtils.cFuncGroupString)]
    public class LenFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]).Length;
        }
    }

    [Function("REPLACE", ParamTypes = "SSS", Group = RPNFunctionUtils.cFuncGroupString)]
    public class ReplaceFunction : RPNFunction
    {
        public override object Calc()
        {
            return Convert.ToString(Params[0]).Replace(Convert.ToString(Params[1]), Convert.ToString(Params[2]));
        }
    }

    [Function("ABS", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class AbsFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Abs(Convert.ToDouble(Params[0]));
        }
    }

    [Function("ROUND", ParamTypes = "NN", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class RoundFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Round(Convert.ToDouble(Params[0]), Convert.ToInt32(Params[1]));
        }
    }

    [Function("RANDOM", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class RandomFunction : RPNFunction
    {
        public override object Calc()
        {
            Random random = new Random();
            return random.Next(Convert.ToInt32(Params[0]));
        }
    }

    [Function("SUM", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class SumFunction : RPNAggregateFunction
    {
        public override void Aggregate()
        {
            base.Aggregate();
            Value += Convert.ToDouble(Owner.RPNList[Owner.Position].GetValue());
        }
    }

    [Function("MAX", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class MaxFunction : RPNAggregateFunction
    {
        public override void Aggregate()
        {
            bool b = Count == 0;
            base.Aggregate();
            double number = Convert.ToDouble(Owner.RPNList[Owner.Position].GetValue());
            if (b || number > Value)
                Value = number;
        }
    }

    [Function("MIN", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class MinFunction : RPNAggregateFunction
    {
        public override void Aggregate()
        {
            bool b = Count == 0;
            base.Aggregate();
            double number = Convert.ToDouble(Owner.RPNList[Owner.Position].GetValue());
            if (b || number < Value)
                Value = number;
        }
    }

    [Function("AVERAGE", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class AverageFunction : RPNAggregateFunction
    {
        public override object Calc()
        {
            return Value / Count;
        }

        public override void Aggregate()
        {
            base.Aggregate();
            Value += Convert.ToDouble(Owner.RPNList[Owner.Position].GetValue());
        }
    }

    [Function("COUNT", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class CountFunction : RPNAggregateFunction
    {
        public override void Aggregate()
        {
            base.Aggregate();
            Value += 1;
        }
    }

    [Function("ACOS", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class ACosFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Acos(Convert.ToDouble(Params[0]));
        }
    }

    [Function("ASIN", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class ASinFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Asin(Convert.ToDouble(Params[0]));
        }
    }

    [Function("ATAN", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class ATanFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Atan(Convert.ToDouble(Params[0]));
        }
    }

    [Function("COS", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class CosFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Cos(Convert.ToDouble(Params[0]));
        }
    }

    [Function("COSH", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class CoshFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Cosh(Convert.ToDouble(Params[0]));
        }
    }

    [Function("EXP", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class ExpFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Exp(Convert.ToDouble(Params[0]));
        }
    }

    [Function("LOG", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class LogFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Log(Convert.ToDouble(Params[0]));
        }
    }

    [Function("LOG10", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class Log10Function : RPNFunction
    {
        public override object Calc()
        {
            return Math.Log10(Convert.ToDouble(Params[0]));
        }
    }

    [Function("SIN", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class SinFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Sin(Convert.ToDouble(Params[0]));
        }
    }

    [Function("SINH", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class SinhFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Sinh(Convert.ToDouble(Params[0]));
        }
    }

    [Function("SQRT", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class SqrtFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Sqrt(Convert.ToDouble(Params[0]));
        }
    }

    [Function("TAN", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class TanFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Tan(Convert.ToDouble(Params[0]));
        }
    }

    [Function("TANH", ParamTypes = "N", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class TanhFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.Tanh(Convert.ToDouble(Params[0]));
        }
    }

    [Function("PI", Group = RPNFunctionUtils.cFuncGroupMath)]
    public class PIFunction : RPNFunction
    {
        public override object Calc()
        {
            return Math.PI;
        }
    }
}
