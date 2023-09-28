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
using CrypTool.PluginBase.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using WorkspaceManager.Model;

namespace CrypTool.CrypWin
{
    public static class TemplatesAnalyzer
    {
        public static void GenerateStatisticsFromTemplate(string templateDir)
        {
            ModelPersistance modelLoader = new ModelPersistance();

            foreach (string file in Directory.GetFiles(templateDir, "*.cwm", SearchOption.AllDirectories))
            {
                FileInfo templateFile = new FileInfo(file);
                if (templateFile.Name.StartsWith("."))
                {
                    continue;
                }

                try
                {
                    using (WorkspaceModel model = modelLoader.loadModel(templateFile.FullName))
                    {
                        //Analyse model connections:
                        foreach (PluginModel pluginModel in model.GetAllPluginModels())
                        {
                            foreach (ConnectorModel inputConnector in pluginModel.GetInputConnectors())
                            {
                                AnalyseConnectorUsage(inputConnector);
                            }
                            foreach (ConnectorModel outputConnector in pluginModel.GetOutputConnectors())
                            {
                                AnalyseConnectorUsage(outputConnector);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Error loading model... Just ignore
                }
            }
        }

        private static void AnalyseConnectorUsage(ConnectorModel connectorModel)
        {
            ComponentConnectionStatistics.ComponentConnector componentConnector = new ComponentConnectionStatistics.ComponentConnector(connectorModel.PluginModel.PluginType, connectorModel.PropertyName);
            foreach (ComponentConnectionStatistics.ComponentConnector otherConnector in AllConnectedConnectors(connectorModel))
            {
                ComponentConnectionStatistics.IncrementConnectionUsage(componentConnector, otherConnector);
            }
        }

        private static IEnumerable<ComponentConnectionStatistics.ComponentConnector> AllConnectedConnectors(ConnectorModel connectorModel)
        {
            foreach (ConnectionModel inputConnection in connectorModel.GetInputConnections())
            {
                yield return new ComponentConnectionStatistics.ComponentConnector(inputConnection.From.PluginModel.PluginType, inputConnection.From.PropertyName);
            }
            foreach (ConnectionModel outputConnection in connectorModel.GetOutputConnections())
            {
                yield return new ComponentConnectionStatistics.ComponentConnector(outputConnection.To.PluginModel.PluginType, outputConnection.To.PropertyName);
            }
        }
    }
}
