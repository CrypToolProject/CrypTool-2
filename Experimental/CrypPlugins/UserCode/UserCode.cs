/*                              
   Copyright 2011, Nils Kopal, Uni Duisburg-Essen

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

using AurelienRibon.Ui.SyntaxHighlightBox;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.UserCode
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CrypTool.Plugins.UserCode.Properties.Resources", "PluginCaption", "PluginTooltip", null, "UserCode/icons/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class UserCode : ICrypComponent
    {
        private readonly UserCodePresentation _presentation = new UserCodePresentation();

        public UserCode()
        {
            _settings = new UserCodeSettings();
            _presentation.TextBox.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);
            _presentation.TextBox.CurrentHighlighter = HighlighterManager.Instance.Highlighters["CSharp"];
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            _settings.Sourcecode = _presentation.TextBox.Text;
        }

        #region Properties

        private object _input1;
        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip")]
        public object Input1
        {
            get => _input1;
            set
            {
                _input1 = value;
                OnPropertyChanged("Input1");
            }
        }

        private object _input2;
        [PropertyInfo(Direction.InputData, "Input2Caption", "Input2Tooltip")]
        public object Input2
        {
            get => _input2;
            set
            {
                _input2 = value;
                OnPropertyChanged("Input2");
            }
        }

        private object _input3;
        [PropertyInfo(Direction.InputData, "Input3Caption", "Input3Tooltip")]
        public object Input3
        {
            get => _input3;
            set
            {
                _input3 = value;
                OnPropertyChanged("Input3");
            }
        }

        private object _input4;
        [PropertyInfo(Direction.InputData, "Input4Caption", "Input4Tooltip")]
        public object Input4
        {
            get => _input4;
            set
            {
                _input4 = value;
                OnPropertyChanged("Input4");
            }
        }

        private object _input5;
        [PropertyInfo(Direction.InputData, "Input5Caption", "Input5Tooltip")]
        public object Input5
        {
            get => _input5;
            set
            {
                _input5 = value;
                OnPropertyChanged("Input5");
            }
        }

        private object _output1;
        [PropertyInfo(Direction.OutputData, "Output1Caption", "Output1Tooltip")]
        public object Output1
        {
            get => _output1;
            set
            {
                _output1 = value;
                OnPropertyChanged("Output1");
            }
        }

        private object _output2;
        [PropertyInfo(Direction.OutputData, "Output2Caption", "Output2Tooltip")]
        public object Output2
        {
            get => _output2;
            set
            {
                _output2 = value;
                OnPropertyChanged("Output2");
            }
        }

        private object _output3;
        [PropertyInfo(Direction.OutputData, "Output3Caption", "Output3Tooltip")]
        public object Output3
        {
            get => _output3;
            set
            {
                _output3 = value;
                OnPropertyChanged("Output3");
            }
        }

        private object _output4;
        [PropertyInfo(Direction.OutputData, "Output4Caption", "Output4Tooltip")]
        public object Output4
        {
            get => _output4;
            set
            {
                _output4 = value;
                OnPropertyChanged("Output4");
            }
        }

        private object _output5;
        [PropertyInfo(Direction.OutputData, "Output5Caption", "Output5Tooltip")]
        public object Output5
        {
            get => _output5;
            set
            {
                _output5 = value;
                OnPropertyChanged("Output5");
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private UserCodeSettings _settings;

        public ISettings Settings
        {
            get => _settings;
            set => _settings = (UserCodeSettings)value;
        }

        public UserControl Presentation => _presentation;

        public void PreExecution()
        {

        }

        public void Execute()
        {
            AppDomain appDomain = null;
            ObjectHandle userCodeObject = null;
            CompilerResults compilerResults = null;

            //1. Compile:

            try
            {
                Random rnd = new Random();
                int id = rnd.Next(int.MaxValue);
                appDomain = AppDomain.CreateDomain("UserCodeDomain" + id);
                CSharpCodeProvider cs = new CSharpCodeProvider();
                ICodeCompiler cc = cs.CreateCompiler();
                CompilerParameters cp = new CompilerParameters { GenerateInMemory = false, CompilerOptions = "/optimize" };
                cp.ReferencedAssemblies.Add(GetType().Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(IPlugin).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(Exception).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(INotifyPropertyChanged).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(BigInteger).Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(AppDomain).Assembly.Location);
                string code = Properties.Resources.UserClass.Replace("//USERCODE//", _settings.Sourcecode);
                compilerResults = cc.CompileAssemblyFromSource(cp, code);
                if (compilerResults.Errors.Count > 0)
                {
                    foreach (object error in compilerResults.Errors)
                    {
                        GuiLogMessage(string.Format("Compile error: {0}", error.ToString()), NotificationLevel.Error);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during code compilation: {0}", ex.Message), NotificationLevel.Error);
                return;
            }

            //2. Execute:

            try
            {
                appDomain.SetData("input1", Input1);
                appDomain.SetData("input2", Input2);
                appDomain.SetData("input3", Input3);
                appDomain.SetData("input4", Input4);
                appDomain.SetData("input5", Input5);

                userCodeObject = appDomain.CreateInstanceFrom(compilerResults.PathToAssembly, "CrypTool.Plugins.UserCode.UserClass");

                Output1 = appDomain.GetData("output1");
                Output2 = appDomain.GetData("output2");
                Output3 = appDomain.GetData("output3");
                Output4 = appDomain.GetData("output4");
                Output5 = appDomain.GetData("output5");

            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during code execution: {0}", ex.Message), NotificationLevel.Error);
            }

            //3. Unload:

            try
            {
                userCodeObject = null;
                AppDomain.Unload(appDomain);
                File.Delete(compilerResults.PathToAssembly);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during removing of assembly: {0}", ex.Message), NotificationLevel.Error);
                return;
            }

            ProgressChanged(1.0, 1.0);
        }

        public void PostExecution()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.TextBox.Text = _settings.Sourcecode;
            }
            , null);
        }

        public void Dispose()
        {

        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }

}
