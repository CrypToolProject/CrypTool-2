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
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class EventsHelper
    {
        //to allow undo/redo of settings, we forse everything to synchronous property changed events
        public static bool AsynchronousPropertyChanged { get; set; } = false;
        public static bool AsynchronousGuiLogMessage { get; set; } = true;
        public static bool AsynchronousProgressChanged { get; set; } = true;
        public static bool AsynchronousStatusChanged { get; set; } = true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GuiLogMessage(GuiLogNotificationEventHandler handler, IPlugin plugin, string message)
        {
            GuiLogMessage(handler, plugin, new GuiLogEventArgs(message, plugin, NotificationLevel.Debug));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GuiLogMessage(GuiLogNotificationEventHandler handler, IPlugin plugin, string message, NotificationLevel level)
        {
            GuiLogMessage(handler, plugin, new GuiLogEventArgs(message, plugin, level));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GuiLogMessage(GuiLogNotificationEventHandler handler, IPlugin plugin, GuiLogEventArgs args)
        {
            if (handler == null)
            {
                return;
            }
            Delegate[] delegates = handler.GetInvocationList();
            if (AsynchronousGuiLogMessage)
            {
                AsyncCallback cleanUp = delegate (IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (GuiLogNotificationEventHandler sink in delegates)
                {
                    sink.BeginInvoke(plugin, args, cleanUp, null);
                }
            }
            else
            {
                foreach (GuiLogNotificationEventHandler sink in delegates)
                {
                    sink.Invoke(plugin, args);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PropertyChanged(PropertyChangedEventHandler del, object sender, string propertyName)
        {
            PropertyChanged(del, sender, new PropertyChangedEventArgs(propertyName));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PropertyChanged(PropertyChangedEventHandler handler, object sender, PropertyChangedEventArgs args)
        {
            if (handler == null)
            {
                return;
            }
            Delegate[] delegates = handler.GetInvocationList();
            if (AsynchronousPropertyChanged)
            {
                AsyncCallback cleanUp = delegate (IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (PropertyChangedEventHandler sink in delegates)
                {
                    sink.BeginInvoke(sender, args, cleanUp, null);
                }
            }
            else
            {
                foreach (PropertyChangedEventHandler sink in delegates)
                {
                    sink.Invoke(sender, args);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProgressChanged(PluginProgressChangedEventHandler handler, IPlugin plugin, double value, double max)
        {
            ProgressChanged(handler, plugin, new PluginProgressEventArgs(value, max));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProgressChanged(PluginProgressChangedEventHandler handler, IPlugin plugin, PluginProgressEventArgs args)
        {
            if (handler == null)
            {
                return;
            }
            Delegate[] delegates = handler.GetInvocationList();
            if (AsynchronousProgressChanged)
            {
                AsyncCallback cleanUp = delegate (IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (PluginProgressChangedEventHandler sink in delegates)
                {
                    sink.BeginInvoke(plugin, args, cleanUp, null);
                }
            }
            else
            {
                foreach (PluginProgressChangedEventHandler sink in delegates)
                {
                    sink.Invoke(plugin, args);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StatusChanged(StatusChangedEventHandler handler, IPlugin plugin, StatusEventArgs args)
        {
            if (handler == null)
            {
                return;
            }
            Delegate[] delegates = handler.GetInvocationList();
            if (AsynchronousStatusChanged)
            {
                AsyncCallback cleanUp = delegate (IAsyncResult asyncResult)
                {
                    asyncResult.AsyncWaitHandle.Close();
                };
                foreach (StatusChangedEventHandler sink in delegates)
                {
                    sink.BeginInvoke(plugin, args, cleanUp, null);
                }
            }
            else
            {
                foreach (StatusChangedEventHandler sink in delegates)
                {
                    sink.Invoke(plugin, args);
                }
            }
        }
    }
}
