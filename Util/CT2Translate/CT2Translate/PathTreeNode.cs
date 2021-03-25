using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1.Properties
{
    class PathTreeNode : TreeNode
    {
        string fullpath;
        string relpath;

        PathTreeNode( string f, string r )
        {
            fullpath = f;
            relpath = r;
        }
    }
}
