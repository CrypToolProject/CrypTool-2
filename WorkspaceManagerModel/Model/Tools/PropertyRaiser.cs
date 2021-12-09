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
    public static class PropertyRaiser
    {
        /// <summary>
        /// Raises an event defined by the eventName 
        /// 
        /// Source found at statckoverflow
        /// see https://stackoverflow.com/questions/198543/how-do-i-raise-an-event-via-reflection-in-net-c
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="e"></param>
        public static void RaiseEvent(this object instance, string eventName, EventArgs e)
        {
            var type = instance.GetType();
            var eventField = type.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (eventField == null)
            {
                throw new Exception(string.Format("Event with name {0} could not be found", eventName));
            }
            var multicastDelegate = eventField.GetValue(instance) as MulticastDelegate;
            if (multicastDelegate == null) 
            {
                return; 
            }
            var invocationList = multicastDelegate.GetInvocationList();
            foreach (var invocationMethod in invocationList)
            {
                invocationMethod.DynamicInvoke(new[] { instance, e });
            }
        }
    }

}
