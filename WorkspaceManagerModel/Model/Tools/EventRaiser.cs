/*                              
   Copyright 2021 Nils Kopal

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

namespace WorkspaceManagerModel.Model.Tools
{
    public static class EventRaiser
    {
        /// <summary>
        /// Raises an event defined by the eventName 
        /// Searches for the event in the given type and all of its base types
        /// 
        /// Source found at statckoverflow
        /// see https://stackoverflow.com/questions/198543/how-do-i-raise-an-event-via-reflection-in-net-c
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="eventArgs"></param>
        public static void RaiseEvent(this object instance, string eventName, EventArgs eventArgs)
        {
            Type type = instance.GetType();
            FieldInfo eventField = null;
            while (eventField == null)
            {
                eventField = type.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (type.BaseType == null)
                {
                    return; //we did not find the event field, so we just return
                }
                type = type.BaseType; //search base type              
            }
            MulticastDelegate multicastDelegate = eventField.GetValue(instance) as MulticastDelegate;
            if (multicastDelegate == null)
            {
                return;
            }
            Delegate[] invocationList = multicastDelegate.GetInvocationList();
            foreach (Delegate invocationMethod in invocationList)
            {
                invocationMethod.DynamicInvoke(new[] { instance, eventArgs });
            }
        }
    }

}
