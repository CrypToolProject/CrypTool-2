using System.Collections.Generic;
using System.Text;
using OnlineDocumentationGenerator.Properties;

namespace OnlineDocumentationGenerator.Reference
{
    public class ReferenceList
    {
        private List<Reference> _references = new List<Reference>();

        public void Add(Reference reference)
        {
            _references.Add(reference);
        }

        public string GetHTMLinkToRef(string refID)
        {
            int c = 1;
            foreach (var reference in _references)
            {
                if (reference.ID == refID)
                {
                    return string.Format("<a href=\"#{0}\">[{1}]</a>", refID, c);
                }
                c++;
            }
            return null;
        }

        public string ToHTML(string lang)
        {
            if (_references.Count == 0)
            {
                return Resources.NoContent;
            }

            var builder = new StringBuilder();
            builder.AppendLine("<p><table border=\"0\" width=\"90%\">");

            int no = 1;
            foreach (var reference in _references)
            {
                if (reference.ID != null)
                {
                    builder.AppendLine(string.Format("<tr><td align=\"right\" valign=\"top\"><div id=\"{0}\">[{1}]</div></td><td valign=\"top\">{2}</td></tr>", reference.ID, no, reference.ToHTML(lang)));
                }
                else
                {
                    builder.AppendLine(string.Format("<tr><td align=\"right\" valign=\"top\"><div>[{0}]</div></td><td valign=\"top\">{1}</td></tr>", no, reference.ToHTML(lang)));
                }
                builder.AppendLine();
                no++;
            }

            builder.AppendLine("</table></p>");
            return builder.ToString();
        }

        public string ToLaTeX(string lang)
        {
            if (_references.Count == 0)
            {
                return Resources.NoContent;
            }

            var builder = new StringBuilder();
            builder.AppendLine("<p><table border=\"0\" width=\"90%\">");

            int no = 1;
            foreach (var reference in _references)
            {
                if (reference.ID != null)
                {
                    builder.AppendLine(string.Format("<tr><td align=\"right\" valign=\"top\"><div id=\"{0}\">[{1}]</div></td><td valign=\"top\">{2}</td></tr>", reference.ID, no, reference.ToHTML(lang)));
                }
                else
                {
                    builder.AppendLine(string.Format("<tr><td align=\"right\" valign=\"top\"><div>[{0}]</div></td><td valign=\"top\">{1}</td></tr>", no, reference.ToHTML(lang)));
                }
                builder.AppendLine();
                no++;
            }

            builder.AppendLine("</table></p>");
            return builder.ToString();
        }
    }
}
