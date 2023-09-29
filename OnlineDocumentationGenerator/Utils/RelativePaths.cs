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
using System.IO;

namespace OnlineDocumentationGenerator.Utils
{
    public static class RelativePaths
    {
        /// <summary>
        /// Returns the relative path of parameter path "file" in relation to parameter path "relDir".
        /// </summary>
        public static string GetRelativePath(FileInfo file, DirectoryInfo relDir)
        {
            return Path.Combine(GetRelativePath(file.Directory, relDir), file.Name);
        }

        /// <summary>
        /// Returns the relative path of parameter path "dir" in relation to parameter path "relDir". 
        /// </summary>
        public static string GetRelativePath(DirectoryInfo dir, DirectoryInfo relDir)
        {
            if (dir.Root.FullName != relDir.Root.FullName)
            {
                return dir.FullName;
            }
            if (dir.FullName == relDir.FullName)
            {
                return "";
            }

            if (!IsParentDir(relDir, dir))
            {
                return Path.Combine("..", GetRelativePath(dir, relDir.Parent));
            }
            return Path.Combine(GetRelativePath(dir.Parent, relDir), dir.Name);
        }

        /// <summary>
        /// Indicates whether parameter path "parentDir" is a parent of parameter path "dir".
        /// </summary>
        public static bool IsParentDir(DirectoryInfo parentDir, DirectoryInfo dir)
        {
            if (dir == null)
            {
                return false;
            }
            if (parentDir.FullName == dir.FullName)
            {
                return true;
            }
            return IsParentDir(parentDir, dir.Parent);
        }
    }
}
