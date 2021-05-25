/*
   Copyright 2021 Nils Kopal <Nils.Kopal@cryptool.org>

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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MorseCode
{
    public abstract class MorseEncoder
    {
        const int DIT_TIME = 100 / 2;
        const int DAH_TIME = 300 / 2;
        const int DIT_DAH_BREAK = 100 / 2;
        const int LETTER_BREAK = 300 / 2;
        const int WORD_BREAK = 700 / 2;

        protected readonly Dictionary<char, string> _mapping = new Dictionary<char, string>();
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public string Encode(string text)
        {
            var builder = new StringBuilder();
            var uppertext = text.Trim().ToUpper();
            for (var i = 0; i < uppertext.Length; i++)
            {
                if (_mapping.ContainsKey(uppertext[i]))
                {
                    //we found a corresponding morse code and put it into the output
                    builder.Append(_mapping[uppertext[i]]);
                    builder.Append(" ");
                }
                else if (uppertext[i] == '\r' && uppertext[i + 1] == '\n')
                {
                    //we detect a linebreak and put it directly into the output
                    builder.Append("\r\n");
                    i++;
                }
                else
                {
                    //no corresponding morse code exists for the found character
                    builder.Append("? ");
                }
                ProgressChanged(((double)i) / uppertext.Length, 1);
            }
            //finally we have to remove the last added space
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        /// <summary>
        /// Creates an output containing the text corresponding to the input morse code
        /// </summary>
        public string Decode(string code)
        {
            var builder = new StringBuilder();
            var tokens = Regex.Replace(code.Trim(), @" +", " ").Split(' ');
            int tokennumber = 0;
            foreach (var token in tokens)
            {
                tokennumber++;
                if (_mapping.ContainsValue(token))
                {
                    //We have no leading or trailing linebreaks because we directly found the token
                    //So we do not need to add line breaks
                    builder.Append(GetKeyFromValue(token));
                }
                else if (token.StartsWith("\r\n") || token.EndsWith("\r\n"))
                {
                    //1. Count leading linebreaks in our token and put them into the output
                    var count = CountStringOccurence(token.TrimEnd(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                    //2. Now replace the trimmed token by its corresponding value
                    builder.Append(GetKeyFromValue(token.Trim()));
                    //3. Now count trailing linebreaks and put them into the output
                    count = CountStringOccurence(token.TrimStart(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                }
                else
                {
                    //we found a token without linebreaks and we do not have a corresponding
                    //character value
                    builder.Append("?");
                }
                ProgressChanged(((double)tokennumber) / tokens.Length, 1);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Plays the given Morse Code
        /// </summary>
        public void Play(string code, int frequency, ref bool stopped)
        {
            using (var toneStream = new MemoryStream())
            {
                using (var dataStream = new MemoryStream())
                {
                    var dit = new Tone();
                    dit.GenerateSound(128, frequency, DIT_TIME);

                    var dah = new Tone();
                    dah.GenerateSound(128, frequency, DAH_TIME);

                    var dit_dah_break = new Tone();
                    dit_dah_break.GenerateSound(0, 0, DIT_DAH_BREAK);

                    var letter_break = new Tone();
                    letter_break.GenerateSound(0, 0, LETTER_BREAK);

                    var word_break = new Tone();
                    word_break.GenerateSound(0, 0, WORD_BREAK);

                    var span = new TimeSpan();
                    foreach (char c in code)
                    {
                        if (c == '.')
                        {
                            dit.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, DIT_TIME));
                            dit_dah_break.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, DIT_DAH_BREAK));
                        }
                        else if (c == '-')
                        {
                            dah.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, DAH_TIME));
                            dit_dah_break.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, DIT_DAH_BREAK));
                        }
                        else if (c == '/')
                        {
                            word_break.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, WORD_BREAK));
                        }
                        else if (c == ' ')
                        {
                            letter_break.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, LETTER_BREAK));
                        }
                        else if (c == '*')
                        {
                            //we add a double break for a *
                            letter_break.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, LETTER_BREAK));
                            letter_break.WriteToStream(toneStream);
                            span = span.Add(new TimeSpan(0, 0, 0, 0, LETTER_BREAK));
                        }
                    }

                    var wave = new Wave();
                    wave.DataChunk.Data = toneStream.ToArray();
                    wave.DataChunk.DwLength = (uint)wave.DataChunk.Data.Length;
                    wave.RiffHeader.ChunkSize = 36 + wave.DataChunk.DwLength - 8;
                    wave.WriteToStream(dataStream);

                    var soundPlayer = new SoundPlayer();
                    soundPlayer.Stream = dataStream;
                    dataStream.Seek(0, SeekOrigin.Begin);
                    var end = DateTime.Now.Add(span);
                    soundPlayer.Play();

                    var start = DateTime.Now;
                    while (DateTime.Now < end && !stopped)
                    {
                        double percentage = (DateTime.Now.Ticks - start.Ticks) / (double)(end.Ticks - start.Ticks);
                        ProgressChanged(percentage, 1);
                        Thread.Sleep(5);
                    }
                    soundPlayer.Stop();
                    ProgressChanged(1, 1);
                }
            }
        }

        /// <summary>
        /// Looks up the mappings and returns a key which belongs to a given value
        /// This is needed to decode a given morse code
        /// If it does not find the given value it returns a '?'
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private char GetKeyFromValue(string token)
        {
            foreach (var s in _mapping)
            {
                if (s.Value.Equals(token))
                {
                    return s.Key;
                }
            }
            return '?';
        }

        /// <summary>
        /// Counts the Occurence of a given stringToSearchFor in a StringToCheck
        /// </summary>
        /// <param name="stringToCheck"></param>
        /// <param name="stringToSearchFor"></param>
        /// <returns></returns>
        private int CountStringOccurence(string stringToCheck, string stringToSearchFor)
        {
            var collection = Regex.Matches(stringToCheck, stringToSearchFor);
            return collection.Count;
        }

        protected void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, null, new PluginProgressEventArgs(value, max));
        }
    }

    public class InternationalMorseEncoder : MorseEncoder
    {
        public InternationalMorseEncoder()
        {
            // Generate our mapping between letters and the morse code
            // Obtained from http://de.wikipedia.org/wiki/Morsecode
            // We use the ITU standard:
            _mapping.Add(' ', "/");
            _mapping.Add('A', ".-");
            _mapping.Add('B', "-...");
            _mapping.Add('C', "-.-.");
            _mapping.Add('D', "-..");
            _mapping.Add('E', ".");
            _mapping.Add('F', "..-.");
            _mapping.Add('G', "--.");
            _mapping.Add('H', "....");
            _mapping.Add('I', "..");
            _mapping.Add('J', ".---");
            _mapping.Add('K', "-.-");
            _mapping.Add('L', ".-..");
            _mapping.Add('M', "--");
            _mapping.Add('N', "-.");
            _mapping.Add('O', "---");
            _mapping.Add('P', ".--.");
            _mapping.Add('Q', "--.-");
            _mapping.Add('R', ".-.");
            _mapping.Add('S', "...");
            _mapping.Add('T', "-");
            _mapping.Add('U', "..-");
            _mapping.Add('V', "...-");
            _mapping.Add('W', ".--");
            _mapping.Add('X', "-..-");
            _mapping.Add('Y', "-.--");
            _mapping.Add('Z', "--..");
            _mapping.Add('0', "-----");
            _mapping.Add('1', ".----");
            _mapping.Add('2', "..---");
            _mapping.Add('3', "...--");
            _mapping.Add('4', "....-");
            _mapping.Add('5', ".....");
            _mapping.Add('6', "-....");
            _mapping.Add('7', "--...");
            _mapping.Add('8', "---..");
            _mapping.Add('9', "----.");
            _mapping.Add('À', ".--.-");
            _mapping.Add('Å', ".--.-");
            _mapping.Add('Ä', ".-.-");
            _mapping.Add('È', ".-..-");
            _mapping.Add('É', "..-..");
            _mapping.Add('Ö', "---.");
            _mapping.Add('Ü', "..--");
            _mapping.Add('ß', "...--..");
            _mapping.Add('Ñ', "--.--");
            _mapping.Add('.', ".-.-.-");
            _mapping.Add(',', "--..--");
            _mapping.Add(':', "---...");
            _mapping.Add(';', "-.-.-.");
            _mapping.Add('?', "..--..");
            _mapping.Add('-', "-....-");
            _mapping.Add('_', "..--.-");
            _mapping.Add('(', "-.--.");
            _mapping.Add(')', "-.--.-");
            _mapping.Add('\'', ".----.");
            _mapping.Add('=', "-...-");
            _mapping.Add('+', ".-.-.");
            _mapping.Add('/', "-..-.");
            _mapping.Add('@', ".--.-.");
        }
    }

    public class ContinentalMorseEncoder : MorseEncoder
    {
        public ContinentalMorseEncoder()
        {
            // Generate our mapping between letters and the morse code
            // Obtained from https://earlyradiohistory.us/1912code.htm
            _mapping.Add(' ', "/");
            _mapping.Add('A', ".-");
            _mapping.Add('B', "-...");
            _mapping.Add('C', "-.-.");
            _mapping.Add('D', "-..");
            _mapping.Add('E', ".");
            _mapping.Add('F', "..-.");
            _mapping.Add('G', "--.");
            _mapping.Add('H', "....");
            _mapping.Add('I', "..");
            _mapping.Add('J', ".---");
            _mapping.Add('K', "-.-");
            _mapping.Add('L', ".-..");
            _mapping.Add('M', "--");
            _mapping.Add('N', "-.");
            _mapping.Add('O', "---");
            _mapping.Add('P', ".--.");
            _mapping.Add('Q', "--.-");
            _mapping.Add('R', ".-.");
            _mapping.Add('S', "...");
            _mapping.Add('T', "-");
            _mapping.Add('U', "..-");
            _mapping.Add('V', "...-");
            _mapping.Add('W', ".--");
            _mapping.Add('X', "-..-");
            _mapping.Add('Y', "-.--");
            _mapping.Add('Z', "--..");            
            _mapping.Add('1', ".----");
            _mapping.Add('2', "..---");
            _mapping.Add('3', "...--");
            _mapping.Add('4', "....-");
            _mapping.Add('5', ".....");
            _mapping.Add('6', "-....");
            _mapping.Add('7', "--...");
            _mapping.Add('8', "---..");
            _mapping.Add('9', "----.");
            _mapping.Add('0', "-----"); 
            //_mapping.Add("CH", "----"); // we do not support bigrams
            _mapping.Add('Ä', ".-.-"); //come frome wikipedia
            _mapping.Add('Ö', "---.");
            _mapping.Add('Ü', "..--");
            _mapping.Add('.', "..*..*..");
            _mapping.Add(',', ".-.-.-");
            _mapping.Add(':', "---...");
            _mapping.Add(';', "-.-.-.");
            _mapping.Add('?', "..--..");
        }
    }

    public class AmericanMorseEncoder : MorseEncoder
    {
        public AmericanMorseEncoder()
        {
            // Generate our mapping between letters and the morse code
            // Obtained from https://en.wikipedia.org/wiki/Morse_code#/media/File:Morse_comparison.svg
            _mapping.Add(' ', "/");
            _mapping.Add('A', ".-");
            _mapping.Add('B', "-...");
            _mapping.Add('C', "..*.");
            _mapping.Add('D', "-..");
            _mapping.Add('E', ".");
            _mapping.Add('F', ".-.");
            _mapping.Add('G', "--.");
            _mapping.Add('H', "....");
            _mapping.Add('I', "..");
            _mapping.Add('J', "-.-.");
            _mapping.Add('K', "-.-");            
            _mapping.Add('L', "-----"); // actually, this is one long -
            _mapping.Add('M', "--");
            _mapping.Add('N', "-.");
            _mapping.Add('O', ".*.");
            _mapping.Add('P', ".....");
            _mapping.Add('Q', "..-.");
            _mapping.Add('R', ".*..");
            _mapping.Add('S', "...");
            _mapping.Add('T', "-");
            _mapping.Add('U', "..-");
            _mapping.Add('V', "...-");
            _mapping.Add('W', ".--");
            _mapping.Add('X', ".-..");
            _mapping.Add('Y', "..*..");
            _mapping.Add('Z', "...*.");
            _mapping.Add('1', ".--.");
            _mapping.Add('2', "..-..");
            _mapping.Add('3', "...-.");
            _mapping.Add('4', "....-");
            _mapping.Add('5', "---");
            _mapping.Add('6', "......");
            _mapping.Add('7', "--..");
            _mapping.Add('8', "-....");
            _mapping.Add('9', "-..-");
            _mapping.Add('.', "..--..");
            _mapping.Add(',', ".-.-");
            _mapping.Add(':', "-.-..");
            _mapping.Add(';', "...*..");
            _mapping.Add('?', "-..-.");
            _mapping.Add('&', ".*...");
        }
    }

    public class NavyMorseEncoder : MorseEncoder
    {
        public NavyMorseEncoder()
        {
            // Generate our mapping between letters and the morse code
            // Obtained from https://earlyradiohistory.us/1912code.htm
            _mapping.Add(' ', "/");
            _mapping.Add('A', "--");
            _mapping.Add('B', "-..-");
            _mapping.Add('C', ".-.");
            _mapping.Add('D', "---");
            _mapping.Add('E', ".-");
            _mapping.Add('F', "---.");
            _mapping.Add('G', "--..");
            _mapping.Add('H', ".--");
            _mapping.Add('I', ".");
            _mapping.Add('J', ".---");
            _mapping.Add('K', "-.-.");
            _mapping.Add('L', "--.");
            _mapping.Add('M', ".--.");
            _mapping.Add('N', "..");
            _mapping.Add('O', "-.");
            _mapping.Add('P', ".-.-");
            _mapping.Add('Q', ".-..");
            _mapping.Add('R', "-..");
            _mapping.Add('S', "-.-");
            _mapping.Add('T', "-");
            _mapping.Add('U', "..-");
            _mapping.Add('V', ".---");
            _mapping.Add('W', "..-.");
            _mapping.Add('X', "-..-");
            _mapping.Add('Y', "...");
            _mapping.Add('Z', "----");
            _mapping.Add('1', "....");
            _mapping.Add('2', "--.-");
            _mapping.Add('3', "...-");
            _mapping.Add('4', "---.");
            _mapping.Add('5', "..--");
            _mapping.Add('6', "--..");
            _mapping.Add('7', ".---");
            _mapping.Add('8', "-...");
            _mapping.Add('9', ".--.");
            _mapping.Add('0', "-..-");
        }
    }
}
