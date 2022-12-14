using System;
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

using Microsoft.Research.SEAL;

namespace CrypTool.Plugins.EncryptedVM
{
    [Author("Robert Stark", "robert.stark@rub.de", "", "")]
    [PluginInfo("CrypTool.Plugins.EncryptedVM.Properties.Resources", "EncryptedVM_Machine_Name", "EncryptedVM_Machine_Tooltip", "EncryptedVM/doc/machine.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class EncryptedVMMachine : ICrypComponent
    {
        #region Private Variables

        private readonly EncryptedVMMachineSettings settings = new EncryptedVMMachineSettings();

        private EncryptionParameters parms;
        private BigPolyArray pubkey;
        private BigPoly seckey;
        private EvaluationKeys evalkeys;

        private Util sealutil;
        private Functions functions;
        private Memory memory;
        private Program program;

        private BigPolyArray[] memory_out;

        private BigPolyArray MEMORY_READ;
        private BigPolyArray VALUE_ONE, VALUE_ZERO;
        private BigPolyArray ALU_carry, ALU_zero, ALU_minus;
        private BigPolyArray t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15;
        private BigPolyArray carry_rol, carry_ror, carry_add, carry_add1, carry_add2, carry_clc, carry_sec, carry_non;
        private BigPolyArray[] pc = new BigPolyArray[Memory.ARRAY_COLS];
        private BigPolyArray[] ac = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b1 = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] nb1 = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_rol = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_ror = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_add = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_clc = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_sec = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_xor = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_and = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_or = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] b_cmp = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray cmd_store, ncmd_store, cmd_rol, cmd_load, cmd_ror, cmd_add, cmd_clc, cmd_sec, cmd_xor, cmd_and, cmd_or, cmd_beq, cmd_jmp, cmd_la, cmd_bmi, cmd_cmp, ncmd_cmp, cmd_a, ncmd_a;
        private BigPolyArray[] load_val = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray zero1, zero2;
        private BigPolyArray[] b_cmp_0 = new BigPolyArray[Memory.ARRAY_COLS + 1];
        private BigPolyArray[] temp31 = new BigPolyArray[Memory.ARRAY_COLS + 1];
        private BigPolyArray[] temp32 = new BigPolyArray[Memory.ARRAY_COLS + 1];
        private BigPolyArray[] pc_linear = new BigPolyArray[Memory.ARRAY_COLS];
        private BigPolyArray[] pc_jump = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] pc_branch = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] load_arg = new BigPolyArray[Memory.WORD_SIZE];
        private BigPolyArray[] one_field = new BigPolyArray[Memory.ARRAY_COLS];

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

        [PropertyInfo(Direction.InputData, "EvaluationKeys_Name", "EvaluationKeys_Tooltip", true)]
        public EvaluationKeys EvalKeysInput
        {
            get
            {
                return evalkeys;
            }
            set
            {
                if (evalkeys != value)
                {
                    evalkeys = value;
                    OnPropertyChanged("EvalKeysInput");
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

        [PropertyInfo(Direction.InputData, "EncryptedVM_Assembler_Program_Name", "EncryptedVM_Assembler_Program_Tooltip", true)]
        public Program ProgramInput
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

        [PropertyInfo(Direction.OutputData, "EncryptedVM_Machine_Output_Name", "EncryptedVM_Machine_Output_Tooltip")]
        public BigPolyArray[] ProgramOutput
        {
            get
            {
                return memory_out;
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

            GuiLogMessage(Properties.Resources.Init_Machine, NotificationLevel.Info);

            sealutil = new Util(p_encparms: parms, p_pubkey: pubkey, p_evalkeys: evalkeys, p_seckey: seckey);
            functions = new Functions(sealutil);
            memory = new Memory(sealutil, functions);

            VALUE_ONE = MEMORY_READ = sealutil.enc_enc(1);
            VALUE_ZERO = ALU_carry = ALU_zero = ALU_minus = sealutil.enc_enc(0);

            one_field[0] = VALUE_ONE;
            for (int i = 1; i < Memory.ARRAY_COLS; i++)
                one_field[i] = VALUE_ZERO;

            for (int i = 0; i < Memory.WORD_SIZE; i++)
            {
                if (i < Memory.ARRAY_COLS + 1)
                {
                    temp31[i] = VALUE_ZERO;
                    temp32[i] = VALUE_ZERO;
                }

                b1[i] = VALUE_ZERO;
                load_arg[i] = VALUE_ZERO;
                load_val[i] = VALUE_ZERO;
            }

            ProgressChanged(0.2, 1);

            GuiLogMessage(Properties.Resources.Loading_Program, NotificationLevel.Info);
            if (!memory.loadmemory(program.memory))
            {
                GuiLogMessage(Properties.Resources.Failed_loading_program, NotificationLevel.Error);
                return;
            }
            pc = program.pc;
            ac = program.ac;

            ProgressChanged(0.25, 1);

            GuiLogMessage(Properties.Resources.Executing_Program, NotificationLevel.Info);
            double cyclechanges = 0.75 / settings.Cycles;
            for (int i = 0; i < settings.Cycles; i++)
            {
                process();
                ProgressChanged(0.25 + (cyclechanges += cyclechanges), 1);
            }

            int c = 0;
            memory_out = new BigPolyArray[memory.cellarray.Rank * memory.cellarray.Length];
            foreach(BigPolyArray poly in memory.cellarray)
                memory_out[c++] = poly;

            GuiLogMessage(Properties.Resources.Finished_executing_program, NotificationLevel.Info);

            GuiLogMessage("#[~: " + sealutil.numofreencrypts + ", &: " + functions.opcountand + ", ^: " + functions.opcountxor + ", !: " + functions.opcountnot + "]", NotificationLevel.Debug);

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

        public void process()
        {
            int starttime = Environment.TickCount;
            int i = 0;

            GuiLogMessage(Properties.Resources.Start_Cycle, NotificationLevel.Info);
            dump("PC", pc, 8, null);
            dump("AC", ac, 8, ALU_carry);

            // PHASE 1: fetch
            GuiLogMessage(Properties.Resources.Phase_1, NotificationLevel.Info);
            b1 = memory.access(pc, ac, MEMORY_READ, b1);
            dump("load", b1, Memory.WORD_SIZE, ALU_carry);

            // PHASE 2: decode
            GuiLogMessage(Properties.Resources.Phase_2, NotificationLevel.Info);

            for (i = 8; i < 12; i++)
                nb1[i] = functions.not(b1[i]);

            String text = "cmd = ";
            for (i = 8; i < Memory.WORD_SIZE; i++)
                text += sealutil.dec_dec(b1[i]) + " ";
            GuiLogMessage(text, NotificationLevel.Debug);

            cmd_store = functions.and4(b1[11], b1[10], b1[9], b1[8]);                   //	1 1 1 1 store  
            GuiLogMessage("cmd_store = " + sealutil.dec_dec(cmd_store), NotificationLevel.Debug);
            cmd_load = functions.and4(b1[11], b1[10], b1[9], nb1[8]);                   //	0 1 1 1 load
            GuiLogMessage("cmd_load = " + sealutil.dec_dec(cmd_load), NotificationLevel.Debug);
            cmd_rol = functions.and4(b1[11], b1[10], nb1[9], b1[8]);                    //	1 0 1 1 rol
            GuiLogMessage("cmd_rol = " + sealutil.dec_dec(cmd_rol), NotificationLevel.Debug);
            cmd_ror = functions.and4(b1[11], b1[10], nb1[9], nb1[8]);                   //	0 0 1 1 ror
            GuiLogMessage("cmd_ror = " + sealutil.dec_dec(cmd_ror), NotificationLevel.Debug);
            cmd_add = functions.and4(b1[11], nb1[10], b1[9], b1[8]);                    //	1 1 0 1 add
            GuiLogMessage("cmd_add = " + sealutil.dec_dec(cmd_add), NotificationLevel.Debug);
            cmd_clc = functions.and4(b1[11], nb1[10], b1[9], nb1[8]);                   //	0 1 0 1 clc
            GuiLogMessage("cmd_clc = " + sealutil.dec_dec(cmd_clc), NotificationLevel.Debug);
            cmd_sec = functions.and4(b1[11], nb1[10], nb1[9], b1[8]);                   //	1 0 0 1 sec
            GuiLogMessage("cmd_sec = " + sealutil.dec_dec(cmd_sec), NotificationLevel.Debug);
            cmd_xor = functions.and4(b1[11], nb1[10], nb1[9], nb1[8]);                  //	0 0 0 1 xor
            GuiLogMessage("cmd_xor = " + sealutil.dec_dec(cmd_xor), NotificationLevel.Debug);
            cmd_and = functions.and4(nb1[11], b1[10], b1[9], b1[8]);                    //	1 1 1 0 and
            GuiLogMessage("cmd_and = " + sealutil.dec_dec(cmd_and), NotificationLevel.Debug);
            cmd_or = functions.and4(nb1[11], b1[10], b1[9], nb1[8]);                    //	0 1 1 0 or
            GuiLogMessage("cmd_or = " + sealutil.dec_dec(cmd_or), NotificationLevel.Debug);
            cmd_beq = functions.and4(nb1[11], b1[10], nb1[9], b1[8]);                   //	1 0 1 0 beq
            GuiLogMessage("cmd_beq = " + sealutil.dec_dec(cmd_beq), NotificationLevel.Debug);
            cmd_jmp = functions.and4(nb1[11], b1[10], nb1[9], nb1[8]);                  //	0 0 1 0 jmp
            GuiLogMessage("cmd_jmp = " + sealutil.dec_dec(cmd_jmp), NotificationLevel.Debug);
            cmd_la = functions.and4(nb1[11], nb1[10], b1[9], b1[8]);                    //	1 1 0 0 la
            GuiLogMessage("cmd_la = " + sealutil.dec_dec(cmd_la), NotificationLevel.Debug);
            cmd_bmi = functions.and4(nb1[11], nb1[10], b1[9], nb1[8]);                  //	0 1 0 0 bmi
            GuiLogMessage("cmd_bmi = " + sealutil.dec_dec(cmd_bmi), NotificationLevel.Debug);
            cmd_cmp = functions.and4(nb1[11], nb1[10], nb1[9], b1[8]);                  //	1 0 0 0 cmp
            GuiLogMessage("cmd_cmp = " + sealutil.dec_dec(cmd_cmp), NotificationLevel.Debug);
            cmd_a = b1[12];
            GuiLogMessage("cmd_a = " + sealutil.dec_dec(cmd_a), NotificationLevel.Debug);

            // PHASE 3: execute 
            GuiLogMessage(Properties.Resources.Phase_3, NotificationLevel.Info);
            ncmd_a = functions.not(cmd_a);
            ncmd_store = functions.not(cmd_store);
            ncmd_cmp = functions.not(cmd_cmp);

            for (i = 0; i < 8; i++)
            {
                t1 = functions.and2(b1[i], ncmd_a);
                t2 = functions.and2(load_arg[i], cmd_a);
                t3 = functions.or2(t1, t2);
                b_cmp[i] = functions.not(t3);
            }

            dump("!cmp", b_cmp, 8, null);
            b_cmp_0 = functions.ALU_add(b_cmp, one_field, VALUE_ZERO);
            copyreg(b_cmp, b_cmp_0);
            dump("-cmp", b_cmp, 8, null);
            dump("  b1", b1, 8, null);
            b_cmp_0 = functions.ALU_add(b_cmp, ac, VALUE_ZERO);
            copyreg(b_cmp, b_cmp_0);
            dump("cmp", b_cmp, 8, null);

            //rol b21
            carry_rol = ac[0];
            for (i = 0; i < 7; i++)
                b_rol[i] = ac[i + 1];
            b_rol[7] = ALU_carry;
            dump("rol", b_rol, 8, carry_rol);

            //ror b22
            carry_ror = ac[7];
            for (i = 7; i > 0; i--)
                b_ror[i] = ac[i - 1];
            b_ror[0] = ALU_carry;
            dump("ror", b_ror, 8, carry_ror);

            //add b31
            temp31 = functions.ALU_add(ac, b1, ALU_carry);
            temp32 = functions.ALU_add(ac, load_arg, ALU_carry);

            for (i = 0; i < 8; i++)
            {
                t1 = functions.and2(temp31[i], ncmd_a);
                t2 = functions.and2(temp32[i], cmd_a);
                t3 = functions.or2(t1, t2);
                b_add[i] = t3;
            }
            carry_add1 = temp31[8];
            carry_add2 = temp32[8];

            t1 = functions.and2(carry_add1, ncmd_a);
            t2 = functions.and2(carry_add2, cmd_a);
            carry_add = functions.or2(t1, t2);
            dump("add", b_add, 8, carry_add);

            //clc b32
            carry_clc = VALUE_ZERO;
            copyreg(b_clc, ac);
            dump("clc", ac, 8, carry_clc);
            //sec b33
            carry_sec = VALUE_ONE;
            copyreg(b_sec, ac);
            dump("sec", ac, 8, carry_sec);

            //xor b41
            for (i = 0; i < 8; i++)
                b_xor[i] = functions.xor(ac[i], b1[i]);
            dump("xor", b_xor, 8, ALU_carry);
            //and b42
            for (i = 0; i < 8; i++)
                b_and[i] = functions.and2(ac[i], b1[i]);
            dump("and", b_and, 8, ALU_carry);
            //or b43
            for (i = 0; i < 8; i++)
                b_or[i] = functions.or2(ac[i], b1[i]);
            dump("or", b_or, 8, ALU_carry);

            // PHASE 4: load/store
            GuiLogMessage(Properties.Resources.Phase_4, NotificationLevel.Info);
            load_val = memory.access(b1, ac, ncmd_store, load_val);

            // PHASE 5: rewrite registers / flags
            GuiLogMessage(Properties.Resources.Phase_5, NotificationLevel.Info);
            for (i = 0; i < 8; i++)
            {
                t1 = functions.and2(b1[i], cmd_load);
                t2 = functions.and2(b_ror[i], cmd_ror);
                t3 = functions.and2(b_rol[i], cmd_rol);
                t4 = functions.and2(b_sec[i], cmd_sec);
                t5 = functions.and2(b_clc[i], cmd_clc);
                t6 = functions.and2(b_add[i], cmd_add);
                t7 = functions.and2(b_and[i], cmd_and);
                t8 = functions.and2(b_xor[i], cmd_xor);
                t9 = functions.and2(b_or[i], cmd_or);
                t11 = functions.and2(load_val[i], cmd_la);
                t12 = functions.and2(ac[i], cmd_beq);
                t13 = functions.and2(ac[i], cmd_bmi);
                t14 = functions.and2(ac[i], cmd_cmp);
                t15 = functions.and2(ac[i], cmd_jmp);
                ac[i] = functions.or15(t1, t2, t3, t4, t5, t6, t7, t8, t9, ac[i], t11, t12, t13, t14, t15);
            }

            t1 = functions.not(ac[0]);
            t2 = functions.not(ac[1]);
            t3 = functions.not(ac[2]);
            t4 = functions.not(ac[3]);
            t5 = functions.not(ac[4]);
            t6 = functions.not(ac[5]);
            t7 = functions.not(ac[6]);
            t8 = functions.not(ac[7]);
            zero1 = functions.and8(t1, t2, t3, t4, t5, t6, t7, t8);

            t1 = functions.not(b_cmp[0]);
            t2 = functions.not(b_cmp[1]);
            t3 = functions.not(b_cmp[2]);
            t4 = functions.not(b_cmp[3]);
            t5 = functions.not(b_cmp[4]);
            t6 = functions.not(b_cmp[5]);
            t7 = functions.not(b_cmp[6]);
            t8 = functions.not(b_cmp[7]);
            zero2 = functions.and8(t1, t2, t3, t4, t5, t6, t7, t8);

            t1 = functions.and2(zero2, cmd_cmp);
            t2 = functions.and2(ALU_zero, cmd_bmi);
            t3 = functions.and2(ALU_zero, cmd_beq);
            t4 = functions.or3(t1, t2, t3);
            t5 = functions.and2(zero1, ncmd_cmp);
            ALU_zero = functions.or2(t5, t4);

            t1 = functions.and2(b_cmp[7], cmd_cmp);
            t2 = functions.and2(ALU_minus, ncmd_cmp);
            ALU_minus = functions.or2(t2, t1);

            t1 = functions.and2(carry_add, cmd_add);
            carry_add = t1;
            t1 = functions.and2(carry_rol, cmd_rol);
            carry_rol = t1;
            t1 = functions.and2(carry_ror, cmd_ror);
            carry_ror = t1;
            t1 = functions.and2(carry_clc, cmd_clc);
            carry_clc = t1;
            t1 = functions.and2(carry_sec, cmd_sec);
            carry_sec = t1;

            t1 = functions.not(cmd_add);
            t2 = functions.not(cmd_rol);
            t3 = functions.not(cmd_ror);
            t4 = functions.not(cmd_clc);
            t5 = functions.not(cmd_sec);
            t6 = functions.and4(t1, t2, t3, t4);
            carry_non = functions.and3(t6, t5, ALU_carry);

            ALU_carry = functions.or6(carry_add, carry_rol, carry_ror, carry_clc, carry_sec, carry_non);

            pc_linear = functions.ALU_addadr(pc, one_field);
            pc_jump = b1;
            pc_branch = b1;

            dump("PC lin", pc_linear, 8, null);
            dump("PC bra", pc_branch, 8, null);
            dump("PC jmp", pc_jump, 8, null);

            GuiLogMessage("ZERO = " + sealutil.dec_dec(ALU_zero) + ", MINUS = " + sealutil.dec_dec(ALU_minus), NotificationLevel.Debug);

            t4 = functions.not(cmd_jmp);
            t2 = functions.not(ALU_minus);
            t6 = functions.not(cmd_bmi);
            t3 = functions.not(ALU_zero);
            t5 = functions.not(cmd_beq);

            GuiLogMessage("ALUnz: " + sealutil.dec_dec(t3) + ", ALUnm: " + sealutil.dec_dec(t2) + ", njmp: " + sealutil.dec_dec(t4) + ", nbmi: " + sealutil.dec_dec(t6) + ", nbeq: " + sealutil.dec_dec(t5), NotificationLevel.Debug);

            for (i = 0; i < Memory.ARRAY_COLS; i++)
            {
                t1 = functions.and2(cmd_jmp, pc_jump[i]);
                t7 = functions.and3(cmd_bmi, t2, pc_linear[i]);
                t8 = functions.and3(cmd_beq, t3, pc_linear[i]);
                t9 = functions.and4(t6, t5, t4, pc_linear[i]);
                t10 = functions.and3(cmd_bmi, pc_branch[i], ALU_minus);
                t11 = functions.and3(cmd_beq, pc_branch[i], ALU_zero);
                t12 = functions.or3(t11, t10, t9);
                pc[i] = functions.or4(t12, t8, t7, t1);
            }

            dump("PC", pc, 8, null);
            dump("AC", ac, 8, ALU_carry);
            GuiLogMessage("ZERO = " + sealutil.dec_dec(ALU_zero), NotificationLevel.Debug);

            GuiLogMessage(Properties.Resources.End_Cycle + " (" + (Environment.TickCount - starttime) + "ms)", NotificationLevel.Info);
        }

        public void copyreg(BigPolyArray[] a, BigPolyArray[] b)
        {
            for (int i = 0; i < Memory.ARRAY_COLS; i++)
                a[i] = b[i];
        }

        public void dump(String cmd, BigPolyArray[] reg, int length, BigPolyArray carry)
        {
            if (length > Memory.WORD_SIZE)
                length = Memory.WORD_SIZE;

            String text = cmd + ": ";

            for (int i = 0; i < length; i++)
                text += sealutil.dec_dec(reg[i]) + " ";

            if (carry != null)
                text += ", carry = " + sealutil.dec_dec(carry);

            GuiLogMessage(text, NotificationLevel.Debug);
        }

        #endregion
    }
}
