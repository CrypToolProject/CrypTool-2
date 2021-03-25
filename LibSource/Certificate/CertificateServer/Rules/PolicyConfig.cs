using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using CrypTool.Util.Logging;
using CrypTool.CertificateLibrary.Network;
using System.Xml.Serialization;

namespace CrypTool.CertificateServer.Rules
{
    /// <summary>
    /// P@Porator policy rule chain - These rules are used to filter out registrations.
    /// The rule chain is saved as XML file containing an ordered list of rules.
    /// The rules are interpreted first come first serve.
    /// Possible filters are 'avatar', 'email' and 'world'.
    /// Possible policies are: 
    /// 'accept' - Registration allowed
    /// 'deny'   - Registration is not allowed
    /// 'manual' - Registration needs to be manually authorized
    /// </summary>
    [Serializable]
    public class PolicyConfig
    {

        #region File name and path

        private static readonly string POLICY_FILENAME = "policy_rules.xml";

        private static readonly string POLICY_FILEPATH = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        private static readonly string EXTENSION_FILENAME = "extension_rules.xml";

        private static readonly string EXTENSION_FILEPATH = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        #endregion


        #region Singleton

        private static PolicyConfig ruleConfig = null;

        private PolicyConfig()
        {
            this.certificatePrograms = new CertificateProgram[] { new CertificateProgram() };
            this.ruleDictionary = new Dictionary<string, PolicyRuleChain>();
        }

        public static PolicyConfig GetPolicyConfig()
        {
            if (ruleConfig == null)
            {
                if (FileAvailable())
                {
                    try
                    {
                        ruleConfig = Load();
                        Log.Debug("P@Porator policy rule chain successfully loaded");

                        // Probably some errors occured, so we trim and sort the rule chain.
                        ruleConfig.SortAndTrim();
                        // Save the config, to add missing values
                        //ruleConfig.Save();
                    }
                    catch (Exception ex)
                    {
                        ruleConfig = new PolicyConfig();
                        Log.Error("P@Porator policy rule chain is malformed! Using default values", ex);
                    }
                }
                else
                {
                    ruleConfig = new PolicyConfig();
                    ruleConfig.Save();
                    Log.Info("No policy rule chain available. Created a new rule chain file with default values.");
                }
                ruleConfig.CreateRuleChainDictionary();
            }
            return ruleConfig;
        }

        #endregion


        #region Management methods

        /// <summary>
        /// Returns true if the policy rule chain file is available.
        /// </summary>
        /// <returns>true if available</returns>
        public static bool FileAvailable()
        {
            if (Directory.Exists(POLICY_FILEPATH) && File.Exists(POLICY_FILENAME))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the current rule chain to the specified file. If no file is given, the default file location will be used.
        /// </summary>
        /// <param name="file">The file name, that will be used to save the rule chain</param>
        public void Save(string file = null)
        {
            try
            {
                if (file == null)
                {
                    file = POLICY_FILEPATH + POLICY_FILENAME;
                }
                ruleConfig.SortAndTrim();
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PolicyConfig));
                StreamWriter writer = File.CreateText(file);
                serializer.Serialize(writer, ruleConfig);
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Log.Error("Could not save policy rule chain!", ex);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Policy Rule Chain:").AppendLine();

            foreach (CertificateProgram program in this.certificatePrograms)
            {
                sb.AppendLine();
                sb.Append("Client Program Configuration:            (");
                for (int i = 0; i < program.Name.Length; i++)
                {
                    sb.Append(" ").Append(program.Name[i]).Append(" ");
                    if (i < program.Name.Length - 1)
                    {
                        sb.Append("|");
                    }
                }
                sb.Append(")").AppendLine();
                sb.AppendLine(program.PolicyRuleChain.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Loads the extension rule chain configuration file.
        /// </summary>
        /// <returns>the extension rule chain configuration</returns>
        private static PolicyConfig Load()
        {
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PolicyConfig));
                StreamReader reader = File.OpenText(POLICY_FILEPATH + POLICY_FILENAME);
                PolicyConfig tempChain = (PolicyConfig)serializer.Deserialize(reader);
                reader.Close();
                return tempChain;
            }
            catch (Exception ex)
            {
                Log.Error("Could not save policy rule chain!", ex);
                return new PolicyConfig();
            }
        }

        /// <summary>
        /// Sorts the rule chain using the id values of each rule. 
        /// Also checks the defined rules for correctness and logs and removes malformed rules.
        /// </summary>
        private void SortAndTrim()
        {
            foreach (CertificateProgram program in this.certificatePrograms)
            {
                program.PolicyRuleChain.SortAndTrim();
            }
        }

        /// <summary>
        /// Generates a dictionary containing the rule chains for each certificate program. 
        /// This dictionary provides fast access to the responsible rule chain.
        /// </summary>
        private void CreateRuleChainDictionary()
        {
            foreach (CertificateProgram program in this.certificatePrograms)
            {
                foreach (string name in program.Name)
                {
                    if (this.ruleDictionary.ContainsKey(name))
                    {
                        if (name == "default")
                        {
                            Log.Warn("The default policy rule chain was overwritten. Check your policy rule chain for duplicate/missing program name entries!");
                        }
                        else
                        {
                            Log.WarnFormat("More than one policy rule chain for program {0} found! Check your policy rule chain!", name);
                        }
                    }
                    this.ruleDictionary.Add(name.ToLower(), program.PolicyRuleChain);
                }
            }
        }

        private Dictionary<string, PolicyRuleChain> ruleDictionary;

        #endregion


        #region Get rule chain for a specific program

        /// <summary>
        /// Gives back the relevant rule chain for the given program name. If there exists no rule chain for
        /// the program name, the default rule chain will be returned.
        /// </summary>
        /// <param name="programName">The name of the program which was used</param>
        /// <returns>The relevant policy rule chain</returns>
        public PolicyRuleChain GetPolicyRuleChain(string programName)
        {
            programName = programName ?? "default";
            programName = programName.ToLower();
            if (!this.ruleDictionary.ContainsKey(programName))
            {
                if (!this.ruleDictionary.ContainsKey("default"))
                {
                    // Update rule chain array as well as dictionary
                    CertificateProgram[] newCertificatePrograms = new CertificateProgram[this.certificatePrograms.Length + 1];
                    Array.Copy(this.certificatePrograms, newCertificatePrograms, this.certificatePrograms.Length);
                    this.certificatePrograms = newCertificatePrograms;
                    this.certificatePrograms[this.certificatePrograms.Length - 1] = new CertificateProgram();
                    this.ruleDictionary.Add("default", this.certificatePrograms[this.certificatePrograms.Length - 1].PolicyRuleChain);
                }
                return this.ruleDictionary["default"];
            }
            return this.ruleDictionary[programName];
        }

        #endregion


        #region Certificate program specific rule chain

        [XmlArray("CertificatePrograms")]
        [XmlArrayItem("Program")]
        public CertificateProgram[] certificatePrograms;

        /// <summary>
        /// This class represents a rule chain for each program.
        /// A rule chain can be applied to more than one program by defining a list of names.
        /// </summary>
        public class CertificateProgram
        {

            public CertificateProgram()
            {
                this.Name = new string[] { "default" };
                this.PolicyRuleChain = new PolicyRuleChain();
            }

            [XmlAttribute]
            public string[] Name { get; set; }

            public PolicyRuleChain PolicyRuleChain { get; set; }
        }

        #endregion

    }
}
