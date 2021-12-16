using System.Collections.Generic;

namespace QuickZip.MiniHtml2
{

    public class CurrentStateType
    {
        private readonly List<HtmlTag> activeStyle = new List<HtmlTag>();
        private bool bold;
        private bool italic;
        private bool underline;
        private string hyperlink;

        public bool Bold => bold;
        public bool Italic => italic;
        public bool Underline => underline;
        public string HyperLink => hyperlink;

        public void UpdateStyle(HtmlTag aTag)
        {
            if (!aTag.IsEndTag)
            {
                activeStyle.Add(aTag);
            }
            else
            {
                for (int i = activeStyle.Count - 1; i >= 0; i--)
                {
                    if ('/' + activeStyle[i].Name == aTag.Name)
                    {
                        activeStyle.RemoveAt(i);
                        break;
                    }
                }
            }

            updateStyle();
        }


        private void updateStyle()
        {
            bold = false;
            italic = false;
            underline = false;
            hyperlink = "";

            foreach (HtmlTag aTag in activeStyle)
            {
                switch (aTag.Name)
                {
                    case "b": bold = true; break;
                    case "i": italic = true; break;
                    case "u": underline = true; break;
                    case "a": if (aTag.Contains("href")) { hyperlink = aTag["href"]; } break;
                }
            }
        }

        public CurrentStateType()
        {

        }



    }

}
