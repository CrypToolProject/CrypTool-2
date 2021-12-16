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

using System.Diagnostics;
using System.Globalization;

namespace Primes.Library.Pari
{
    public class PariBridge
    {
        private Process m_PariProcess;
        private static ProcessStartInfo m_PariProcessStartInfo;
        private static readonly string m_PathToGp = "";

        public static void Initialize(string pathtogp)
        {
            if (!string.IsNullOrEmpty(pathtogp))
            {
                m_PariProcessStartInfo = new ProcessStartInfo(pathtogp)
                {
                    Arguments = @"-q",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
        }

        static PariBridge()
        {
        }

        private Process PariProcess
        {
            get
            {
                if (PariBridge.IsInitialized)
                {
                    if (m_PariProcess == null)
                    {
                        m_PariProcess = Process.Start(m_PariProcessStartInfo);
                    }

                    if (m_PariProcess.HasExited)
                    {
                        m_PariProcess.Start();
                    }
                    return Process.Start(m_PariProcessStartInfo);
                }
                else { throw new PariInitilizationException(m_PathToGp); }
            }
        }

        private string Execute(string expression)
        {
            Process p = PariProcess;
            p.StandardInput.Write(expression);
            p.StandardInput.Close();
            string result = p.StandardOutput.ReadToEnd();
            int index = result.IndexOf('m', result.IndexOf('m') + 1) + 1;
            result = result.Substring(index, result.Length - index);
            index = result.IndexOf(char.ConvertFromUtf32(27));
            result = result.Substring(0, index);
            result = result.Replace('.', ',');
            return result;
        }

        public double LiX(double x)
        {
            string expression = string.Format("intnum (x=2,{0},1/log(x))", x);
            string result = Execute(expression);
            return double.Parse(result, NumberStyles.Float);
        }

        public int PiX(double x)
        {
            string expression = string.Format("primepi({0})", x);
            string result = Execute(expression);
            return int.Parse(result);
        }

        public static bool IsInitialized => m_PariProcessStartInfo != null;
    }
}
