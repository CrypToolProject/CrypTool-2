/*                              
   Copyright 2010 Nils Kopal

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
using System.Windows;
using WorkspaceManagerModel.Model.Interfaces;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Abstract super class for all Model Elements which will have
    /// a representation by a View class. It is needed to represent 
    /// Coordinates and Dimensions
    /// </summary>
    [Serializable]
    public abstract class VisualElementModel
    {
        internal VisualElementModel()
        {

        }

        /// <summary>
        /// The Zindex is the "layer" in which this Element is located
        /// </summary>
        public int ZIndex;

        /// <summary>
        /// Current Position of this VisualElementModel
        /// </summary>
        internal Point Position;

        /// <summary>
        /// Minimum Width of this VisualElementModel
        /// </summary>
        internal double MinWidth = 250;

        /// <summary>
        /// Minimum Height of this VisualElementModel
        /// </summary>      
        internal double MinHeight = 200;

        /// <summary>
        /// Width of this VisualElementModel
        /// </summary>
        internal double Width = 0;

        /// <summary>
        /// Height of this VisualElementModel
        /// </summary>
        internal double Height = 0;

        /// <summary>
        /// Name of this VisualElementModel
        /// </summary>
        internal string Name;

        /// <summary>
        /// Indicates that this Model Element needs a GUI Update
        /// </summary>
        public bool GuiNeedsUpdate { get; set; }

        /// <summary>
        /// View Element of this VisualElement
        /// </summary>
        [NonSerialized]
        public IUpdateableView UpdateableView = null;

        /// <summary>
        /// Get the current Position of this VisualElementModel
        /// </summary>
        /// <returns></returns>
        public Point GetPosition()
        {
            return Position;
        }

        /// <summary>
        /// Get the minimum width of this VisualElementModel
        /// </summary>
        /// <returns></returns>        
        public double GetMinWidth()
        {
            return MinWidth;
        }

        /// <summary>
        /// Get the minimum height of this VisualElementModel
        /// </summary>
        /// <returns></returns>        
        public double GetMinHeight()
        {
            return MinHeight;
        }

        /// <summary>
        /// Get the width of this VisualElementModel
        /// </summary>
        /// <returns></returns>
        public double GetWidth()
        {
            return Width;
        }

        /// <summary>
        /// Get the current height of this VisualElementModel
        /// </summary>
        /// <returns></returns>
        public double GetHeight()
        {
            return Height;
        }

        /// <summary>
        /// Get the name of thisVisualElementModel
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return Name;
        }

    }

}
