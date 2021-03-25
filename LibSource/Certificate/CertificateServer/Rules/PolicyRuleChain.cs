using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.CertificateLibrary.Network;
using CrypTool.Util.Logging;
using System.Xml.Serialization;

namespace CrypTool.CertificateServer.Rules
{
    [Serializable]
    public class PolicyRuleChain
    {
        public PolicyRuleChain()
            : this("manual")
        {
        }

        public PolicyRuleChain(string defaultPolicy)
        {
            this.Rules = new PolicyRule[0];

            if (defaultPolicy == null)
            {
                Log.Warn("No default policy set. Using 'manual' as default!");
                defaultPolicy = "manual";
            }
            this.DefaultPolicy = defaultPolicy;
        }

        public void SortAndTrim()
        {
            List<PolicyRule> trimmedRules = new List<PolicyRule>();
            foreach (PolicyRule rule in this.Rules)
            {
                if (rule.Id > 0 && rule.IsValidPolicyRule())
                {
                    trimmedRules.Add(rule);
                }
            }
            this.Rules = trimmedRules.ToArray();
            Array.Sort(this.Rules);
        }

        public string GetPolicy(CertificateRegistration certReg)
        {
            string result = null;
            foreach (PolicyRule rule in this.Rules)
            {
                result = rule.GetPolicy(certReg);
                if (result != null)
                {
                    break;
                }
            }
            return result ?? this.DefaultPolicy;
        }

        public void GenerateExampleRule(int id, string attribute, string regEx, string policy)
        {
            PolicyRule[] newRules = new PolicyRule[this.Rules.Length + 1];
            Array.Copy(this.Rules, newRules, this.Rules.Length);
            newRules[this.Rules.Length] = new PolicyRule(id, policy, new Filter(attribute, regEx));
            this.Rules = newRules;
        }

        public override string ToString()
        {
            if (this.Rules.Length == 0)
            {
                return "The policy rule chain is empty! Default policy: " + this.DefaultPolicy;
            }
            StringBuilder sb = new StringBuilder();
            foreach (PolicyRule rule in this.Rules)
            {
                sb.AppendLine(rule.ToString());
            }
            sb.AppendLine("Default Policy is set to:                " + this.DefaultPolicy);
            return sb.ToString();
        }

        public PolicyRule[] Rules { get; set; }

        [XmlAttribute]
        public string DefaultPolicy 
        {
            get { return this.defaultPolicy; }
            set
            {
                if (value == null)
                {
                    this.defaultPolicy = "manual";
                    Log.Warn("No default policy specified. Using 'manual' as default!");
                }
                else
                {
                    this.defaultPolicy = value.ToLower();
                    if (!this.defaultPolicy.Equals("accept") && !this.defaultPolicy.Equals("deny") && !this.defaultPolicy.Equals("manual"))
                    {
                        Log.Warn("Invalid default policy. Using 'manual' as default!");
                        this.defaultPolicy = "manual";
                    }
                }
            }
        }

        private string defaultPolicy;
    }
}
