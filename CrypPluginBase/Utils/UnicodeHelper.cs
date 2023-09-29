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
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CrypTool.PluginBase.Utils
{
    /// <summary>
    /// Helper class to map unicode codes to names and vice versa
    /// lookup table from: http://www.unicode.org/Public/UNIDATA/UnicodeData.txt
    /// </summary>
    public class UnicodeHelper
    {
        private static readonly Dictionary<string, long> _NameToId = new Dictionary<string, long>();
        private static readonly Dictionary<long, string> _IdToName = new Dictionary<long, string>();

        /// <summary>
        /// Creates lookup dictionaries for unicode names/ids
        /// </summary>
        static UnicodeHelper()
        {
            try
            {
                string unicodeData = Properties.Resources.UnicodeData;
                using (StringReader reader = new StringReader(unicodeData))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] split = line.Split(';');
                        long char_code = long.Parse(split[0], NumberStyles.HexNumber);
                        string char_name = split[1].ToUpper().Replace(" ", "");
                        if (!_NameToId.ContainsKey(char_name))
                        {
                            _NameToId.Add(char_name, char_code);
                        }
                        if (!_IdToName.ContainsKey(char_code))
                        {
                            _IdToName.Add(char_code, char_name);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing; should never happen
            }
        }

        /// <summary>
        /// Returns the unicode code for the given name
        /// Returns -1 if it does not exist
        /// Keep in mind that names are stored "ToUpper"-case and whitespaces are removed
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static long GetIdByName(string name)
        {
            if (_NameToId.ContainsKey(name.ToUpper()))
            {
                return _NameToId[name.ToUpper()];
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns the name for the given unicode code
        /// Returns null if it does not exist
        /// Keep in mind that names are stored "ToUpper"-case and whitespaces are removed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetNameById(long id)
        {
            if (_IdToName.ContainsKey(id))
            {
                return _IdToName[id];
            }
            else
            {
                return null;
            }
        }
    }
}
