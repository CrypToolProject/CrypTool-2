using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlineDocumentationGenerator.Generators.LaTeXGenerator
{
    class Helper
    {
        public static string EscapeLaTeX(string value)
        {
            var sb = new StringBuilder(value);

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

            var greekLetters = new Dictionary<char, string>() {
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
            foreach (var letter in sb.ToString().ToLower().Reverse())
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
