/*                              
   Copyright 2011 Nils Kopal

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
using System.IO;
using System.Reflection;
using System.Windows;
using CrypTool.PluginBase;
using WorkspaceManager.Model;
using WorkspaceManagerModel.Properties;

namespace WorkspaceManagerModel.Model.Operations
{

    /// <summary>
    /// Abstract basic class representing a single operation which modifies the model
    /// </summary>
    public abstract class Operation
    {
        public Operation(VisualElementModel model)
        {
            Model = model;
        }

        public int Identifier { get; protected set; }
        public VisualElementModel Model { get; internal set; }
        internal abstract object Execute(WorkspaceModel workspaceModel, bool events = true);
        internal abstract void Undo(WorkspaceModel workspaceModel);
        internal bool SavedHere;
    }

    /// <summary>
    /// Creates a new PluginModel
    /// </summary>
    public sealed class NewPluginModelOperation : Operation
    {
        private Point Position = new Point(0, 0);
        private readonly double Width = 0;
        private readonly double Height = 0;
        private readonly Type PluginType = null;

        public NewPluginModelOperation(Point position, double width, double height, Type pluginType)
            : base(null)
        {
            Position = position;
            Width = width;
            Height = height;
            PluginType = pluginType;
            Identifier = 0;
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            if (Model == null)
            {
                Model = workspaceModel.newPluginModel(Position,
                    Width,
                    Height,
                    PluginType);
            }
            else
            {
                workspaceModel.addPluginModel((PluginModel)Model);
            }
            if (events)
            {
                workspaceModel.OnNewChildElement(Model);
            }

            return Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deletePluginModel((PluginModel)Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes an existing PluginModel
    /// </summary>
    public sealed class DeletePluginModelOperation : Operation
    {
        private Point Position = new Point(0, 0);
        private readonly double Width = 0;
        private readonly double Height = 0;
        private readonly Type PluginType = null;

        public DeletePluginModelOperation(PluginModel model)
            : base(model)
        {
            Position = model.GetPosition();
            Width = model.GetWidth();
            Height = model.GetHeight();
            PluginType = model.PluginType;
            Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            workspaceModel.deletePluginModel((PluginModel)Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addPluginModel((PluginModel)Model);
            workspaceModel.OnNewChildElement(Model);
        }

        #endregion
    }

    /// <summary>
    /// Creates a new ConnectionModel
    /// </summary>
    public sealed class NewConnectionModelOperation : Operation
    {
        private readonly ConnectorModel From = null;
        private readonly ConnectorModel To = null;
        private readonly Type ConnectionType = null;

        public NewConnectionModelOperation(ConnectorModel from, ConnectorModel to, Type connectionType) :
            base(null)
        {
            From = from;
            To = to;
            ConnectionType = connectionType;
            Identifier = 0;
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            if (Model == null)
            {
                Model = workspaceModel.newConnectionModel(From, To, ConnectionType);
            }
            else
            {
                workspaceModel.addConnectionModel((ConnectionModel)Model);
            }
            if (events)
            {
                workspaceModel.OnNewChildElement(Model);
            }

            return Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteConnectionModel((ConnectionModel)Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes a ConnectionModel
    /// </summary>
    public class DeleteConnectionModelOperation : Operation
    {
        public DeleteConnectionModelOperation(ConnectionModel model) :
            base(model)
        {
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            workspaceModel.deleteConnectionModel((ConnectionModel)Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addConnectionModel((ConnectionModel)Model);
            workspaceModel.OnNewChildElement(Model);
        }

        #endregion
    }

    /// <summary>
    /// Creates a new ImageModel
    /// </summary>
    public sealed class NewImageModelOperation : Operation
    {
        private readonly Uri ImgUri;

        public NewImageModelOperation(Uri imgUri)
            : base(null)
        {
            ImgUri = imgUri;
            Identifier = GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            if (Model == null)
            {
                Model = workspaceModel.newImageModel(ImgUri);
            }
            else
            {
                workspaceModel.addImageModel((ImageModel)Model);
            }
            if (events)
            {
                workspaceModel.OnNewChildElement(Model);
            }

            return Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteImageModel((ImageModel)Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes an ImageModel
    /// </summary>
    public sealed class DeleteImageModelOperation : Operation
    {
        public DeleteImageModelOperation(ImageModel model) :
            base(model)
        {
            Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            workspaceModel.deleteImageModel((ImageModel)Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addImageModel((ImageModel)Model);
            workspaceModel.OnNewChildElement(Model);
        }

        #endregion
    }

    /// <summary>
    /// Creates a new TextModel
    /// </summary>
    public sealed class NewTextModelOperation : Operation
    {
        public NewTextModelOperation()
            : base(null)
        {
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            if (Model == null)
            {
                Model = workspaceModel.newTextModel();
            }
            else
            {
                workspaceModel.addTextModel((TextModel)Model);
            }
            workspaceModel.OnNewChildElement(Model);
            return Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteTextModel((TextModel)Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes a TextModel
    /// </summary>
    public sealed class DeleteTextModelOperation : Operation
    {
        public DeleteTextModelOperation(TextModel model) :
            base(model)
        {
            Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            workspaceModel.deleteTextModel((TextModel)Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addTextModel((TextModel)Model);
            workspaceModel.OnNewChildElement(Model);
        }

        #endregion
    }


    /// <summary>
    /// Moves the Position of an existing VisualElementModel
    /// </summary>
    public sealed class MoveModelElementOperation : Operation
    {
        private Point OldPosition = new Point(0, 0);
        private Point NewPosition = new Point(0, 0);

        public MoveModelElementOperation(VisualElementModel model, Point newPosition)
            : base(model)
        {
            OldPosition = model.GetPosition();
            NewPosition = newPosition;
            Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            if (OldPosition.Equals(NewPosition))
            {
                return false;
            }
            Model.Position = NewPosition;
            workspaceModel.OnChildPositionChanged(Model, OldPosition, NewPosition);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            Model.Position = OldPosition;
            workspaceModel.OnChildPositionChanged(Model, NewPosition, OldPosition);
        }

        #endregion
    }

    /// <summary>
    /// Resizes an existing VisualElementModel
    /// </summary>
    public sealed class ResizeModelElementOperation : Operation
    {
        private readonly double OldWidth = 0;
        private readonly double OldHeight = 0;
        private readonly double NewWidth = 0;
        private readonly double NewHeight = 0;

        public ResizeModelElementOperation(VisualElementModel model, double newWidth, double newHeight)
            : base(model)
        {
            OldWidth = model.GetWidth();
            OldHeight = model.GetHeight();
            NewWidth = newWidth;
            NewHeight = newHeight;
            Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            if (OldWidth.Equals(NewWidth) && OldHeight.Equals(NewHeight))
            {
                return false;
            }
            Model.Width = NewWidth;
            Model.Height = NewHeight;
            workspaceModel.OnChildSizeChanged(Model, OldWidth, NewWidth, OldHeight, NewHeight);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            Model.Width = OldWidth;
            Model.Height = OldHeight;
            workspaceModel.OnChildSizeChanged(Model, NewWidth, OldWidth, NewHeight, OldHeight);
        }

        #endregion
    }

    /// <summary>
    /// Rename a model element
    /// </summary>
    public sealed class RenameModelElementOperation : Operation
    {
        private readonly string OldName = null;
        private readonly string NewName = null;

        public RenameModelElementOperation(VisualElementModel model, string newName)
            : base(model)
        {
            OldName = model.Name;
            NewName = newName;
            Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            if (OldName.Equals(NewName))
            {
                return false;
            }
            Model.Name = NewName;
            workspaceModel.OnRenamedChildElement(Model, OldName, NewName);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            Model.Name = OldName;
            workspaceModel.OnRenamedChildElement(Model, NewName, OldName);
        }

        #endregion
    }

    /// <summary>
    /// Wrapper around n Operations which will operate as one single operation
    /// </summary>
    public sealed class MultiOperation : Operation
    {
        private readonly List<Operation> _operations = null;

        public MultiOperation(List<Operation> operations) :
            base(null)
        {
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }
            _operations = operations;
            foreach (Operation operation in operations)
            {
                Identifier += operation.Identifier;
            }
        }

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            foreach (Operation op in _operations)
            {
                op.Execute(workspaceModel, events);
            }
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            List<Operation> reversedOperations = new List<Operation>(_operations);
            reversedOperations.Reverse();

            foreach (Operation op in reversedOperations)
            {
                op.Undo(workspaceModel);
            }
        }
    }

    public sealed class ChangeSettingOperation : Operation
    {
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly ISettings _settings;
        private readonly string _propertyName;

        public ChangeSettingOperation(WorkspaceModel workspaceModel, ISettings settings, string propertyName, object value) :
            base(null)
        {
            _settings = settings;
            _propertyName = propertyName;
            _oldValue = workspaceModel.UndoRedoManager.SettingsManager.GetCurrentSettingValue(settings.GetHashCode() + "_" + propertyName);
            _newValue = value;
        }

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            PropertyInfo property = _settings.GetType().GetProperty(_propertyName);
            object currentValue = property.GetValue(_settings);
            if (_newValue != currentValue)
            {
                property.SetValue(_settings, _newValue);
            }
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            PropertyInfo property = _settings.GetType().GetProperty(_propertyName);
            object currentValue = property.GetValue(_settings);
            if (_oldValue != currentValue)
            {
                property.SetValue(_settings, _oldValue);
            }
        }
    }


    [Serializable]
    public class SerializationWrapper
    {
        public List<VisualElementModel> elements = null;
        internal List<PersistantPlugin> persistantPlugins = new List<PersistantPlugin>();
    }

    public sealed class CopyOperation : Operation
    {
        private readonly List<VisualElementModel> _copiedElements;
        private readonly List<PersistantPlugin> _persistantPlugins;

        public CopyOperation(SerializationWrapper wrapper)
            : base(null)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            try
            {
                //Save all Settings of each Plugin
                foreach (VisualElementModel element in wrapper.elements)
                {
                    PluginModel pluginModel = element as PluginModel;
                    if (pluginModel != null && pluginModel.Plugin.Settings != null)
                    {
                        PropertyInfo[] arrpInfo = pluginModel.Plugin.Settings.GetType().GetProperties();

                        PersistantPlugin persistantPlugin = new PersistantPlugin
                        {
                            PluginModel = pluginModel
                        };

                        foreach (PropertyInfo pInfo in arrpInfo)
                        {
                            DontSaveAttribute[] dontSave = (DontSaveAttribute[])pInfo.GetCustomAttributes(typeof(DontSaveAttribute), false);
                            if (pInfo.CanWrite && dontSave.Length == 0)
                            {
                                PersistantSetting persistantSetting = new PersistantSetting();
                                if (pInfo.PropertyType.IsEnum)
                                {
                                    persistantSetting.Value = "" + pInfo.GetValue(pluginModel.Plugin.Settings, null).GetHashCode();
                                }
                                else
                                {
                                    persistantSetting.Value = "" + pInfo.GetValue(pluginModel.Plugin.Settings, null);
                                }
                                persistantSetting.Name = pInfo.Name;
                                persistantSetting.Type = pInfo.PropertyType.FullName;
                                persistantPlugin.PersistantSettingsList.Add(persistantSetting);
                            }

                        }
                        wrapper.persistantPlugins.Add(persistantPlugin);
                    }
                }

                //deep copy model elements
                XMLSerialization.XMLSerialization.Serialize(wrapper, writer);
                SerializationWrapper deserializedWrapper = ((SerializationWrapper)XMLSerialization.XMLSerialization.Deserialize(writer));
                _copiedElements = deserializedWrapper.elements;
                _persistantPlugins = deserializedWrapper.persistantPlugins;

            }
            finally
            {
                writer.Close();
                stream.Close();
            }
        }

        /// <summary>
        /// Adds all ConnectionModels which are "between" PluginModels located in the list to the list
        /// </summary>
        /// <param name="elements">List with PluginModels</param>
        /// <returns></returns>
        public static List<VisualElementModel> SelectConnections(List<VisualElementModel> elements)
        {
            foreach (VisualElementModel visualElementModel in new List<VisualElementModel>(elements))
            {
                PluginModel pluginModel = visualElementModel as PluginModel;
                if (pluginModel != null)
                {
                    foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
                    {
                        foreach (ConnectionModel connectionModel in connectorModel.InputConnections)
                        {
                            if (((connectionModel.From.IControl || elements.Contains(connectionModel.From.PluginModel)) && !elements.Contains(connectionModel)))
                            {
                                elements.Add(connectionModel);
                            }
                            if (connectionModel.From.IControl && !elements.Contains(connectionModel.From.PluginModel))
                            {
                                elements.Add(connectionModel.From.PluginModel);

                            }
                        }
                    }
                    foreach (ConnectorModel connectorModel in pluginModel.OutputConnectors)
                    {
                        foreach (ConnectionModel connectionModel in connectorModel.OutputConnections)
                        {
                            if (((connectionModel.From.IControl || elements.Contains(connectionModel.From.PluginModel)) && !elements.Contains(connectionModel)))
                            {
                                elements.Add(connectionModel);
                            }
                            if (connectionModel.To.IControl && !elements.Contains(connectionModel.To.PluginModel))
                            {
                                elements.Add(connectionModel.To.PluginModel);
                            }
                        }
                    }
                }
            }
            return elements;
        }

        internal override object Execute(WorkspaceModel workspaceModel, bool events = true)
        {
            foreach (VisualElementModel visualElementModel in _copiedElements)
            {
                PluginModel pluginModel = visualElementModel as PluginModel;
                ConnectorModel connectorModel = visualElementModel as ConnectorModel;
                ConnectionModel connectionModel = visualElementModel as ConnectionModel;
                TextModel textModel = visualElementModel as TextModel;
                ImageModel imageModel = visualElementModel as ImageModel;

                if (pluginModel != null)
                {
                    workspaceModel.AllPluginModels.Add(pluginModel);
                    pluginModel.WorkspaceModel = workspaceModel;

                    //add input/output connectors of this pluginModel to the WorkspaceModel
                    foreach (ConnectorModel myConnectorModel in pluginModel.InputConnectors)
                    {
                        workspaceModel.AllConnectorModels.Add(myConnectorModel);
                    }
                    foreach (ConnectorModel myConnectorModel in pluginModel.OutputConnectors)
                    {
                        workspaceModel.AllConnectorModels.Add(myConnectorModel);
                    }
                }
                if (connectorModel != null)
                {
                    connectorModel.WorkspaceModel = workspaceModel;
                    workspaceModel.AllConnectorModels.Add(connectorModel);
                }
                if (connectionModel != null)
                {
                    connectionModel.IsCopy = true;
                    connectionModel.WorkspaceModel = workspaceModel;
                    workspaceModel.AllConnectionModels.Add(connectionModel);
                }
                if (textModel != null)
                {
                    textModel.WorkspaceModel = workspaceModel;
                    workspaceModel.AllTextModels.Add(textModel);
                }
                if (imageModel != null)
                {
                    imageModel.WorkspaceModel = workspaceModel;
                    workspaceModel.AllImageModels.Add(imageModel);
                }
            }

            //restore all settings of each plugin
            foreach (PersistantPlugin persistantPlugin in _persistantPlugins)
            {
                if (persistantPlugin.PluginModel.Plugin.Settings == null)
                {
                    continue; // do not attempt deserialization if plugin type has no settings
                }

                foreach (PersistantSetting persistantSetting in persistantPlugin.PersistantSettingsList)
                {

                    PropertyInfo[] arrpInfo = persistantPlugin.PluginModel.Plugin.Settings.GetType().GetProperties();
                    foreach (PropertyInfo pInfo in arrpInfo)
                    {
                        try
                        {
                            DontSaveAttribute[] dontSave =
                                (DontSaveAttribute[])pInfo.GetCustomAttributes(typeof(DontSaveAttribute), false);
                            if (dontSave.Length == 0)
                            {
                                if (pInfo.Name.Equals(persistantSetting.Name))
                                {
                                    if (persistantSetting.Type.Equals("System.String"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       persistantSetting.Value, null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int16"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       short.Parse(persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int32"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       int.Parse(persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int64"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       long.Parse(persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Double"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       double.Parse(persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Boolean"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       bool.Parse(persistantSetting.Value), null);
                                    }
                                    else if (pInfo.PropertyType.IsEnum)
                                    {
                                        int.TryParse(persistantSetting.Value, out int result);
                                        object newEnumValue = Enum.ToObject(pInfo.PropertyType, result);
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings, newEnumValue, null);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                string.Format(Resources.CopyOperation_Execute_Could_not_restore_the_setting___0___of_plugin___1__, persistantSetting.Name, persistantPlugin.PluginModel.Name), ex);
                        }
                    }
                }
            }

            foreach (VisualElementModel visualElementModel in new List<VisualElementModel>(_copiedElements))
            {
                PluginModel pluginModel = visualElementModel as PluginModel;
                ConnectionModel connectionModel = visualElementModel as ConnectionModel;

                if (pluginModel != null)
                {
                    //remove connections which should not be copied
                    //deletes implicitly all PluginModels and ConnectorModels which
                    //are now not referenced any more (will be deleted by garbage collector)
                    foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
                    {
                        foreach (ConnectionModel myconnectionModel in new List<ConnectionModel>(connectorModel.InputConnections))
                        {
                            if (!_copiedElements.Contains(myconnectionModel.From.PluginModel))
                            {
                                _copiedElements.Remove(myconnectionModel);
                                connectorModel.InputConnections.Remove(myconnectionModel);
                                workspaceModel.AllConnectionModels.Remove(myconnectionModel);
                                myconnectionModel.From.OutputConnections.Remove(myconnectionModel);
                            }
                        }
                    }
                    foreach (ConnectorModel connectorModel in pluginModel.OutputConnectors)
                    {
                        foreach (ConnectionModel myconnectionModel in new List<ConnectionModel>(connectorModel.OutputConnections))
                        {
                            if (!_copiedElements.Contains(myconnectionModel.To.PluginModel))
                            {
                                _copiedElements.Remove(myconnectionModel);
                                connectorModel.OutputConnections.Remove(myconnectionModel);
                                workspaceModel.AllConnectionModels.Remove(myconnectionModel);
                                myconnectionModel.To.InputConnections.Remove(myconnectionModel);
                            }
                        }
                    }

                    //initialize plugin and register event handlers
                    try
                    {
                        pluginModel.Plugin.Initialize();
                        pluginModel.PercentageFinished = 0;
                        if (pluginModel.Plugin.Settings != null)
                        {
                            pluginModel.Plugin.Settings.Initialize();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format(Resources.CopyOperation_Execute_Error_while_initializing___0__, pluginModel.Name), ex);
                    }
                    pluginModel.Plugin.OnGuiLogNotificationOccured += workspaceModel.GuiLogMessage;
                    pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;
                    pluginModel.Plugin.OnPluginStatusChanged += pluginModel.PluginStatusChanged;
                    if (pluginModel.Plugin.Settings != null)
                    {
                        pluginModel.Plugin.Settings.PropertyChanged += pluginModel.SettingsPropertyChanged;
                    }

                    //refresh language stuff and register connector model event handlers
                    //also set correct workspace model
                    foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
                    {
                        //refresh language stuff
                        foreach (PropertyInfoAttribute property in connectorModel.PluginModel.Plugin.GetProperties())
                        {
                            if (property.PropertyName.Equals(connectorModel.PropertyName))
                            {
                                connectorModel.ToolTip = property.ToolTip;
                                connectorModel.Caption = property.Caption;
                                break;
                            }
                        }
                        connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                        connectorModel.WorkspaceModel = workspaceModel;
                    }
                    foreach (ConnectorModel connectorModel in pluginModel.OutputConnectors)
                    {
                        //refresh language stuff
                        foreach (PropertyInfoAttribute property in connectorModel.PluginModel.Plugin.GetProperties())
                        {
                            if (property.PropertyName.Equals(connectorModel.PropertyName))
                            {
                                connectorModel.ToolTip = property.ToolTip;
                                connectorModel.Caption = property.Caption;
                                break;
                            }
                        }
                        connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                        connectorModel.WorkspaceModel = workspaceModel;
                    }
                    pluginModel.StoreAllDefaultInputConnectorValues();
                }

                if (connectionModel != null)
                {
                    ConnectorModel from = connectionModel.From;
                    ConnectorModel to = connectionModel.To;
                    try
                    {
                        if (from.IControl && to.IControl)
                        {
                            object data = null;
                            //Get IControl data from "to"                       
                            data = to.PluginModel.Plugin.GetType().GetProperty(to.PropertyName).GetValue(to.PluginModel.Plugin, null);
                            PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                            propertyInfo.SetValue(from.PluginModel.Plugin, data, null);

                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format(Resources.CopyOperation_Execute_Error_while_restoring_IControl_Connection_between___0___to___1____Workspace_surely_will_not_work_well_, from.PluginModel.Name, to.PluginModel.Name), ex);
                    }
                }
            }

            //move model elements x+50 y+50
            foreach (VisualElementModel visualElementModel in _copiedElements)
            {
                visualElementModel.Position = new Point(visualElementModel.Position.X + 50, visualElementModel.Position.Y + 50);
            }

            //fire events for the view to draw the elements
            foreach (VisualElementModel visualElementModel in _copiedElements)
            {
                if (events)
                {
                    workspaceModel.OnNewChildElement(visualElementModel);
                }
            }
            return true;
        }

        public List<VisualElementModel> GetCopiedModelElements()
        {
            return new List<VisualElementModel>(_copiedElements);
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            foreach (VisualElementModel visualElementModel in _copiedElements)
            {
                PluginModel pluginModel = visualElementModel as PluginModel;
                ConnectorModel connectorModel = visualElementModel as ConnectorModel;
                ConnectionModel connectionModel = visualElementModel as ConnectionModel;
                TextModel textModel = visualElementModel as TextModel;
                ImageModel imageModel = visualElementModel as ImageModel;

                if (pluginModel != null)
                {
                    workspaceModel.AllPluginModels.Remove(pluginModel);
                }
                if (connectorModel != null)
                {
                    workspaceModel.AllConnectorModels.Remove(connectorModel);
                }
                if (connectionModel != null)
                {
                    workspaceModel.AllConnectionModels.Remove(connectionModel);
                }
                if (textModel != null)
                {
                    workspaceModel.AllTextModels.Remove(textModel);
                }
                if (imageModel != null)
                {
                    workspaceModel.AllImageModels.Remove(imageModel);
                }
                workspaceModel.OnDeletedChildElement(visualElementModel);
            }
        }
    }
}
