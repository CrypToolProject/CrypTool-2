/*
    Copyright 2013 Christopher Konze, University of Kassel
 
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
using System.Drawing;
using VisualDecoder_ocr.model;

namespace VisualDecoder_ocr.Decoders
{
    /// <summary>
    /// The OCRDecoder trys to do an optical character recocnition on the given picture
    /// </summary>
    internal class OCRDecoder : ImageDecoder
    {
        private const string TessractData = @"Lib\tessdata\";
        public OCRDecoder(VisualDecoder caller): base(caller){}

        public override DecoderItem Decode(byte[] input, VisualDecoderSettings settings)
        {
            var result =  new DecoderItem();
            var language = settings.OCRLanguage.ToString();

            //use eng if the user dont know the language
            if (language.Equals(VisualDecoderSettings.OCRLanguages.unkown.ToString()))
                language = VisualDecoderSettings.OCRLanguages.eng.ToString();

            using (var ocr =  new tessnet2.Tesseract()){
                try
                {
                    ocr.Init(TessractData, language, settings.NumericMode);
                    List<tessnet2.Word> r1 = ocr.DoOCR(ByteArrayToImage(input), Rectangle.Empty);
                    
                    //aggregate resultTest
                    string resultText = "";
                    int lc = tessnet2.Tesseract.LineCount(r1);
                    for (int i = 0; i < lc; i++)
                        resultText += tessnet2.Tesseract.GetLineText(r1, i) + "\n";
                    
                    //fill result
                    result.CodePayload = resultText;
                    result.CodeType = "none";
                    result.BitmapWithMarkedCode = input;
                    ocr.Clear();
                }
                catch (Exception) // well, the ocr lib sucks... it sometimes trows memory leaks. 
                                  // But it is the best opensource lib available.
                {
                }
            }
            return result;
        }
    }
}