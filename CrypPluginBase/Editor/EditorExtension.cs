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

namespace CrypTool.PluginBase.Editor
{
    public static class EditorExtension
    {
        public static EditorInfoAttribute GetEditorInfoAttribute(this IEditor editor)
        {
            return GetEditorInfoAttribute(editor.GetType());
        }

        public static EditorInfoAttribute GetEditorInfoAttribute(this Type type)
        {
            try
            {
                EditorInfoAttribute[] attributes = (EditorInfoAttribute[])type.GetCustomAttributes(typeof(EditorInfoAttribute), false);
                if (attributes.Length == 1)
                {
                    return attributes[0];
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
