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
using OnlineDocumentationGenerator.Properties;
using System.Collections.Generic;
using System.Text;

namespace OnlineDocumentationGenerator.Reference
{
    public class ReferenceList
    {
        private readonly List<Reference> _references = new List<Reference>();

        public void Add(Reference reference)
        {
            _references.Add(reference);
        }

        public string GetHTMLinkToRef(string refID)
        {
            int c = 1;
            foreach (Reference reference in _references)
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

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<p><table border=\"0\" width=\"90%\">");

            int no = 1;
            foreach (Reference reference in _references)
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

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<p><table border=\"0\" width=\"90%\">");

            int no = 1;
            foreach (Reference reference in _references)
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
