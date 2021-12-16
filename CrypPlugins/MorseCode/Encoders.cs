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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MorseCode
{
    public abstract class MorseEncoder
    {
        /*
            The length of a dot is 1 time unit.
            A dash is 3 time units.
            The space between symbols (dots and dashes) of the same letter is 1 time unit.
            The space between letters is 3 time units.
            The space between words is 7 time units.         
            Found at: https://www.codebug.org.uk/learn/step/541/morse-code-timing-rules/
         */
        private const int DIT_TIME = 1;
        private const int DAH_TIME = 3;
        private const int DIT_DAH_BREAK = 1;
        private const int LETTER_BREAK = 3;
        private const int WORD_BREAK = 7;

        protected readonly Dictionary<char, string> _mapping = new Dictionary<char, string>();
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event EventHandler<WaveEventArgs> OnWaveFileGenerated;

        public virtual string Encode(string text)
        {
            StringBuilder builder = new StringBuilder();
            string uppertext = text.Trim().ToUpper();
            for (int i = 0; i < uppertext.Length; i++)
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
        public virtual string Decode(string code)
        {
            StringBuilder builder = new StringBuilder();
            string[] tokens = Regex.Replace(code.Trim(), @" +", " ").Split(' ');
            int tokennumber = 0;
            foreach (string token in tokens)
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
                    int count = CountStringOccurence(token.TrimEnd(), "\r\n");
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
        public void Play(string code, int frequency, int tickDuration, double volume, ref bool stopped)
        {

            Tone dit = new Tone();
            dit.GenerateSound(volume * 32767, frequency, DIT_TIME * tickDuration);

            Tone dah = new Tone();
            dah.GenerateSound(volume * 32767, frequency, DAH_TIME * tickDuration);

            Tone dit_dah_break = new Tone();
            dit_dah_break.GenerateSound(0, 0, DIT_DAH_BREAK * tickDuration);

            Tone letter_break = new Tone();
            letter_break.GenerateSound(0, 0, LETTER_BREAK * tickDuration);

            Tone word_break = new Tone();
            word_break.GenerateSound(0, 0, WORD_BREAK * tickDuration);

            double characterCounter = 0;
            foreach (char c in code)
            {
                using (MemoryStream toneStream = new MemoryStream())
                {
                    //create a timespawn for waiting to play the next tone
                    int waiting_time = 0;

                    if (c == '.')
                    {
                        dit.WriteToStream(toneStream);
                        waiting_time += DIT_TIME * tickDuration;
                        dit_dah_break.WriteToStream(toneStream);
                        waiting_time += DIT_DAH_BREAK * tickDuration;
                    }
                    else if (c == '-')
                    {
                        dah.WriteToStream(toneStream);
                        waiting_time += DAH_TIME * tickDuration;
                        dit_dah_break.WriteToStream(toneStream);
                        waiting_time += DIT_DAH_BREAK * tickDuration;
                    }
                    else if (c == '/')
                    {
                        word_break.WriteToStream(toneStream);
                        waiting_time += WORD_BREAK * tickDuration;
                    }
                    else if (c == ' ')
                    {
                        letter_break.WriteToStream(toneStream);
                        waiting_time += LETTER_BREAK * tickDuration;
                    }
                    else if (c == '*')
                    {
                        //we add a double break for a *
                        letter_break.WriteToStream(toneStream);
                        waiting_time += LETTER_BREAK * tickDuration;
                        letter_break.WriteToStream(toneStream);
                        waiting_time += LETTER_BREAK * tickDuration;
                    }

                    OnWaveFileGenerated?.Invoke(this, new WaveEventArgs(toneStream.ToArray()));

                    characterCounter++;
                    double percentageProgress = characterCounter / code.Length;
                    ProgressChanged(percentageProgress, 1);

                    Thread.Sleep(waiting_time);

                    if (stopped)
                    {
                        return;
                    }
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
        protected char GetKeyFromValue(string token)
        {
            foreach (KeyValuePair<char, string> s in _mapping)
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
        protected int CountStringOccurence(string stringToCheck, string stringToSearchFor)
        {
            MatchCollection collection = Regex.Matches(stringToCheck, stringToSearchFor);
            return collection.Count;
        }

        protected void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, null, new PluginProgressEventArgs(value, max));
        }


    }

    public class WaveEventArgs : EventArgs
    {
        public byte[] WaveFile { get; private set; }
        public WaveEventArgs(byte[] toneValue)
        {
            WaveFile = toneValue;
        }
    }


    public class InternationalMorseEncoder : MorseEncoder
    {
        public InternationalMorseEncoder()
        {
            // Generate our mapping of letters and the morse code
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
            // Generate our mapping of letters and the morse code
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
            // Generate our mapping of letters and the morse code
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
            // Generate our mapping of letters and the morse code
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

    public class TapCodeEncoder : MorseEncoder
    {
        public TapCodeEncoder()
        {
            // Generate our mapping of letters and the tap code
            // Based on the description from Wikipedia: https://en.wikipedia.org/wiki/Tap_code
            _mapping.Add(' ', "/");

            _mapping.Add('A', ". .");
            _mapping.Add('B', ". ..");
            _mapping.Add('C', ". ...");
            _mapping.Add('D', ". ....");
            _mapping.Add('E', ". .....");

            _mapping.Add('F', ".. .");
            _mapping.Add('G', ".. ..");
            _mapping.Add('H', ".. ...");
            _mapping.Add('I', ".. ....");
            _mapping.Add('J', ".. .....");

            _mapping.Add('L', "... .");
            _mapping.Add('M', "... ..");
            _mapping.Add('N', "... ...");
            _mapping.Add('O', "... ....");
            _mapping.Add('P', "... .....");

            _mapping.Add('Q', ".... .");
            _mapping.Add('R', ".... ..");
            _mapping.Add('S', ".... ...");
            _mapping.Add('T', ".... ....");
            _mapping.Add('U', ".... .....");

            _mapping.Add('V', "..... .");
            _mapping.Add('W', "..... ..");
            _mapping.Add('X', "..... ...");
            _mapping.Add('Y', "..... ....");
            _mapping.Add('Z', "..... .....");
        }

        public override string Encode(string text)
        {
            string uppertext = text.Trim().ToUpper();

            //the tap code has C = K
            uppertext = uppertext.Replace("K", "C");
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < uppertext.Length; i++)
            {
                if (_mapping.ContainsKey(uppertext[i]))
                {
                    //we found a corresponding tapp code and put it into the output
                    builder.Append(_mapping[uppertext[i]]);
                    builder.Append("  "); //after each letter, we have a longer pause
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

        public override string Decode(string code)
        {
            StringBuilder builder = new StringBuilder();
            string[] tokens = Regex.Replace(code.Trim(), @" +", " ").Split(' ');
            int tokennumber = 0;

            string lastToken = string.Empty;
            bool first = true;

            foreach (string currentToken in tokens)
            {
                tokennumber++;

                if (_mapping.ContainsValue(currentToken.Trim()))
                {
                    if (first == false)
                    {
                        //we already have a half of a token, but not the second one so we output ?
                        builder.Append("?");
                    }
                    //1. Count leading linebreaks in our currentToken and put them into the output
                    int count = CountStringOccurence(currentToken.TrimEnd(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                    //2. Now replace the trimmed token by its corresponding value
                    builder.Append(GetKeyFromValue(currentToken.Trim()));
                    //3. Now count trailing linebreaks and put them into the output
                    count = CountStringOccurence(currentToken.TrimStart(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                    first = true;
                    continue;
                }

                if (!IsValidTapToken(currentToken))
                {
                    first = true;
                    //1. Count leading linebreaks in our token and put them into the output
                    int count = CountStringOccurence(currentToken.TrimEnd(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                    //2. Now replace the trimmed token by its corresponding value
                    builder.Append("?");
                    //3. Now count trailing linebreaks and put them into the output
                    count = CountStringOccurence(currentToken.TrimStart(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                    continue;
                }

                //here, we create a combined token consisting of two tokens, e.g. ".. ..." 
                if (first)
                {
                    lastToken = currentToken;
                    first = false;
                    continue;
                }
                else
                {
                    first = true;
                }
                string combinedToken = lastToken + " " + currentToken;

                if (_mapping.ContainsValue(combinedToken))
                {
                    //We have no leading or trailing linebreaks because we directly found the token
                    //So we do not need to add line breaks
                    builder.Append(GetKeyFromValue(combinedToken));
                }
                else if (combinedToken.StartsWith("\r\n") || combinedToken.EndsWith("\r\n"))
                {
                    //1. Count leading linebreaks in our token and put them into the output
                    int count = CountStringOccurence(combinedToken.TrimEnd(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                    //2. Now replace the trimmed token by its corresponding value
                    builder.Append(GetKeyFromValue(combinedToken.Trim()));
                    //3. Now count trailing linebreaks and put them into the output
                    count = CountStringOccurence(combinedToken.TrimStart(), "\r\n");
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
        /// Tap tokens only consists of .
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool IsValidTapToken(string token)
        {
            foreach (char c in token)
            {
                if (c != '.' && c != '\r' && c != '\n' && c != '/')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
