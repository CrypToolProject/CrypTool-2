using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CrypTool.PluginBase.Editor
{
    public static class ComponentConnectionStatistics
    {
        public delegate void GuiLogMessageHandler(string message, NotificationLevel logLevel);
        public static event GuiLogMessageHandler OnGuiLogMessageOccured;

        public delegate void StatisticResetHandler();
        public static event StatisticResetHandler OnStatisticReset;
        
        public class ComponentConnector
        {
            private readonly Type _component;
            private readonly string _connectorName;

            public string ConnectorName
            {
                get { return _connectorName; }
            }

            public Type Component
            {
                get { return _component; }
            }

            public ComponentConnector(Type component, string connectorName)
            {
                _component = component;
                _connectorName = connectorName;
            }
            
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (ComponentConnector)) return false;
                return Equals((ComponentConnector) obj);
            }

            public bool Equals(ComponentConnector other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other._component, _component) && Equals(other._connectorName, _connectorName);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_component != null ? _component.GetHashCode() : 0)*397) ^ (_connectorName != null ? _connectorName.GetHashCode() : 0);
                }
            }
        }

        public class ConnectorStatistics
        {
            private readonly Dictionary<ComponentConnector, uint> _connectorUsages = new Dictionary<ComponentConnector, uint>();

            public Dictionary<ComponentConnector, uint> ConnectorUsages
            {
                get { return _connectorUsages; }
            }

            public void IncrementConnectorUsage(ComponentConnector otherConnector)
            {
                IncrementConnectorUsage(otherConnector, 1);
            }

            public void IncrementConnectorUsage(ComponentConnector otherConnector, uint add)
            {
                if (!_connectorUsages.ContainsKey(otherConnector))
                {
                    _connectorUsages.Add(otherConnector, 0);
                }
                _connectorUsages[otherConnector] += add;
            }

            public IEnumerable<ComponentConnector> GetSortedConnectorUsages()
            {
                return _connectorUsages.OrderByDescending(x => x.Value).Select(x => x.Key);
            }
        }

        private static readonly Dictionary<ComponentConnector, ConnectorStatistics> Statistics = new Dictionary<ComponentConnector, ConnectorStatistics>();

        public static void SaveCurrentStatistics(string file)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("componentConnectionStatistics");
            doc.AppendChild(root);

            foreach (var connectorStatistic in Statistics)
            {
                var cs = doc.CreateElement("connectorStatistic");
                root.AppendChild(cs);
                var attr = doc.CreateAttribute("component");
                attr.Value = connectorStatistic.Key.Component.FullName;
                cs.Attributes.Append(attr);
                attr = doc.CreateAttribute("connector");
                attr.Value = connectorStatistic.Key.ConnectorName;
                cs.Attributes.Append(attr);

                foreach (var otherConnector in connectorStatistic.Value.ConnectorUsages)
                {
                    var cc = doc.CreateElement("connectedConnector");
                    cs.AppendChild(cc);
                    attr = doc.CreateAttribute("component");
                    attr.Value = otherConnector.Key.Component.FullName;
                    cc.Attributes.Append(attr);
                    attr = doc.CreateAttribute("connector");
                    attr.Value = otherConnector.Key.ConnectorName;
                    cc.Attributes.Append(attr);
                    attr = doc.CreateAttribute("count");
                    attr.Value = otherConnector.Value.ToString();
                    cc.Attributes.Append(attr);
                }
            }

            doc.Save(file);
        }

        public static void LoadCurrentStatistics(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            var root = doc.DocumentElement;
            foreach (var child in root.ChildNodes)
            {
                var cs = (XmlElement) child;
                var component = cs.GetAttribute("component");
                var connector = cs.GetAttribute("connector");

                if (ComponentInformations.AllLoadedPlugins.ContainsKey(component))
                {
                    var componentConnector = new ComponentConnector(ComponentInformations.AllLoadedPlugins[component], connector);
                    foreach (var innerChild in cs.ChildNodes)
                    {
                        var cc = (XmlElement) innerChild;
                        component = cc.GetAttribute("component");
                        connector = cc.GetAttribute("connector");
                        if (ComponentInformations.AllLoadedPlugins.ContainsKey(component))
                        {
                            var count = uint.Parse(cc.GetAttribute("count"));
                            var otherComponentConnector = new ComponentConnector(ComponentInformations.AllLoadedPlugins[component], connector);
                            if (!Statistics.ContainsKey(componentConnector))
                            {
                                Statistics.Add(componentConnector, new ConnectorStatistics());
                            }
                            Statistics[componentConnector].IncrementConnectorUsage(otherComponentConnector, count);
                        }
                    }
                }
                else
                {
                    GuiLogMessageOccured(string.Format("Can't find component type {0} in running system.", component), NotificationLevel.Warning);
                }
            }
        }

        public static void IncrementConnectionUsage(Type fromComponent, string fromConnectorName, Type toComponent, string toConnectorName)
        {
            var from = new ComponentConnector(fromComponent, fromConnectorName);
            var to = new ComponentConnector(toComponent, toConnectorName);
            IncrementConnectionUsage(from, to);
        }

        public static void IncrementConnectionUsage(ComponentConnector from, ComponentConnector to)
        {
            if (!Statistics.ContainsKey(from))
            {
                Statistics.Add(from, new ConnectorStatistics());
            }
            Statistics[from].IncrementConnectorUsage(to);
        }

        public static IEnumerable<ComponentConnector> GetMostlyUsedComponentsFromConnector(Type component, string connectorName)
        {
            var componentConnector = new ComponentConnector(component, connectorName);
            return GetMostlyUsedComponentsFromConnector(componentConnector);
        }

        public static IEnumerable<ComponentConnector> GetMostlyUsedComponentsFromConnector(ComponentConnector componentConnector)
        {
            if (!Statistics.ContainsKey(componentConnector))
            {
                return null;
            }
            return Statistics[componentConnector].GetSortedConnectorUsages();
        }

        public static void Reset()
        {
            Statistics.Clear();
            StatisticResetOccured();
        }
        
        public static void StatisticResetOccured()
        {
            if (OnStatisticReset != null)
                OnStatisticReset();
        }

        private static void GuiLogMessageOccured(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogMessageOccured != null)
                OnGuiLogMessageOccured(message, loglevel);
        }
    }
}
