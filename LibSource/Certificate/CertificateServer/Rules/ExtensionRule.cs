using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.Util.Logging;
using System.Collections;
using CrypTool.CertificateLibrary.Network;
using System.Xml.Serialization;

namespace CrypTool.CertificateServer.Rules
{

    /// <summary>
    /// Represents a single certificate extension rule.
    /// </summary>
    [Serializable]
    public class ExtensionRule : IComparable
    {

        public ExtensionRule()
            : this(0, new Extension(), new Filter())
        {
        }

        public ExtensionRule(int id, Extension ext, params Filter[] filters)
        {
            if (ext == null)
            {
                Log.Warn(String.Format("Error in extension rule with ID: {0} | No extension specified!", id));
                this.Id = 0;
                return;
            }
            if (filters == null || filters.Length == 0)
            {
                Log.Warn(String.Format("Error in extension rule with ID: {0} | No filter specified!", id));
                this.Id = 0;
                return;
            }
            this.Id = id;
            this.Filters = filters;
            this.Extension = ext;
        }

        public bool IsValidExtensionRule()
        {
            if (this.Filters == null)
            {
                return false;
            }
            foreach (Filter filter in this.Filters)
            {
                if (!filter.IsValidFilter())
                {
                    return false;
                }
            }
            if (!this.Extension.IsValidExtension())
            {
                return false;
            }
            return true;
        }

        public Extension GetExtension(CertificateRegistration certReg)
        {
            if (certReg == null)
            {
                throw new ArgumentNullException("Certificate Registration can not be null!");
            }
            bool doesMatch = true;
            foreach (Filter filter in this.Filters)
            {
                if (filter.Attribute == "avatar" && !filter.IsMatch(certReg.Avatar))
                {
                    doesMatch = false;
                    break;
                } else if (filter.Attribute == "email" && !filter.IsMatch(certReg.Email))
                {
                    doesMatch = false;
                    break;
                } else if (filter.Attribute == "world" && !filter.IsMatch(certReg.World))
                {
                    doesMatch = false;
                    break;
                }
            }
            return (doesMatch) ? this.Extension : null;
        }

        public int CompareTo(object obj)
        {
            ExtensionRule rule = obj as ExtensionRule;
            if (rule == null)
            {
                throw new ArgumentException("Can not compare an extension rule to something that is not a rule!");
            }
            return Id.CompareTo(rule.Id);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerator filterEnum = this.Filters.GetEnumerator();
            if (filterEnum.MoveNext())
            {
                Filter firstFilter = filterEnum.Current as Filter;
                sb.Append(String.Format("'{0}' = '{1}'", firstFilter.Attribute, firstFilter.RegEx));

                while (filterEnum.MoveNext())
                {
                    Filter filter = filterEnum.Current as Filter;
                    sb.Append(String.Format(" AND '{0}' = '{1}'", filter.Attribute, filter.RegEx));
                }
                sb.Append(String.Format(" -> '{0}'", this.Extension.ToString()));
                return sb.ToString();
            }
            else
            {
                return "No filter found";
            }
        }

        #region Properties

        [XmlAttribute]
        public int Id { get; set; }

        public Filter[] Filters { get; set; }

        public Extension Extension { get; set; }

        #endregion

    }
}
