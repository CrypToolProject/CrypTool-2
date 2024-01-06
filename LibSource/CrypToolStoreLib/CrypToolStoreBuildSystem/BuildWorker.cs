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
using CrypToolStoreLib.Client;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CrypToolStoreBuildSystem
{
    /// <summary>
    /// A build worker builds sources and uploads the created assembly to the CrypToolStoreServer
    /// </summary>
    public class BuildWorker
    {
        private const string BUILD_FOLDER = "Build";
        private const string SOURCE_FILE_NAME = "Source";
        private const string BUILD_TARGET = "x64";

        private BuildLogger Logger = new BuildLogger();
        private Configuration Config = Configuration.GetConfiguration();
        private X509Certificate2 ServerCertificate { get; set; }

        /// <summary>
        /// Reference to source to build
        /// </summary>
        public Source Source
        {
            get;
            set;
        }

        /// <summary>
        /// Is the build Worker currently running?
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Name of the csproj file of the plugin
        /// </summary>
        private string CSProjFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Filename/path of code signing certificate
        /// </summary>
        public string SigningCertificatePfxFile
        {
            get;
            set;
        }

        /// <summary>
        /// Password of code signing certificate
        /// </summary>
        public string SigningCertificatePassword
        {
            get;
            set;
        }

        /// <summary>
        /// Will contain std out and err fo msbuild
        /// </summary>
        private StringBuilder msbuild_Log
        {
            get;
            set;
        }

        /// <summary>
        /// Will contain std out and err fo msbuild
        /// </summary>
        private StringBuilder signtool_Log
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source"></param>
        public BuildWorker(Source source, X509Certificate2 serverCetificate)
        {
            ServerCertificate = serverCetificate;
            Source = source;
        }

        /// <summary>
        /// Starts this BuildWorker
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            Task buildWorkerTask = new Task(BuildWorkerTaskMethod);
            buildWorkerTask.Start();
        }

        /// <summary>
        /// Stops this BuildWorker
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// "Main" method of the worker
        /// builds one source and uploads its assembly to the CrypToolStoreServer
        /// </summary>
        public void BuildWorkerTaskMethod()
        {
            Logger.LogText(string.Format("(General) Started build of source-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            bool buildError = true; //set build error to true; at the end, if everything is ok, error is set to false
                                    //this is needed for the update of the Source's state in the CrypToolStoreDatabase
            try
            {
                // 0) Worker sets source to building state
                SetToBuildingState();

                // 1) Worker creates folder for plugin (e.g. Build\Plugin-1-1, = Plugin-PluginId-SourceId)
                if (!CreateBuildFolder())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 2) Worker creates folder structure in plugin folder
                // --> \plugin           contains source
                // --> \build_output     contains builded plugins
                // --> build_plugin.xml  contains msbuild script

                // note: Also makes references to
                // --> signing certificate
                // --> custom build tasks
                // --> ct2 libraries (CrypCore.dll and CrypPluginBase.dll)
                if (!CreateBuildSubFoldersAndFiles())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 3) Worker downloads zip file
                if (!DownloadZipFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 4) Worker extracts zip file
                if (!ExtractZipFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 5) Worker searches for exactly one csproj file in the root folder, i.e. "plugin"
                // --> if it finds 0 or more than 1, the build Worker fails at this point
                if (!SearchCSProjFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 6) Worker modifies csproj file
                // --> changes references to CrypPluginBase to correct path (hint: dont forget <private>false</private>)
                // --> changes output folder of "Release" target to "build_output" folder
                if (!ModifyCSProjFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 7) Worker modifies msbuild script
                // --> change name of target project to name of csproj file found in "plugin" folder
                if (!ModifyMsBuildScript())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 8) Worker starts "msbuild.exe" (hint: set correct password for signtool to allow it opening signing certificate)
                // --> msbuild compiles the plugin
                if (!BuildPlugin())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 9) Worker checks, if assembly file exists in "build_output" (if not => ERROR)
                if (!CheckBuild())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 10) Worker calls signtool to sign the created assembly/assemblies
                if (!SignPlugin())
                {
                    return;
                }
              
                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 11) Create meta file containing meta information
                if (!CreateMetaFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 12)  Worker zips everything located in "build_output" -- this also includes "de/ru" etc subfolders of the plugin
                // --> zip name is "Assembly-1-1.zip, = Assembly-PluginId-SourceId")
                if (!CreateAssemblyZip())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 13) Worker uploads assembly zip file to CrypToolStore Server, and also updates source data in database
                if (!UploadAssemblyZip())
                {
                    return;
                }

                //if the build process reaches this point, we have no build error
                buildError = false;
                Logger.LogText(string.Format("(General) Finished build of source-{0}-{1} without errors", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                Logger.LogText(string.Format("(General) Exception occured during build of source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
            }
            finally
            {
                // 14) Worker cleans up by deleting build folder (also in case of an error)
                try
                {                    
                    CleanUp();
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("(General) Exception occured during cleanup of source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }

                // 15) Set state of source in database to BUILDED or ERROR
                //     also put build_log in database
                try
                {
                    SetFinalBuildStateAndUploadBuildlog(buildError);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("(General) Exception occured during SetFinalBuildStateAndUploadBuildlog of source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
                IsRunning = false;

                // 16) Send email to developer
                try
                {
                    SendEmailToDeveloper(buildError);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("(General) Exception occured during SendEmailToDeveloper of source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        /// <summary>
        ///  0) Worker sets source to building state
        /// </summary>
        private bool SetToBuildingState()
        {
            Logger.LogText(string.Format("(Buildstep 0) Set source-{0}-{1} to state: {2}", Source.PluginId, Source.PluginVersion, BuildState.BUILDING.ToString()), this, Logtype.Info);

            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerCertificate = ServerCertificate;
            client.ServerAddress = Config.GetConfigEntry("ServerAddress");
            client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
            client.Connect();
            client.Login(Config.GetConfigEntry("Username"), Config.GetConfigEntry("Password"));

            try
            {
                //get source for update
                DataModificationOrRequestResult result = client.GetSource(Source.PluginId, Source.PluginVersion);
                if (!result.Success)
                {
                    Logger.LogText(string.Format("(Buildstep 0) Could not get source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, result.Message), this, Logtype.Error);
                    return false;
                }
                Source source = (Source)result.DataObject;
                //update that source to building state
                source.BuildState = BuildState.BUILDING.ToString();
                source.BuildLog = string.Format("Buildserver started build worker at {0}", DateTime.Now);
                result = client.UpdateSource(source);
                if (!result.Success)
                {
                    Logger.LogText(string.Format("(Buildstep 0) Could not set source-{0}-{1} to state {2}: {3}", Source.PluginId, Source.PluginVersion, BuildState.BUILDING, result.Message), this, Logtype.Error);
                    return false;
                }

                Logger.LogText(string.Format("(Buildstep 0) Source-{0}-{1} is now in state: {2}", Source.PluginId, Source.PluginVersion, BuildState.BUILDING.ToString()), this, Logtype.Info);
                return true;
            }
            finally
            {
                client.Disconnect();
            }
        }

        /// <summary>
        /// 1) Checks, if the BUILD_FOLDER exists, if not it creats it
        /// Also creates SOURCE_FILE_NAME-PluginId-PluginVersion folder for the actual build
        /// </summary>
        /// <returns></returns>
        private bool CreateBuildFolder()
        {
            lock (BUILD_FOLDER)
            {
                if (!Directory.Exists(BUILD_FOLDER))
                {
                    Directory.CreateDirectory(BUILD_FOLDER);
                    Logger.LogText(string.Format("(Buildstep 1) Created build folder: {0}", BUILD_FOLDER), this, Logtype.Info);                    
                }
            }

            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            if (!Directory.Exists(buildfoldername))
            {                
                Directory.CreateDirectory(buildfoldername);
                Logger.LogText(string.Format("(Buildstep 1) Created build folder for source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, buildfoldername), this, Logtype.Info);
                return true;
            }
            else
            {
                Logger.LogText(string.Format("(Buildstep 1) Folder for source-{0}-{1} already exists. Maybe because of faulty previous build. Abort now", Source.PluginId, Source.PluginVersion), this, Logtype.Error);
                return false;
            }
        }

        /// <summary>
        ///  2) Worker creates folder structure in plugin folder
        ///  --> \plugin           contains source
        ///  --> \build_output     contains builded plugins
        ///  --> build_plugin.xml  contains msbuild script
        ///  
        ///      note: Also makes references to
        ///  --> signing certificate
        ///  --> custom build tasks
        ///  --> ct2 libraries (CrypCore.dll and CrypPluginBase.dll)
        /// </summary>
        /// <returns></returns>
        private bool CreateBuildSubFoldersAndFiles()
        {
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            //1. create plugin folder
            Directory.CreateDirectory(buildfoldername + @"\plugin");
            Logger.LogText(string.Format("(Buildstep 2) Created plugin folder for source-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);

            //2. create build_output folder
            Directory.CreateDirectory(buildfoldername + @"\build_output");
            Logger.LogText(string.Format("(Buildstep 2) Created build_output folder for source-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);

            //3. create build_plugin.xml
            using (Stream stream = new FileStream(buildfoldername + @"\build_plugin.xml", FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("<Project DefaultTargets=\"BuildCrypPlugin\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
                    writer.WriteLine("  <Import Project=\"..\\..\\CustomBuildTasks\\CustomBuildTasks.Targets\"/>");
                    writer.WriteLine("  <Target Name=\"BuildCrypPlugin\">");
                    writer.WriteLine("    <MSBuild Projects=\"$PROJECT$\" Targets=\"Build\" />");
                    writer.WriteLine("  </Target>");
                    writer.WriteLine("</Project>");
                }
            }
            Logger.LogText(string.Format("(Buildstep 2) Created build_plugin.xml for source-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            return true;
        }

        /// <summary>
        /// 3) Worker downloads zip file and extracts complete content into "plugin" folder
        /// </summary>
        /// <returns></returns>
        private bool DownloadZipFile()
        {
            Logger.LogText(string.Format("(Buildstep 3) Start downloading source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerCertificate = ServerCertificate;
            client.ServerAddress = Config.GetConfigEntry("ServerAddress");
            client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
            client.Connect();
            client.Login(Config.GetConfigEntry("Username"), Config.GetConfigEntry("Password"));

            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            DateTime startTime = DateTime.Now;
            bool stop = false;
            DataModificationOrRequestResult result = client.DownloadSourceZipFile(Source, string.Format(buildfoldername + @"\plugin\source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), ref stop);
            client.Disconnect();

            if (result.Success)
            {
                Logger.LogText(string.Format("(Buildstep 3) Downloaded source-{0}-{1}.zip in {2}", Source.PluginId, Source.PluginVersion, DateTime.Now.Subtract(startTime)), this, Logtype.Info);
                return true;
            }
            else
            {
                Logger.LogText(string.Format("(Buildstep 3) Download of source-{0}-{1}.zip failed. Message was: {2}", Source.PluginId, Source.PluginVersion, result.Message), this, Logtype.Error);
                return false;
            }            
        }

        /// <summary>
        ///  4) Worker extracts zip file
        /// </summary>
        /// <returns></returns>
        private bool ExtractZipFile()
        {
            Logger.LogText(string.Format("(Buildstep 4) Start extracting source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;
            ZipFile.ExtractToDirectory(buildfoldername + string.Format(@"\plugin\source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), buildfoldername + @"\plugin\");
            File.Delete(buildfoldername + string.Format(@"\plugin\source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion));
            Logger.LogText(string.Format("(Buildstep 4) Finished extracting source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            return true;
        }

        /// <summary>
        ///  5) Worker searches for exactly one csproj file in the root folder, i.e. "plugin"        
        ///  --> if it finds 0 or more than 1, the build Worker fails at this point
        /// </summary>
        /// <returns></returns>
        private bool SearchCSProjFile()
        {
            int counter = 0;
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            //Search for the csproj file in folder structure
            SearchDir(buildfoldername, ref counter, "csproj");

            //We only allow exactly one csproj file per Source
            if (counter == 0)
            {
                Logger.LogText(string.Format("(Buildstep 5) Source-{0}-{1} does not contain any csproj file", Source.PluginId, Source.PluginVersion), this, Logtype.Error);
                return false;
            }
            if (counter > 1)
            {
                Logger.LogText(string.Format("(Buildstep 5) source-{0}-{1} contains more than one csproj file", Source.PluginId, Source.PluginVersion), this, Logtype.Error);
            }

            Logger.LogText(string.Format("(Buildstep 5) Found csproj file in source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, CSProjFileName), this, Logtype.Info);
            return true;
        }

        /// <summary>
        /// This method walks through the dedicated dir and its subdirs and searches for csproj files
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="counter"></param>
        private void SearchDir(string dir, ref int counter, string fileEnding, bool recursive = true)
        {            
            string[] files = Directory.GetFiles(dir);
            foreach (string name in files)
            {
                if (name.ToLower().EndsWith(fileEnding))
                {
                    CSProjFileName = name;
                    counter++;
                }
            }
            string[] dirs = Directory.GetDirectories(dir);
            if (recursive)
            {
                foreach (string dir2 in dirs)
                {
                    SearchDir(dir2, ref counter, fileEnding);
                }
            }
        }

        /// <summary>
        ///  6) Worker modifies csproj file
        ///  --> changes references to CrypPluginBase to correct path (hint: dont forget <private>false</private>)
        ///  --> changes output folder of "Release" target to "build_output" folder
        /// </summary>
        /// <returns></returns>
        private bool ModifyCSProjFile()
        {            
            //Step 0: load csproj xml file
            XDocument csprojXDocument = XDocument.Load(CSProjFileName);

            XNamespace defaultNamespace = csprojXDocument.Root.GetDefaultNamespace();

            //Step 1: change output path (of Release) to correct path
            IEnumerable<XElement> outputPaths = csprojXDocument.Descendants();

            bool changedOutputPath = false;
            foreach (XElement outputPath in outputPaths)
            {
                if (outputPath.Name.LocalName.ToLower().Equals("outputpath") && outputPath.Value.ToLower().Contains("release"))
                {                    
                    outputPath.Value = @"..\build_output\";
                    changedOutputPath = true;
                    Logger.LogText(@"(Buildstep 6) Changed output path of Release target", this, Logtype.Info);                    
                }
            }
            
            //Step 2: change project reference to correct path of needed dlls                                   
            IEnumerable<XElement> projectReferences = csprojXDocument.Descendants();
            
            foreach (XElement projectReference in projectReferences)
            {
                XAttribute includeAttribute = projectReference.Attribute("Include");
                
                //change CrypPluginBase
                if (projectReference.Name.LocalName.ToLower().Equals("projectreference") && 
                    includeAttribute != null && !string.IsNullOrEmpty(includeAttribute.Value) && 
                    includeAttribute.Value.ToLower().Contains("cryppluginbase"))
                {
                    //change include attribute value
                    includeAttribute.Value = @"CrypPluginBase";                    

                    //change/add private element
                    XElement privateElement = projectReference.Element("Private");
                    if (privateElement != null)
                    {
                        privateElement.Value = "false";                        
                    }
                    else
                    {
                        privateElement = new XElement(defaultNamespace + "Private");
                        privateElement.Value = "false";
                        projectReference.Add(privateElement);
                    }

                    //Change type of reference
                    projectReference.Name = defaultNamespace + "Reference";                    

                    //Add hint path to CrypPluginBase                    
                    XElement hintPathElement = new XElement(defaultNamespace + "HintPath");
                    hintPathElement.Value = @"..\..\..\CT2_Libraries\CrypPluginBase.dll";
                    projectReference.Add(hintPathElement);

                    Logger.LogText("(Buildstep 6) Changed reference to CrypPluginBase", this, Logtype.Info);
                }

                //change CrypCore
                if (projectReference.Name.LocalName.ToLower().Equals("projectreference") &&
                    includeAttribute != null && !string.IsNullOrEmpty(includeAttribute.Value) &&
                    includeAttribute.Value.ToLower().Contains("crypcore"))
                {
                    //change include attribute value
                    includeAttribute.Value = @"CrypCore";

                    //change/add private element
                    XElement privateElement = projectReference.Element("Private");
                    if (privateElement != null)
                    {
                        privateElement.Value = "false";
                    }
                    else
                    {
                        privateElement = new XElement(defaultNamespace + "Private");
                        privateElement.Value = "false";
                        projectReference.Add(privateElement);
                    }

                    //Change type of reference
                    projectReference.Name = defaultNamespace + "Reference";

                    //Add hint path to CrypPluginBase                    
                    XElement hintPathElement = new XElement(defaultNamespace + "HintPath");
                    hintPathElement.Value = @"..\..\..\CT2_Libraries\CrypCore.dll";
                    projectReference.Add(hintPathElement);

                    Logger.LogText("(Buildstep 6) Changed reference to CrypCore", this, Logtype.Info);
                }

                //change CrypWin
                if (projectReference.Name.LocalName.ToLower().Equals("projectreference") &&
                    includeAttribute != null && !string.IsNullOrEmpty(includeAttribute.Value) &&
                    includeAttribute.Value.ToLower().Contains("crypwin"))
                {
                    //change include attribute value
                    includeAttribute.Value = @"CrypWin";

                    //change/add private element
                    XElement privateElement = projectReference.Element("Private");
                    if (privateElement != null)
                    {
                        privateElement.Value = "false";
                    }
                    else
                    {
                        privateElement = new XElement(defaultNamespace + "Private");
                        privateElement.Value = "false";
                        projectReference.Add(privateElement);
                    }

                    //Change type of reference
                    projectReference.Name = defaultNamespace + "Reference";

                    //Add hint path to CrypWin                    
                    XElement hintPathElement = new XElement(defaultNamespace + "HintPath");
                    hintPathElement.Value = @"..\..\..\CT2_Libraries\CrypWin.exe";
                    projectReference.Add(hintPathElement);

                    Logger.LogText("(Buildstep 6) Changed reference to CrypWin.exe", this, Logtype.Info);
                }

                //change OnlineDocumentationGenerator
                if (projectReference.Name.LocalName.ToLower().Equals("projectreference") &&
                    includeAttribute != null && !string.IsNullOrEmpty(includeAttribute.Value) &&
                    includeAttribute.Value.ToLower().Contains("onlinedocumentationgenerator"))
                {
                    //change include attribute value
                    includeAttribute.Value = @"OnlineDocumentationGenerator";

                    //change/add private element
                    XElement privateElement = projectReference.Element("Private");
                    if (privateElement != null)
                    {
                        privateElement.Value = "false";
                    }
                    else
                    {
                        privateElement = new XElement(defaultNamespace + "Private");
                        privateElement.Value = "false";
                        projectReference.Add(privateElement);
                    }

                    //Change type of reference
                    projectReference.Name = defaultNamespace + "Reference";

                    //Add hint path to OnlineDocumentationGenerator                    
                    XElement hintPathElement = new XElement(defaultNamespace + "HintPath");
                    hintPathElement.Value = @"..\..\..\CT2_Libraries\OnlineDocumentationGenerator.dll";
                    projectReference.Add(hintPathElement);

                    Logger.LogText("(Buildstep 6) Changed reference to OnlineDocumentationGenerator", this, Logtype.Info);
                }

                //change WorkspaceManagerModel
                if (projectReference.Name.LocalName.ToLower().Equals("projectreference") &&
                    includeAttribute != null && !string.IsNullOrEmpty(includeAttribute.Value) &&
                    includeAttribute.Value.ToLower().Contains("workspacemanagermodel"))
                {
                    //change include attribute value
                    includeAttribute.Value = @"WorkspaceManagerModel";

                    //change/add private element
                    XElement privateElement = projectReference.Element("Private");
                    if (privateElement != null)
                    {
                        privateElement.Value = "false";
                    }
                    else
                    {
                        privateElement = new XElement(defaultNamespace + "Private");
                        privateElement.Value = "false";
                        projectReference.Add(privateElement);
                    }

                    //Change type of reference
                    projectReference.Name = defaultNamespace + "Reference";

                    //Add hint path to OnlineDocumentationGenerator                    
                    XElement hintPathElement = new XElement(defaultNamespace + "HintPath");
                    hintPathElement.Value = @"..\..\..\CT2_Libraries\WorkspaceManagerModel.dll";
                    projectReference.Add(hintPathElement);

                    Logger.LogText("(Buildstep 6) Changed reference to WorkspaceManagerModel", this, Logtype.Info);
                }

                //change Newtonsoft.Json
                if (includeAttribute != null && !string.IsNullOrEmpty(includeAttribute.Value) && 
                    includeAttribute.Value.ToLower().Contains("newtonsoft.json") &&
                    projectReference.Elements().ElementAt(0).Name.LocalName.ToLower().Equals("hintpath"))
                {
                    projectReference.Elements().ElementAt(0).Value = @"..\..\..\CT2_Libraries\Newtonsoft.Json.dll";
                    Logger.LogText("(Buildstep 6) Changed reference to Newtonsoft.Json", this, Logtype.Info);
                }

                //change Newtonsoft.Json
                if (includeAttribute != null && !string.IsNullOrEmpty(includeAttribute.Value) &&
                    includeAttribute.Value.ToLower().Contains("wpftoolkit.extended") &&
                    projectReference.Elements().ElementAt(0).Name.LocalName.ToLower().Equals("hintpath"))
                {
                    projectReference.Elements().ElementAt(0).Value = @"..\..\..\CT2_Libraries\WPFToolkit.Extended.dll";
                    Logger.LogText("(Buildstep 6) Changed reference to WPFToolkit.Extended", this, Logtype.Info);
                }
            }

            if (!changedOutputPath)
            {
                Logger.LogText("(Buildstep 6) Did not find Release target to change output path of build", this, Logtype.Error);
                return false;
            }
                
            csprojXDocument.Save(CSProjFileName,SaveOptions.OmitDuplicateNamespaces);
            Logger.LogText(string.Format("(Buildstep 6) Wrote changes to {0}", CSProjFileName), this, Logtype.Info);

            return true;
        }

        /// <summary>
        ///  7) Worker modifies msbuild script
        ///  --> change name of target project to name of csproj file found in "plugin" folder
        /// </summary>
        /// <returns></returns>
        private bool ModifyMsBuildScript()
        {
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;
            string script = File.ReadAllText(buildfoldername + @"\build_plugin.xml", UTF8Encoding.UTF8);
            //here, we remove everything from the path and create \\plugin\\xyz.csproj (where xyz is the csproj filename)
            string[] paths = CSProjFileName.Split('\\');
            string csprojfilepath = "plugin\\" + paths[paths.Length - 1];
            script = script.Replace("$PROJECT$", csprojfilepath);
            File.WriteAllText(buildfoldername + @"\build_plugin.xml", script);
            Logger.LogText(string.Format("(Buildstep 7) Modified build_plugin.xml: changed $PROJECT$ to {0}", csprojfilepath), this, Logtype.Info);
            return true;
        }

        /// <summary>
        ///  8) Worker starts "msbuild.exe" (hint: set correct password for signtool to allow it opening signing certificate)
        ///  --> msbuild compiles the plugin
        ///  --> signtool is also started and signs the builded assembly file
        /// </summary>
        /// <returns></returns>
        private bool BuildPlugin()
        {
            Logger.LogText(string.Format("(Buildstep 8) Starting actual build of source-{0}-{1} using msbuild.exe", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            ProcessStartInfo info = new ProcessStartInfo("msbuild.exe");
            info.Arguments = buildfoldername + string.Format("\\build_plugin.xml /p:Platform={0} /p:Configuration=Release /p:CertificatePfxFile=\"{1}\" /p:CertificatePassword=\"{2}\"", BUILD_TARGET, SigningCertificatePfxFile, SigningCertificatePassword);
            info.CreateNoWindow = false;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            Process process = new Process();
            process.StartInfo = info;            
            process.OutputDataReceived += CaptureOutput_msbuild;
            process.ErrorDataReceived += CaptureError_msbuild;
            msbuild_Log = new StringBuilder();
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Logger.LogText(string.Format("(Buildstep 8) Output of msbuild for source-{0}-{1}:\r\n{2}", Source.PluginId, Source.PluginVersion, msbuild_Log.ToString()), this, process.ExitCode == 0 ? Logtype.Info : Logtype.Error);

            if (process.ExitCode != 0)
            {
                Logger.LogText(string.Format("(Buildstep 8) Build of source-{0}-{1} failed", Source.PluginId, Source.PluginVersion), this, Logtype.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Redirects outputstream to msbuild_Log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureOutput_msbuild(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                //we remove absolute directory infos from the messages since these are "confidential"
                string message = e.Data.Replace(Directory.GetCurrentDirectory(), "");
                msbuild_Log.AppendLine(message);
            }
        }

        /// <summary>
        /// Redirects errorstream to msbuild_Log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureError_msbuild(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                //we remove absolute directory infos from the messages since these are "confidential"
                string message = e.Data.Replace(Directory.GetCurrentDirectory(), "");
                msbuild_Log.AppendLine(message);
            }
        }

        /// <summary>
        ///  9) Worker checks, if assembly file exists in "build_output" (if not => ERROR)
        /// </summary>
        /// <returns></returns>
        private bool CheckBuild()
        {
            int counter=0;
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion + @"\build_output";
            SearchDir(buildfoldername, ref counter, "dll", false);

            if (counter == 0)
            {
                Logger.LogText(string.Format("(Buildstep 9) Did not find any dll-file in build_output folder after building source-{0}-{1}. Abort now", Source.PluginId, Source.PluginVersion), this, Logtype.Error);
                return false;
            } 
            Logger.LogText(string.Format("(Buildstep 9) Found {0} dll-file in build_output folder after building source-{1}-{2}", counter, Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            return true;
        }

        /// <summary>
        ///  10) Worker calls signtool to sign the created assembly/assemblies
        /// </summary>
        /// <returns></returns>
        private bool SignPlugin()
        {
            if (string.IsNullOrEmpty(SigningCertificatePfxFile))
            {
                Logger.LogText("(Buildstep 10) No signing certificate given. Omit the signing build step now", this, Logtype.Warning);
                return true;
            }
            Logger.LogText("(Buildstep 10) Signing created assembly/assemblies now", this, Logtype.Info);

            string path = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion + @"\build_output\";
            StringBuilder filenames = new StringBuilder();
            foreach (var filename in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
            {
                filenames.Append(" ");
                filenames.Append(filename);
            }

            ProcessStartInfo info = new ProcessStartInfo("signtool.exe");
            info.Arguments = string.Format("sign /f {0} /p {1} /t {2} {3}", SigningCertificatePfxFile, SigningCertificatePassword, Config.GetConfigEntry("BuildServer_TimeStampServer"), filenames.ToString());
            info.CreateNoWindow = false;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            Process process = new Process();
            process.StartInfo = info;
            process.OutputDataReceived += CaptureOutput_signtool;
            process.ErrorDataReceived += CaptureError_signtool;
            signtool_Log = new StringBuilder();
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Logger.LogText(string.Format("(Buildstep 10) Output of signtool:\r\n{0}",signtool_Log.ToString()), this, process.ExitCode == 0 ? Logtype.Info : Logtype.Error);

            if (process.ExitCode != 0)
            {
                Logger.LogText("(Buildstep 10) Signing failed", this, Logtype.Error);
                return false;
            }
            Logger.LogText("(Buildstep 10) Created assembly/assemblies signed", this, Logtype.Info);
            return true;
        }

        /// <summary>
        /// Redirects outputstream to signtool_Log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureOutput_signtool(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                //we remove absolute directory infos from the messages since these are "confidential"
                string message = e.Data.Replace(Directory.GetCurrentDirectory(), "");
                signtool_Log.AppendLine(message);
            }
        }

        /// <summary>
        /// Redirects errorstream to signtool_Log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaptureError_signtool(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                //we remove absolute directory infos from the messages since these are "confidential"
                string message = e.Data.Replace(Directory.GetCurrentDirectory(), "");
                signtool_Log.AppendLine(message);
            }
        }


        /// <summary>
        ///  11) Create meta file containing meta information
        /// </summary>
        /// <returns></returns>
        private bool CreateMetaFile()
        {
            Logger.LogText(string.Format("(Buildstep 11) Start creating pluginmetainfo.xml for assembly-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);

            string metafilename = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion + @"\build_output\pluginmetainfo.xml";

            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerCertificate = ServerCertificate;
            client.ServerAddress = Config.GetConfigEntry("ServerAddress");
            client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
            client.Connect();
            client.Login(Config.GetConfigEntry("Username"), Config.GetConfigEntry("Password"));

            DataModificationOrRequestResult result = client.GetPlugin(Source.PluginId);
            Plugin plugin = (Plugin)result.DataObject;

            client.Disconnect();

            using (Stream stream = new FileStream(metafilename, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("<xml>");
                    writer.WriteLine("	<Plugin>");
                    writer.WriteLine("		<Id>{0}</Id>", Source.PluginId);
                    writer.WriteLine("		<Version>{0}</Version>", Source.PluginVersion);
                    writer.WriteLine("		<BuildVersion>{0}</BuildVersion>", Source.BuildVersion + 1);
                    writer.WriteLine("		<Name>{0}</Name>", plugin.Name);
                    writer.WriteLine("		<Icon>{0}</Icon>", System.Convert.ToBase64String(plugin.Icon));
                    writer.WriteLine("	</Plugin>");
                    writer.WriteLine("</xml>");
                }
            }

            Logger.LogText(string.Format("(Buildstep 11) Created pluginmetainfo.xml for assembly-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            return true;
        }

        /// <summary>
        ///  12)  Worker zips everything located in "build_output" -- this also includes "de/ru" etc subfolders of the plugin
        ///  --> zip name is "assembly-1-1.zip, = assembly-PluginId-SourceId")
        /// </summary>
        /// <returns></returns>
        private bool CreateAssemblyZip()
        {           
            Logger.LogText(string.Format("(Buildstep 12) Start creating assembly-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);

            //remove pdb-files since these are not needed in the zip file
            DirectoryInfo DirectoryInfo = new DirectoryInfo(BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion + @"\" + "build_output\\");
            FileInfo[] pdbfiles = DirectoryInfo.GetFiles("*.pdb");
            for (int i = 0; i < pdbfiles.Length; i++)
            {
                pdbfiles[i].Delete();
            }                                              
            //create actual zipfile
            string zipfile_path_and_name = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion + @"\assembly-" + Source.PluginId + "-" + Source.PluginVersion + ".zip";
            ZipFile.CreateFromDirectory(BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion + @"\" + "build_output\\", zipfile_path_and_name, CompressionLevel.Optimal, false);

            Logger.LogText(string.Format("(Buildstep 12) Created assembly-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            return true;
        }

        /// <summary>
        /// 13) Worker uploads assembly zip file to CrypToolStore Server, and also updates source data in database
        /// </summary>
        /// <returns></returns>
        private bool UploadAssemblyZip()
        {
            Logger.LogText(string.Format("(Buildstep 13) Start uploading assembly zipfile for source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerCertificate = ServerCertificate;
            client.ServerAddress = Config.GetConfigEntry("ServerAddress");
            client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
            client.Connect();
            client.Login(Config.GetConfigEntry("Username"), Config.GetConfigEntry("Password"));

            string zipfile_path_and_name = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion + @"\assembly-" + Source.PluginId + "-" + Source.PluginVersion + ".zip";
            

            DateTime startTime = DateTime.Now;
            bool stop = false;
            DataModificationOrRequestResult result = client.UploadAssemblyZipFile(Source, zipfile_path_and_name, ref stop);
            client.Disconnect();

            if (result.Success)
            {
                Logger.LogText(string.Format("(Buildstep 13) Uploaded assembly zipfile of source-{0}-{1}.zip in {2}", Source.PluginId, Source.PluginVersion, DateTime.Now.Subtract(startTime)), this, Logtype.Info);
                return true;
            }
            else
            {
                Logger.LogText(string.Format("(Buildstep 13) Upload of assembly zipfile of source-{0}-{1}.zip failed. Message was: {2}", Source.PluginId, Source.PluginVersion, result.Message), this, Logtype.Error);
                return false;
            }            
        }

        /// <summary>
        /// 14) Worker cleans up by deleting build folder (also in case of an error)
        /// </summary>
        private void CleanUp()
        {
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;
            if (Directory.Exists(buildfoldername))
            {
                Directory.Delete(buildfoldername, true);
                Logger.LogText(string.Format("(Buildstep 14) Deleted build folder for source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, buildfoldername), this, Logtype.Info);                
            }            
        }

        /// <summary>
        ///  15) Set state of source in database to BUILDED or ERROR
        ///      also upload build_log to CrypToolStore database
        /// </summary>
        private void SetFinalBuildStateAndUploadBuildlog(bool buildError)
        {
            Logger.LogText(string.Format("(Buildstep 15) Set final build state of source-{0}-{1} and upload build log", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerCertificate = ServerCertificate;
            client.ServerAddress = Config.GetConfigEntry("ServerAddress");
            client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
            client.Connect();
            client.Login(Config.GetConfigEntry("Username"), Config.GetConfigEntry("Password"));

            DataModificationOrRequestResult result = client.GetSource(Source.PluginId, Source.PluginVersion);
            Source source = (Source)result.DataObject;

            if (buildError)
            {
                Logger.LogText(string.Format("(Buildstep 15) Set state of source-{0}-{1} to ERROR", Source.PluginId, Source.PluginVersion), this, Logtype.Error);
                source.BuildState = BuildState.ERROR.ToString();
            }
            else
            {
                //increment build version only if there was not error
                source.BuildVersion++;

                Logger.LogText(string.Format("(Buildstep 15) Set state of source-{0}-{1} to SUCCESS", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
                source.BuildState = BuildState.SUCCESS.ToString();
            }

            //upload complete log to database
            source.BuildLog = Logger.ToString();

            result = client.UpdateSource(source);

            client.Disconnect();

            if (result.Success)
            {
                Logger.LogText(string.Format("(Buildstep 15) Uploaded build log of source-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            }
            else
            {
                Logger.LogText(string.Format("(Buildstep 15) Setting final build state of source-{0}-{1} and upload build log failed. Message was: {2}", Source.PluginId, Source.PluginVersion, result.Message), this, Logtype.Error);
            }           
        }

        /// <summary>
        /// 16) Send email to developer
        /// </summary>
        private void SendEmailToDeveloper(bool buildError)
        {
            Logger.LogText(string.Format("(Buildstep 16) Send email to developer of source-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            //Get email of developer
            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerCertificate = ServerCertificate;
            client.ServerAddress = Config.GetConfigEntry("ServerAddress");
            client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
            client.Connect();
            client.Login(Config.GetConfigEntry("Username"), Config.GetConfigEntry("Password"));
            Plugin plugin = (Plugin)client.GetPlugin(Source.PluginId).DataObject;
            Developer developer = (Developer)client.GetDeveloper(plugin.Username).DataObject;
            client.Disconnect();

            //check, if email is valid
            if (string.IsNullOrEmpty(developer.Email) || !IsValidEmail(developer.Email))
            {
                Logger.LogText(string.Format("(Buildstep 16) Cannot send build email to {0} since we have no valid email address (={1}) of this developer!", plugin.Username, developer.Email), this, Logtype.Warning);
                return;
            }
            MailClient mailClient = new MailClient(Config.GetConfigEntry("EmailServerAddress"), int.Parse(Config.GetConfigEntry("EmailServerPort")));

            MailAddress from = new MailAddress(Config.GetConfigEntry("BuildServer_MailAddress"),Config.GetConfigEntry("BuildServer_MailName"));
            MailAddress to = new MailAddress(developer.Email, developer.Username);

            string subject;
            StringBuilder bodyBuilder = new StringBuilder(); //yes, its a string builder for a mail body... thus, it is a body builder :-)

            if (buildError)
            {
                subject = string.Format("[CrypToolStore Build System] Build Error occured while building your Source-{0}-{1}", Source.PluginId, Source.PluginVersion);
                bodyBuilder.AppendLine(string.Format("Dear {0},", plugin.Username));
                bodyBuilder.AppendLine();
                bodyBuilder.AppendLine(string.Format("The build of Source-{0}-{1} has failed.", Source.PluginId, Source.PluginVersion));
                bodyBuilder.AppendLine("Please see attached buildlog.txt for additional information.");
                bodyBuilder.AppendLine();
                bodyBuilder.AppendLine("Kind regards,");
                bodyBuilder.AppendLine("The CrypToolStore Build System");             
            }
            else
            {
                subject = string.Format("[CrypToolStore Build System] Build of your Source-{0}-{1} successfully finished", Source.PluginId, Source.PluginVersion);
                bodyBuilder.AppendLine(string.Format("Dear {0},", plugin.Username));
                bodyBuilder.AppendLine();
                bodyBuilder.AppendLine(string.Format("The build of Source-{0}-{1} was a success.", Source.PluginId, Source.PluginVersion));
                bodyBuilder.AppendLine("Please see attached buildlog.txt for additional information.");
                bodyBuilder.AppendLine();
                bodyBuilder.AppendLine("Kind regards,");
                bodyBuilder.AppendLine("The CrypToolStore Build System");
            }

            mailClient.SendEmail(from, to, subject, bodyBuilder.ToString(), "buildlog.txt", Logger.ToString());
            Logger.LogText(string.Format("(Buildstep 16) Successfully sent email to developer of Source-{0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);

            //send an email to buildserver admin
            if (!string.IsNullOrEmpty(Config.GetConfigEntry("BuildServer_AdminEmailAddress")) && !string.IsNullOrEmpty(Config.GetConfigEntry("BuildServer_AdminEmailAddress")))
            {
                to = new MailAddress(Config.GetConfigEntry("BuildServer_AdminEmailAddress"), Config.GetConfigEntry("BuildServer_AdminEmailAddress"));
                mailClient.SendEmail(from, to, subject, bodyBuilder.ToString(), "buildlog.txt", Logger.ToString());
                Logger.LogText(string.Format("(Buildstep 16) Successfully sent email to builderserver admin"), this, Logtype.Info);
            }
        }

        /// <summary>
        /// Checks, if the given emailaddress is valid
        /// </summary>
        /// <param name="emailaddress"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}