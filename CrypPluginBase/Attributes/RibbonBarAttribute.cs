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
using System.Reflection;

namespace CrypTool.PluginBase
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class RibbonBarAttribute : Attribute
    {
        # region multi language properties
        private readonly string caption;
        public string Caption
        {
            get
            {
                if (MultiLanguage && caption != null)
                {
                    return PluginType.GetPluginStringResource(caption);
                }
                else
                {
                    return caption;
                }
            }
        }

        private readonly string toolTip;
        public string ToolTip
        {
            get
            {
                if (MultiLanguage && toolTip != null)
                {
                    return PluginType.GetPluginStringResource(toolTip);
                }
                else
                {
                    return toolTip;
                }
            }
        }

        public readonly string groupName;
        public string GroupName
        {
            get
            {
                if (MultiLanguage && groupName != null)
                {
                    return PluginType.GetPluginStringResource(groupName);
                }
                else
                {
                    return groupName;
                }
            }
        }

        public bool HasGroupName => groupName != null && groupName != string.Empty;
        #endregion multi language properties        

        #region translation helpers
        private Type pluginType;

        /// <summary>
        /// Gets or sets the type of the plugin. This value is set by extension method if ResourceFile exists. 
        /// It is used to access the plugins resources to translate the text elements.
        /// </summary>
        /// <value>The type of the plugin.</value>
        public Type PluginType
        {
          get => pluginType;
          set => pluginType = value;
        }

        private bool MultiLanguage => PluginType != null;
        #endregion translation helpers

        #region public attributes
        private MethodInfo method;
        public MethodInfo Method
        {
            get => method;
            set
            {
                if (method == null)
                {
                    method = value;
                }
                else
                {
                    throw new ArgumentException("This setter should only be accessed once.");
                }
            }
        }

        private string propertyName;
        public string PropertyName
        {
            get => propertyName;
            set
            {
                // This value should be readonly but for user convenience we set it in extension method. 
                // This setter should only be accessed once.
                if (propertyName == null)
                {
                    propertyName = value;
                }
                else
                {
                    throw new ArgumentException("This setter should only be accessed once.");
                }
            }
        }

        public readonly int Order;
        public readonly ControlType ControlType;
        public readonly string[] ControlValues;
        public readonly string FileExtension;
        public readonly bool ChangeableWhileExecuting;
        public readonly int ImageNumber;
        # endregion public attributes

        public RibbonBarAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting, ControlType controlType, params string[] controlValues)
        {
            this.caption = caption;
            this.toolTip = toolTip;
            this.groupName = groupName;
            Order = order;
            ControlType = controlType;
            ControlValues = controlValues;
            ChangeableWhileExecuting = changeableWhileExecuting;
            ImageNumber = -1;
        }

        public RibbonBarAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting, ControlType controlType, int imageNumber)
        {
            this.caption = caption;
            this.toolTip = toolTip;
            this.groupName = groupName;
            Order = order;
            ControlType = controlType;
            ImageNumber = imageNumber;
        }
    }
}
