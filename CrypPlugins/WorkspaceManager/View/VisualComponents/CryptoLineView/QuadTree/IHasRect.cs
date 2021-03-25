/*                              
   Copyright 208 Michael Coyle 

   Licensed under the Code Project Open License (CPOL) 1.02
*/

using System.Drawing;

namespace QuadTreeLib
{
    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IHasRect
    {
        RectangleF Rectangle { get; }
    }
}
