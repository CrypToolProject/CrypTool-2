/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CrypTool.Plugins.DECRYPTTools
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.DECRYPTTools.Properties.Resources", "DECRYPTClustererCaption", "DECRYPTClustererTooltip", "DECRYPTTools/userdoc.xml", "DECRYPTTools/icon.png")]
    [ComponentCategory(ComponentCategory.DECRYPTProjectComponent)]
    public class DECRYPTClusterer : ICrypComponent
    {
        private ClusterSet _clusterset;
        private readonly DECRYPTClustererPresentation _presentation;
        private readonly DECRYPTClustererSettings _settings;

        public DECRYPTClusterer()
        {
            _presentation = new DECRYPTClustererPresentation(this);
            _settings = new DECRYPTClustererSettings();
        }

        /// <summary>
        /// Input of a json record of the DECRYPT database
        /// </summary>
        [PropertyInfo(Direction.InputData, "TextDocumentCaption", "TextDocumentTooltip")]
        public string TextDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Output of a complete cluster as string
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ClusterOutputCaption", "ClusterOutputTooltip")]
        public string ClusterOutput
        {
            get;
            set;
        }

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        /// <summary>
        /// Execute (main) method of this component
        /// </summary>
        public void Execute()
        {
            if (TextDocument != null)
            {
                ProgressChanged(0, 1);
                try
                {
                    //NoNomenclatureParser with regularCodeLength = 1 just creates single symbol lines
                    NoNomenclatureParser parser = new NoNomenclatureParser(1, null)
                    {
                        DECRYPTTextDocument = TextDocument
                    };
                    TextDocument document = parser.GetTextDocument();
                    parser.ShowCommentsPlaintextCleartext = false;
                    parser.CleanupDocument(document);
                    _clusterset.AddDocument(document);
                    int documentCount = _clusterset.Documents.Count;
                    int clusterCount = _clusterset.Clusters.Count;
                    ProgressChanged(1, 1);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception occured while trying to add document to internal cluster set: {0}", ex.Message), NotificationLevel.Error);
                }

            }
            TextDocument = null;
        }

        public void Initialize()
        {

        }

        public void PostExecution()
        {

        }

        /// <summary>
        /// Create new ClusterSet in PreExecution
        /// </summary>
        public void PreExecution()
        {
            _clusterset = new ClusterSet(_settings.MatchThreshold != 0 ? _settings.MatchThreshold : 15);
            _presentation.CurrentClusterSet = _clusterset;
        }

        public void Stop()
        {

        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Outputs a complete cluster
        /// </summary>
        /// <param name="cluster"></param>
        public void OutputCluster(Cluster cluster)
        {
            Task.Run(() =>
            {
                StringBuilder clusterOutputBuilder = new StringBuilder();
                foreach (TextDocumentWithFrequencies document in cluster.Documents)
                {
                    foreach (Util.Page page in document.TextDocument.Pages)
                    {
                        foreach (Line line in page.Lines)
                        {
                            foreach (Token token in line.Tokens)
                            {
                                foreach (Symbol symbol in token.Symbols)
                                {
                                    clusterOutputBuilder.Append(symbol);
                                    clusterOutputBuilder.Append(" ");
                                }
                            }
                            clusterOutputBuilder.AppendLine();
                        }
                        clusterOutputBuilder.AppendLine();
                    }
                    clusterOutputBuilder.AppendLine();
                }
                ClusterOutput = clusterOutputBuilder.ToString();
                OnPropertyChanged("ClusterOutput");
            });
        }
    }
}