/*
   Copyright 2018, Nils Kopal, kopal@cryptool.org

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyInfoUpdater
{
    class Program
    {
        const string COMPANY = "CrypTool 2 Team";
        const string COPYRIGHT = "Copyright © CrypTool 2 Team";
        const string VERSION = "2.1.0.0";
        const string BUILDTYPE = "Ct2BuildType.Developer";
        const string INSTALLATIONTYPE = "Ct2InstallationType.Developer";
        
        /// <summary>
        /// Main, only change path; then execute
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string path = @"C:\Users\nilsk\Desktop\CrypTool2\trunk";
            UpdateAssemblyInfoFiles(path);
        }

        /// <summary>
        /// Updates all AssemblyInfo.cs in the given path (and its subfolders)
        /// </summary>
        /// <param name="path"></param>
        static void UpdateAssemblyInfoFiles(string path)
        {
            foreach (string file in Directory.EnumerateFiles(path, "AssemblyInfo.cs", SearchOption.AllDirectories))
            {
                Console.WriteLine("Update:" + file);
                UpdateAssemblyInfo(file);
            }
        }

        /// <summary>
        /// Updates a single AssemblyInfo.cs file
        /// </summary>
        /// <param name="file"></param>
        private static void UpdateAssemblyInfo(string file)
        {
            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("//"))
                {
                    //we dont change outcommented lines
                    continue;
                }
                if (lines[i].Contains("AssemblyCompany"))
                {
                    lines[i] = String.Format("[assembly: AssemblyCompany(\"{0}\")]", COMPANY);
                }
                else if (lines[i].Contains("AssemblyCopyright"))
                {
                    lines[i] = String.Format("[assembly: AssemblyCopyright(\"{0}\")]", COPYRIGHT);
                }
                else if (lines[i].Contains("AssemblyVersion"))
                {
                    lines[i] = String.Format("[assembly: AssemblyVersion(\"{0}\")]", VERSION);
                }
                else if (lines[i].Contains("AssemblyCt2BuildType"))
                {
                    lines[i] = String.Format("[assembly: AssemblyCt2BuildType({0})]", BUILDTYPE);
                }
                else if (lines[i].Contains("AssemblyCt2InstallationType"))
                {
                    lines[i] = String.Format("[assembly: AssemblyCt2InstallationType({0})]", INSTALLATIONTYPE);
                }
            }
            File.WriteAllLines(file, lines);
            Console.WriteLine(String.Format("Wrote file {0}", file));
        }
    }
}
