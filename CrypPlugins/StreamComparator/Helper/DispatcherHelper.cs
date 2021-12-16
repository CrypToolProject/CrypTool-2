/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.Reflection;
using System.Windows.Threading;

namespace CrypTool.StreamComparator
{
    public static class DispatcherHelper
    {
        private delegate object ExecuteMethodDelegate(object obj, string methodname, object[] parameters);
        private delegate object GetPropertyDelegate(object obj, string property);
        private delegate void SetPropertyDelegate(object obj, string property, object[] parameters);

        //ExecuteMethod(dispatcher, m_List, "Add", new object[] { 1 });

        public static object ExecuteMethod(Dispatcher dispatcher, object obj, string methodname, object[] parameters)
        {
            return dispatcher.Invoke(DispatcherPriority.Normal, new ExecuteMethodDelegate(DoExecuteMethod), obj, new object[] { methodname, parameters });
        }

        public static object GetProperty(Dispatcher dispatcher, object obj, string property)
        {
            return dispatcher.Invoke(DispatcherPriority.Normal, new GetPropertyDelegate(DoGetProperty), obj, property);
        }

        public static void SetProperty(Dispatcher dispatcher, object obj, string property, object[] parameters)
        {
            if (dispatcher != null && property != null && parameters != null && parameters.Length == 1)
            {
                dispatcher.Invoke(DispatcherPriority.Normal, new SetPropertyDelegate(DoSetProperty), obj, new object[] { property, parameters });
            }
        }

        private static void DoSetProperty(object obj, string property, object[] parameters)
        {
            PropertyInfo pi = obj.GetType().GetProperty(property);
            if (pi != null)
            {
                try
                {
                    pi.SetValue(obj, parameters[0], null);
                }
                catch
                {
                }
            }
        }

        private static object DoGetProperty(object obj, string property)
        {
            PropertyInfo pi = obj.GetType().GetProperty(property);

            if (pi != null)
            {
                try
                {
                    return pi.GetValue(obj, null);
                }
                catch
                {
                }
            }
            return null;
        }

        private static object DoExecuteMethod(object obj, string methodname, object[] parameters)
        {
            MethodInfo mi = obj.GetType().GetMethod(methodname);

            if (mi == null)
            {
                foreach (Type t in obj.GetType().GetInterfaces())
                {
                    mi = t.GetMethod(methodname);
                    if (mi != null)
                    {
                        break;
                    }
                }
            }
            if (mi != null)
            {
                try
                {
                    return mi.Invoke(obj, parameters);
                }
                catch
                {

                }
            }
            return null;
        }
    }
}
