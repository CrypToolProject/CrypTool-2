/*
   Copyright 2022 Vasily Mikhalev, CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace HagelinMachine
{
    public class HagelinEnums
    {
        public enum UnknownSymbolHandling
        {
            Ignore,
            Remove,
            Replace
        }
        public enum ModelType
        {
            CX52a,
            CX52b,
            CX52c,
            C52d,
            CXM,              // The 5 stepping bars affect the typewheel
            CXM_LATE_VERSION, // The 5 stepping bars do not affect the typewheel
            FRANCE,
            EIRE,
            M209,
            Custom
        }

        public enum ModeType
        {
            Encrypt,
            Decrypt
        }

        public enum CamType
        {
            Never = '0',
            IfDisplaced = 'A',
            IfNotDisplaced = 'B',
            Always = 'C'
        }

        public enum ToothType
        {
            DisplaceWhenShifted,
            NeverDisplace,
            DisplaceWhenNotShifted
        }

        public enum BarType
        {   // lugs: l - has lugs, u - has no lugs,
            // typeWheel displacement (tooth type): d - advance type wheel if side bar displaced, n  - no affect on typewheel, i - advance typewheel when side bar not displaced (inverse)
            // cams: 0 - never, B - not displaced, A - displaced, C - always
            //     custom ,   // defined by user 
            ldA00000num1,
            ldB00000num2,
            ld0A0000num3,
            ld0B0000num4,
            ld00A000num5,
            ld00B000num6,
            ld000A00num7,
            ld000B00num8,
            ld0000A0num9,
            ld0000B0num10,
            ld00000Anum11,
            ld00000Bnum12,
            ld000000num13,
            ldBBBBBBnum14,
            ldCCCCCCnum15,
            ud00CCCCnum16,
            udCCCCCCnum17,
            ud000000num18,
            ld00BBBBnum35,
            ldAAAAAAnum54,
            ldAA0000num56,
            ldCA0000num57,
            ld00AA00num60,
            ld0000AAnum64,
            ld00CCCCnum68,

            ln00A000num105,
            ln00B000num106,
            ln000A00num107,
            ln000B00num108,
            ln0000A0num109,
            ln00000Anum111,
            lnCA0000num157
        }

        public enum WheelType
        { //  custom,
            W17_10_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q,  //Size, Offset, Positions
            W19_11_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S,
            W21_12_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U,
            W23_13_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U_V_X,
            W25_9_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U_V_X_Y_Z, //For M209 this offset is different, we manualy fix it in the implementation class
            W26_11_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U_V_W_X_Y_Z,//For M209 this offset is different, we manualy fix it in the implementation class
            W29_12_A_B_C_D_E_F_G_H_I_10_J_K_L_M_N_O_P_Q_R_20_S_T_U_V_W_X_Y_Z_29,
            W31_13_A_B_C_D_E_06_F_G_H_I_J_12_K_L_M_N_O_P_19_Q_R_S_T_U_25_V_W_X_Y_Z_31,
            W34_14_A_02_B_04_C_06_D_08_E_10_F_12_G_14_H_16_I_18_J_20_K_22_L_24_M_26_N_28_O_30_P_32_Q_34,
            W37_15_A_B_C_04_D_E_07_F_G_10_H_I_J_14_K_L_17_M_N_20_O_P_Q_24_R_S_27_T_U_30_V_W_X_34_Y_Z_37,
            W38_16_A_02_B_04_C_06_D_08_E_10_F_12_G_14_H_16_I_18_J_20_K_22_L_24_M_26_N_28_O_30_P_32_Q_34_R_36_S_38,
            W41_17_A_B_03_C_D_06_E_F_09_G_11_H_I_14_J_K_17_L_M_20_N_22_O_P_25_Q_R_28_S_T_31_U_33_V_W_36_X_Y_39_Z_41,
            W42_18_A_02_B_04_C_06_D_08_E_10_F_12_G_14_H_16_I_18_J_20_K_22_L_24_M_26_N_28_O_30_P_32_Q_34_R_36_S_38_T_40_U_42,
            W43_18_A_B_03_C_05_D_E_08_F_10_G_H_13_I_15_J_K_18_L_20_M_N_23_O_25_P_Q_28_R_30_S_T_33_U_35_V_W_38_X_40_Y_Z_43_,
            W46_19_A_02_B_04_C_06_D_08_E_10_F_12_G_14_H_16_I_18_J_20_K_22_L_24_M_26_N_28_O_30_P_32_Q_34_R_36_S_38_T_40_U_42_V_44_X_46,
            W47_20_A_B_03_C_05_D_07_E_09_F_G_12_H_14_I_16_J_18_K_L_21_M_23_N_25_O_27_P_29_Q_R_32_S_34_T_36_U_38_V_W_41_X_43_Y_45_Z_47
        }

        public enum PluginStates
        {
            ModelSelection,
            WheelsSelection,
            BarsSelection,
            InnerKeySetupPins,
            InnerKeySetupLugs,
            ExternalKeySetup,
            ModeOpGroup,
            Encryption,
            EncryptionDone
        }
    }
}
