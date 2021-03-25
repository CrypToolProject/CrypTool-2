using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    class Consistency
    {
        public static void main(String[] args)
        {
            String c3 = "avgshazbhgzslcwigilryddgikqrrgidtziaikwzypcfzriqznnyrkzllagfwf";
            String p3 = "ca ts ar ei nt en de dt ot ea ch us th at no te ve ry th in gi nx na tu re ha sa pu rp os ex".Replace(" ", "");
            test(p3, c3);

            String c4 = "vknhxogknsvlbxnqnukffucqvuxopbgxvgbxufvgxytmnuhnnrgvglkqbpvurwqdemkx";
            String p4 = "experience is the worst teacher it gives the test before presenting the lesxsonx".Replace(" ", "");
            test(p4, c4);

            String c7 = "kxhcponkmykrpslvyqryzpvfkxupkxzpptxdcrkxcxryxsbddskxzpyxepufpakoucpzcrtixuuciq";
            String p7 = "thegrasxsmaybegrexenerontheothersideofthefencebutxtheresprobablymoreofittomowx".Replace(" ", "");
            test(p7, c7);

            String c9 = ("sa cb av hm ka do st th ps" +
                    "mn qs fr hm sx bt su tw tg wg mh mc ok sd oz" +
                    "ts fy tw ts vc ec gs gt wl dl sr oz tb tl ps" +
                    "tg ex cm co dl kh wl wg mh ex av").Replace(" ", "");
            String p9 = "thecoxordinatesarenorthfortydegrexeszeropointfivethrexetwowestseventyfivedegreestwopointfivezerotwox";
            test(p9, c9);
        }

        private static void test(String p, String c)
        {
            String s = "";

            for (int i = 0; i < c.Length; i += 2)
            {
                bool found = false;
                for (int j = 0; j < c.Length; j += 2)
                {
                    if (j == i) continue;

                    if (c.Substring(i, 2).ToLower() == c.Substring(j, 2).ToLower())
                    {
                        s += c.Substring(i, 2);
                        found = true;
                        break;
                    }

                    if (c[i] == c[j + 1] && c[i + 1] == c[j])
                    {
                        s += c.Substring(i, 2);
                        found = true;
                        break;
                    }
                }

                if (!found)
                    s += "  ";
            }

            Console.Write(String.Format("{0}\n{1}\n{2}\n\n", c, s, p));

            for (int i = 0; i < c.Length; i += 2)
                Console.Write(String.Format("{0} {1} {2}{3}{4}{5}\n",
                        p.Substring(i, 2),
                        c.Substring(i, 2),
                        (c[i] == p[i + 1]) ? "!" : "",
                        (c[i + 1] == p[i]) ? "!" : "",
                        (c[i] == p[i]) ? "X" : "",
                        (c[i + 1] == p[i + 1]) ? "X" : ""));
        }
    }
}