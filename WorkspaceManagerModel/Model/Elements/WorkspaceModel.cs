/*                              
   Copyright 2010 Nils Kopal

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Editor;
using WorkspaceManager.Execution;
using WorkspaceManager.Model.Tools;
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.Model
{
    public enum ConversionLevel
    {
        Red,
        Yellow,
        Green,
        NA
    };

    /// <summary>
    /// Class to represent our Workspace
    /// </summary>
    [Serializable]
    public class WorkspaceModel : VisualElementModel, IDisposable
    {
        public double Zoom
        {
            get;
            set;
        }

        /// <summary>
        /// The executing editor
        /// </summary>
        [NonSerialized]
        private IEditor myEditor;
        public IEditor MyEditor
        {
            get => myEditor;
            set => myEditor = value;

        }

        /// <summary>
        /// My ExecutionEngine which currently executes me
        /// </summary>
        [NonSerialized]
        internal ExecutionEngine ExecutionEngine;

        /// <summary>
        /// Create a new WorkspaceModel
        /// </summary>
        public WorkspaceModel()
        {
            AllPluginModels = new List<PluginModel>();
            AllConnectionModels = new List<ConnectionModel>();
            AllConnectorModels = new List<ConnectorModel>();
            AllImageModels = new List<ImageModel>();
            AllTextModels = new List<TextModel>();
            UndoRedoManager = new UndoRedoManager(this);
        }

        [NonSerialized]
        public UndoRedoManager UndoRedoManager;

        /// <summary>
        /// Is true, if there are unsaved changes in this WorkspaceModel
        /// (UndoRedo manager can undo or settings of a Plugin have changes)
        /// </summary>
        public bool HasChanges
        {
            get
            {
                if (UndoRedoManager.HasUnsavedChanges())
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Tell this model if its executed or not
        /// </summary>
        [NonSerialized]
        private bool beingExecuted = false;
        internal bool IsBeingExecuted
        {
            get => beingExecuted;
            set => beingExecuted = value;
        }

        /// <summary>
        /// All PluginModels of our Workspace Model
        /// </summary>
        internal List<PluginModel> AllPluginModels;

        /// <summary>
        /// Get all PluginModels of our Workspace Model (ordered by ZIndex)
        /// </summary>
        public ReadOnlyCollection<PluginModel> GetAllPluginModels()
        {
            return AllPluginModels.OrderBy(x => x.ZIndex).ToList().AsReadOnly();
        }

        /// <summary>
        /// All Connector Models of our Workspace Model
        /// </summary>
        internal List<ConnectorModel> AllConnectorModels;

        /// <summary>
        /// Get all Connector Models of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<ConnectorModel> GetAllConnectorModels()
        {
            return AllConnectorModels.AsReadOnly();
        }

        /// <summary>
        /// All ConnectionModels of our Workspace Model
        /// </summary>
        internal List<ConnectionModel> AllConnectionModels;

        /// <summary>
        /// Get all ConnectionModels of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<ConnectionModel> GetAllConnectionModels()
        {
            return AllConnectionModels.AsReadOnly();
        }

        /// <summary>
        /// All ImageModels of our Workspace Model
        /// </summary>
        internal List<ImageModel> AllImageModels;

        /// <summary>
        /// Get all ImageModels of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<ImageModel> GetAllImageModels()
        {
            return AllImageModels.AsReadOnly();
        }

        /// <summary>
        /// All TextModels of our Workspace Model
        /// </summary>
        internal List<TextModel> AllTextModels;

        /// <summary>
        /// Get all TextModels of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<TextModel> GetAllTextModels()
        {
            return AllTextModels.AsReadOnly();
        }

        /// <summary>
        /// Creates a new PluginModel belonging to this WorkspaceModel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        internal PluginModel newPluginModel(Point position, double width, double height, Type pluginType)
        {
            PluginModel pluginModel = new PluginModel
            {
                WorkspaceModel = this,
                Position = position,
                PluginType = pluginType,
                Name = pluginType.GetPluginInfoAttribute().Caption,
                RepeatStart = false
            };
            pluginModel.generateConnectors();
            pluginModel.Plugin.OnGuiLogNotificationOccured += GuiLogMessage;
            pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;
            pluginModel.Plugin.OnPluginStatusChanged += pluginModel.PluginStatusChanged;
            pluginModel.Plugin.Initialize();
            if (pluginModel.Plugin.Settings != null)
            {
                pluginModel.Plugin.Settings.PropertyChanged += pluginModel.SettingsPropertyChanged;
            }
            AllPluginModels.Add(pluginModel);
            pluginModel.StoreAllDefaultInputConnectorValues();
            return pluginModel;
        }

        /// <summary>
        /// Add an existing PluginModel to this WorkspaceModel
        /// </summary>
        /// <param name="pluginModel"></param>
        /// <returns></returns>
        public void addPluginModel(PluginModel pluginModel)
        {
            pluginModel.WorkspaceModel = this;
            AllPluginModels.Add(pluginModel);
            foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
            {
                connectorModel.WorkspaceModel = this;
                foreach (ConnectionModel connection in connectorModel.InputConnections)
                {
                    connection.WorkspaceModel = this;
                    AllConnectionModels.Add(connection);
                }
                foreach (ConnectionModel connection in connectorModel.OutputConnections)
                {
                    connection.WorkspaceModel = this;
                    AllConnectionModels.Add(connection);
                }
                AllConnectorModels.Add(connectorModel);
            }
            foreach (ConnectorModel connectorModel in pluginModel.OutputConnectors)
            {
                connectorModel.WorkspaceModel = this;
                foreach (ConnectionModel connection in connectorModel.OutputConnections)
                {
                    connection.WorkspaceModel = this;
                    AllConnectionModels.Add(connection);
                }
                foreach (ConnectionModel connection in connectorModel.InputConnections)
                {
                    connection.WorkspaceModel = this;
                    AllConnectionModels.Add(connection);
                }
                AllConnectorModels.Add(connectorModel);
            }
            pluginModel.Plugin.Initialize();
            pluginModel.StoreAllDefaultInputConnectorValues();
        }

        /// <summary>
        /// Creates a new Connection starting at "from"-Connector going to "to"-Connector with
        /// the given connectionType
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="connectionType"></param>
        /// <returns></returns>
        internal ConnectionModel newConnectionModel(ConnectorModel from, ConnectorModel to, Type connectionType)
        {
            ConnectionModel connectionModel = new ConnectionModel
            {
                WorkspaceModel = this,
                From = from,
                To = to
            };
            from.OutputConnections.Add(connectionModel);
            to.InputConnections.Add(connectionModel);
            connectionModel.ConnectionType = connectionType;

            //If we connect two IControls we have to set data directly:
            if (from.IControl && to.IControl)
            {
                object data = null;
                //Get IControl data from "to"                
                data = to.PluginModel.Plugin.GetType().GetProperty(to.PropertyName).GetValue(to.PluginModel.Plugin, null);

                //Set IControl data                
                PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                propertyInfo.SetValue(from.PluginModel.Plugin, data, null);

            }

            //call events on PluginModels to show that they have new connections
            from.PluginModel.OnConnectorPlugstateChanged(from, PlugState.Plugged);
            to.PluginModel.OnConnectorPlugstateChanged(to, PlugState.Plugged);

            AllConnectionModels.Add(connectionModel);
            return connectionModel;
        }

        /// <summary>
        /// Add an existing ConnectionModel to this WorkspaceModel
        /// </summary>
        /// <param name="connectionModel"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void addConnectionModel(ConnectionModel connectionModel)
        {
            ConnectorModel from = connectionModel.From;
            ConnectorModel to = connectionModel.To;
            from.OutputConnections.Add(connectionModel);
            to.InputConnections.Add(connectionModel);

            //If we connect two IControls we have to set data directly:
            if (from.IControl && to.IControl)
            {
                object data = null;
                //Get IControl data from "to"
                data = to.PluginModel.Plugin.GetType().GetProperty(to.PropertyName).GetValue(to.PluginModel.Plugin, null);

                //Set IControl data
                PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                propertyInfo.SetValue(from.PluginModel.Plugin, data, null);

            }

            //call events on PluginModels to show that they have new connections
            from.PluginModel.OnConnectorPlugstateChanged(from, PlugState.Plugged);
            to.PluginModel.OnConnectorPlugstateChanged(to, PlugState.Plugged);

            AllConnectionModels.Add(connectionModel);
        }

        /// <summary>
        /// Creates a new ImageModel containing the under imgUri stored Image
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal ImageModel newImageModel(Uri imgUri)
        {
            ImageModel imageModel = new ImageModel(imgUri)
            {
                WorkspaceModel = this
            };
            AllImageModels.Add(imageModel);
            return imageModel;
        }

        /// <summary>
        /// Add ImageModel containing the under imgUri stored Image
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal void addImageModel(ImageModel imageModel)
        {
            AllImageModels.Add(imageModel);
            OnNewChildElement(imageModel);
        }

        /// <summary>
        /// Creates a new TextModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal TextModel newTextModel()
        {
            TextModel textModel = new TextModel
            {
                WorkspaceModel = this
            };
            AllTextModels.Add(textModel);
            return textModel;
        }

        /// <summary>
        /// Add a TextModel to this WorkspaceModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal void addTextModel(TextModel textModel)
        {
            AllTextModels.Add(textModel);
        }

        /// <summary>
        /// Deletes the given ImageModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal bool deleteImageModel(ImageModel imageModel)
        {
            OnDeletedChildElement(imageModel);
            return AllImageModels.Remove(imageModel);
        }

        /// <summary>
        /// Deletes the given TextModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal bool deleteTextModel(TextModel textModel)
        {
            OnDeletedChildElement(textModel);
            return AllTextModels.Remove(textModel);
        }

        /// <summary>
        /// Deletes the pluginModel and all of its Connectors and the connected Connections
        /// from our WorkspaceModel
        /// </summary>
        /// <param name="pluginModel"></param>
        /// <returns></returns>
        internal bool deletePluginModel(PluginModel pluginModel)
        {
            //we can only delete PluginModels which are part of our WorkspaceModel
            if (AllPluginModels.Contains(pluginModel))
            {
                // remove all InputConnectors belonging to this pluginModel from our WorkspaceModel
                foreach (ConnectorModel inputConnector in new List<ConnectorModel>(pluginModel.InputConnectors))
                {
                    AllConnectorModels.Remove(inputConnector);
                }

                // remove all OutputConnectors belonging to this pluginModel from our WorkspaceModel
                foreach (ConnectorModel outputConnector in new List<ConnectorModel>(pluginModel.OutputConnectors))
                {
                    AllConnectorModels.Remove(outputConnector);
                }
                pluginModel.Plugin.Dispose();
                OnDeletedChildElement(pluginModel);
                return AllPluginModels.Remove(pluginModel);
            }
            return false;
        }


        /// <summary>
        /// Deletes the connectorModel and the connected Connections
        /// from our WorkspaceModel
        /// </summary>
        /// <param name="connectorModel"></param>
        /// <returns></returns>
        internal bool deleteConnectorModel(ConnectorModel connectorModel)
        {
            //we can only delete ConnectorModels which are part of our WorkspaceModel
            if (AllConnectorModels.Contains(connectorModel))
            {

                //remove all input ConnectionModels belonging to this Connector from our WorkspaceModel
                foreach (ConnectionModel connectionModel in new List<ConnectionModel>(connectorModel.InputConnections))
                {
                    //deleteConnectionModel(connectionModel);
                    ModifyModel(new DeleteConnectionModelOperation(connectionModel));
                }

                //remove all output ConnectionModels belonging to this Connector from our WorkspaceModel
                foreach (ConnectionModel outputConnection in new List<ConnectionModel>(connectorModel.OutputConnections))
                {
                    //deleteConnectionModel(outputConnection);
                    ModifyModel(new DeleteConnectionModelOperation(outputConnection));
                }


                //remove the connector model from the outputconnectors of this plugin
                if (connectorModel.PluginModel.OutputConnectors.Contains(connectorModel))
                {
                    connectorModel.PluginModel.OutputConnectors.Remove(connectorModel);
                }

                //remove the connector model from the inputconnectors of this plugin
                if (connectorModel.PluginModel.InputConnectors.Contains(connectorModel))
                {
                    connectorModel.PluginModel.InputConnectors.Remove(connectorModel);
                }

                OnDeletedChildElement(connectorModel);
                return AllConnectorModels.Remove(connectorModel);
            }
            return false;
        }

        /// <summary>
        /// Removes the connectionModel from our Workspace Model and removes it from all Connectors
        /// </summary>
        /// <param name="connectionModel"></param>
        /// <returns></returns>
        internal bool deleteConnectionModel(ConnectionModel connectionModel)
        {
            if (connectionModel == null)
            {
                return false;
            }

            ConnectorModel to = connectionModel.To;
            ConnectorModel from = connectionModel.From;

            to.InputConnections.Remove(connectionModel);

            if (from.IControl && to.IControl)
            {
                //Delete plugin model
                to.WorkspaceModel.ModifyModel(new DeletePluginModelOperation(to.PluginModel));
                //Set IControl data to null                             
                PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                propertyInfo.SetValue(from.PluginModel.Plugin, null, null);
            }

            from.OutputConnections.Remove(connectionModel);

            //call events on PluginModels to show that they have lost connections
            from.PluginModel.OnConnectorPlugstateChanged(from, PlugState.Unplugged);
            to.PluginModel.OnConnectorPlugstateChanged(to, PlugState.Unplugged);

            OnDeletedChildElement(connectionModel);
            return AllConnectionModels.Remove(connectionModel);
        }

        /// <summary>
        /// Sets all Connections and Connectors to state nonActive/noData
        ///      all plugins to state Normal
        ///      deletes all stored log events
        /// </summary>
        internal void resetStates()
        {
            foreach (PluginModel pluginModel in AllPluginModels)
            {
                pluginModel.State = PluginModelState.Normal;
                pluginModel.PercentageFinished = 0;
            }
            foreach (ConnectionModel connection in AllConnectionModels)
            {
                connection.Active = false;
            }
            foreach (ConnectorModel connector in AllConnectorModels)
            {
                connector.DataQueue.Clear();
                connector.LastData = null;
            }
        }

        /// <summary>
        /// Modify the current WorkspaceModel by using an operation
        /// returns the created object or true if its not a 'new' operation
        /// Some operations may return false to indicate that there was 'nothing to change':
        ///     Example: you change the size from x to x -> return false
        ///     Example: you change the size from x to y -> return true
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="events">Should the model fire events?</param>
        public object ModifyModel(Operation operation, bool events = false)
        {
            if (UndoRedoManager.IsCurrentlyWorking)
            {
                return false;
            }
            if ((operation is ChangeSettingOperation))
            {
                UndoRedoManager.DidOperation(operation);
                UndoRedoManager.SettingsManager.StoreCurrentSettingValues();
                return true;
            }
            else
            {
                object operationReturn = operation.Execute(this, events);
                UndoRedoManager.SettingsManager.StoreCurrentSettingValues();
                if (operationReturn is bool)
                {
                    if (((bool)operationReturn) == true)
                    {
                        UndoRedoManager.DidOperation(operation);
                        return true;
                    }
                    return false;
                }
                UndoRedoManager.DidOperation(operation);
                return operationReturn;
            }
        }

        /// <summary>
        /// Returns the PluginModel with the given name
        /// Returns null if there is no PluginModel with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PluginModel GetPluginModelByName(string name)
        {
            foreach (PluginModel pluginModel in AllPluginModels)
            {
                if (pluginModel.Name == name)
                {
                    return pluginModel;
                }
            }
            return null;
        }

        /// <summary>
        /// "Something" logged
        /// </summary>
        [field: NonSerialized]
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// A childs position of this WorkspaceModel changed
        /// </summary>     
        [field: NonSerialized]
        public event EventHandler<PositionArgs> ChildPositionChanged;

        /// <summary>
        /// A childs size of this WorkspaceModel changed
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<SizeArgs> ChildSizeChanged;

        /// <summary>
        /// A child of this WorkspaceModel is created
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ModelArgs> NewChildElement;

        /// <summary>
        /// A child of this WorkspaceModel is deleted
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ModelArgs> DeletedChildElement;

        /// <summary>
        /// A child of this WorkspaceModel is deleted
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<NameArgs> ChildNameChanged;

        /// <summary>
        /// Loggs a gui message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        internal void GuiLogMessage(IPlugin sender, GuiLogEventArgs args)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(sender, args);
            }
        }


        /// <summary>
        /// Call this to tell the environment that a childs position changed
        /// </summary>
        /// <param name="effectedModelElement"></param>
        /// <param name="oldPosition"></param>
        /// <param name="newPosition"></param>
        internal void OnChildPositionChanged(VisualElementModel effectedModelElement, Point oldPosition, Point newPosition)
        {
            if (ChildPositionChanged != null)
            {
                ChildPositionChanged(this, new PositionArgs(effectedModelElement, oldPosition, newPosition));
            }
        }

        /// <summary>
        ///  Call this to tell the environment that a childs size changed
        /// </summary>
        /// <param name="effectedModelElement"></param>
        /// <param name="oldWidth"></param>
        /// <param name="newWidth"></param>
        /// <param name="oldHeight"></param>
        /// <param name="newHeight"></param>
        internal void OnChildSizeChanged(VisualElementModel effectedModelElement, double oldWidth, double newWidth, double oldHeight, double newHeight)
        {
            if (ChildSizeChanged != null)
            {
                ChildSizeChanged(this, new SizeArgs(effectedModelElement, oldWidth, newWidth, oldHeight, newHeight));
            }
        }

        /// <summary>
        /// Call this to tell the environment that we created a new child
        /// </summary>
        /// <param name="effectedModelElement"></param>
        internal void OnNewChildElement(VisualElementModel effectedModelElement)
        {
            if (NewChildElement != null)
            {
                NewChildElement(this, new ModelArgs(effectedModelElement));
            }
        }

        /// <summary>
        /// Call this to tell the environment that we deleted a child
        /// </summary>
        /// <param name="effectedModelElement"></param>
        internal void OnDeletedChildElement(VisualElementModel effectedModelElement)
        {
            if (DeletedChildElement != null)
            {
                DeletedChildElement(this, new ModelArgs(effectedModelElement));
            }
        }

        /// <summary>
        /// Call this to tell the environment that we renamed a child
        /// </summary>
        /// <param name="effectedModelElement"></param>
        internal void OnRenamedChildElement(VisualElementModel effectedModelElement, string oldname, string newname)
        {
            if (ChildNameChanged != null)
            {
                ChildNameChanged(this, new NameArgs(effectedModelElement, oldname, newname));
            }
        }

        /// <summary>
        /// Checks wether a Connector and a Connector are compatible to be connected
        /// They are compatible if their types are equal or the base type of the Connector
        /// is equal to the type of the other Connector. There exists also the following connection
        /// possibilities:
        /// - int and BigInteger
        /// - string and byte array
        /// - string and cstream 
        /// It is false if already exists a ConnectionModel between both given ConnectorModels
        /// </summary>
        /// <param name="connectorModelA"></param>
        /// <param name="connectorModelB"></param>
        /// <returns></returns>
        public static ConversionLevel compatibleConnectors(ConnectorModel connectorModelA, ConnectorModel connectorModelB)
        {
            if (connectorModelA == null)
            {
                return ConversionLevel.Red;
            }
            if (connectorModelB == null)
            {
                return ConversionLevel.Red;
            }

            if (connectorModelA.PluginModel == connectorModelB.PluginModel)
            {
                return ConversionLevel.NA;
            }

            if (connectorModelA.Outgoing && connectorModelB.Outgoing)
            {
                return ConversionLevel.Red;
            }

            if (!connectorModelA.Outgoing && !connectorModelB.Outgoing)
            {
                return ConversionLevel.Red;
            }

            foreach (ConnectionModel connectionModel in connectorModelA.WorkspaceModel.AllConnectionModels)
            {
                if ((connectionModel.From == connectorModelA && connectionModel.To == connectorModelB) ||
                   (connectionModel.From == connectorModelB && connectionModel.To == connectorModelA))
                {
                    return ConversionLevel.Red;
                }
            }

            // wander 2011-07-06: workaround for #280. May be removed safely in future.
            if (connectorModelA.ConnectorType == null)
            {
                connectorModelA.ConnectorType = typeof(object);
            }

            if (connectorModelB.ConnectorType == null)
            {
                connectorModelB.ConnectorType = typeof(object);
            }

            if (connectorModelA.ConnectorType.Equals(connectorModelB.ConnectorType)
                || connectorModelA.ConnectorType.FullName == "System.Object"
                || connectorModelB.ConnectorType.FullName == "System.Object"
                || connectorModelA.ConnectorType.IsSubclassOf(connectorModelB.ConnectorType)
                || connectorModelA.ConnectorType.GetInterfaces().Contains(connectorModelB.ConnectorType))
            {
                return ConversionLevel.Green;
            }

            if (((connectorModelA.ConnectorType.FullName == "System.Int32" || connectorModelA.ConnectorType.FullName == "System.Int64") && connectorModelB.ConnectorType.FullName == "System.Numerics.BigInteger")
                || ((connectorModelB.ConnectorType.FullName == "System.Int32" || connectorModelB.ConnectorType.FullName == "System.Int64") && connectorModelA.ConnectorType.FullName == "System.Numerics.BigInteger")
                || (connectorModelB.ConnectorType.FullName == "System.String" && connectorModelA.ConnectorType.FullName == "System.Byte[]")
                || (connectorModelB.ConnectorType.FullName == "System.Byte[]" && connectorModelA.ConnectorType.FullName == "System.String")
                || (connectorModelB.ConnectorType.FullName == "System.String" && connectorModelA.ConnectorType.FullName == "System.Int32")
                || (connectorModelB.ConnectorType.FullName == "System.String" && connectorModelA.ConnectorType.FullName == "System.Int64")
                || (connectorModelB.ConnectorType.FullName == "System.String" && connectorModelA.ConnectorType.FullName == "System.Numerics.BigInteger")
                || (connectorModelB.ConnectorType.FullName == "System.String" && connectorModelA.ConnectorType.FullName == "System.Boolean")
                || (connectorModelB.ConnectorType.FullName == "System.String" && connectorModelA.ConnectorType.FullName == "CrypTool.PluginBase.IO.ICrypToolStream")
                || (connectorModelB.ConnectorType.FullName == "CrypTool.PluginBase.IO.ICrypToolStream" && connectorModelA.ConnectorType.FullName == "System.String"))
            {
                return ConversionLevel.Yellow;
            }

            return ConversionLevel.Red;
        }

        /// <summary>
        /// Call Dispose on all PluginModels.Plugin
        /// </summary>
        public void Dispose()
        {
            foreach (PluginModel pluginModel in AllPluginModels)
            {
                pluginModel.Plugin.Dispose();
            }
        }

        /// <summary>
        /// Returns the hash value (SHA-256) of this workspace
        /// To compute the hash we use
        /// - Settings of all plugins
        /// - Connection between all plugins
        /// 
        /// We furthermore sort all values before hashing to avoid different 
        /// hashes based on random order of the values
        /// </summary>
        /// <returns>SHA256 Hash Value</returns>
        public byte[] ComputeWorkspaceHash()
        {
            MemoryStream stream = new MemoryStream();
            //Add all setting values for hashing
            PluginModel[] pluginModels = GetAllPluginModels().ToArray();
            List<string> helper = new List<string>();

            foreach (PluginModel pluginModel in pluginModels)
            {
                if (pluginModel.Plugin.Settings != null)
                {
                    PropertyInfo[] propertyInfos = pluginModel.Plugin.Settings.GetType().GetProperties();
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        DontSaveAttribute[] dontSave = (DontSaveAttribute[])propertyInfo.GetCustomAttributes(typeof(DontSaveAttribute), false);
                        if (propertyInfo.CanWrite && dontSave.Length == 0)
                        {
                            string value = "" + propertyInfo.GetValue(pluginModel.Plugin.Settings, null);
                            helper.Add(value);
                        }
                    }
                }
            }
            //Add all connection infos for hashing
            //Connection info is: "FromPluginname.Connectorname->ToPluginname.Connectorname"
            ConnectionModel[] connectionModels = GetAllConnectionModels().ToArray();
            foreach (ConnectionModel connectionModel in connectionModels)
            {
                string value = connectionModel.From.PluginModel.PluginTypeName + "." + connectionModel.From.PropertyName + "->" + connectionModel.To.PluginModel.PluginTypeName + connectionModel.To.PropertyName;
                helper.Add(value);
            }
            helper.Sort();
            foreach (string str in helper)
            {
                byte[] bytes = ASCIIEncoding.ASCII.GetBytes(str);
                stream.Write(bytes, 0, bytes.Length);
            }
            SHA256 sha256 = SHA256Managed.Create();
            byte[] hash = sha256.ComputeHash(stream.ToArray());
            stream.Close();
            return hash;
        }

        public void InitializeSettings()
        {
            foreach (PluginModel pluginModel in GetAllPluginModels().ToArray())
            {
                if (pluginModel.Plugin.Settings != null)
                {
                    pluginModel.Plugin.Settings.Initialize();
                }
            }
        }

        /// <summary>
        /// Deletes all Elements of this Model by calling the appropriate delete functions
        /// This also forces the UI to update each delete
        /// </summary>
        public void DeleteAllModelElements()
        {
            foreach (ConnectionModel connectionModel in new List<ConnectionModel>(GetAllConnectionModels()))
            {
                deleteConnectionModel(connectionModel);
            }
            foreach (PluginModel pluginModel in new List<PluginModel>(GetAllPluginModels()))
            {
                deletePluginModel(pluginModel);
            }
            foreach (TextModel textModel in new List<TextModel>(GetAllTextModels()))
            {
                deleteTextModel(textModel);
            }
            foreach (ImageModel imageModel in new List<ImageModel>(GetAllImageModels()))
            {
                deleteImageModel(imageModel);
            }
        }
    }

    /// <summary>
    /// Event args which "knows" the effected model
    /// </summary>
    public class ModelArgs : EventArgs
    {
        public VisualElementModel EffectedModelElement { get; private set; }

        public ModelArgs(VisualElementModel effectedModelElement)
        {
            EffectedModelElement = effectedModelElement;
        }
    }

    /// <summary>
    /// Event args which also "knows" old and new positions
    /// </summary>
    public class PositionArgs : ModelArgs
    {
        public Point OldPosition { get; internal set; }
        public Point NewPosition { get; internal set; }
        internal PositionArgs(VisualElementModel model, Point oldPosition, Point newPosition) :
            base(model)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }

    /// <summary>
    /// Event args which also "knows" old and new size (Width, Height)
    /// </summary>
    public class SizeArgs : ModelArgs
    {
        public double OldWidth { get; internal set; }
        public double NewWidth { get; internal set; }
        public double OldHeight { get; internal set; }
        public double NewHeight { get; internal set; }

        internal SizeArgs(VisualElementModel model, double oldWidth, double newWidth, double oldHeight, double newHeight) :
            base(model)
        {
            OldWidth = oldWidth;
            NewWidth = newWidth;
            OldHeight = oldHeight;
            NewHeight = newHeight;
        }
    }

    /// <summary>
    /// Event args which "knows" old and new name of the model element
    /// </summary>
    public class NameArgs : ModelArgs
    {
        public string Oldname { get; internal set; }
        public string NewName { get; internal set; }

        internal NameArgs(VisualElementModel model, string oldname, string newname) :
            base(model)
        {
            Oldname = oldname;
            NewName = newname;
        }
    }
}