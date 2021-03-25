using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

namespace CrypTool.RPNExpression
{
    public enum RPNTokenType
    {
        Unknown,
        Eof,
        StartParentheses,
        EndParentheses,
        Ident,
        String,
        Number,
        DateTime,
        Boolean,
        EndParam,
        SquareBracket,

        //operators
        Mulitiply,
        Divide,
        Div,
        Mod,
        Plus,
        Minus,
        Less,
        Greater,
        LessOrEqual,
        GreateOrEqual,
        NotEqual,
        Equal,
        Or,
        And,
        Not,
        JustPlus,
        JustMinus
    }

    public enum RPNObjectType
    {
        Operand,
        Operator, 
        Function, 
        DataField, 
        Variable
    }

    public class ExprEnvironment
    {
        private SortedList<string, Type> functionList = new SortedList<string, Type>();

        public SortedList<string, Type> FunctionList
        {
            get { return functionList; }
        }

        public void RegisterFunction(Type funcType)
        {
            FunctionAttribute funcAttrib = (FunctionAttribute)Attribute.GetCustomAttribute(funcType, typeof(FunctionAttribute));
            functionList.Add(funcAttrib.FunctionName.ToUpper(), funcType);
        }

        public virtual void CalcDataField(string fieldName, ref object value)
        {
            throw new Exception("Can't find DataField " + fieldName + ".");
        }

        public RPNFunction FindFunction(RPNExpr expr, string func)
        {
            int index = functionList.IndexOfKey(func);

            if (index > -1)
            {
                Assembly assembly = Assembly.GetAssembly(functionList.Values[index]);
                RPNFunction rpnFunc = assembly.CreateInstance(functionList.Values[index].FullName) as RPNFunction;
                rpnFunc.Owner = expr;
                rpnFunc.ObjectType = RPNObjectType.Function;

                return rpnFunc;
            }
            else
            {
                return null;
            }
        }
    }

    public abstract class RPNObject
    {
        public RPNExpr Owner;
        public RPNObjectType ObjectType; 

        public abstract object GetValue();

        public RPNObject()
        {
        }

        public RPNObject(RPNExpr owner, RPNObjectType objectType)
        {
            Owner = owner;
            ObjectType = objectType;
        }

        public virtual bool IsAggregate()
        {
            return false;
        }

        protected bool IsNumeric(object value)
        {
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            string str = Convert.ToString(value, provider);
            Double result;
            return Double.TryParse(str, NumberStyles.Integer | NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture, out result);
        }
    }

    public class RPNOperator: RPNObject
    {
        public RPNTokenType OperatorType;
        public string OperatorStr; 

        public RPNOperator(RPNExpr owner, RPNObjectType objectType) : base(owner, objectType)
        {
        }

        public override string ToString()
        {
            return OperatorStr;
        }

        public override object GetValue()
        {
            Owner.Position--;

            object value1 = Owner.RPNList[Owner.Position].GetValue();

            switch (OperatorType)
            {
                case RPNTokenType.Not:
                    if (!(value1 is bool))
                        throw new Exception("Can't calculate " + RPNTokenType.Not.ToString() + " " + Convert.ToString(value1) + ".");
                    return !Convert.ToBoolean(value1);
                case RPNTokenType.JustPlus:
                    return value1;
                case RPNTokenType.JustMinus:
                    if (!IsNumeric(value1))
                        throw new Exception("Can't calculate " + RPNTokenType.JustMinus.ToString() + " " + Convert.ToString(value1) + ".");
                    return -Convert.ToDouble(value1);
                default:
                    object value2 = null;
                    if (Owner.Position >= 0)
                        value2 = Owner.RPNList[Owner.Position].GetValue();
                    switch (OperatorType)
                    {
                        case RPNTokenType.Plus:
                            /*if (value1 is string)
                            {
                                return Convert.ToString(value2) + Convert.ToString(value1);
                            }
                            else if (IsNumeric(value1))
                            {*/
                                return Convert.ToInt32(value2) ^ Convert.ToInt32(value1);
                            /*}
                            else
                                throw new Exception("Can't calculate " + RPNTokenType.Plus.ToString() + " " + Convert.ToString(value1) + ".");
                             */
                        /*case RPNTokenType.Minus:
                            if (IsNumeric(value1))
                            {
                                return Convert.ToDouble(value2) - Convert.ToDouble(value1);
                            }
                            else
                                throw new Exception("Can't calculate " + RPNTokenType.Minus.ToString() + " " + Convert.ToString(value1) + ".");
                         */
                        case RPNTokenType.Mulitiply:
                            return Convert.ToInt32(value2) & Convert.ToInt32(value1);
                        /*case RPNTokenType.Divide:
                            return Convert.ToDouble(value2) / Convert.ToDouble(value1);
                        case RPNTokenType.Less:
                            return Convert.ToDouble(value2) < Convert.ToDouble(value1);
                        case RPNTokenType.Greater:
                            return Convert.ToDouble(value2) > Convert.ToDouble(value1);
                        case RPNTokenType.LessOrEqual:
                            return Convert.ToDouble(value2) <= Convert.ToDouble(value1);
                        case RPNTokenType.GreateOrEqual:
                            return Convert.ToDouble(value1) <= Convert.ToDouble(value2);
                        case RPNTokenType.NotEqual:
                            return Convert.ToDouble(value2) != Convert.ToDouble(value1);
                        case RPNTokenType.Equal:
                            return Convert.ToDouble(value2) == Convert.ToDouble(value1);
                        case RPNTokenType.Or:
                            return Convert.ToBoolean(value2) || Convert.ToBoolean(value1);
                        case RPNTokenType.And:
                            return Convert.ToBoolean(value2) && Convert.ToBoolean(value1);
                         */
                        default:
                            throw new Exception("Can't calculate " + RPNTokenType.Not.ToString() + " " + Convert.ToString(value1) + ".");
                    }
            }
        }
    }

    public class RPNOperand: RPNObject
    {
        public object Value;

        public RPNOperand(RPNExpr owner, RPNObjectType objectType) : base(owner, objectType)
        {
        }

        public override string ToString()
        {
            return Convert.ToString(Value);
        }

        public override object GetValue()
        {
            Owner.Position--;
            return Value;
        }
    }

    public class RPNDataField : RPNObject
    {
        public string Field;

        public RPNDataField(RPNExpr owner, RPNObjectType objectType) : base(owner, objectType)
        {
        }

        public override string ToString()
        {
            return Field;
        }

        public override object GetValue()
        {
            Owner.Position--;
            object result = null;
            Owner.Environment.CalcDataField(Field, ref result);
            return result;
        }
    }

    public class RPNVariable : RPNObject
    {
        public string Name;

        public RPNVariable(RPNExpr owner, RPNObjectType objectType) : base(owner, objectType)
        {
        }

        public override string ToString()
        {
            return Name;
        }

        public override object GetValue()
        {
            Owner.Position--;
            return Owner.VariableByName(Name).value;
        }
    }

    public class FunctionAttribute : Attribute
    {
        private string functionName;
        private string paramTypes;
        private string group;
        private string paramNames;
        private string description;

        public string FunctionName
        {
            get { return functionName; }
            set { functionName = value; }
        }
        
        public string ParamTypes
        {
            get { return paramTypes; }
            set { paramTypes = value; }
        }
        
        public string Group
        {
            get { return group; }
            set { group = value; }
        }
        
        public string ParamNames
        {
            get { return paramNames; }
            set { paramNames = value; }
        }
        
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public FunctionAttribute(string functionName)
        {
            this.functionName = functionName;
        }
    }

    public abstract class RPNFunction : RPNObject
    {
        protected List<object> Params = new List<object>();

        public override string ToString()
        {
            FunctionAttribute funcAttrib = (FunctionAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(FunctionAttribute));
            return funcAttrib.FunctionName;
        }

        public override object GetValue()
        {
            Owner.Position--;
            FunctionAttribute funcAttrib = (FunctionAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(FunctionAttribute));

            if (funcAttrib.ParamTypes != null)
            {
                for (int i = 0; i < funcAttrib.ParamTypes.Length; i++)
                {
                    Params.Insert(0, Owner.RPNList[Owner.Position].GetValue());
                }
            }
            CheckParams();

            return Calc();
        }

        private void DoRaise(string type, string value, int parameterIndex)
        {
            throw new Exception("Parameter " + Convert.ToString(parameterIndex + 1) +
                                " should be of type " + type +
                                ". Value of parameter is " + value + ".");
        }

        public abstract object Calc();

        public virtual void Aggregate()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void CheckParams()
        {
            FunctionAttribute funcAttrib = (FunctionAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(FunctionAttribute));

            if (funcAttrib.ParamTypes != null)
            {
                for (int i = 0; i < funcAttrib.ParamTypes.Length - 1; i++)
                {
                    switch (funcAttrib.ParamTypes[i])
                    {
                        case 'B':
                            if (!(Params[i] is bool))
                                DoRaise("Boolean", Convert.ToString(Params[i]), i);
                            break;
                        case 'D':
                            if (!(Params[i] is DateTime))
                                DoRaise("DateTime", Convert.ToString(Params[i]), i);
                            break;
                        case 'N':
                            if (!(IsNumeric(Params[i])))
                                DoRaise("Number", Convert.ToString(Params[i]), i);
                            break;
                        case 'S':
                            if (!(Params[i] is string))
                                DoRaise("String", Convert.ToString(Params[i]), i);
                            break;
                    }
                }
            }
        }
    }

    public class RPNAggregateFunction : RPNFunction
    {
        public double Value;
        public int Count;

        public override object Calc()
        {
            return Value;
        }

        public override bool IsAggregate()
        {
            return true;
        }

        public override void Aggregate()
        {
            Owner.Position--;
            Count++;
        }

        public override void Reset()
        {
            Value = 0;
            Count = 0;
        }
    }

    public class Variable
    {
        public string name; //private !!!
        public object value; //private !!!

        public Variable(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }

    public class RPNParser
    {
        public RPNExpr Owner;

        private char[] operators = { '+', '-', '*', '/', '<', '>', '=', '%' };
        private string[] doubleOperators = { "<>", ">=", "<=", "%=", "/=" };

        private string expr;
        private int curPos;
        private RPNTokenType tokenType;
        private string token;
        
        private void SkipBlanks()
        {
            //while not at the end of the string and char at position is 32 or lower
            while ((curPos < expr.Length) && (expr[curPos] <= 32))
                 curPos++;
        }

        private void SetString(int from, int len)
        {
            token = expr.Substring(from, len);
        }

        private bool IsOperator(char c)
        {
            for (int i = 0; i < operators.Length; i++)
                if (c == operators[i])
                    return true;

            return false;
        }

        private bool IsDoubleOperator(string s)
        {
            for (int i = 0; i < doubleOperators.Length; i++)
                if (s == doubleOperators[i])
                    return true;

            return false;
        }

        public RPNParser(string expr)
	    {
            this.expr = expr;
	    }

        private void CheckToken(string s)
        {
            if (Token != s)
                throw new Exception("Token >" + s + "< expected but >" + Token + "< found in expression >" + expr); // + "< at position " + Convert.ToString((int)oCurPos - (int)OrgText) + ".");
        }

        private void NotTerminated(string s)
        {
            throw new Exception(s + " not terminated in expression >" + expr + "<.");
        }

        public void NextToken()
        {
            int pos;

            SkipBlanks();

            if (curPos == expr.Length)
            {
                tokenType = RPNTokenType.Eof;
                token = "";
            }        
            else if (char.IsLetter(expr[curPos])) //Function or boolean operator
            {
                pos = curPos;
                curPos++;
                while ((curPos < expr.Length) && char.IsLetterOrDigit(expr[curPos]))
                    curPos++;
               
                tokenType = RPNTokenType.Ident;
                SetString(pos, curPos - pos);

                //check for AND, OR, NOT, TRUE, FALSE
                switch (token.ToUpper())
                {
                    case "AND":
                        tokenType = RPNTokenType.And;
                        break;
                    case "OR":
                        tokenType = RPNTokenType.Or;
                        break;
                    case "NOT":
                        tokenType = RPNTokenType.Not;
                        break;
                    case "TRUE":
                        tokenType = RPNTokenType.Boolean;
                        break;
                    case "FALSE":
                        tokenType = RPNTokenType.Boolean;
                        break;
                }    
            }
            else if (expr[curPos] == '\'') //String
            {
                bool terminated = false;
                curPos++;
                pos = curPos;

                while (curPos < expr.Length)
                {
                    if (expr[curPos] == '\'')
                    {
                        terminated = true;
                        tokenType = RPNTokenType.String;
                        SetString(pos, curPos - pos);
                        curPos++;
                        break;
                    }
                    curPos++;
                }

                if (!terminated)
                {
                    NotTerminated("String");
                }
            }        
            else if (IsOperator(expr[curPos])) //operator
            {
                char aOperator = expr[curPos];
                pos = curPos;
                curPos++;
                while (curPos < expr.Length && IsDoubleOperator(aOperator + Convert.ToString(expr[curPos])))
                    curPos++;
                SetString(pos, curPos - pos);
                switch (token)
                {
                    case "*":
                        tokenType = RPNTokenType.Mulitiply;
                        break;
                    case "/":
                        tokenType = RPNTokenType.Divide;
                        break;
                    case "/=":
                        tokenType = RPNTokenType.Div;
                        break;
                    case "%=":
                        tokenType = RPNTokenType.Mod;
                        break;
                    case "+":
                        tokenType = RPNTokenType.Plus;
                        break;
                    case "-":
                        tokenType = RPNTokenType.Minus;
                        break;
                    case "<":
                        tokenType = RPNTokenType.Less;
                        break;
                    case ">":
                        tokenType = RPNTokenType.Greater;
                        break;
                    case "<=":
                        tokenType = RPNTokenType.LessOrEqual;
                        break;
                    case ">=":
                        tokenType = RPNTokenType.GreateOrEqual;
                        break;
                    case "<>":
                        tokenType = RPNTokenType.NotEqual;
                        break;
                    case "=":
                        tokenType = RPNTokenType.Equal;
                        break;
                }
            }
            else if (char.IsDigit(expr[curPos])) //number
            {
                pos = curPos;
                curPos++;
                while (curPos < expr.Length && (char.IsDigit(expr[curPos]) || expr[curPos] == Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)))
                    curPos++;
                tokenType = RPNTokenType.Number;
                SetString(pos, curPos - pos);
            }
            else if (expr[curPos] == '"') //date
            {
                curPos++;
                pos = curPos;
                while (true)
                {
                    if ((curPos == expr.Length - 1) && (expr[curPos] != '"'))
                        NotTerminated("Date");
                    else if (expr[curPos] == '"')
                    {
                        tokenType = RPNTokenType.DateTime;
                        SetString(pos, curPos - pos);
                        curPos++;
                        break;
                    }
                    curPos++;
                }
            }
            else if (expr[curPos] == '(') //parentheses
            {
                curPos++;
                tokenType = RPNTokenType.StartParentheses;
                token = "(";
            }
            else if (expr[curPos] == ')')
            {
                curPos++;
                tokenType = RPNTokenType.EndParentheses;
                token = ")";
            }
            else if (expr[curPos] == '[') //square bracket
            {
                curPos++;
                pos = curPos;
                while (true)
                {
                    if ((curPos == expr.Length - 1) && (expr[curPos] != ']'))
                        NotTerminated("Square Bracket");
                    else if (expr[curPos] == ']')
                    {
                        tokenType = RPNTokenType.SquareBracket;
                        SetString(pos, curPos - pos);
                        curPos++;
                        break;
                    }
                    curPos++;
                }
            }
            else if (expr[curPos] == ';')
            {
                curPos++;
                tokenType = RPNTokenType.EndParam;
                token = ";";
            }        
            else //error
            {
                tokenType = RPNTokenType.Unknown;
                token = "";
            }
        }

        public string Token
        {
            get { return token; }
        }

        public RPNTokenType TokenType
        {
            get { return tokenType; }
        }

        private void UnexpectedToken()
        {
            throw new Exception("Unexpected token >" + Token + "< in expression >" + expr + "< at position " + curPos.ToString() + ".");
        }

        private void Factor()
        {
            RPNOperand operand;
            RPNOperator aOperator;
            switch (TokenType)
            {
                case RPNTokenType.Plus:
                case RPNTokenType.Minus:
                    aOperator = new RPNOperator(Owner, RPNObjectType.Operator);
                    switch (TokenType)
                    {
                        case RPNTokenType.Plus:
                            (aOperator as RPNOperator).OperatorType = RPNTokenType.JustPlus;
                            break;
                        case RPNTokenType.Minus:
                            (aOperator as RPNOperator).OperatorType = RPNTokenType.JustMinus;
                            break;
                    }
                    aOperator.OperatorStr = Token;
                    NextToken();
                    Term();
                    Owner.RPNList.Add(aOperator);
                    break;
                case RPNTokenType.StartParentheses:
                    NextToken();
                    Evaluate();

                    CheckToken(")");

                    NextToken();
                    break;
                case RPNTokenType.SquareBracket:
                    RPNDataField dataField = new RPNDataField(Owner, RPNObjectType.DataField);
                    dataField.Field = Token;
                    Owner.RPNList.Add(dataField);
                    NextToken();
                    break;
                case RPNTokenType.Ident:
                    RPNFunction func;
                    if (Owner.Environment != null)
                      func = Owner.Environment.FindFunction(Owner, Token.ToUpper());
                    else
                      func = null;
                    if (func != null)
                    {
                        FunctionAttribute funcAttrib = (FunctionAttribute)Attribute.GetCustomAttribute(func.GetType(), typeof(FunctionAttribute));
                        if (funcAttrib.ParamTypes != null && funcAttrib.ParamTypes.Length > 0)
                        {
                            NextToken();
                            for (int i = 0; i < funcAttrib.ParamTypes.Length; i++)
                            {
                                if (i == 0)
                                    CheckToken("(");
                                else
                                    CheckToken(";");
                                NextToken();
                                Evaluate();
                            }
                            CheckToken(")");
                        }
                        Owner.RPNList.Add(func);
                    }
                    else
                    {
                        RPNVariable var = Owner.FindVariable(Token.ToUpper());
                        if (var != null)
                            Owner.RPNList.Add(var);
                        else
                            throw new Exception("Unknown ident >" + Token + "< in expression >" + Owner.expression + "<.");
                    }
                    NextToken();
                    break;
                case RPNTokenType.Number:
                    operand = new RPNOperand(Owner, RPNObjectType.Operand);
                    float number = float.Parse(Token);
                    operand.Value = (object)number;
                    Owner.RPNList.Add(operand);
                    NextToken();
                    break;
                case RPNTokenType.String:
                    operand = new RPNOperand(Owner, RPNObjectType.Operand);
                    operand.Value = Token;
                    Owner.RPNList.Add(operand);
                    NextToken();
                    break;
                case RPNTokenType.DateTime:
                    operand = new RPNOperand(Owner, RPNObjectType.Operand);
                    try
                    {
                        operand.Value = Convert.ToDateTime(Token);
                    }
                    catch
                    {
                        throw new Exception(Token + " is not a valid date.");
                    }
                    Owner.RPNList.Add(operand);
                    NextToken();
                    break;
                case RPNTokenType.Boolean:
                    operand = new RPNOperand(Owner, RPNObjectType.Operand);
                    operand.Value = Convert.ToBoolean(Token);
                    Owner.RPNList.Add(operand);
                    NextToken();
                    break;
                case RPNTokenType.Not:
                    aOperator = new RPNOperator(Owner, RPNObjectType.Operator);
                    aOperator.OperatorType = TokenType;
                    aOperator.OperatorStr = Token;
                    NextToken();
                    Factor();
                    Owner.RPNList.Add(aOperator);
                    break;
                default:
                    UnexpectedToken();
                    break;

            }
        }

        private void Term()
        {
            Factor();

            switch (TokenType)
            {
                case RPNTokenType.Mulitiply:
                case RPNTokenType.Divide:
                case RPNTokenType.Div:
                case RPNTokenType.Mod:
                case RPNTokenType.And:
                    RPNOperator aOperator = new RPNOperator(Owner, RPNObjectType.Operator);
                    aOperator.OperatorType = TokenType;
                    aOperator.OperatorStr = Token;

                    NextToken();
                    Term();

                    Owner.RPNList.Add(aOperator);
                    break;
            }
        }

        private void Expression()
        {
            Term();

            switch (TokenType)
            {
                case RPNTokenType.Plus:
                case RPNTokenType.Minus:
                case RPNTokenType.Or:
                    RPNOperator aOperator = new RPNOperator(Owner, RPNObjectType.Operator);
                    aOperator.OperatorType = TokenType;
                    aOperator.OperatorStr = Token;

                    NextToken();
                    Expression();

                    Owner.RPNList.Add(aOperator);
                    break;
            }
        }

        public void Evaluate()
        {
            Expression();

            switch (TokenType)
            {
                case RPNTokenType.Less:
                case RPNTokenType.Greater:
                case RPNTokenType.LessOrEqual:
                case RPNTokenType.GreateOrEqual:
                case RPNTokenType.NotEqual:
                case RPNTokenType.Equal:
                    RPNOperator aOperator = new RPNOperator(Owner, RPNObjectType.Operator);
                    aOperator.OperatorType = TokenType;
                    aOperator.OperatorStr = Token;

                    NextToken();
                    Evaluate();

                    Owner.RPNList.Add(aOperator);
                    break;

                case RPNTokenType.EndParentheses:
                    break;
                case RPNTokenType.EndParam:
                    return;
                case RPNTokenType.Eof:
                    return;
            }
        }
    }

    public class RPNExpr
    {
        public string expression;

        public ExprEnvironment Environment;
        public SortedList<string, Variable> Variables = new SortedList<string, Variable>();
        public int Position;
        public List<RPNObject> RPNList = new List<RPNObject>();

        public RPNExpr()
        {
        }

        public RPNExpr(string expression)
	    {
            this.expression = expression;
	    }

        public void Prepare()
        {
            RPNList.Clear();

            RPNParser parser = new RPNParser(expression);
            parser.Owner = this;
            parser.NextToken();
            parser.Evaluate();
        }

        public object GetValue()
        {
            Position = RPNList.Count - 1;
            return RPNList[Position].GetValue();
        }

        public void AddVariable(Variable variable)
        {
            Variables.Add(variable.name.ToUpper(), variable);
        }

        public Variable VariableByName(string name)
        {
            int index = Variables.IndexOfKey(name);

            if (index > -1)
                return Variables.Values[index];
            else
                return null;
        }

        public RPNVariable FindVariable(string pToken)
        {
            Variable var = VariableByName(pToken);

            if (var != null)
            {
                RPNVariable result = new RPNVariable(this, RPNObjectType.Variable);
                result.Name = pToken;
                return result;
            }
            return null;
        }
    }
}