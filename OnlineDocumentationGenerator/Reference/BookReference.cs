using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Reference
{
    public class BookReference : Reference
    {
        public string Name
        {
            get
            {
                return GetLocalizedProperty("Name", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            }
        }

        public string Author
        {
            get
            {
                return GetLocalizedProperty("Author", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            }
        }

        public string Publisher
        {
            get
            {
                return GetLocalizedProperty("Publisher", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            }
        }

        public string Year
        {
            get
            {
                return GetLocalizedProperty("Year", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            }
        }

        public BookReference(XElement linkReferenceElement) : base(linkReferenceElement)
        {
            foreach (var e in linkReferenceElement.Elements())
            {
                var lang = "en";
                if (e.Attribute("lang") != null)
                {
                    var cult = new CultureInfo(e.Attribute("lang").Value);
                    lang = cult.TwoLetterISOLanguageName;
                }

                if (e.Name == "author")
                {
                    SetLocalizedProperty("Author", lang, e.Value);
                }
                else if (e.Name == "publisher")
                {
                    SetLocalizedProperty("Publisher", lang, e.Value);
                }
                else if (e.Name == "name")
                {
                    SetLocalizedProperty("Name", lang, e.Value);
                }
                else if (e.Name == "year")
                {
                    SetLocalizedProperty("Year", lang, e.Value);
                }
            }
        }

        public override string ToHTML(string lang)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(HttpUtility.HtmlEncode(Author));
            builder.Append(". ");
            builder.Append(HttpUtility.HtmlEncode(Name));
            builder.Append(". ");
            builder.Append("<i>" + HttpUtility.HtmlEncode(Publisher) + "</i>");
            builder.Append(" (" + HttpUtility.HtmlEncode(Year) + ")");            
            return builder.ToString();
        }
    }
}
