using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StatusGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            new StatusGenerator(Path.Combine("..", ".."), Path.Combine("..", "..", "PluginOverview.html"));
        }
    }
}
