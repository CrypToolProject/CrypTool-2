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

using Primes.Library.FactorTree;

namespace Primes.WpfControls.ShapeManagement.Ellipse
{
    public enum EllipseAllign { Left, Right }

    public class EllipseItem
    {
        public EllipseItem(GmpFactorTreeNode id)
        {
            ID = id;
        }

        #region Properties

        private EllipseItem m_Parent;

        public EllipseItem Parent
        {
            get => m_Parent;
            set => m_Parent = value;
        }

        private GmpFactorTreeNode m_ID;

        public GmpFactorTreeNode ID
        {
            get => m_ID;
            set => m_ID = value;
        }

        private object m_Value;

        public object Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        private object m_Description;

        public object Description
        {
            get => m_Description;
            set => m_Description = value;
        }

        private double m_X;

        public double X
        {
            get => m_X;
            set => m_X = value;
        }

        private double m_Y;

        public double Y
        {
            get => m_Y;
            set => m_Y = value;
        }

        private double m_Width;

        public double Width
        {
            get => m_Width;
            set => m_Width = value;
        }

        private double m_Height;

        public double Height
        {
            get => m_Height;
            set => m_Height = value;
        }

        private bool m_IsPrime;

        public bool IsPrime
        {
            get => m_IsPrime;
            set => m_IsPrime = value;
        }

        private bool m_IsRoot;

        public bool IsRoot
        {
            get => m_IsRoot;
            set => m_IsRoot = value;
        }

        private EllipseAllign m_Allign;

        public EllipseAllign Allign
        {
            get => m_Allign;
            set => m_Allign = value;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(EllipseItem))
            {
                return (obj as EllipseItem).m_ID == m_ID;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}