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
using CrypTool.Core;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;


namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //added this to fix problems that may occur while copying from clipboard:
            //see https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
            System.Runtime.InteropServices.COMException comException = e.Exception as System.Runtime.InteropServices.COMException;
            if (comException != null && comException.ErrorCode == -2147221040)
            {
                e.Handled = true;
            }
            else
            {
                //Prevent the application from crashing because of unhandled exceptions in GUI thread. Only show error dialog an continue instead:
                UnhandledExceptionDialog.ShowModalDialog(e.Exception, AssemblyHelper.Version, AssemblyHelper.InstallationType.ToString(), AssemblyHelper.BuildType.ToString(), AssemblyHelper.ProductName);
                e.Handled = true;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                UnhandledExceptionDialog.ShowModalDialog((Exception)e.ExceptionObject, AssemblyHelper.Version, AssemblyHelper.InstallationType.ToString(), AssemblyHelper.BuildType.ToString(), AssemblyHelper.ProductName);
            }, null);
        }



        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandledExceptionDialog.ShowModalDialog(e.Exception, AssemblyHelper.Version, AssemblyHelper.InstallationType.ToString(), AssemblyHelper.BuildType.ToString(), AssemblyHelper.ProductName);
        }
    }
}
