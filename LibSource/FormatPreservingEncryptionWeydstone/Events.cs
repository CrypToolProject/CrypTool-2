/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace FormatPreservingEncryptionWeydstone
{

    // Indicates that new output is obtainable
    public class OutputChangedEventArgs : EventArgs
    {
        public OutputChangedEventArgs(string text)
        {
            Text = text;
        }

        // The new Output
        public string Text
        {
            get;
            set;
        }
    }

    // Indicates that the progress value has changed
    public class ProgressChangedEventArgs : EventArgs
    {
        public ProgressChangedEventArgs(double progress)
        {
            Progress = progress;
        }

        // The new progress value
        public double Progress
        {
            get;
            set;
        }
    }

}
