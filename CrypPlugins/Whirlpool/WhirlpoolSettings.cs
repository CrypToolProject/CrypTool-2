//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://svn.cryptool.org/CrypTool2/trunk/CrypPlugins/Whirlpool/WhirlpoolSettings.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 8983                                                                           $://
// $Author:: kopal                                                                            $://
// $Date:: 2021-03-24 14:51:34 +0100 (Mi, 24 Mrz 2021)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using CrypTool.PluginBase;
using System.ComponentModel;

namespace Whirlpool
{
    /// <summary>
    /// Settings for PKCS#5 v2
    /// </summary>
    public class WhirlpoolSettings : ISettings
    {
        ///// <summary>
        ///// length of calculated hash in bits
        ///// </summary>
        //private int length = 256;
        //[TaskPane( "LengthCaption", "LengthTooltip", the hash length in bits, must be a multiple of 8.", "", 2, false, ControlType.TextBox, ValidationType.RangeInteger, -64, 2048)]
        //public int Length
        //{
        //    get
        //    {
        //        return length;
        //    }
        //    set
        //    {
        //        length = value;
        //        if (length < 0) // change from bytes to bits [hack]
        //            length *= -8;

        //        while ((length & 0x07) != 0) // go to the next multiple of 8
        //            length++;

        //        OnPropertyChanged("Settings");
        //    }
        //}

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
//
