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
using System.IO;

namespace CrypTool.PluginBase.IO
{
    public class DirectoryHelper
    {
        private const string CrypTool2 = "CrypTool2";
        private const string crypPlugins = "CrypPlugins";
        private const string tempFiles = "Temp";
        private const string samples = "Templates";
        private const string languageStatistics = "LanguageStatistics";

        public static string DirectoryCrypPlugins
        {
            get;
            private set;
        }

        public static string DirectoryLocal
        {
            get;
            private set;
        }

        public static string DirectoryLocalTemp
        {
            get;
            private set;
        }

        public static string DirectorySamples
        {
            get;
            private set;
        }

        public static string BaseDirectory
        {
            get;
            private set;
        }

        public static string DirectoryLanguageStatistics
        {
            get;
            private set;
        }

        static DirectoryHelper()
        {
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryCrypPlugins = Path.Combine(BaseDirectory, crypPlugins);
            DirectoryLocal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CrypTool2);
            DirectoryLocalTemp = Path.Combine(DirectoryLocal, tempFiles);
            DirectorySamples = Path.Combine(BaseDirectory, samples);
            DirectoryLanguageStatistics = Path.Combine(BaseDirectory, languageStatistics);
        }

        public static string GetNewTempFilePath()
        {
            return GetNewTempFilePath(string.Empty);
        }

        /// <summary>
        /// Returns path to a unique file name which can be used for temporary files.
        /// The generated file name contains a Guid and does not use Path.GetTempFileName().
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetNewTempFilePath(string extension)
        {
            string filePath;
            do
            {
                if (extension.Length > 0)
                {
                    filePath = Path.Combine(DirectoryHelper.DirectoryLocalTemp, Guid.NewGuid().ToString() + "." + extension);
                }
                else
                {
                    filePath = Path.Combine(DirectoryHelper.DirectoryLocalTemp, Guid.NewGuid().ToString());
                }
            } while (File.Exists(filePath)); // sanity check for GUID collision

            // ensure the temp directory exists
            if (!Directory.Exists(DirectoryHelper.DirectoryLocalTemp))
            {
                Directory.CreateDirectory(DirectoryHelper.DirectoryLocalTemp);
            }

            return filePath;
        }
    }
}
