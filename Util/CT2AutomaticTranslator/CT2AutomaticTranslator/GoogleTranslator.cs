/*
   Copyright 2018 Nils Kopal, nils.kopal@uni-kassel.de

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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Apis.Util;

namespace CT2AutomaticTranslator
{
    /// <summary>
    /// Class for using the google cloud translation service
    /// </summary>
    public class GoogleTranslator
    {
        private static TranslateService _TranslateService;

        /// <summary>
        /// Inizialize using a google api key
        /// </summary>
        /// <param name="key"></param>
        public static void Init(string key)
        {
            _TranslateService = new TranslateService(new BaseClientService.Initializer()
            {
                ApiKey = key,
                ApplicationName = "CT2AutomaticTranslator"
            });
        }

        /// <summary>
        /// Translates the given list of strings using the google cloud
        /// if translation fails, it will try to repeat with 5, 10, 15 sec waiting
        /// After 3. fail, it will just throw the exception of the google cloud
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static List<string> Translate(List<string> text, string lang)
        {
            if (_TranslateService == null)
            {
                throw new Exception("Uninitialized! Please call Init(key) method with a valid google api key!");
            }
            if (text == null || text.Count == 0)
            {
                throw new ArgumentException("Given 0 strings to translate!");
            }
            if (lang == null || lang.Equals("") || lang.Equals(String.Empty))
            {
                throw new ArgumentException(String.Format("No valid target language given: {0}", lang));
            }
            int counter = 0;
            do
            {
                try
                {
                    Console.WriteLine("Sending to google cloud now {0} strings for translation...", text.Count);
                    var response = _TranslateService.Translations.List(text, lang).Execute();
                    List<string> translations = new List<string>();
                    foreach (var translation in response.Translations)
                    {
                        translations.Add(translation.TranslatedText);
                    }
                    Console.WriteLine("Received translation...");
                    return translations;
                }
                catch (Exception ex)
                {
                    counter++;
                    Console.Error.WriteLine("{0}. Exception during translation with google cloud: {1}", counter, ex.Message);
                    Console.Error.WriteLine("Waiting {0} seconds...", counter * 5);
                    Thread.Sleep(counter * 5000);
                    if (counter < 3)
                    {
                        Console.WriteLine("Retrying now...");
                    }
                    else
                    {
                        Console.Error.WriteLine("3rd time failed... Stopping now by throwing exception...");
                        throw ex;
                    }
                }
            } while (true);
        }
    }
}
