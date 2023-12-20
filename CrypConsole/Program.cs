/*
   Copyright 2023 Nils Kopal <kopal<AT>CrypTool.org>

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
using System.Windows;

namespace CrypTool.CrypConsole
{
    public class CrypConsole
    {
        public static string[] Args
        {
            private set;
            get;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Args = args;
            Application main = new Application
            {
                StartupUri = new Uri("Main.xaml", System.UriKind.Relative)
            };
            main.Run();
        }
    }
}
