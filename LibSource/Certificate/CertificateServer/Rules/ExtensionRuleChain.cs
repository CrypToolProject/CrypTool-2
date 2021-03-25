using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.CertificateLibrary.Network;

namespace CrypTool.CertificateServer.Rules
{
    public class ExtensionRuleChain
    {
        public ExtensionRuleChain()
        {
            this.Rules = new ExtensionRule[0];
        }

        public void SortAndTrim()
        {
            List<ExtensionRule> trimmedRules = new List<ExtensionRule>();
            foreach (ExtensionRule rule in this.Rules)
            {
                if (rule.Id > 0 && rule.IsValidExtensionRule())
                {
                    trimmedRules.Add(rule);
                }
            }
            this.Rules = trimmedRules.ToArray();
            Array.Sort(this.Rules);
        }

        public Extension[] GetExtensions(CertificateRegistration certReg)
        {
            HashSet<Extension> extSet = new HashSet<Extension>(new ExtensionEqualityComparer());
            foreach (ExtensionRule rule in this.Rules)
            {
                Extension result = rule.GetExtension(certReg);
                if (result != null)
                {
                    extSet.Add(result);
                }
            }
            Extension[] extArray = new Extension[extSet.Count];
            extSet.CopyTo(extArray);
            return extArray;
        }

        public override string ToString()
        {
            if (this.Rules.Length == 0)
            {
                return "The extension rule chain is empty!";
            }
            StringBuilder sb = new StringBuilder();
            foreach (ExtensionRule rule in this.Rules)
            {
                sb.AppendLine(rule.ToString());
            }
            return sb.ToString();
        }

        public void GenerateExampleRule(int id, string attribute, string regEx, string oid, string value)
        {
            ExtensionRule[] newRules = new ExtensionRule[this.Rules.Length + 1];
            Array.Copy(this.Rules, newRules, this.Rules.Length);
            newRules[this.Rules.Length] = new ExtensionRule(id, new Extension(oid, value), new Filter(attribute, regEx));
            this.Rules = newRules;
        }

        public ExtensionRule[] Rules { get; set; }
    }
}
