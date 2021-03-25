using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CrypTool.Util.Logging;

namespace CrypTool.CertificateServer.Rules
{
    [Serializable]
    public class Filter
    {

        private Regex regExp;

        public Filter()
            : this(null, null)
        {
        }

        public Filter(string attribute, string regEx)
        {
            if (attribute != null)
            {
                this.Attribute = attribute.ToLower();
            }
            this.RegEx = regEx;
        }

        public bool IsValidFilter()
        {
            if (String.IsNullOrWhiteSpace(this.Attribute) || String.IsNullOrWhiteSpace(this.RegEx))
            {
                return false;
            }
            try
            {
                this.regExp = new Regex(this.RegEx);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("The entered filter '{0} = {1}' is no valid regular expression!", this.Attribute, this.RegEx), ex);
                return false;
            }
        }

        public bool IsMatch(string value)
        {
            if (this.Attribute == null || this.regExp == null)
            {
                return false;
            }
            return this.regExp.IsMatch(value);
        }

        [XmlAttribute]
        public string Attribute { get; set; }

        public string RegEx { get; set; }
    }
}
