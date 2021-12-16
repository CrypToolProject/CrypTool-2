/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.Converter
{
    public enum OutputTypes { StringType = 0, IntType, ShortType, ByteType, DoubleType, BigIntegerType, ByteArrayType, CrypToolStreamType, BooleanType, UIntArrayType };

    [Author("Raoul Falk, Dennis Nolte", "falk@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Converter.Properties.Resources", "PluginCaption", "PluginTooltip", "Converter/DetailedDescription/doc.xml", "Converter/icons/icon.png", "Converter/icons/tostring.png", "Converter/icons/toint.png", "Converter/icons/toshort.png", "Converter/icons/tobyte.png", "Converter/icons/todouble.png", "Converter/icons/tobig.png", "Converter/icons/tobytearray.png", "Converter/icons/toCrypToolstream.png", "Converter/icons/toboolean.png", "Converter/icons/tointarray.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    internal class Converter : ICrypComponent
    {
        #region private variables

        private ConverterSettings settings = new ConverterSettings();
        private object inputOne;
        private object output;

        #endregion

        #region public interfaces

        public Converter()
        {
            // this.settings = new ConverterSettings();
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
            settings.PropertyChanged += settings_PropertyChanged;
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (ConverterSettings)value;
        }

        public System.Windows.Controls.UserControl Presentation => null;

        [PropertyInfo(Direction.InputData, "InputOneCaption", "InputOneTooltip", true)]
        public object InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => inputOne;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value != inputOne)
                {
                    inputOne = value;
                    OnPropertyChanged("InputOne");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", true)]
        public object Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (inputOne == null)
                {
                    return null;
                }

                if (settings.Converter == OutputTypes.ByteArrayType)
                {
                    if (output == null)
                    {
                        return null;
                    }

                    if (!settings.ReverseOrder)
                    {
                        return (byte[])output;
                    }

                    byte[] temp = new byte[((byte[])output).Length];
                    Buffer.BlockCopy((byte[])output, 0, temp, 0, ((byte[])output).Length);
                    Array.Reverse(temp);
                    return temp;
                }
                else if (settings.Converter == OutputTypes.CrypToolStreamType)
                {
                    byte[] streamData = null;

                    if (inputOne is ICrypToolStream)
                    {
                        if (!settings.ReverseOrder)
                        {
                            return (ICrypToolStream)inputOne;
                        }

                        streamData = ICrypToolStreamToByteArray((ICrypToolStream)inputOne);
                    }
                    else if (inputOne is byte[])
                    {
                        streamData = (byte[])inputOne;
                    }
                    else if (inputOne is byte)
                    {
                        streamData = new byte[] { (byte)inputOne };
                    }
                    else if (inputOne is bool)
                    {
                        streamData = new byte[] { (byte)(((bool)InputOne) ? 1 : 0) };
                    }
                    else if (inputOne is string)
                    {
                        streamData = GetBytesForEncoding((string)inputOne, settings.OutputEncoding);
                    }
                    else if (inputOne is BigInteger)
                    {
                        streamData = ((BigInteger)inputOne).ToByteArray();
                    }

                    if (streamData != null)
                    {
                        if (!settings.ReverseOrder)
                        {
                            return new CStreamWriter(streamData);
                        }

                        byte[] temp = new byte[streamData.Length];
                        Buffer.BlockCopy(streamData, 0, temp, 0, streamData.Length);
                        Array.Reverse(temp);
                        return new CStreamWriter(temp);
                    }
                    else
                    {
                        GuiLogMessage("Conversion from " + inputOne.GetType().Name + " to CrypToolstream is not yet implemented", NotificationLevel.Error);
                        return null;
                    }
                }
                else
                {
                    return output;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                output = value;
                OnPropertyChanged("Output");
            }
        }

        #endregion

        #region IPlugin members

        public void Dispose()
        {
        }

        private byte[] CStreamReaderToByteArray(CStreamReader stream)
        {
            stream.WaitEof();
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadFully(buffer);
            return buffer;
        }

        private byte[] ICrypToolStreamToByteArray(ICrypToolStream stream)
        {
            return CStreamReaderToByteArray(stream.CreateReader());
        }

        private BigInteger ByteArrayToBigInteger(byte[] buffer, bool msb)
        {
            byte[] temp = new byte[buffer.Length + 1];

            if (msb)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    temp[buffer.Length - 1 - i] = buffer[i];
                }
            }
            else
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    temp[i] = buffer[i];
                }
            }

            return new BigInteger(temp);
        }

        private byte[] HexstringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
            {
                hex = "0" + hex;
            }

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        private byte[] TryMatchHex(string s)
        {
            byte[] result = null;

            Match match = Regex.Match(s, "[a-fA-F0-9]+");

            if (match.Success)  // includes hex characters?
            {
                s = Regex.Replace(s, "0[xX]", "");  // remove hex specifiers
                s = Regex.Replace(s, "[^a-fA-F0-9]", "");   // remove all non-hex characters
                result = HexstringToByteArray(s);
            }

            return result;
        }

        private double StringToDouble(string s, string cultureString = "en")
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(cultureString);
            return Convert.ToDouble(s, culture.NumberFormat);
        }

        private uint[] StringToIntArray(string s)
        {
            List<uint> result = new List<uint>();
            uint numberbase = (uint)settings.Digits.Length;

            if (s.Length % settings.DigitsGroup != 0)
            {
                GuiLogMessage("Length of input is not a multiple of digits block size.", NotificationLevel.Warning);
            }

            for (int i = 0; i < s.Length; i += settings.DigitsGroup)
            {
                uint n = 0;
                int size = Math.Min(s.Length - i, settings.DigitsGroup);

                for (int j = 0; j < size; j++)
                {
                    int ofs = (settings.DigitsEndianness == 0) ? j : size - 1 - j;
                    int d = settings.Digits.IndexOf(s[i + ofs]);
                    if (d < 0)
                    {
                        GuiLogMessage("Illegal digit '" + s[i + ofs] + "' in input detected", NotificationLevel.Error);
                        return null;
                    }
                    n = (n * numberbase) + (uint)d;
                }

                result.Add(n);
            }

            return result.ToArray();
        }

        private uint[] ByteArrayToIntArray(byte[] buffer)
        {
            List<uint> result = new List<uint>();
            uint numberbase = (uint)settings.DigitsBase;

            if (buffer.Length % settings.DigitsGroup != 0)
            {
                GuiLogMessage("Length of input is not a multiple of digits block size.", NotificationLevel.Warning);
            }

            for (int i = 0; i < buffer.Length; i += settings.DigitsGroup)
            {
                uint n = 0;
                int size = Math.Min(buffer.Length - i, settings.DigitsGroup);

                for (int j = 0; j < size; j++)
                {
                    int ofs = (settings.DigitsEndianness == 0) ? j : size - 1 - j;
                    int d = buffer[i + ofs] - settings.DigitsOffset;
                    if (d < 0)
                    {
                        GuiLogMessage("Illegal value '" + buffer[i + ofs] + "' in input detected", NotificationLevel.Error);
                        return null;
                    }
                    n = (n * numberbase) + (uint)d;
                }

                result.Add(n);
            }

            return result.ToArray();
        }

        private string IntArrayToString(uint[] u)
        {
            uint numberbase = (uint)settings.Digits.Length;
            BigInteger maxnumber = BigInteger.Pow(numberbase, settings.DigitsGroup);

            if (maxnumber > uint.MaxValue)
            {
                GuiLogMessage("The setting allows numbers that are bigger than the capacity of UInt.", NotificationLevel.Error);
                return null;
            }

            char[] result = new char[settings.DigitsGroup * u.Length];
            int i = 0;

            foreach (uint n in u)
            {
                uint x = n;

                if (x >= maxnumber)
                {
                    GuiLogMessage("The numbers in the input array must be smaller than " + maxnumber + ". " + x + " is too big to be converted.", NotificationLevel.Warning);
                    x %= (uint)maxnumber;
                }

                for (int j = 0; j < settings.DigitsGroup; j++)
                {
                    int ofs = !(settings.DigitsEndianness == 0) ? j : settings.DigitsGroup - 1 - j;
                    result[i + ofs] = settings.Digits[(int)(x % numberbase)];
                    x /= numberbase;
                }

                i += settings.DigitsGroup;
            }

            return new string(result);
        }

        private byte[] IntArrayToByteArray(uint[] u)
        {
            uint numberbase = (uint)settings.DigitsBase;
            BigInteger maxnumber = BigInteger.Pow(numberbase, settings.DigitsGroup);

            if (maxnumber - 1 > uint.MaxValue)
            {
                GuiLogMessage("The setting allows numbers that are bigger than the capacity of UInt.", NotificationLevel.Error);
                return null;
            }

            byte[] result = new byte[settings.DigitsGroup * u.Length];
            int i = 0;

            foreach (uint n in u)
            {
                uint x = n;

                if (x >= maxnumber)
                {
                    GuiLogMessage("The numbers in the input array must be smaller than " + maxnumber + ". " + x + " is too big to be converted.", NotificationLevel.Warning);
                    x %= (uint)maxnumber;
                }

                for (int j = 0; j < settings.DigitsGroup; j++)
                {
                    int ofs = !(settings.DigitsEndianness == 0) ? j : settings.DigitsGroup - 1 - j;
                    result[i + ofs] = (byte)(settings.DigitsOffset + (int)(x % numberbase));
                    x /= numberbase;
                }

                i += settings.DigitsGroup;
            }

            return result;
        }

        private Type GetType(OutputTypes t)
        {
            switch (t)
            {
                case OutputTypes.StringType: return typeof(string);
                case OutputTypes.IntType: return typeof(int);
                case OutputTypes.ShortType: return typeof(short);
                case OutputTypes.ByteType: return typeof(byte);
                case OutputTypes.DoubleType: return typeof(double);
                case OutputTypes.BigIntegerType: return typeof(BigInteger);
                case OutputTypes.UIntArrayType: return typeof(uint[]);
                case OutputTypes.ByteArrayType: return typeof(byte[]);
                case OutputTypes.BooleanType: return typeof(bool);
                case OutputTypes.CrypToolStreamType: return typeof(ICrypToolStream);
                default: return null;
            }
        }

        private byte[] GetBytesForEncoding(string s, ConverterSettings.EncodingTypes encoding)
        {
            if (s == null)
            {
                return null;
            }

            switch (encoding)
            {
                case ConverterSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetBytes(s);

                case ConverterSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetBytes(s);

                case ConverterSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetBytes(s);

                case ConverterSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetBytes(s);

                case ConverterSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetBytes(s);

                case ConverterSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetBytes(s);

                case ConverterSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetBytes(s);

                default:    // should never be reached
                    return Encoding.Default.GetBytes(s);
            }
        }

        private string GetStringForEncoding(byte[] bytes, ConverterSettings.EncodingTypes encoding)
        {
            if (bytes == null)
            {
                return null;
            }

            switch (encoding)
            {
                case ConverterSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetString(bytes);

                case ConverterSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetString(bytes);

                case ConverterSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetString(bytes);

                case ConverterSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetString(bytes);

                case ConverterSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetString(bytes);

                case ConverterSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetString(bytes);

                case ConverterSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetString(bytes);

                default:
                    return Encoding.Default.GetString(bytes);
            }
        }

        public bool ConvertToOutput(object input)
        {
            if (input == null)
            {
                return false;
            }

            #region ConvertFromTypes

            #region ConvertFromICrypToolStream
            if (input is ICrypToolStream)
            {
                switch (settings.Converter)
                {
                    case OutputTypes.CrypToolStreamType:
                        {
                            Output = (ICrypToolStream)input;
                            break;
                        }

                    case OutputTypes.StringType:
                        {
                            byte[] buffer = ICrypToolStreamToByteArray((ICrypToolStream)input);
                            Output = GetStringForEncoding(buffer, settings.InputEncoding);
                            break;
                        }

                    case OutputTypes.ByteArrayType:
                        {
                            Output = ICrypToolStreamToByteArray((ICrypToolStream)input);
                            break;
                        }

                    case OutputTypes.BigIntegerType:
                        {
                            byte[] buffer = ICrypToolStreamToByteArray((ICrypToolStream)input);
                            Output = ByteArrayToBigInteger(buffer, settings.Endianness);
                            break;
                        }

                    case OutputTypes.UIntArrayType:
                        {
                            byte[] buffer = ICrypToolStreamToByteArray((ICrypToolStream)input);

                            uint[] result = (settings.DigitsDefinition == 1)
                                ? ByteArrayToIntArray(buffer)
                                : StringToIntArray(GetStringForEncoding(buffer, settings.InputEncoding));

                            if (result == null)
                            {
                                return false;
                            }

                            Output = result;
                            break;
                        }

                    default:
                        GuiLogMessage("Conversion from " + input.GetType() + " to " + GetType(settings.Converter) + " is not implemented", NotificationLevel.Error);
                        return false;
                }

                return true;
            }
            #endregion
            #region ConvertFromUIntArray
            else if (input is uint[])
            {
                switch (settings.Converter)
                {
                    case OutputTypes.UIntArrayType: // uint[] to uint[]
                        {
                            Output = (uint[])input;
                            return true;
                        }

                    case OutputTypes.StringType:    // uint[] to string
                        {
                            string result = (settings.DigitsDefinition == 1)
                                ? GetStringForEncoding(IntArrayToByteArray((uint[])input), settings.InputEncoding)
                                : IntArrayToString((uint[])input);

                            if (result == null)
                            {
                                return false;
                            }

                            Output = result;
                            return true;
                        }

                    case OutputTypes.ByteArrayType:    // uint[] to byte[]
                        {
                            byte[] result = (settings.DigitsDefinition == 1)
                                ? IntArrayToByteArray((uint[])input)
                                : GetBytesForEncoding(IntArrayToString((uint[])input), settings.InputEncoding);

                            if (result == null)
                            {
                                return false;
                            }

                            Output = result;
                            return true;
                        }

                    case OutputTypes.CrypToolStreamType:    // uint[] to CrypToolStream
                        {
                            byte[] result = (settings.DigitsDefinition == 1)
                                ? IntArrayToByteArray((uint[])input)
                                : GetBytesForEncoding(IntArrayToString((uint[])input), settings.InputEncoding);

                            if (result == null)
                            {
                                return false;
                            }

                            CStreamWriter stream = new CStreamWriter(result);
                            Output = stream;
                            return true;
                        }

                    default:
                        GuiLogMessage("Conversion from UInt[] to the chosen type is not implemented", NotificationLevel.Error);
                        return false;
                }
            }
            #endregion
            #region ConvertFromByteArray
            else if (input is byte[])
            {
                switch (settings.Converter)
                {
                    case OutputTypes.BigIntegerType: // byte[] to BigInteger
                        {
                            byte[] temp = (byte[])input;
                            Output = ByteArrayToBigInteger(temp, settings.Endianness);
                            return true;
                        }
                    case OutputTypes.IntType: // byte[] to int
                        {
                            try
                            {
                                byte[] temp = new byte[4];
                                Array.Copy((byte[])input, temp, 4);
                                if (settings.Endianness)
                                {
                                    Array.Reverse(temp);
                                }

                                Output = BitConverter.ToInt32(temp, 0);
                                return true;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to integer: " + e.Message, NotificationLevel.Error);
                                return false;
                            }
                        }
                    case OutputTypes.ShortType: // byte[] to short
                        {
                            try
                            {
                                byte[] temp = new byte[2];
                                Array.Copy((byte[])input, temp, 2);
                                if (settings.Endianness)
                                {
                                    Array.Reverse(temp);
                                }

                                Output = BitConverter.ToInt16(temp, 0);
                                return true;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to short: " + e.Message, NotificationLevel.Error);
                                return false;
                            }
                        }
                    case OutputTypes.ByteType: // byte[] to byte
                        {
                            try
                            {
                                Output = ((byte[])input)[0];
                                return true;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to byte: " + e.Message, NotificationLevel.Error);
                                return false;
                            }
                        }
                    case OutputTypes.StringType: // byte[] to String
                        {
                            Output = GetStringForEncoding((byte[])input, settings.InputEncoding);
                            return true;
                        }
                    case OutputTypes.ByteArrayType: // byte[] to byte[]
                        {
                            Output = (byte[])input;
                            return true;
                        }
                    case OutputTypes.UIntArrayType: // byte[] to uint[]
                        {
                            byte[] buffer = (byte[])input;

                            uint[] result = (settings.DigitsDefinition == 1)
                                ? ByteArrayToIntArray(buffer)
                                : StringToIntArray(GetStringForEncoding(buffer, settings.InputEncoding));

                            if (result == null)
                            {
                                return false;
                            }

                            Output = result;
                            return true;
                        }
                }
            }
            #endregion
            #region ConvertFromBigInteger
            else if (input is BigInteger)
            {
                try
                {
                    switch (settings.Converter)
                    {
                        case OutputTypes.ByteArrayType: // BigInteger to byte[]
                            {
                                Output = ((BigInteger)input).ToByteArray();
                                return true;
                            }
                        case OutputTypes.IntType: // BigInteger to int
                            {
                                Output = (int)((BigInteger)input);
                                return true;
                            }
                        case OutputTypes.ShortType: // BigInteger to short
                            {
                                Output = (short)((BigInteger)input);
                                return true;
                            }
                        case OutputTypes.ByteType: // BigInteger to byte
                            {
                                Output = (byte)((BigInteger)input);
                                return true;
                            }
                        case OutputTypes.BigIntegerType: // BigInteger to BigInteger
                            {
                                Output = (BigInteger)input;
                                return true;
                            }
                        default:
                            {
                                string fmt = settings.Format.Trim();

                                Match m;

                                m = Regex.Match(fmt, @"^([0-9]+)(?:\(([0-9]+)\))?$");
                                if (m.Success)
                                {
                                    int b = int.Parse(m.Groups[1].Value);
                                    int l = string.IsNullOrEmpty(m.Groups[2].Value) ? 0 : int.Parse(m.Groups[2].Value);
                                    if (l > 10000)
                                    {
                                        throw new Exception("Maximum size of base " + b + " string exceeded.");
                                    }

                                    Output = ((BigInteger)input).ToBaseString(b).PadLeft(l, '0');
                                    return true;
                                }

                                m = Regex.Match(settings.Format, @"^([bohx#])(?:(?<open>\()?([0-9]+)(?(open)\)))?$", RegexOptions.IgnoreCase);
                                if (m.Success)
                                {
                                    int b = 0;
                                    string s = "";
                                    switch (m.Groups[1].Value.ToUpper()[0])
                                    {
                                        case 'B': b = 2; s = "binary"; break;
                                        case 'O': b = 8; s = "octal"; break;
                                        default: b = 16; s = "hexadecimal"; break;
                                    }
                                    int l = string.IsNullOrEmpty(m.Groups[2].Value) ? 0 : int.Parse(m.Groups[2].Value);
                                    if (l > 10000)
                                    {
                                        throw new Exception("Maximum size of " + s + " string exceeded.");
                                    }

                                    Output = ((BigInteger)input).ToBaseString(b).PadLeft(l, '0');
                                    return true;
                                }

                                Output = ((BigInteger)input).ToString(settings.Format);
                                return true;
                            }
                    }
                }
                catch (FormatException ex)
                {
                    ShowFormatErrorMessage(ex);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Error);
                }
            }
            #endregion
            #region  ConvertFromInt
            else if (input is int)
            {
                try
                {
                    switch (settings.Converter)
                    {
                        case OutputTypes.BigIntegerType: // int to BigInteger
                            {
                                Output = (BigInteger)((int)input);
                                return true;
                            }
                        case OutputTypes.ShortType: // int to short
                            {
                                Output = (short)((int)input);
                                return true;
                            }
                        case OutputTypes.ByteType: // int to byte
                            {
                                Output = (byte)((int)input);
                                return true;
                            }
                        case OutputTypes.ByteArrayType: // int to byte[]
                            {
                                Output = BitConverter.GetBytes((int)input);
                                return true;
                            }
                        default:
                            {
                                Output = ((int)input).ToString(settings.Format);
                                return true;
                            }
                    }
                }
                catch (FormatException ex)
                {
                    ShowFormatErrorMessage(ex);
                }
            }
            #endregion
            #region ConvertFromShort
            else if (input is short)
            {
                if (settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((short)input);
                    return true;
                }
                if (settings.Converter == OutputTypes.StringType && !string.IsNullOrEmpty(settings.Format))
                {
                    try
                    {
                        Output = ((short)input).ToString(settings.Format);
                        return true;
                    }
                    catch (FormatException ex)
                    {
                        ShowFormatErrorMessage(ex);
                    }
                }
            }
            #endregion
            #region ConvertFromByte
            else if (input is byte)
            {
                if (settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = new byte[] { (byte)input };
                    return true;
                }
                if (settings.Converter == OutputTypes.StringType && !string.IsNullOrEmpty(settings.Format))
                {
                    try
                    {
                        Output = ((byte)input).ToString(settings.Format);
                        return true;
                    }
                    catch (FormatException ex)
                    {
                        ShowFormatErrorMessage(ex);
                    }
                }
            }
            #endregion
            #region ConvertFromDouble
            else if (input is double)
            {
                if (settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((double)input);
                    return true;
                }
                if (settings.Converter == OutputTypes.StringType && !string.IsNullOrEmpty(settings.Format))
                {
                    try
                    {
                        Output = ((double)input).ToString(settings.Format);
                        return true;
                    }
                    catch (FormatException ex)
                    {
                        ShowFormatErrorMessage(ex);
                    }
                }
            }
            #endregion
            #region ConvertFromBool
            else if (input is bool)
            {
                switch (settings.Converter)
                {
                    case OutputTypes.BooleanType:
                        Output = input;
                        return true;

                    case OutputTypes.StringType:
                        Output = input.ToString();
                        return true;

                    case OutputTypes.IntType:
                        Output = (bool)input ? 1 : 0;
                        return true;

                    case OutputTypes.ShortType:
                        Output = (short)((bool)input ? 1 : 0);
                        return true;

                    case OutputTypes.ByteType:
                        Output = (byte)((bool)input ? 1 : 0);
                        return true;

                    case OutputTypes.ByteArrayType:
                        Output = new byte[] { (byte)(((bool)input) ? 1 : 0) };
                        return true;

                    case OutputTypes.BigIntegerType:
                        Output = (BigInteger)((bool)input ? 1 : 0);
                        return true;

                    case OutputTypes.DoubleType:
                        Output = (double)((bool)input ? 1 : 0);
                        return true;

                    case OutputTypes.CrypToolStreamType:
                        Output = new byte[] { (byte)(((bool)input) ? 1 : 0) };
                        return true;

                    default:
                        GuiLogMessage("Could not convert from bool to chosen type: ", NotificationLevel.Error);
                        return false;
                }
            }
            #endregion

            #endregion

            if (input is string[])
            {
                string[] inputarray = (string[])input;
                if (settings.Converter == OutputTypes.BooleanType)
                {
                    bool[] result = new bool[inputarray.Length];
                    for (int i = 0; i < inputarray.Length; i++)
                    {
                        result[i] = inputarray[i] == "1";
                    }
                    Output = result;
                    return true;
                }
                if (settings.Converter == OutputTypes.BigIntegerType)
                {
                    BigInteger[] result = new BigInteger[inputarray.Length];
                    try // can be read as parseable expression?
                    {
                        for (int i = 0; i < inputarray.Length; i++)
                        {
                            result[i] = BigIntegerHelper.ParseExpression(inputarray[i]);
                        }
                    }
                    catch (Exception)
                    {
                        GuiLogMessage("Could not convert input to BigInteger", NotificationLevel.Error);
                        return false;
                    }
                    Output = result;
                    return true;
                }
            }

            // the string representation is used for all upcoming operations
            string inpString = Convert.ToString(input);

            #region ConvertFromString

            switch (settings.Converter) // convert to what?
            {
                #region ConvertToString
                case OutputTypes.StringType:
                    {
                        if (settings.Numeric)
                        {
                            try // can be read as parseable expression?
                            {
                                Output = BigIntegerHelper.ParseExpression(inpString).ToString();
                                return true;
                            }
                            catch (Exception) { }
                        }

                        Output = inpString;
                        return true;
                    }
                #endregion
                #region ConvertToInt
                case OutputTypes.IntType:
                    {
                        try // can be read as int from decimal string?
                        {
                            Output = Convert.ToInt32(inpString);
                            return true;
                        }
                        catch (Exception)
                        {
                        }

                        try // can be read as int from hexadecimal string?
                        {
                            Output = Convert.ToInt32(inpString, 16);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to integer: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToShort
                case OutputTypes.ShortType:
                    {
                        try // can be read as short from decimal string?
                        {
                            Output = Convert.ToInt16(inpString);
                            return true;
                        }
                        catch (Exception)
                        {
                        }

                        try // can be read as short from hexadecimal string?
                        {
                            Output = Convert.ToInt16(inpString, 16);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to short: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToByte
                case OutputTypes.ByteType:
                    {
                        try // can be read as byte from decimal string?
                        {
                            Output = Convert.ToByte(inpString);
                            return true;
                        }
                        catch (Exception)
                        {
                        }

                        try // can be read as byte hexadecimal string?
                        {
                            Output = Convert.ToByte(inpString, 16);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to byte: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToDouble
                case OutputTypes.DoubleType:
                    {
                        try // can be read as double?
                        {
                            Output = StringToDouble(inpString, settings.FormatAmer ? "en" : "de");
                            GuiLogMessage("Converting String to double is not safe. Digits may have been cut off.", NotificationLevel.Warning);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to double: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToBigInteger
                case OutputTypes.BigIntegerType:
                    {
                        try // can be read as parseable expression?
                        {
                            Output = BigIntegerHelper.ParseExpression(inpString);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Invalid Big Number input: " + ex.Message, NotificationLevel.Warning);
                        }

                        // remove all non-hex characters and parse as hexstring
                        byte[] result = TryMatchHex(inpString);
                        if (result != null)
                        {
                            Output = ByteArrayToBigInteger(result, settings.Endianness);
                            GuiLogMessage("Parsing string as hexadecimal number.", NotificationLevel.Warning);
                            return true;
                        }

                        GuiLogMessage("Could not convert input to BigInteger.", NotificationLevel.Error);
                        return false;
                    }
                #endregion
                #region ConvertToIntArray
                #endregion
                #region ConvertToByteArray
                case OutputTypes.ByteArrayType:
                    {
                        if (settings.Numeric) // apply user setting concerning numeric interpretation of input (else input is read as string)
                        {
                            try // can be read as parseable expression?
                            {
                                Output = BigIntegerHelper.ParseExpression(inpString).ToByteArray();
                                return true;
                            }
                            catch (Exception) { }

                            try // can be read as Hexstring?
                            {
                                byte[] result = TryMatchHex(inpString);
                                if (result != null)
                                {
                                    Output = result;
                                    return true;
                                }
                            }
                            catch (Exception) { }

                            try // can be read as double
                            {
                                double tempDouble = StringToDouble(inpString, settings.FormatAmer ? "en" : "de");
                                byte[] temp = BitConverter.GetBytes(tempDouble);
                                Output = temp;

                                double test = BitConverter.ToDouble(temp, 0);
                                GuiLogMessage("Converting String to double is not safe. Digits may have been cut off: " + test.ToString(), NotificationLevel.Warning);

                                return true;
                            }
                            catch (Exception) { }
                        }

                        // numeric interpretation NOT selected:
                        Output = GetBytesForEncoding(inpString, settings.OutputEncoding);
                        return true;
                    }
                #endregion
                #region ConvertToCrypToolStream
                case OutputTypes.CrypToolStreamType:
                    {
                        if (input is byte[] || input is byte || input is BigInteger || input is string)
                        {
                            OnPropertyChanged("Output");
                            return true;
                        }
                        else
                        {
                            GuiLogMessage("Conversion from " + input.GetType().Name + " to CrypToolStream is not yet implemented", NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToUIntArray
                case OutputTypes.UIntArrayType: // byte[] to uint[]
                    {
                        uint[] result = (settings.DigitsDefinition == 1)
                            ? ByteArrayToIntArray(GetBytesForEncoding(inpString, settings.InputEncoding))
                            : StringToIntArray(inpString);

                        if (result == null)
                        {
                            return false;
                        }

                        Output = result;
                        return true;
                    }
                #endregion
                #region ConvertToAnythingLeft
                default:
                    return false;
                    #endregion
            }

            #endregion
        }

        private void ShowFormatErrorMessage(FormatException ex)
        {
            GuiLogMessage(string.Format("Format error: {0}.", ex.Message), NotificationLevel.Error);
        }

        public void Execute()
        {
            ProgressChanged(0, 100);
            if (ConvertToOutput(InputOne))
            {
                ProgressChanged(100, 100);
            }
        }

        private string setText(byte[] bytes, ConverterSettings.PresentationFormat presentation)
        {
            switch (presentation)
            {
                case ConverterSettings.PresentationFormat.Text:
                    return GetStringForEncoding(bytes, settings.OutputEncoding);

                case ConverterSettings.PresentationFormat.Hex:
                    return BitConverter.ToString(bytes).Replace("-", "");

                case ConverterSettings.PresentationFormat.Base64:
                    return Convert.ToBase64String(bytes);

                case ConverterSettings.PresentationFormat.Decimal:
                    return string.Join(" ", Array.ConvertAll(bytes, item => item.ToString()));

                default:
                    return null;
            }
        }

        public string DoubleCleanup(string inpString) //apply user selected input format
        {
            if (settings.FormatAmer)
            {
                string temp1 = inpString.Replace(",", "");
                if (!(temp1.IndexOf(".") == temp1.LastIndexOf(".")))
                {
                    string tempXY = temp1.Insert(0, "X");
                    return tempXY;
                }
                if (temp1.Contains(".") && temp1.IndexOf(".") == temp1.LastIndexOf("."))
                {
                    string temp2 = temp1.Replace(".", ",");
                    return temp2;
                }
                else
                {
                    string temp3 = inpString.Replace(".", "");
                    return temp3;
                }
            }
            else
            {
                return inpString;
            }

        }

        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
            settings.UpdateIcon();
        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {

        }

        #endregion 

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region event handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ConvertToOutput(InputOne);
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
