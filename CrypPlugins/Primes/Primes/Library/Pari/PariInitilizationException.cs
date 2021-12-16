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

namespace Primes.Library.Pari
{
    public class PariInitilizationException : Exception
    {
        private string m_Path = null;

        public string Path
        {
            get => m_Path;
            set => m_Path = value;
        }

        public PariInitilizationException(string path)
            : base(string.Format("Could not find gp.exe in {0}", path))
        {
            m_Path = path;
        }
    }
}