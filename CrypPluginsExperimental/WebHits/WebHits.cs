/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Net;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using CrypTool.Plugins.WebHits.Properties;


namespace CrypTool.Plugins.WebHits
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Olga Kieselmann", "kieselmann@uni-kassel.de", "CrypTool 2 Team", "http://www.ais.uni-kassel.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CrypTool.Plugins.WebHits.Properties.Resources", "WebHitsCaption", "PluginTooltip", "WebHits/DetailedDescription/doc.xml", new[] { "WebHits/images/search.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class WebHits : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        //private readonly WebHitsSettings settings;
        private ISettings settings;
        private URLTablePresentation presentation;
        private string searchTerm;
        List<ResultEntry> urls = new List<ResultEntry>();
        #endregion

        ///<summary>
        ///Constructor
        ///</summary>
        ///
        public WebHits()
        {
            presentation = new URLTablePresentation();
            Presentation = presentation;
        }

        public ISettings Settings
        {
            get { return null; }
        }


        #region Data Properties
        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        /// 
        [PropertyInfo(Direction.InputData, "Search string", "InputstringTooltip", mandatory: false)]
        public string SearchTerm
        {
            get { return this.searchTerm; }
            set
            {
                if (value != this.searchTerm)
                {
                    this.searchTerm = value;               
                    OnPropertyChanged("SearchTerm");
                }
            }
        }


        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>

        [PropertyInfo(Direction.OutputData, "Number", "OutputStringTooltip", false)]
        public int Number
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>


        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {       
            Number = 0;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (string.IsNullOrEmpty(SearchTerm))
                return;
            try
            {
                ProgressChanged(0, 100);
                urls.Clear();
                GoogleSearch();                                          
                OnPropertyChanged("Number");                
                ProgressChanged(100, 100);
            }
            catch (System.Exception ex)
            {
                GuiLogMessage(string.Format("Failure: {0}", ex.Message), NotificationLevel.Error);
            }

        }

        private void GoogleSearch()
        {
            ////key and cx are given from Google 
            string key = "AIzaSyCS2IgxYgcd5eMdCv6wiiBJlcRtMhdE8fg";
            string cx = "008159240840838912779:vtvqbhheguo";

            string url = "https://www.googleapis.com/customsearch/v1?key=" + key + "&cx=" + cx + "&q=" + SearchTerm;

            WebClient webclient = new WebClient();
            try
            {
                webclient.Encoding = System.Text.Encoding.UTF8;
                webclient.Proxy = null;//to accelerate the start
                var json = webclient.DownloadString(url);

                DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(typeof(RootObject));
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                RootObject obj = (RootObject)jsonSer.ReadObject(stream);               

                this.presentation.Assign_Values(obj, SearchTerm);

                Number = Convert.ToInt32(obj.searchInformation.totalResults);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                    GuiLogMessage("Failure: Bad domain name, " + ex.Message, NotificationLevel.Error);                   
                else if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = (HttpWebResponse)ex.Response;
                    if (response.StatusDescription.Contains("Forbidden"))
                    {
                        GuiLogMessage(string.Format(Resources.WebHits_GoogleSearch_Failure_Limit, response.StatusDescription),          
                        NotificationLevel.Error)
                        ;
                    }
                    else
                    {
                        GuiLogMessage("Failure: " + response.StatusDescription, NotificationLevel.Error);
                    }
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        GuiLogMessage("Failure: Not there! " + response.StatusCode, NotificationLevel.Error); 
                }
            }
            
        }
        

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }

    public class ResultEntry
    {
        public int Ranking { get; set; }
        public string HitURL { get; set; }
    }
}
