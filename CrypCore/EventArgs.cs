/*
   Copyright 2008 - 2022 CrypTool Team

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

namespace CrypTool.Core
{
    public class PluginManagerEventArgs : EventArgs
    {
        public readonly Exception Exception;

        public PluginManagerEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public PluginManagerEventArgs(string message)
        {
            Exception = new Exception(message);
        }
    }

    public class PluginLoadedEventArgs : EventArgs
    {
        public readonly int CurrentPluginNumber;
        public readonly int NumberPluginsFound;
        public readonly string AssemblyName;

        public PluginLoadedEventArgs(int currentPluginNumber, int numberPluginsFound, string assemblyName)
        {
            CurrentPluginNumber = currentPluginNumber;
            NumberPluginsFound = numberPluginsFound;
            AssemblyName = assemblyName;
        }
    }
}
