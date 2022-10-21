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
using CrypTool.PluginBase.Attributes;
using System;
using System.Diagnostics;
using System.Reflection;

namespace CrypTool.PluginBase.Miscellaneous
{
    /*
     * Access assembly information of executing assembly (which is CrypPluginBase).
     */
    public static class AssemblyHelper
    {
        public static Ct2BuildType BuildType
        {
            get; private set;
        }

        public static string ProductName
        {
            get; private set;
        }

        public static Version Version
        {
            get; private set;
        }

        public static Ct2InstallationType InstallationType
        {
            get; private set;
        }

        static AssemblyHelper()
        {
            { // BuildType
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCt2BuildTypeAttribute), false);
                if (attributes != null && attributes.Length >= 1)
                {
                    AssemblyCt2BuildTypeAttribute attr = (AssemblyCt2BuildTypeAttribute)attributes[0];
                    AssemblyHelper.BuildType = attr.BuildType;
                }
                else
                {
                    AssemblyHelper.BuildType = Ct2BuildType.Developer;
                }
            }

            { // InstallationType
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCt2InstallationTypeAttribute), false);
                if (attributes != null && attributes.Length >= 1)
                {
                    AssemblyCt2InstallationTypeAttribute attr = (AssemblyCt2InstallationTypeAttribute)attributes[0];
                    AssemblyHelper.InstallationType = attr.InstallationType;
                }
                else
                {
                    AssemblyHelper.InstallationType = Ct2InstallationType.Developer;
                }
            }

            { // ProductName
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes != null && attributes.Length >= 1)
                {
                    AssemblyProductAttribute attr = (AssemblyProductAttribute)attributes[0];
                    AssemblyHelper.ProductName = attr.Product;
                }
            }

            { // Version
                AssemblyHelper.Version = GetVersion(Assembly.GetExecutingAssembly());
            }
        }

        public static Version GetVersion(Assembly asm)
        {
            return new Version(GetVersionString(asm));
        }

        public static string GetVersionString(Assembly asm)
        {
            if (asm == null || asm.Location == null)
            {
                throw new ArgumentNullException("asm");
            }

            return FileVersionInfo.GetVersionInfo(asm.Location).FileVersion;
        }
    }
}
