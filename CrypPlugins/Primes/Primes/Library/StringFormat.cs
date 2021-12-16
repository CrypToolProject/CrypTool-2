/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Text;

namespace Primes.Library
{
    public static class StringFormat
    {
        public static string FormatString(string input, int lenght)
        {
            StringBuilder result = new StringBuilder();

            if (input.Length <= lenght)
            {
                result.Append(input);
            }
            else
            {
                while (input.Length > lenght)
                {
                    int min = Math.Min(input.Length, lenght);
                    result.Append(input.Substring(0, min));
                    result.Append("\n");
                    input = input.Substring(min, input.Length - min);
                }
            }

            return result.ToString();
        }

        public static string FormatDoubleToIntString(double value)
        {
            try
            {
                return Convert.ToInt32(value).ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
