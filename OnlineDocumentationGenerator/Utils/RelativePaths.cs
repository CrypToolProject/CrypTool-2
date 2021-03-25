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
