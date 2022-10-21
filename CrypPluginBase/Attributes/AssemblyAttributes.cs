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

namespace CrypTool.PluginBase.Attributes
{
    /************************ Ct2BuildType ************************/
    public enum Ct2BuildType
    {
        Developer = 0,
        Nightly = 1,
        Beta = 2,
        Stable = 3
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyCt2BuildTypeAttribute : Attribute
    {
        public Ct2BuildType BuildType
        {
            get; set;
        }

        public AssemblyCt2BuildTypeAttribute(Ct2BuildType type)
        {
            BuildType = type;
        }

        public AssemblyCt2BuildTypeAttribute(int type)
        {
            BuildType = (Ct2BuildType)type;
        }
    }

    /********************** Ct2InstallationType ********************/
    public enum Ct2InstallationType
    {
        Developer = 0,
        ZIP = 1,
        MSI = 2,
        NSIS = 3
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyCt2InstallationTypeAttribute : Attribute
    {
        public Ct2InstallationType InstallationType
        {
            get;
            set;
        }

        public AssemblyCt2InstallationTypeAttribute(Ct2InstallationType type)
        {
            InstallationType = type;
        }

        public AssemblyCt2InstallationTypeAttribute(int type)
        {
            InstallationType = (Ct2InstallationType)type;
        }
    }
}
