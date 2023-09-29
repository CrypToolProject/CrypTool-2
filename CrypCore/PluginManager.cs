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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml;

namespace CrypTool.Core
{
    /// <summary>
    /// PluginManager Class  
    /// </summary>
    public class PluginManager
    {
        private readonly HashSet<string> disabledAssemblies = new HashSet<string>();

        /// <summary>
        /// Counter for the dll files that were found
        /// </summary>
        private int availablePluginsApproximation = 0;

        /// <summary>
        /// Subdirectory of all crypplugins
        /// </summary>
        private const string PluginDirectory = "CrypPlugins";

        /// <summary>
        /// Subdirectory in which plugins of CrypToolStore are stored and loaded from
        /// </summary>
        public const string CrypToolStoreDirectory = @"CrypTool2\CrypToolStore";

        /// <summary>
        /// Fires if an exception occurs
        /// </summary>
        public event CrypCoreExceptionEventHandler OnExceptionOccured;

        /// <summary>
        /// Fires if an info occurs
        /// </summary>
        public event CrypCoreDebugEventHandler OnDebugMessageOccured;

        /// <summary>
        /// Occurs when a plugin was loaded
        /// </summary>
        public event CrypCorePluginLoadedHandler OnPluginLoaded;

        /// <summary>
        /// Folder for plugins that are delivered with CrypTool 2
        /// </summary>
        private readonly string crypPluginsFolder;

        /// <summary>
        /// Folder for plugins of CrypToolStore
        /// </summary>
        private readonly string CrypToolStorePluginFolder;

        /// <summary>
        /// Loaded Assemblies
        /// </summary>
        private readonly Dictionary<string, Assembly> loadedAssemblies;

        /// <summary>
        /// Loaded Types
        /// </summary>
        private readonly Dictionary<string, Type> loadedTypes;

        /// <summary>
        /// Found Assemblies
        /// </summary>
        private readonly Dictionary<string, Assembly> foundAssemblies = new Dictionary<string, Assembly>();

        /// <summary>
        /// Loads all assemblies from CrypPlugins and the given CrypToolStore sub folder
        /// </summary>
        /// <param name="disabledAssemblies"></param>
        /// <param name="CrypToolStoreSubFolder"></param>
        public PluginManager(HashSet<string> disabledAssemblies, string CrypToolStoreSubFolder)
        {
            CrypToolStorePluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CrypToolStoreDirectory);
            CrypToolStorePluginFolder = Path.Combine(CrypToolStorePluginFolder, CrypToolStoreSubFolder);
            CrypToolStorePluginFolder = Path.Combine(CrypToolStorePluginFolder, "plugins");

            //create folder for CrypToolStore if it does not exsit
            if (!Directory.Exists(CrypToolStorePluginFolder))
            {
                Directory.CreateDirectory(CrypToolStorePluginFolder);
            }

            //update everything of the CrypToolStore, i.e. installations, deletions, and updates
            UpdateCrypToolStoreFoldersAndPlugins();

            this.disabledAssemblies = disabledAssemblies;
            crypPluginsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginDirectory);
            loadedAssemblies = new Dictionary<string, Assembly>();
            loadedTypes = new Dictionary<string, Type>();
        }

        /// <summary>
        /// Returns a type of a loaded assembly
        /// </summary>
        /// <param name="assemblyName">Assembly Name</param>
        /// <param name="typeName">Type Name</param>
        /// <returns>Return the type or null if no type could be found</returns>
        public Type LoadType(string assemblyName, string typeName)
        {
            if (loadedAssemblies.ContainsKey(assemblyName))
            {
                return loadedAssemblies[assemblyName].GetType(typeName, false);
            }
            return null;
        }

        /// <summary>
        /// Returns all types found in the plugins
        /// </summary>
        /// <param name="state">Load type from all plugins or from signed only</param>
        /// <returns></returns>
        public Dictionary<string, Type> LoadTypes(AssemblySigningRequirement state)
        {
            availablePluginsApproximation = AvailablePluginsApproximation(new DirectoryInfo(crypPluginsFolder));
            availablePluginsApproximation += AvailablePluginsApproximation(new DirectoryInfo(CrypToolStorePluginFolder), true);
            int currentPosition = FindAssemblies(new DirectoryInfo(crypPluginsFolder), state, foundAssemblies, 0);
            FindAssemblies(new DirectoryInfo(CrypToolStorePluginFolder), state, foundAssemblies, currentPosition, true);
            LoadTypes(foundAssemblies);
            return loadedTypes;
        }

        /// <summary>
        /// Find all CrypPlugins
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private int AvailablePluginsApproximation(DirectoryInfo directory, bool recursive = false)
        {
            int count = 0;
            if (recursive)
            {
                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    count += AvailablePluginsApproximation(subDirectory, recursive);
                }
            }
            return count + directory.GetFiles("*.dll").Length;
        }

        /// <summary>
        /// Search for assemblies in given directory
        /// </summary>
        /// <param name="directory">directory</param>
        /// <param name="state">Search for all or only for signed assemblies</param>
        /// <param name="foundAssemblies">list of found assemblies</param>
        private int FindAssemblies(DirectoryInfo directory, AssemblySigningRequirement state, Dictionary<string, Assembly> foundAssemblies, int currentPosition = 0, bool recursive = false)
        {
            foreach (FileInfo fileInfo in directory.GetFiles("*.dll"))
            {
                if (disabledAssemblies != null && disabledAssemblies.Contains(fileInfo.Name))
                {
                    continue;
                }

                currentPosition++;
                try
                {
                    Assembly asm = Assembly.Load(AssemblyName.GetAssemblyName(fileInfo.FullName));

                    string key = GetAssemblyKey(asm.FullName, state);
                    if (key == null)
                    {
                        throw new UnknownFileFormatException(fileInfo.FullName);
                    }

                    bool sendMessage = false;
                    if (!foundAssemblies.ContainsKey(key))
                    {
                        foundAssemblies.Add(key, asm);
                        sendMessage = true;
                    }
                    else if (GetVersion(asm) > GetVersion(foundAssemblies[key]))
                    {
                        foundAssemblies[key] = asm;
                        sendMessage = true;
                    }

                    if (sendMessage)
                    {
                        SendDebugMessage("Loaded Assembly \"" + asm.FullName + "\" from file: " + fileInfo.FullName);
                        if (OnPluginLoaded != null)
                        {
                            OnPluginLoaded(this, new PluginLoadedEventArgs(currentPosition, availablePluginsApproximation, string.Format("{0} Version={1}", asm.GetName().Name, GetVersion(asm))));
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                    SendExceptionMessage(string.Format(Resources.Exceptions.non_plugin_file, fileInfo.FullName));
                }
                catch (Exception ex)
                {
                    SendExceptionMessage(ex);
                }
            }

            if (recursive)
            {
                //search all subfolders for assemblies
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    FindAssemblies(dir, state, foundAssemblies, currentPosition);
                }
            }

            return currentPosition;
        }

        /// <summary>
        /// Returns version of the given assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Version GetVersion(Assembly assembly)
        {
            string fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            if (fileVersion == null)
            {
                return assembly.GetName().Version;
            }

            return new Version(fileVersion);
        }

        /// <summary>
        /// Interate the found assemblies and add well known types
        /// </summary>
        /// <param name="foundAssemblies">list of found assemblies</param>
        private void LoadTypes(Dictionary<string, Assembly> foundAssemblies)
        {
            string interfaceName = "CrypTool.PluginBase.IPlugin";

            foreach (Assembly asm in foundAssemblies.Values)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                try
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (type.GetInterface(interfaceName) != null && !loadedTypes.ContainsKey(type.FullName))
                        {
                            loadedTypes.Add(type.FullName, type);
                            if (!loadedAssemblies.ContainsKey(assemblyName.Name))
                            {
                                loadedAssemblies.Add(assemblyName.Name, asm);
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException tle)
                {
                    if (OnExceptionOccured != null)
                    {
                        OnExceptionOccured(this, new PluginManagerEventArgs(new TypeLoadException(asm.FullName + "\n" + tle.LoaderExceptions[0].Message)));
                    }
                }
                catch (Exception exception)
                {
                    if (OnExceptionOccured != null)
                    {
                        OnExceptionOccured(this, new PluginManagerEventArgs(new TypeLoadException(asm.FullName + "\n" + exception.Message)));
                    }
                }
            }
        }

        /// <summary>
        /// Create a unique key for each assembly
        /// </summary>
        /// <param name="assemblyFullName">Full name of the assembly</param>
        /// <param name="state">Signed or unsigned</param>
        /// <returns>Returns the key or null if public key is null and signing is required</returns>
        private string GetAssemblyKey(string assemblyFullName, AssemblySigningRequirement state)
        {
            AssemblyName asmName = new AssemblyName(assemblyFullName);
            if (state == AssemblySigningRequirement.LoadSignedAssemblies)
            {
                if (asmName.KeyPair.PublicKey == null)
                {
                    return null;
                }

                return asmName.Name + "__" + asmName.KeyPair.ToString();
            }
            return asmName.Name;
        }
        /// <summary>
        /// Sends a debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendDebugMessage(string message)
        {
            if (OnDebugMessageOccured != null)
            {
                OnDebugMessageOccured(this, new PluginManagerEventArgs(message));
            }
        }

        private void SendExceptionMessage(Exception ex)
        {
            if (OnExceptionOccured != null)
            {
                OnExceptionOccured(this, new PluginManagerEventArgs(ex));
            }
        }

        private void SendExceptionMessage(string message)
        {
            if (OnExceptionOccured != null)
            {
                OnExceptionOccured(this, new PluginManagerEventArgs(message));
            }
        }

        /// <summary>
        /// Updates CrypToolStore folders and plugins
        /// </summary>
        private void UpdateCrypToolStoreFoldersAndPlugins()
        {
            foreach (string xmlfilename in Directory.GetFiles(CrypToolStorePluginFolder, "*.xml"))
            {
                try
                {
                    ProcessXmlFile(xmlfilename);
                }
                catch (Exception ex)
                {
                    SendExceptionMessage(string.Format("Exception occured while processing CrypToolStore installation file {0}: {1}", xmlfilename, ex.Message));
                    try
                    {
                        if (File.Exists(xmlfilename))
                        {
                            File.Delete(xmlfilename);
                        }
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }
            }
        }

        /// <summary>
        /// processes an installation xml file of the CrypToolStore
        /// </summary>
        /// <param name="xmlfilename"></param>
        private void ProcessXmlFile(string xmlfilename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlfilename);

            XmlNode installationNode = xml.SelectSingleNode("installation");
            XmlNode pluginNode = installationNode.SelectSingleNode("plugin");
            XmlNode nameNode = pluginNode.SelectSingleNode("name");
            XmlNode idNode = pluginNode.SelectSingleNode("id");
            XmlNode versionNode = pluginNode.SelectSingleNode("version");

            string type = installationNode.Attributes["type"].Value;
            string name = nameNode.InnerText;
            string id = idNode.InnerText;
            string version = versionNode.InnerText;

            string assemblyzipfilename = Path.Combine(CrypToolStorePluginFolder, string.Format("assembly-{0}-{1}.zip", id, version));
            string pluginfoldername = Path.Combine(CrypToolStorePluginFolder, string.Format("plugin-{0}", id, version));

            if (Directory.Exists(pluginfoldername) &&
                 (type.Equals("installation") ||
                  type.Equals("deletion")))
            {
                Directory.Delete(pluginfoldername, true);
            }

            if (type.Equals("installation"))
            {
                Directory.CreateDirectory(pluginfoldername);
                ZipFile.ExtractToDirectory(assemblyzipfilename, pluginfoldername);
                SendDebugMessage(string.Format("Installation of \"{0}\" completed", name));
                File.Delete(assemblyzipfilename);
            }

            if (File.Exists(xmlfilename))
            {
                File.Delete(xmlfilename);
            }
        }

    }
}
