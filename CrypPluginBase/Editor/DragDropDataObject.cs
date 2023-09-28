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
namespace CrypTool.PluginBase.Editor
{
    /// <summary>
    /// This class is used to give assembly and type to editor via drag and drop. 
    /// An object of "Type" can't be used as content of DataObject, wich is the container
    /// class for drag and drop opereation.
    /// </summary>
    public class DragDropDataObject
    {
        public readonly string AssemblyFullName;
        public readonly string TypeFullName;
        public readonly string Identifier;

        public DragDropDataObject(string assemblyFullName, string typeFullName, string identifier)
        {
            AssemblyFullName = assemblyFullName;
            TypeFullName = typeFullName;
            Identifier = identifier;
        }
    }
}
