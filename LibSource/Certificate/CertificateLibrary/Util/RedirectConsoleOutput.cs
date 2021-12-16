using System;
using System.IO;
using System.Text;

namespace CrypTool.Util.Logging
{
    public class RedirectConsoleOutput : TextWriter
    {
        public delegate void ConsoleOutputEventHandler(string text);
        public static event ConsoleOutputEventHandler OnConsoleOutput;

        private void Execute(string value)
        {
            if (OnConsoleOutput != null)
            {
                OnConsoleOutput(value);
            }
        }
        private void ExecuteLine(string value)
        {
            value += Environment.NewLine;
            Execute(value);
        }
        public override void Write(bool value)
        {
            Execute(value.ToString());
        }
        public override void Write(char value)
        {
            Execute(value.ToString());
        }
        public override void Write(char[] buffer)
        {
            string result = string.Empty;
            for (int i = 0; i < buffer.Length; i++)
            {
                result += buffer[i].ToString();
            }
            Execute(result);
        }
        public override void Write(char[] buffer, int index, int count)
        {
            string result = string.Empty;
            for (int i = index; i < index + count; i++)
            {
                result += buffer[i].ToString();
            }
            Execute(result);
        }
        public override void Write(decimal value)
        {
            Execute(value.ToString());
        }
        public override void Write(double value)
        {
            Execute(value.ToString());
        }
        public override void Write(float value)
        {
            Execute(value.ToString());
        }
        public override void Write(int value)
        {
            Execute(value.ToString());
        }
        public override void Write(long value)
        {
            Execute(value.ToString());
        }
        public override void Write(object value)
        {
            Execute(value.ToString());
        }
        public override void Write(string format, object arg0)
        {
            Execute(string.Format(format, arg0));
        }
        public override void Write(string format, object arg0, object arg1)
        {
            Execute(string.Format(format, arg0, arg1));
        }
        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            Execute(string.Format(format, arg0, arg1, arg2));
        }
        public override void Write(string format, params object[] arg)
        {
            Execute(string.Format(format, arg));
        }
        public override void Write(string value)
        {
            Execute(value.ToString());
        }
        public override void Write(uint value)
        {
            Execute(value.ToString());
        }
        public override void Write(ulong value)
        {
            Execute(value.ToString());
        }
        public override void WriteLine()
        {
            ExecuteLine("");
        }
        public override void WriteLine(bool value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(char value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(char[] buffer)
        {
            string result = string.Empty;
            for (int i = 0; i < buffer.Length; i++)
            {
                result += buffer[i].ToString();
            }
            ExecuteLine(result);
        }
        public override void WriteLine(char[] buffer, int index, int count)
        {
            string result = string.Empty;
            for (int i = index; i < index + count; i++)
            {
                result += buffer[i].ToString();
            }
            ExecuteLine(result);
        }
        public override void WriteLine(decimal value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(double value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(float value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(int value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(long value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(object value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(string format, object arg0)
        {
            ExecuteLine(string.Format(format, arg0));
        }
        public override void WriteLine(string format, object arg0, object arg1)
        {
            ExecuteLine(string.Format(format, arg0, arg1));
        }
        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            ExecuteLine(string.Format(format, arg0, arg1, arg2));
        }
        public override void WriteLine(string format, params object[] arg)
        {
            ExecuteLine(string.Format(format, arg));
        }
        public override void WriteLine(string value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(uint value)
        {
            ExecuteLine(value.ToString());
        }
        public override void WriteLine(ulong value)
        {
            ExecuteLine(value.ToString());
        }
        public override Encoding Encoding => Encoding.ASCII;
    }
}
