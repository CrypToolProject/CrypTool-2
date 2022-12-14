using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

using Microsoft.Research.SEAL;

namespace CrypTool.Plugins.EncryptedVM
{
    [Author("Robert Stark", "robert.stark@rub.de", "", "")]
    [PluginInfo("CrypTool.Plugins.EncryptedVM.Properties.Resources", "EncryptedVM_Assembler_Name", "EncryptedVM_Assembler_Tooltip", "EncryptedVM/doc/assembler.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class EncryptedVMAssembler : ICrypComponent
    {
        #region Private Variables

        private readonly EncryptedVMAssemblerSettings settings = new EncryptedVMAssemblerSettings();

        private EncryptionParameters parms;
        private BigPolyArray pubkey;
        private Util sealutil;
        private Functions functions;
        private Program program;

        private string input = "", source = "";
        private string[] lines;
        private List<Match>[] matches;
        private string ac, pc;
        private Dictionary<string, int[]> labels;
        private BigPolyArray[] acpoly, pcpoly;
		private uint numInstr;

        private static readonly string zeroto255 = @"([0-9]|[1][0-9]{1,2}|2[0-4][0-9]|25[0-5])";
        private static readonly string word = @"[a-zA-Z_\-]+";
        private static readonly string ops = @"(SEC|CLC)";
        private static readonly string opswithparam = @"(CMP|BMI|J|BEQ|OR|AND|XOR|ADD|ROR|ROL|L|CMPa|La|ORa|ANDa|XORa|ADDa|STa)";
        private static readonly Regex r255 = new Regex(zeroto255);
        private static readonly Regex rac = new Regex(@"INITAC(?<ac>" + zeroto255 + @")");
        private static readonly Regex rpc = new Regex(@"INITPC(?<pc>" + zeroto255 + @"|" + word + @")");
        private static readonly Regex rp = new Regex(@"(?<line>((?<label>" + word + @"):)?((?<op>" + ops + @")|((?<op>" + opswithparam + @")(?<param>" + word + @"|" + zeroto255 + @")));)");
        private static readonly Regex invalidChars = new Regex(@"[^0-9a-zA-Z_\-;:\n]");

        private static readonly Dictionary<string, int> opcodes = new Dictionary<string, int>() { {"CMP", 1 }, { "BMI", 2 }, { "J", 4 }, { "BEQ", 5 }, { "OR", 6 }, { "AND", 7 }, { "XOR", 8 }, { "SEC", 9 },
            { "CLC", 10 }, { "ADD", 11}, { "ROR", 12}, { "ROL", 13}, { "L", 14}, {"CMPa", 17 }, {"La", 18 }, { "ORa", 22}, {"ANDa", 23 }, {"XORa", 24 },{"ADDa", 27 }, {"STa", 31 }}; // supports no binary data

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "EncryptionParameters_Name", "EncryptionParameters_Tooltip", true)]
        public EncryptionParameters ParametersInput
        {
            get
            {
                return parms;
            }
            set
            {
                if (parms != value)
                {
                    parms = value;
                    OnPropertyChanged("ParametersInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "PublicKey_Name", "PublicKey_Tooltip", true)]
        public BigPolyArray PubKeyInput
        {
            get
            {
                return pubkey;
            }
            set
            {
                if (pubkey != value)
                {
                    pubkey = value;
                    OnPropertyChanged("PubKeyInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "EncryptedVM_Assembler_SourceCode_Name", "EncryptedVM_Assembler_SourceCode_Tooltip", false)]
        public string ProgramInput
        {
            get
            {
                return source;
            }
            set
            {
                if (source != value)
                {
                    source = value;
                    OnPropertyChanged("ProgramInput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "EncryptedVM_Assembler_Program_Name", "EncryptedVM_Assembler_Program_Tooltip")]
        public Program ProgramOutput
        {
            get
            {
                return program;
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            sealutil = new Util(p_encparms: parms, p_pubkey: pubkey);
            functions = new Functions(sealutil);
            program = new Program();

            if (settings.Source)
                try
                {
                    input = File.ReadAllText(settings.SourcePath);
                }
                catch
                {
                    GuiLogMessage(String.Format(Properties.Resources.Failed_to_read_source_code_file, settings.SourcePath), NotificationLevel.Error);
                    return;
                }
            else
                input = source;

            input = invalidChars.Replace(input, ""); // Remove every character but [0-9a-zA-Z_-;:\r\n]
            lines = input.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); // Split program into lines
            if (lines.Length < 3)
            {
                GuiLogMessage(Properties.Resources.Invalid_program_no_code, NotificationLevel.Error);
                return;
            }

            ProgressChanged(0.25, 1);

            matches = new List<Match>[lines.Length];

            // Search for AC
            matches[0] = new List<Match>(new Match[] { rac.Match(lines[0]) });
            if (!matches[0][0].Success)
            {
                GuiLogMessage(String.Format(Properties.Resources.No_ac_defined, 0), NotificationLevel.Error);
                return;
            }
            if (matches[0][0].NextMatch().Success)
                GuiLogMessage(String.Format(Properties.Resources.Ac_specified_multiple_times, 0), NotificationLevel.Warning);
            ac = matches[0][0].Groups["ac"].Value;

            // Search for PC
            matches[1] = new List<Match>(new Match[] { rpc.Match(lines[1]) });
            if (!matches[1][0].Success)
            {
                GuiLogMessage(String.Format(Properties.Resources.No_pc_defined, 1), NotificationLevel.Error);
                return;
            }
            else if (matches[1][0].NextMatch().Success)
                GuiLogMessage(String.Format(Properties.Resources.Pc_specified_multiple_times, 1), NotificationLevel.Warning);
            pc = matches[1][0].Groups["pc"].Value;

            // Parse program and search for labels
            labels = new Dictionary<string, int[]>();
            for (int i = 2, j = 0, address = 0; i < lines.Length; i++, j = 0)
            {
                matches[i] = new List<Match>();
                matches[i].Add(rp.Match(lines[i]));
                for (;;)
                {
                    if (matches[i][j].Success)
                    {
                        if (matches[i][j].Groups["label"].Value != "")
                            if (!labels.ContainsKey(matches[i][j].Groups["label"].Value))
                            {
                                labels.Add(matches[i][j].Groups["label"].Value, new int[] { i - 2, j, address });
                                GuiLogMessage("\"" + matches[i][j].Groups["label"].Value + "\" @ " + address, NotificationLevel.Debug);
                            }
                            else
                            {
                                GuiLogMessage(String.Format(Properties.Resources.Label_used_multiple_times, matches[i][j].Groups["label"].Value, i, j), NotificationLevel.Error);
                                return;
                            }

                        address++;

                        if (matches[i][j].Index + matches[i][j].Value.Length == lines[i].Length) // end of line
                            break;

                        matches[i].Add(matches[i][j].NextMatch());
                        j++;
                    }
                    else
                    {
                        GuiLogMessage(String.Format(Properties.Resources.Invalid_syntax, i, j), NotificationLevel.Error);
                        return;
                    }
                }
            }

            ProgressChanged(0.5, 1);
            // Assemble program

            // AC
            GuiLogMessage("AC: " + ac, NotificationLevel.Debug);
            acpoly = functions.encode(Memory.ARRAY_COLS, Int32.Parse(ac));
            for (int i = 0; i < Memory.ARRAY_COLS; i++)
                program.ac[i] = acpoly[i];

            // PC
            if (labels.ContainsKey(pc))
            {
                GuiLogMessage("PC: " + pc + " (" + labels[pc][2] + ")", NotificationLevel.Debug);

                pcpoly = functions.encode(Memory.ARRAY_COLS, labels[pc][2]);
                for (int i = 0; i < Memory.ARRAY_COLS; i++)
                    program.pc[i] = pcpoly[i];
            }
            else if (r255.Match(pc).Success)
            {
                GuiLogMessage("PC: " + pc, NotificationLevel.Debug);

                pcpoly = functions.encode(Memory.ARRAY_COLS, Int32.Parse(pc));
                for (int i = 0; i < Memory.ARRAY_COLS; i++)
                    program.pc[i] = pcpoly[i];
            }
            else
            {
                GuiLogMessage(String.Format(Properties.Resources.Pc_label_not_found, pc, 1), NotificationLevel.Error);
                return;
            }

            ProgressChanged(0.6, 1);

            // Code
			numInstr = 0;
            for (int i = 2; i < matches.Length; i++)
                for (int j = 0; j < matches[i].Count; j++)
                {
                    if (matches[i][j].Groups["param"].Value == "")
                    {
                        GuiLogMessage(matches[i][j].Groups["op"].Value, NotificationLevel.Debug);

                        foreach (BigPolyArray poly in functions.encode(Memory.ARRAY_COLS, 0))
                            program.memory.Add(poly);

                        foreach (BigPolyArray poly in functions.encode(Memory.WORD_SIZE - Memory.ARRAY_COLS, opcodes[matches[i][j].Groups["op"].Value]))
                            program.memory.Add(poly);
							
						numInstr++;
                    }
                    else if (labels.ContainsKey(matches[i][j].Groups["param"].Value))
                    {
                        GuiLogMessage(matches[i][j].Groups["op"].Value + " " + matches[i][j].Groups["param"].Value + " (" + labels[matches[i][j].Groups["param"].Value][2] + ")", NotificationLevel.Debug);

                        foreach (BigPolyArray poly in functions.encode(Memory.ARRAY_COLS, labels[matches[i][j].Groups["param"].Value][2]))
                            program.memory.Add(poly);

                        foreach (BigPolyArray poly in functions.encode(Memory.WORD_SIZE - Memory.ARRAY_COLS, opcodes[matches[i][j].Groups["op"].Value]))
                            program.memory.Add(poly);
							
						numInstr++;
                    }
                    else if (r255.Match(matches[i][j].Groups["param"].Value).Success)
                    {
                        GuiLogMessage(matches[i][j].Groups["op"].Value + " " + matches[i][j].Groups["param"].Value, NotificationLevel.Debug);

                        foreach (BigPolyArray poly in functions.encode(Memory.ARRAY_COLS, Int32.Parse(matches[i][j].Groups["param"].Value)))
                            program.memory.Add(poly);

                        foreach (BigPolyArray poly in functions.encode(Memory.WORD_SIZE - Memory.ARRAY_COLS, opcodes[matches[i][j].Groups["op"].Value]))
                            program.memory.Add(poly);
							
						numInstr++;
                    }
                    else
                    {
                        GuiLogMessage(String.Format(Properties.Resources.Label_not_found, matches[i][j].Groups["param"].Value, i, j), NotificationLevel.Error); 
                        return;
                    }
					
					if (numInstr > 4 /* Memory.ARRAY_ROWS */ + 2)
					{
						GuiLogMessage(Properties.Resources.Invalid_program_too_much_code, NotificationLevel.Error);
						return;
					}
                }

            ProgressChanged(1, 1);

            OnPropertyChanged("ProgramOutput");   			
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion 
    }
}
