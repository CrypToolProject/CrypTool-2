using System.Collections.Generic;

namespace KeyTextBox
{
    public static class FormatHelper
    {
        public static List<char> GetPossibleCharactersFromFormat(string format, int index)
        {
            if (index >= format.Length)
            {
                return null;
            }

            var result = new List<char>(1);
            if (format[index] == '[')
            {
                index++;
                do
                {
                    var curChar = format[index++];
                    result.Add(curChar);
                    if (format[index] == '-')
                    {
                        index++;
                        for (curChar++; curChar <= format[index]; curChar++)
                        {
                            result.Add(curChar);
                        }
                        index++;
                    }
                } while (format[index] != ']');
            }
            else
            {
                result.Add(format[index]);
            }

            return result;
        }

        public static int GetNextFormatIndex(string format, int index)
        {
            if (format[index] == '[')
            {
                do
                {
                    index++;
                } while (format[index] != ']');
            }
            return index + 1;
        }
    }
}
