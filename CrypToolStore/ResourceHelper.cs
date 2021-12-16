/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.IO;
using System.Windows;

namespace CrypTool.CrypToolStore
{
    /// <summary>
    /// This class allows to locate and download resources from CrypToolStore
    /// defined by resourceId and resourceVersion
    /// </summary>
    public class ResourceHelper
    {
        private static readonly object LockObject = new object();

        /// <summary>
        /// Get the path to a resource's folder path if the resource exists.
        /// If it does not exist, it prompts the user to download it
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="resourceVersion"></param>
        /// <param name="description"></param>
        /// <returns>the path if it exists; otherwise returns null</returns>
        public static string GetResourceFile(int resourceId, int resourceVersion, IPlugin plugin, string description)
        {
            lock (LockObject)
            {
                try
                {
                    string resourcesFolder = GetResourcesFolder();
                    //we create the resources folder if it does not exist
                    if (!Directory.Exists(resourcesFolder))
                    {
                        Directory.CreateDirectory(resourcesFolder);
                    }

                    //now, we check, if the requested resource folder and file exist
                    string file = Path.Combine(resourcesFolder, string.Format("resource-{0}-{1}", resourceId, resourceVersion));
                    file = Path.Combine(file, string.Format("Resource-{0}-{1}.bin", resourceId, resourceVersion));
                    if (File.Exists(file))
                    {
                        return file;
                    }
                    //the resource file does not exists; thus, we download the resource from CrypToolStoreServer
                    return DownloadResource(resourceId, resourceVersion, plugin, description);
                }
                catch (Exception)
                {
                    //wtf?
                    return null;
                }
            }
        }

        /// <summary>
        /// Deletes the file (=> complete folder) of the resource identified by resourceId and resourceVersion
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="resourceVersion"></param>
        public static void DeleteResourceFile(int resourceId, int resourceVersion)
        {
            string resourcesFolder = GetResourcesFolder();
            //we create the resources folder if it does not exist
            if (!Directory.Exists(resourcesFolder))
            {
                Directory.CreateDirectory(resourcesFolder);
            }

            //now, we check, if the requested resource folder exists
            resourcesFolder = Path.Combine(resourcesFolder, string.Format("resource-{0}-{1}", resourceId, resourceVersion));
            if (Directory.Exists(resourcesFolder))
            {
                Directory.Delete(resourcesFolder);
            }
        }

        /// <summary>
        /// This method shows a download dialog for downloading the resource to the resource folder
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="resourceVersion"></param>
        /// <param name="plugin"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        private static string DownloadResource(int resourceId, int resourceVersion, IPlugin plugin, string description)
        {
            //check CrypToolStore if resource exists
            string path = null;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    DownloadResourceDataFileWindow downloadResourceDataFileWindow =
                        new DownloadResourceDataFileWindow(resourceId, resourceVersion, plugin, description);
                    //position download window in the middle of CrypTool 2:
                    downloadResourceDataFileWindow.Left = Application.Current.MainWindow.Width / 2 - downloadResourceDataFileWindow.Width / 2;
                    downloadResourceDataFileWindow.Top = Application.Current.MainWindow.Height / 2 - downloadResourceDataFileWindow.Height / 2;
                    //show download dialog
                    downloadResourceDataFileWindow.ShowDialog();
                    path = downloadResourceDataFileWindow.Path;
                }
                catch (Exception)
                {
                    //wtf?
                }
            }));
            return path;
        }

        /// <summary>
        /// Returns the absolute path to the resources folder
        /// </summary>
        /// <returns></returns>
        internal static string GetResourcesFolder()
        {
            //Translate the Ct2BuildType to a folder name for CrypToolStore plugins                
            string CrypToolStoreSubFolder = "";
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    CrypToolStoreSubFolder = "Developer";
                    break;
                case Ct2BuildType.Nightly:
                    CrypToolStoreSubFolder = "Nightly";
                    break;
                case Ct2BuildType.Beta:
                    CrypToolStoreSubFolder = "Beta";
                    break;
                case Ct2BuildType.Stable:
                    CrypToolStoreSubFolder = "Release";
                    break;
                default: //if no known version is given, we assume developer
                    CrypToolStoreSubFolder = "Developer";
                    break;
            }
            string CrypToolStorePluginFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PluginManager.CrypToolStoreDirectory);
            CrypToolStorePluginFolder = System.IO.Path.Combine(CrypToolStorePluginFolder, CrypToolStoreSubFolder);
            CrypToolStorePluginFolder = System.IO.Path.Combine(CrypToolStorePluginFolder, "resources");
            return CrypToolStorePluginFolder;
        }
    }
}
