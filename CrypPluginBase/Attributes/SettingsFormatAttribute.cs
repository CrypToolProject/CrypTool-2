/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.PluginBase
{
    /// <summary>
    /// This optional attribute can be used to display author information in the 
    /// settings pane. 
    /// 
    /// Some usage samples:
    /// [SettingsFormat(0, "UltraBold", "Italic", "Black", "White", Orientation.Horizontal)] - Default column width is "*"
    /// [SettingsFormat(0, "UltraBold", "Italic", "Black", "White", Orientation.Horizontal, "Auto", "Auto")]
    /// [SettingsFormat(0, "UltraBold", "Italic", "Black", "White", Orientation.Horizontal, "1*", "5*")]
    /// [SettingsFormat(0, "UltraBold", "Italic", "Black", "White", Orientation.Horizontal, "group1")] - group one should be used at least twice, otherwise it would be bootless. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsFormatAttribute : Attribute
    {
        #region public_properties
        /// <summary>
        /// Optical Ident of 
        /// </summary>
        public readonly int Ident;

        /// <summary>
        /// The font weight for the headline text
        /// 
        /// Valid values are: 
        /// 
        /// For Black, Bold, DemiBold, ExtraBold, Heavy, Medium, SemiBold, or UltraBold: Bold is enabled. 
        /// For ExtraLight, Light, Normal, Regular, Thin, or UltraLight: Bold is disabled. 
        /// </summary>
        public readonly FontWeight FontWeight;

        /// <summary>
        /// The font style for the headline text
        /// </summary>
        public readonly FontStyle FontStyle;

        /// <summary>
        /// Foreground color
        /// </summary>
        public readonly Brush ForeGround;

        /// <summary>
        /// Background color
        /// </summary>
        public readonly Brush BackGround;

        /// <summary>
        /// Stackpanel alignment
        /// </summary>
        public readonly Orientation Orientation;

        /// <summary>
        /// Column width of column one. Is only used when no group is selected. 
        /// Value samples: Auto, Auto or 1*, 2* Last sample indicates that column 1
        /// wants to be given twice as much of the available space as the row marked with 1*. 
        /// </summary>
        public readonly GridLength WidthCol1;

        /// <summary>
        /// Column width of column two. Is only used when no group is selected. 
        /// </summary>
        public readonly GridLength WidthCol2;

        /// <summary>
        /// You can place more n itmes in a vertical group using the same group name for different task pane settings.
        /// </summary>
        public readonly string VerticalGroup;

        /// <summary>
        /// Gets a value indicating whether this instance has vertical group.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has vertical group; otherwise, <c>false</c>.
        /// </value>
        public bool HasVerticalGroup => VerticalGroup != null && VerticalGroup != string.Empty;
        #endregion public_properties

        private const string DEFAULT_COL_WIDTH = "*";
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFormatAttribute"/> class.
        /// </summary>
        /// <param name="indent">The indent.</param>
        public SettingsFormatAttribute(int indent, string fontWeight, string fontStyle)
          : this(indent, fontWeight, fontStyle, "Black", "White", Orientation.Vertical, DEFAULT_COL_WIDTH, DEFAULT_COL_WIDTH, null)
        {
        }

        /// <summary>
        /// This constructor should be used when using vertical orientation, because values for column width are not used in this case.
        /// </summary>
        public SettingsFormatAttribute(int indent, string fontWeight, string fontStyle, string foreGroundColor, string backGroundColor, Orientation orientation)
          : this(indent, fontWeight, fontStyle, foreGroundColor, backGroundColor, orientation, DEFAULT_COL_WIDTH, DEFAULT_COL_WIDTH, null)
        {
        }

        /// <summary>
        /// This constructor should be used when using vertical orientation with sub groups
        /// </summary>
        public SettingsFormatAttribute(int indent, string fontWeight, string fontStyle, string foreGroundColor, string backGroundColor, Orientation orientation, string verticalGroup)
          : this(indent, fontWeight, fontStyle, foreGroundColor, backGroundColor, orientation, DEFAULT_COL_WIDTH, DEFAULT_COL_WIDTH, verticalGroup)
        {
        }

        public SettingsFormatAttribute(int indent, string fontWeight, string fontStyle, string foreGroundColor, string backGroundColor, Orientation orientation, string widthCol1, string widthCol2)
          : this(indent, fontWeight, fontStyle, foreGroundColor, backGroundColor, orientation, widthCol1, widthCol2, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFormatAttribute"/> class.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <param name="fontStyle">The font style.</param>
        /// <param name="foreGroundColor">Color of the fore ground.</param>
        /// <param name="backGroundColor">Color of the back ground.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="widthCol1">The width col1.</param>
        /// <param name="widthCol2">The width col2.</param>
        /// <param name="verticalGroup">The vertical group.</param>
        public SettingsFormatAttribute(int indent, string fontWeight, string fontStyle, string foreGroundColor, string backGroundColor, Orientation orientation, string widthCol1, string widthCol2, string verticalGroup)
        {
            Ident = indent;
            try
            {
                FontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(fontWeight);
            }
            catch
            {
                FontWeight = FontWeights.Normal;
            }
            try
            {
                FontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(fontStyle);
            }
            catch
            {
                FontStyle = FontStyles.Normal;
            }
            try
            {
                ForeGround = (Brush)new BrushConverter().ConvertFromString(foreGroundColor);
            }
            catch
            {
                ForeGround = Brushes.Black;
            }
            try
            {
                BackGround = (Brush)new BrushConverter().ConvertFromString(backGroundColor);
            }
            catch
            {
                BackGround = Brushes.White;
            }

            Orientation = orientation;

            try
            {
                WidthCol1 = (GridLength)new GridLengthConverter().ConvertFromString(widthCol1);
            }
            catch
            {
                WidthCol1 = new GridLength();
            }
            try
            {
                WidthCol2 = (GridLength)new GridLengthConverter().ConvertFromString(widthCol2);
            }
            catch
            {
                WidthCol2 = new GridLength();
            }
            VerticalGroup = verticalGroup;
        }
    }
}
