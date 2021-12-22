/*                              
   Copyright 2010 Nils Kopal

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

using System;
using System.IO;
using System.Numerics;
using System.Windows.Media;
using CrypTool.PluginBase;

namespace WorkspaceManager.Model
{
    public static class ColorHelper
    {
        /// <summary>
        /// Saves the Application settings. If save fails it tries to save again. If
        /// 2. try fails too app settings wont be saved
        /// </summary>
        private static void SaveApplicationsSettings()
        {
            try
            {
                WorkspaceManagerModel.Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                //if saving failed try one more time
                try
                {
                    WorkspaceManagerModel.Properties.Settings.Default.Save();
                }
                catch (Exception)
                {
                    //if saving failed again we do not try it again
                }
            }
        }


        public static Color AsymmetricColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.AsymmetricColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.AsymmetricColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color ClassicColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ClassicColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ClassicColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color SymmetricColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.SymmetricColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.SymmetricColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color ToolsColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ToolsColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ToolsColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color HashColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.HashColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.HashColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color AnalysisGenericColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.AnalysisGenericColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.AnalysisGenericColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color AnalysisSpecificColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.AnalysisSpecificColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.AnalysisSpecificColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color IntegerColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.IntegerColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.IntegerColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color ByteColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ByteColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ByteColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color DoubleColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.DoubleColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.DoubleColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color BoolColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.BoolColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.BoolColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color StreamColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.StreamColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.StreamColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color StringColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.StringColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.StringColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color ObjectColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ObjectColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ObjectColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color BigIntegerColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.BigIntegerColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.BigIntegerColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color SteganographyColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.SteganographyColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.SteganographyColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color ProtocolColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.ProtocolColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.ProtocolColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static Color DefaultColor
        {
            get => DrawingToMedia(WorkspaceManagerModel.Properties.Settings.Default.DefaultColor);
            set
            {
                WorkspaceManagerModel.Properties.Settings.Default.DefaultColor = MediaToDrawing(value);
                SaveApplicationsSettings();
            }
        }

        public static System.Windows.Media.Color DrawingToMedia(System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Drawing.Color MediaToDrawing(System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Set colors to default values
        /// </summary>
        public static void SetDefaultColors()
        {
            WorkspaceManagerModel.Properties.Settings.Default.Reset();
        }

        /// <summary>
        /// Returns a Color for a given Type
        /// 
        /// example:
        ///     System.String -> Colors.LightGray;
        ///     
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Color GetColor(Type type)
        {
            try
            {
                if (type == null)
                {
                    return DefaultColor;
                }

                ComponentCategoryAttribute[] attr = type.GetComponentCategoryAttributes();
                if (attr == null || attr.Length == 0)
                {
                    return DefaultColor;
                }

                switch (attr[0].Category) // consider first attribute found, ignore remaining ones
                {
                    case ComponentCategory.CiphersClassic:
                        return ClassicColor;
                    case ComponentCategory.CiphersModernSymmetric:
                        return SymmetricColor;
                    case ComponentCategory.CiphersModernAsymmetric:
                        return AsymmetricColor;
                    case ComponentCategory.Steganography:
                        return SteganographyColor;
                    case ComponentCategory.HashFunctions:
                        return HashColor;
                    case ComponentCategory.CryptanalysisSpecific:
                        return AnalysisSpecificColor;
                    case ComponentCategory.CryptanalysisGeneric:
                        return AnalysisGenericColor;
                    case ComponentCategory.Protocols:
                        return ProtocolColor;
                    case ComponentCategory.ToolsBoolean:
                    case ComponentCategory.ToolsDataflow:
                    case ComponentCategory.ToolsDataInputOutput:
                    case ComponentCategory.ToolsMisc:
                        return ToolsColor;
                    default:
                        return DefaultColor;
                }
            }
            catch (Exception)
            {
                return Colors.Black;
            }

        }

        /// <summary>
        /// Get a color for a ConnectorModel (drawed as line)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Color GetLineColor(Type type)
        {
            try
            {
                if (typeof(int).Equals(type) || typeof(int[]).Equals(type))
                {
                    return IntegerColor;
                }

                if (typeof(byte[]).Equals(type) || typeof(byte[]).Equals(type))
                {
                    return ByteColor;
                }

                if (typeof(double).Equals(type) || typeof(double[]).Equals(type))
                {
                    return DoubleColor;
                }

                if (typeof(bool).Equals(type) || typeof(bool[]).Equals(type))
                {
                    return BoolColor;
                }

                if (typeof(Stream).Equals(type))
                {
                    return StreamColor;
                }

                if (typeof(string).Equals(type) || typeof(string[]).Equals(type))
                {
                    return StringColor;
                }

                if (typeof(object).Equals(type))
                {
                    return ObjectColor;
                }

                if (typeof(BigInteger).Equals(type))
                {
                    return BigIntegerColor;
                }

                return DefaultColor;
            }
            catch (Exception)
            {
                return Colors.Black;
            }
        }

        public static Color GetColorLight(Type type)
        {
            Color clr = ColorHelper.GetColor(type);
            System.Drawing.Color clr2 = System.Windows.Forms.ControlPaint.Light(System.Windows.Forms.ControlPaint.LightLight(System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B)));
            return Color.FromArgb(clr2.A, clr2.R, clr2.G, clr2.B);
        }
    }
}
