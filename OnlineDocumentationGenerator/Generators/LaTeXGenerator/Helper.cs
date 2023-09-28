/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlineDocumentationGenerator.Generators.LaTeXGenerator
{
    internal class Helper
    {
        public static string EscapeLaTeX(string value)
        {
            StringBuilder sb = new StringBuilder(value);

            sb.Replace("\\", "$\\textbackslash$")
                .Replace("{", "\\{").Replace("}", "\\}")
                .Replace("[", "{[}").Replace("]", "{]}")
                .Replace("#", "{\\#}").Replace("_", "{\\_}")
                .Replace("&", "\\&").Replace("%", "\\%")
                .Replace("~", "{\\textasciitilde}").Replace("^", "{\\textasciicircum}")
                .Replace("`", "{\\glq}").Replace("´", "{\\grq}")
                .Replace("\"", "{\\textquotedblright}").Replace("“", "\"'").Replace("„", "\"`")
                .Replace("∈", "$\\in$")
                .Replace("∨", "$\\vee$")
                .Replace("∧", "$\\wedge$")
                .Replace("¬", "$\\lnot$")
                .Replace("<", "{\\textless}")
                .Replace(">", "{\\textgreater}");

            Dictionary<char, string> greekLetters = new Dictionary<char, string>() {
                  //{'ά', " \\'{$\\alpha$} "}
                  {'ά', "$\\alpha$"}
                , {'α', "$\\alpha$"}, {'β', "$\\beta$"}, {'γ', "$\\gamma$"}
                , {'δ', "$\\delta$"}, {'ε', "$\\epsilon$ "}, {'ζ', "$\\zeta$"}
                , {'η', "$\\eta$"}, {'θ', "$\\theta$"}, {'ί', "$\\iota$"}, {'ι', "$\\iota$"}
                , {'κ', "$\\kappa$"}, {'λ', "$\\lambda$"}, {'μ', "$\\mu$"}
                , {'ν', "$\\nu$"}, {'ξ', "$\\xi$"}, {'ο', "o"}, {'ό', "\\'{o}"}
                , {'π', "$\\pi$"}, {'ρ', "$\\rho$"}, {'σ', "$\\sigma$"}
                , {'τ', "$\\tau$"}, {'υ', "$\\upsilon$"}, {'φ', "$\\phi$"}
                , {'χ', "$\\chi$"}, {'ψ', "$\\psi$"}, {'ω', "$\\omega$"}
                , {'ə', "{\\textschwa}"}
                , {'ˈ', "{\\textprimstress}"}
                , {'ɪ', "{\\textsci}"}
                , {'ː', "{\\textlengthmark}"}
                , {'ʒ', "{\\textyogh}"}
                , {'ɛ', "{\\textepsilon}"}
                , {'ʁ', "{\\textinvscr}"}
                , {'ˌ', "{\\textsecstress}"}
            };

            int c = sb.Length - 1;
            foreach (char letter in sb.ToString().ToLower().Reverse())
            {
                if (greekLetters.ContainsKey(letter))
                {
                    sb.Remove(c, 1);
                    sb.Insert(c, greekLetters[letter]);
                }
                c--;
            }

            return sb.ToString();
        }
    }
}
