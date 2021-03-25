using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateEditor
{
    public class LocalizedTemplateData
    {
        public string Lang { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Keywords { get; set; }

        public object AllKeywords
        {
            get
            {
                var res = "";
                if (Keywords == null)
                    return res;

                foreach (var k in Keywords)
                {
                    res += k + ", ";
                }

                if (res.Length > 0)
                {
                    return res.Substring(0, res.Length - 2);
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
