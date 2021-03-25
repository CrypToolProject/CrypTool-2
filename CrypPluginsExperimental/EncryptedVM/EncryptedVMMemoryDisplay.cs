using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

using Microsoft.Research.SEAL;

namespace CrypTool.Plugins.EncryptedVM
{
    [Author("Robert Stark", "robert.stark@rub.de", "", "")]
    [PluginInfo("CrypTool.Plugins.EncryptedVM.Properties.Resources", "EncryptedVM_MemoryDisplay_Name", "EncryptedVM_MemoryDisplay_Tooltip", "EncryptedVM/doc/display.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class EncryptedVMMemoryDisplay : ICrypComponent
    {
        #region Private Variables

        private readonly EncryptedVMMemoryDisplaySettings settings = new EncryptedVMMemoryDisplaySettings();

        private Util sealutil;
        private EncryptionParameters parms;
        private BigPoly seckey;

        private BigPolyArray[] enc_memory;
        private bool[] dec_bool_memory;
        private string dec_memory;

        private readonly CSharpCodeProvider provider = new CSharpCodeProvider();
        private CompilerParameters cparms = new CompilerParameters();
        private CompilerResults results;
        private string program = "";
        private static string code = @"
                            using System;
            
                            namespace UserFunctions
                            {                
                                public class DisplayFunction
                                {                
                                    public static string Function(bool[] memory)
                                    {
                                        string output = """";
                                        program
                                        return output;
                                    }

                                    public static bool[] SubMemory(bool[] data, uint index, uint length)
                                    {
                                        bool[] result = new bool[length];
                                        Array.Copy(data, index, result, 0, length);
                                        return result;
                                    }

                                    public static int Decode(bool[] values)
                                    {
                                        int res = 0;
                                        for (int i = values.Length - 1 ; i >= 0; i--)
                                        {
                                            res |= values[i] ? 1 : 0;
                                            if (i > 0)
                                                res <<= 1;
                                        }
                                        return res;
                                    }
                                }
                            }
                        ";
        private string finalcode;
        private bool errors;
        

        private int shift, val;

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

        [PropertyInfo(Direction.InputData, "SecretKey_Name", "SecretKey_Tooltip", true)]
        public BigPoly SecKeyInput
        {
            get
            {
                return seckey;
            }
            set
            {
                if (seckey != value)
                {
                    seckey = value;
                    OnPropertyChanged("SecKeyInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "EncryptedVM_Memory_Encrypted_Name", "EncryptedVM_Memory_Encrypted_Tooltip", true)]
        public BigPolyArray[] MemoryInput
        {
            get
            {
                return enc_memory;
            }
            set
            {
                if (enc_memory != value)
                {
                    enc_memory = value;
                    OnPropertyChanged("MemoryInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "EncryptedVM_Memory_Program_Name", "EncryptedVM_Memory_Program_Tooltip", false)]
        public string ProgramInput
        {
            get
            {
                return program;
            }
            set
            {
                if (program != value)
                {
                    program = value;
                    OnPropertyChanged("ProgramInput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "EncryptedVM_Memory_Decrypted_Name", "EncryptedVM_Memory_Decrypted_Tooltip")]
        public string MemoryOutput
        {
            get
            {
                return dec_memory;
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

            sealutil = new Util(p_encparms: parms, p_seckey: seckey);

            if (!settings.Mode)
            {
                log_line(Properties.Resources.Dump_Start);
                for (int i = 0; i < settings.Rows; i++)
                {
                    shift = 1; val = 0;
                    log_text(i + ":\t");
                    for (int j = 0; j < Memory.WORD_SIZE; j++)
                    {
                        if (j < Memory.ARRAY_COLS)
                            if (sealutil.dec_dec(enc_memory[i * Memory.WORD_SIZE + j]) == 1)
                                val += shift;
                        shift *= 2;
                        log_text(sealutil.dec_dec(enc_memory[i * Memory.WORD_SIZE + j]) + " ");
                    }
                    log_line(" " + val);
                }
                log_line(Properties.Resources.Dump_End);
            }
            else
            {
                finalcode = code.Replace("program", @program);
                
                results = provider.CompileAssemblyFromSource(cparms, finalcode);

                errors = false;
                foreach (CompilerError compilerError in results.Errors)
                {
                    if (compilerError.IsWarning)
                        GuiLogMessage("Compiler: " + compilerError, NotificationLevel.Warning);
                    else
                    {
                        GuiLogMessage("Compiler: " + compilerError, NotificationLevel.Error);
                        errors = true;
                    }
                }
                if (errors)
                    return;

                ProgressChanged(0.5, 1);

                Func<bool[], string> display_memory = (Func<bool[], string>)Delegate.CreateDelegate(typeof(Func<bool[], string>), results.CompiledAssembly.GetType("UserFunctions.DisplayFunction").GetMethod("Function"));

                dec_bool_memory = new bool[enc_memory.Length];
                int c = 0;
                foreach (BigPolyArray poly in enc_memory)
                    if(poly != null)
                        dec_bool_memory[c++] = sealutil.dec_dec(poly) == 1 ? true : false;

                dec_memory = display_memory(dec_bool_memory);

                OnPropertyChanged("MemoryOutput");
            }

            ProgressChanged(1, 1);
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

            cparms.GenerateInMemory = true;
            cparms.IncludeDebugInformation = false;
            cparms.WarningLevel = 4;
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

        #region Helper Methods

        private void log_text(string text)
        {
            dec_memory += text;
            OnPropertyChanged("MemoryOutput");
        }

        private void log_line(string line)
        {
            dec_memory += line + "\r\n";
            OnPropertyChanged("MemoryOutput");
        }

        #endregion
    }
}
