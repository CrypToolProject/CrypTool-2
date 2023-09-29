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

namespace CrypTool.PluginBase
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class FunctionListAttribute : Attribute
    {
        # region multi language properties
        private readonly string function;
        public string Function
        {
            get
            {
                if (MultiLanguage && function != null)
                {
                    return PluginType.GetPluginStringResource(function);
                }
                else
                {
                    return function;
                }
            }
        }

        private readonly string path;
        public string Path
        {
            get
            {
                if (MultiLanguage && path != null)
                {
                    return PluginType.GetPluginStringResource(path);
                }
                else
                {
                    return path;
                }
            }
        }
        # endregion multi language properties

        # region normal properties
        public string PropertyName; // will be set in extension-method
        //public FunctionList FunctionList { get; set; } // will be set in extension-method
        #endregion normal properties

        # region translation helpers

        /// <summary>
        /// Gets or sets the type of the plugin. This value is set by extension method if ResourceFile exists. 
        /// It is used to access the plugins resources to translate the text elements.
        /// </summary>
        /// <value>The type of the plugin.</value>
        public Type PluginType { get; set; }

        private bool MultiLanguage => PluginType != null && PluginType.GetPluginInfoAttribute().ResourceFile != null;
        #endregion translation helpers

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionListAttribute"/> class.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="caption">The function.</param>
        /// <param name="toolTip">The path.</param>
        public FunctionListAttribute(string function, string path)
        {
            this.function = function ?? "";
            this.path = path ?? "";
        }

        public FunctionListAttribute(string function)
        {
            this.function = function ?? "";
            path = "";
        }

        #endregion constructor
    }
}