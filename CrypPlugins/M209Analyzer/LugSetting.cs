using CrypTool.PluginBase.Utils.Datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M209Analyzer
{
    internal class LugSetting
    {
        public string[] Bar = new string[27] {
            "36","06","16","15","45","04","04","04","04",
            "20","20","20","20","20","20","20","20","20",
            "20","25","25","05","05","05","05","05","05"
        };

        public void Randomize()
        {

        }

        public LugSetting[] GetNeighborLugs(string V)
        {
            // variable neighborhood approach
            return new LugSetting[] {new LugSetting() };
        }

        public void ApplyTransformationSimple()
        {
            // These simple transformations consist of reducing the count of one type of bars, and increasing the count of another type.
        }

        public void ApplyTransformationComplex()
        {
            // Those consist of reducing the count of two types of bar, and increasing the counts of two other types.
        }
    }
}
