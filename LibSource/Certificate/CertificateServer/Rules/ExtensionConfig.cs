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
    /// P@Porator extension rule chain - These rules are used to add individual extensions to certificates.
    /// The rule chain is saved as XML file containing an ordered list of rules. There can be defined different rule chains for
    /// each program.
    /// All rules are checked sequentially, several extensions can be applied together.
    /// If two or more applicable extensions refer to the same object identifier, only the first extension will be added.
    /// Possible filters are 'avatar', 'email' and 'world'.
    /// </summary>
    [Serializable]
    public class ExtensionConfig
    {

        #region File name and path

        private static readonly string EXTENSION_FILENAME = "extension_rules.xml";

        private static readonly string EXTENSION_FILEPATH = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        #endregion


        #region Singleton

        private static ExtensionConfig ruleConfig = null;

        private ExtensionConfig()
        {
            this.certificatePrograms = new CertificateProgram[] { new CertificateProgram() };
            this.ruleDictionary = new Dictionary<string, ExtensionRuleChain>();
        }

        public static ExtensionConfig GetExtensionConfig()
        {
            if (ruleConfig == null)
            {
                if (FileAvailable())
                {
                    try
                    {
                        ruleConfig = Load();
                        Log.Debug("P@Porator extension rule chain successfully loaded");
                        // Probably some errors occured, so we trim and sort the rule chain.
                        ruleConfig.SortAndTrim();
                        // Save the config, to add missing values
                        //ruleConfig.Save();
                    }
                    catch (Exception ex)
                    {
                        ruleConfig = new ExtensionConfig();
                        Log.Error("P@Porator extension rule chain is malformed! Using default values", ex);
                    }
                }
                else
                {
                    ruleConfig = new ExtensionConfig();
                    ruleConfig.Save();
                    Log.Info("No extension rule chain available. Created a new rule chain file with default values.");
                }
                ruleConfig.CreateRuleChainDictionary();
            }
            return ruleConfig;
        }

        #endregion


        #region Management methods

        /// <summary>
        /// Returns true if the extension rule chain file is available.
        /// </summary>
        /// <returns>true if available</returns>
        public static bool FileAvailable()
        {
            if (Directory.Exists(EXTENSION_FILEPATH) && File.Exists(EXTENSION_FILENAME))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the current rule chain to the specified file. If no file is given, the default file location will be used.
        /// </summary>
        /// <param name="file">The file name, that will be used to save the rule chain</param>
        public void Save(string filepath = null)
        {
            try
            {
                if (filepath == null)
                {
                    filepath = EXTENSION_FILEPATH + EXTENSION_FILENAME;
                }
                ruleConfig.SortAndTrim();
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExtensionConfig));
                StreamWriter writer = File.CreateText(filepath);
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
            sb.Append("Extension Rule Chain:").AppendLine();

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
                sb.AppendLine(program.ExtensionRuleChain.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Loads the extension rule chain configuration file.
        /// </summary>
        /// <returns>the extension rule chain configuration</returns>
        private static ExtensionConfig Load()
        {
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ExtensionConfig));
                StreamReader reader = File.OpenText(EXTENSION_FILEPATH + EXTENSION_FILENAME);
                ExtensionConfig tempChain = (ExtensionConfig)serializer.Deserialize(reader);
                reader.Close();
                return tempChain;
            }
            catch (Exception ex)
            {
                Log.Error("Could not save extension rule chain!", ex);
                return new ExtensionConfig();
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
                program.ExtensionRuleChain.SortAndTrim();
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
                            Log.Warn("The default extension rule chain was overwritten. Check your extension rule chain for duplicate/missing program name entries!");
                        }
                        else
                        {
                            Log.WarnFormat("More than one extension rule chain for program {0} found! Check your extension rule chain!", name);
                        }
                    }
                    this.ruleDictionary.Add(name.ToLower(), program.ExtensionRuleChain);
                }
            }
        }

        private Dictionary<string, ExtensionRuleChain> ruleDictionary;

        #endregion


        #region Get rule chain for a specific program

        /// <summary>
        /// Returns all applicable extensions for a given certificate registration.
        /// </summary>
        /// <param name="certReg">The certificate registration data</param>
        /// <returns>An array of extensions</returns>
        public Extension[] GetExtensions(CertificateRegistration certReg)
        {
            return GetExtensionRuleChain(certReg.ProgramName).GetExtensions(certReg);
        }

        /// <summary>
        /// Returns the relevant rule chain for the given program name. If there exists no rule chain for
        /// the program name, the default rule chain will be returned.
        /// </summary>
        /// <param name="programName">The name of the program which was used</param>
        /// <returns>The relevant extension rule chain</returns>
        public ExtensionRuleChain GetExtensionRuleChain(string programName)
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
                    this.ruleDictionary.Add("default", this.certificatePrograms[this.certificatePrograms.Length - 1].ExtensionRuleChain);
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
                this.ExtensionRuleChain = new ExtensionRuleChain();
            }

            [XmlAttribute]
            public string[] Name { get; set; }

            public ExtensionRuleChain ExtensionRuleChain { get; set; }
        }

        #endregion

    }
}
