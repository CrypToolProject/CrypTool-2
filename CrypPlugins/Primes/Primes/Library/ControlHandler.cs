/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Primes.Library
{
    public static class ControlHandler
    {
        #region Contructors

        private static readonly object m_CreateObjectlockObject;
        private static readonly object m_SetPropertyValuelockObject;
        private static readonly object m_GetPropertyValuelockObject;
        private static readonly object m_ExecuteMethodlockObject;

        private delegate object CreateObjectDelegate(Type type);
        private delegate object CreateObjectWithParametersDelegate(Type t, Type[] parametertypes, object[] parameters);
        private delegate void SetValueDelegate(object obj, string valuename, object value);
        private delegate object ExecuteMethodDelegate(object obj, string methodname, object[] parameters);
        private delegate object GetPropertyValueDelegate(object obj, string valuename);
        private static Dispatcher m_Dispatcher;

        public static Dispatcher Dispatcher
        {
            set => m_Dispatcher = value;
        }

        static ControlHandler()
        {
            m_CreateObjectlockObject = new object();
            m_SetPropertyValuelockObject = new object();
            m_ExecuteMethodlockObject = new object();
            m_GetPropertyValuelockObject = new object();
        }

        #endregion

        #region Buttons

        public static void SetButtonEnabled(Button btn, bool enabled)
        {
            SetPropertyValue(btn, "IsEnabled", enabled);
        }

        public static void EnableButton(Button btn)
        {
            SetButtonEnabled(btn, true);
        }

        public static void DisableButton(Button btn)
        {
            SetButtonEnabled(btn, false);
        }

        public static void SetButtonVisibility(Button btn, Visibility visibility)
        {
            SetPropertyValue(btn, "Visibility", visibility);
        }

        #endregion

        public static Label CreateLabel(string text, Style style)
        {
            Label result = CreateObject(typeof(Label)) as Label;
            SetPropertyValue(result, "Content", text);

            if (style == null)
            {
                SetPropertyValue(result, "Foreground", Brushes.Black);
            }
            else
            {
                SetPropertyValue(result, "Style", style);
            }

            return result;
        }

        public static void ClearChildren(FrameworkElement container)
        {
            UIElementCollection children = GetPropertyValue(container, "Children") as UIElementCollection;
            int count = (int)GetPropertyValue(children, "Count");
            while (count > 0)
            {
                ExecuteMethod(children, "Clear");
                count = (int)GetPropertyValue(children, "Count");
            }
        }

        public static void AddChild(UIElement child, IAddChild container)
        {
            ExecuteMethod(container, "AddChild", new object[] { child });
        }

        public static void AddChild(UIElement child, Panel container)
        {
            UIElementCollection children = ControlHandler.GetPropertyValue(container, "Children") as UIElementCollection;

            ControlHandler.ExecuteMethod(children, "Add", new object[] { child });
        }

        public static void SetElementContent(ContentControl element, object value)
        {
            SetPropertyValue(element, "Content", value);
        }

        public static void SetCursor(FrameworkElement owner, Cursor dest)
        {
            SetPropertyValue(owner, "Cursor", dest);
        }

        #region Grid

        public static GridLength CreateGridLength(double value, GridUnitType type)
        {
            GridLength gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { value, type });
            return gl;
        }

        public static RowDefinition CreateRowDefintion()
        {
            RowDefinition rd = ControlHandler.CreateObject(typeof(RowDefinition)) as RowDefinition;
            return rd;
        }

        public static RowDefinition CreateRowDefintion(GridLength gl)
        {
            RowDefinition rd = ControlHandler.CreateRowDefintion();
            ControlHandler.SetPropertyValue(rd, "Height", gl);
            return rd;
        }

        public static void AddRowDefintion(Grid g, double value, GridUnitType type)
        {
            RowDefinitionCollection rowDefinitions =
              ControlHandler.GetPropertyValue(g, "RowDefinitions") as RowDefinitionCollection;

            GridLength gl = ControlHandler.CreateGridLength(value, type);
            RowDefinition rd = ControlHandler.CreateRowDefintion(gl);
            ControlHandler.ExecuteMethod(rowDefinitions, "Add", new object[] { rd });
        }

        public static ColumnDefinition CreateColumnDefintion()
        {
            ColumnDefinition cd = ControlHandler.CreateObject(typeof(ColumnDefinition)) as ColumnDefinition;
            return cd;
        }

        public static ColumnDefinition CreateColumnDefintion(GridLength gl)
        {
            ColumnDefinition cd = ControlHandler.CreateColumnDefintion();
            ControlHandler.SetPropertyValue(cd, "Width", gl);
            return cd;
        }

        public static void AddColumnDefintion(Grid g, double value, GridUnitType type)
        {
            ColumnDefinitionCollection colDefinitions =
              ControlHandler.GetPropertyValue(g, "ColumnDefinitions") as ColumnDefinitionCollection;

            GridLength gl = ControlHandler.CreateGridLength(value, type);
            ColumnDefinition rd = ControlHandler.CreateColumnDefintion(gl);
            ControlHandler.ExecuteMethod(colDefinitions, "Add", new object[] { rd });
        }

        #endregion

        #region Object independent Functions

        public static object CreateObject(Type type)
        {
            //lock (m_CreateObjectlockObject)
            {
                return m_Dispatcher.Invoke(DispatcherPriority.Send, new CreateObjectDelegate(DoCreateObject), type);
            }
        }

        public static object CreateObject(Type t, Type[] parametertypes, object[] parameters)
        {
            //lock (m_CreateObjectlockObject)
            {
                return m_Dispatcher.Invoke(DispatcherPriority.Send, new CreateObjectWithParametersDelegate(DoCreateObject), t, new object[] { parametertypes, parameters });
            }
        }

        private static object DoCreateObject(Type t)
        {
            ConstructorInfo ci = t.GetConstructor(new Type[] { });
            return ci.Invoke(new object[] { });
        }

        private static object DoCreateObject(Type t, Type[] parametertypes, object[] parameters)
        {
            ConstructorInfo ci = t.GetConstructor(parametertypes);
            return ci.Invoke(parameters);
        }

        public static void SetPropertyValue(object obj, string valuename, object value)
        {
            SetPropertyValue(m_Dispatcher, obj, valuename, value);
        }

        public static void SetPropertyValue(Dispatcher dispatcher, object obj, string valuename, object value)
        {
            if (obj != null)
            {
                //lock (m_SetPropertyValuelockObject)
                {
                    dispatcher.Invoke(DispatcherPriority.Send, new SetValueDelegate(DoSetPropertyValue), obj, new object[] { valuename, value });
                }
            }
        }

        private static void DoSetPropertyValue(object obj, string valuename, object value)
        {
            obj.GetType().GetProperty(valuename).SetValue(obj, value, null);
        }

        public static object GetPropertyValue(object obj, string valuename)
        {
            return GetPropertyValue(m_Dispatcher, obj, valuename);
        }

        public static object GetPropertyValue(Dispatcher dispatcher, object obj, string valuename)
        {
            if (obj != null)
            {
                //lock (m_SetPropertyValuelockObject)
                {
                    return dispatcher.Invoke(DispatcherPriority.Send, new GetPropertyValueDelegate(DoGetPropertyValue), obj, new object[] { valuename });
                }
            }
            else
            {
                return null;
            }
        }

        private static object DoGetPropertyValue(object obj, string valuename)
        {
            return obj.GetType().GetProperty(valuename).GetValue(obj, null);
        }

        public static object ExecuteMethod(object obj, string methodname)
        {
            return ExecuteMethod(m_Dispatcher, obj, methodname, new object[] { });
        }

        public static object ExecuteMethod(object obj, string methodname, object[] parameters)
        {
            return ExecuteMethod(m_Dispatcher, obj, methodname, parameters);
        }

        public static object ExecuteMethod(object obj, string methodname, object parameter)
        {
            return ExecuteMethod(m_Dispatcher, obj, methodname, new object[] { parameter });
        }

        public static object ExecuteMethod(Dispatcher dispatcher, object obj, string methodname, object[] parameters)
        {
            return dispatcher.Invoke(DispatcherPriority.Send, new ExecuteMethodDelegate(DoExecuteMethod), obj, new object[] { methodname, parameters });
        }

        public static object ExecuteMethod(Dispatcher dispatcher, object obj, string methodname, object[] parameters, Type[] types)
        {
            return dispatcher.Invoke(DispatcherPriority.Send, new ExecuteMethodDelegate(DoExecuteMethod), obj, new object[] { methodname, parameters, types });
        }

        private static object DoExecuteMethod(object obj, string methodname, object[] parameters)
        {
            Type[] types = null;
            List<object> _parameters = new List<object>();

            foreach (object o in parameters)
            {
                if (o.GetType() == typeof(Type[]))
                {
                    types = (Type[])o;
                }
                else
                {
                    _parameters.Add(o);
                }
            }

            MethodInfo mi = null;

            if (types != null)
            {
                mi = obj.GetType().GetMethod(methodname, types);
            }
            else
            {
                mi = obj.GetType().GetMethod(methodname);
            }

            if (mi == null)
            {
                foreach (Type t in obj.GetType().GetInterfaces())
                {
                    if (types != null)
                    {
                        mi = t.GetType().GetMethod(methodname, types);
                    }
                    else
                    {
                        mi = t.GetType().GetMethod(methodname);
                    }
                    if (mi != null)
                    {
                        break;
                    }
                }
            }

            try
            {
                if (mi != null)
                {
                    return mi.Invoke(obj, _parameters.ToArray());
                }
            }
            catch
            {
            }

            return null;
        }

        #endregion
    }
}
