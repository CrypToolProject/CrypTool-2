using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public class Pair : ICloneable
    {
        public int LeftMember;
        public int RightMember;

        public Pair()
        {
            LeftMember = 0;
            RightMember = 0;
        }

        public Pair(int leftMember, int rightMember)
        {
            LeftMember = leftMember;
            RightMember = rightMember;
        }

        public object Clone()
        {
            var clone = new Pair()
            {
                LeftMember = this.LeftMember,
                RightMember = this.RightMember
            };
            return clone;
        }

        public override string ToString()
        {
            return ("LeftMember = " + LeftMember + " RightMember = " +RightMember);
        }
    }
}
