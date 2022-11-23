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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using CrypTool.PluginBase;
using WorkspaceManagerModel.Properties;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class to represent the Connection between two Connector Models
    /// </summary>
    [Serializable]
    public class ConnectorModel : VisualElementModel
    {
        internal ConnectorModel()
        {
            InputConnections = new List<ConnectionModel>();
            OutputConnections = new List<ConnectionModel>();
        }

        #region private members

        /// <summary>
        /// Name of the Connector type
        /// </summary>
        private string ConnectorTypeName = null;

        /// <summary>
        /// Name of the Connector assembly
        /// </summary>
        private string ConnectorTypeAssemblyName = null;

        #endregion

        #region public members

        /// <summary>
        /// The property of the plugin behind this connectorModel
        /// </summary>      
        [NonSerialized]
        internal PropertyInfo property = null;

        /// <summary>
        /// The PluginModel this Connector belongs to
        /// </summary>
        public PluginModel PluginModel { get; internal set; }

        public int Index = int.MinValue;

        /// <summary>
        /// The Type of the Connector Model
        /// </summary>        
        public Type ConnectorType
        {
            get
            {
                if (ConnectorTypeName != null && !ConnectorTypeName.Equals(""))
                {
                    if (ConnectorTypeName.Equals("System.Numerics.BigInteger"))
                    {
                        return typeof(System.Numerics.BigInteger);
                    }
                    if (ConnectorTypeName.Equals("System.Numerics.BigInteger[]"))
                    {
                        return typeof(System.Numerics.BigInteger[]);
                    }

                    Assembly assembly = Assembly.Load(ConnectorTypeAssemblyName);
                    Type t = assembly.GetType(ConnectorTypeName);
                    if (t != null)
                    {
                        return t;
                    }
                }

                //we do not know the type. Maybe the developer changed it, so we try to get 
                //it from the plugin
                foreach (PropertyInfoAttribute property in PluginModel.Plugin.GetProperties())
                {
                    if (property.PropertyName.Equals(Name))
                    {
                        Type type = property.PropertyInfo.PropertyType;
                        ConnectorTypeName = type.FullName;
                        ConnectorTypeAssemblyName = type.Assembly.GetName().Name;
                        return type;
                    }
                }

                //we didnt get the type of this connector model so we return null
                return null;

            }
            internal set
            {
                ConnectorTypeName = value.FullName;
                ConnectorTypeAssemblyName = value.Assembly.GetName().Name;
            }
        }

        /// <summary>
        /// Is this Connector Outgoing?
        /// </summary>
        public bool Outgoing { get; internal set; }

        /// <summary>
        /// Is this Connector an IControl Connector
        /// </summary>
        public bool IControl { get; internal set; }

        /// <summary>
        /// The InputConnections of this ConnectorModel
        /// </summary>
        internal List<ConnectionModel> InputConnections;

        /// <summary>
        /// Get the input connections of this ConnectorModel
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<ConnectionModel> GetInputConnections()
        {
            return InputConnections.AsReadOnly();
        }

        /// <summary>
        /// The OutputConnections of this ConnectorModel
        /// </summary>
        internal List<ConnectionModel> OutputConnections;

        /// <summary>
        /// Get the output connections of this ConnectorModel
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<ConnectionModel> GetOutputConnections()
        {
            return OutputConnections.AsReadOnly();
        }

        /// <summary>
        /// The Orientation of this ConnectorModel
        /// </summary>
        public ConnectorOrientation Orientation = ConnectorOrientation.Unset;

        /// <summary>
        /// The WorkspaceModel of this PluginModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; internal set; }

        /// <summary>
        /// Is this Connectors Data mandatory?
        /// </summary>
        /// <returns></returns>
        public bool IsMandatory
        {
            get;
            internal set;
        }

        /// <summary>
        /// This is the value, each property of a component is set to, before the workspace is being executed
        /// The value is read after execution of the initialize method of the component
        /// </summary>
        [NonSerialized]
        public object DefaultValue;

        /// <summary>
        /// Data of this Connector
        /// </summary>
        [NonSerialized]
        public Queue DataQueue = Queue.Synchronized(new Queue());

        /// <summary>
        /// LastData of this Connector
        /// </summary>
        [NonSerialized]
        public object LastData;

        /// <summary>
        /// Name of the represented Property of the IPlugin of this ConnectorModel
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        /// ToolTip of this Connector
        /// </summary>
        public string ToolTip { get; internal set; }

        /// <summary>
        /// ToolTip of this Connector
        /// </summary>
        [NonSerialized]
        public string Caption = string.Empty;

        /// <summary>
        /// Plugin informs the Connector that a PropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PropertyChangedOnPlugin(object sender, PropertyChangedEventArgs args)
        {
            try
            {             
                if (!WorkspaceModel.IsBeingExecuted || !Outgoing || sender != PluginModel.Plugin || !args.PropertyName.Equals(PropertyName))
                {
                    return;
                }
                
                if (property == null)
                {
                    property = sender.GetType().GetProperty(args.PropertyName);
                }
                object data = property.GetValue(sender, null);
                if (data == null)
                {
                    return;
                }

                LastData = data;

                List<ConnectionModel> outputConnections = OutputConnections;
                foreach (ConnectionModel connectionModel in outputConnections)
                {
                    connectionModel.To.DataQueue.Enqueue(data);
                    connectionModel.To.LastData = data;
                    connectionModel.Active = true;
                    connectionModel.GuiNeedsUpdate = true;
                    connectionModel.To.PluginModel.resetEvent.Set();
                }
                
            }
            catch (Exception ex)
            {
                if (WorkspaceModel.ExecutionEngine != null)
                {
                    WorkspaceModel.ExecutionEngine.GuiLogMessage(string.Format(Resources.ConnectorModel_PropertyChangedOnPlugin_Error_occured_during_propagating_of_new_value_of___0___of_Output___1_____2_, PluginModel.Name, Name, ex.Message), NotificationLevel.Error);
                }
            }
        }

        #endregion                
    }

    public enum ConnectorOrientation
    {
        North,
        South,
        West,
        East,
        Unset
    };
}
