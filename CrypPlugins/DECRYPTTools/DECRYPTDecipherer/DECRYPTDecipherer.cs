/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using Page = CrypTool.Plugins.DECRYPTTools.Util.Page;

namespace CrypTool.Plugins.DECRYPTTools
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.DECRYPTTools.Properties.Resources", "DECRYPTDeciphererCaption", "DECRYPTDeciphererTooltip", "DECRYPTTools/userdoc.xml", "DECRYPTTools/icon.png")]
    [ComponentCategory(ComponentCategory.DECRYPTProjectComponent)]
    public class DECRYPTDecipherer : ICrypComponent
    {
        #region Private Variables

        private string _DECRYPTTextDocument;
        private string _DECRYPTKeyDocument;
        private string _outputText;
        private readonly DECRYPTDeciphererPresentation _presentation = new DECRYPTDeciphererPresentation();
        private readonly DECRYPTDeciphererSettings _settings = new DECRYPTDeciphererSettings();

        #endregion

        #region Constructor

        public DECRYPTDecipherer()
        {
            _presentation.OnGuiLogNotificationOccured += ForwardGuiLogNotification;
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Input of a DECRYPTTextDocument (cipher file)
        /// </summary>
        [PropertyInfo(Direction.InputData, "DECRYPTTextDocumentCaption", "DECRYPTTextDocumentTooltip")]
        public string DECRYPTTextDocument
        {
            get => _DECRYPTTextDocument;
            set => _DECRYPTTextDocument = value;
        }

        /// <summary>
        /// Input of DECRYPTKeyDocument (key file)
        /// </summary>
        [PropertyInfo(Direction.InputData, "DECRYPTKeyDocumentCaption", "DECRYPTKeyTooltip")]
        public string DECRYPTKeyDocument
        {
            get => _DECRYPTKeyDocument;
            set => _DECRYPTKeyDocument = value;
        }

        /// <summary>
        /// Output of text (parsed or parsed + decoded)
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public string OutputText
        {
            get => _outputText;
            set => _outputText = value;
        }

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Methods

        public void Dispose()
        {

        }

        public void Execute()
        {
            Util.Decoder decoder = null;
            if (DECRYPTKeyDocument != null)
            {
                decoder = new Util.Decoder(DECRYPTKeyDocument);
                decoder.OnGuiLogNotificationOccured += ForwardGuiLogNotification;
            }

            List<Token> nulls = _settings.GetNulls();
            if (decoder != null)
            {
                foreach (Token nullToken in decoder.GetNulls())
                {
                    nulls.Add(nullToken);
                }
            }

            Parser parser = null;
            switch (_settings.ParserType)
            {
                case ParserType.NoNomenclatureParser:
                    parser = new NoNomenclatureParser(2, nulls);
                    break;
                case ParserType.Nomenclature3DigitsEndingWithNull1DigitsParser:
                    parser = new Nomenclature3DigitsEndingWithNull1DigitsParser(nulls);
                    break;
                case ParserType.Nomenclature3DigitsEndingWithNull2DigitsParser:
                    parser = new Nomenclature3DigitsEndingWithNull2DigitsParser(nulls);
                    break;
                case ParserType.Nomenclature4DigitsWithPrefixParser:
                    parser = new Nomenclature4DigitsWithPrefixParser(_settings.GetPrefix(), nulls);
                    break;
                case ParserType.Francia4Parser:
                    parser = new Francia4Parser(nulls);
                    break;
                case ParserType.Francia6Parser:
                    parser = new Francia6Parser(nulls);
                    break;
                case ParserType.Francia17Parser:
                    parser = new Francia17Parser(nulls);
                    break;
                case ParserType.Francia18Parser:
                    parser = new Francia18Parser(nulls);
                    break;
                case ParserType.VariableLengthHomophonicCipher:
                    parser = new VariableLengthHomophonicCipher(nulls, decoder);
                    break;
                case ParserType.Francia346:
                    parser = new Francia346Parser(nulls);
                    break;
                case ParserType.Francia283:
                    parser = new Francia283Parser();
                    break;
                case ParserType.ManuallySplittedTextParser:
                    parser = new ManuallySplittedTextParser();
                    break;
                case ParserType.SimpleSingleTokenParser:
                default:
                    parser = new SimpleSingleTokenParser();
                    break;
            }

            //Step 1: Apply parser
            parser.OnGuiLogNotificationOccured += ForwardGuiLogNotification;
            parser.DECRYPTTextDocument = DECRYPTTextDocument;
            parser.ShowCommentsPlaintextCleartext = _settings.ShowCommentsPlaintextCleartext;
            parser.ShowUnknownTranscriptionSymbols = _settings.ShowUnknownTranscriptionSymbols;
            DateTime startTime = DateTime.Now;
            TextDocument document = parser.GetTextDocument();
            parser.CleanupDocument(document);
            if (document == null)
            {
                return;
            }
            GuiLogMessage(string.Format("Parsed document in {0}ms", (DateTime.Now - startTime).TotalMilliseconds), NotificationLevel.Info);

            //Step 2: Decode parsed document
            if (decoder != null)
            {
                startTime = DateTime.Now;
                if (_settings.UseKeyAsPlaintext)
                {
                    //we use the ManuallySplittedTextParser to get plaintext mapping for the ciphertext
                    ManuallySplittedTextParser manuallySplittedTextParser = new ManuallySplittedTextParser(true)
                    {
                        DECRYPTTextDocument = DECRYPTKeyDocument
                    };
                    TextDocument plaintextDocument = manuallySplittedTextParser.GetTextDocument();

                    foreach (Page page in document.Pages)
                    {
                        Page plaintextPage = plaintextDocument.Pages[page.PageNumber - 1];
                        foreach (Line line in page.Lines)
                        {
                            if (line.LineType == LineType.Comment)
                            {
                                continue;
                            }
                            if (line.LineNumber - 1 >= plaintextPage.Lines.Count)
                            {
                                break;
                            }
                            Line plaintextLine = plaintextPage.Lines[line.LineNumber - 1];
                            for (int i = 0; i < line.Tokens.Count; i++)
                            {
                                Token ciphertextToken = line.Tokens[i];
                                if (ciphertextToken.TokenType == TokenType.Tag)
                                {
                                    continue;
                                }
                                if (i > plaintextLine.Tokens.Count - 1)
                                {
                                    break;
                                }
                                Token plaintextToken = plaintextLine.Tokens[i];
                                ciphertextToken.DecodedSymbols = plaintextToken.Symbols;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Page page in document.Pages)
                    {
                        foreach (Line line in page.Lines)
                        {
                            decoder.Decode(line);
                        }
                    }
                }
                GuiLogMessage(string.Format("Decoded document in {0}ms", (DateTime.Now - startTime).TotalMilliseconds), NotificationLevel.Info);
            }

            //Step 3: Show final result
            _presentation.ShowDocument(document);

            if (decoder != null)
            {
                StringBuilder outputBuilder = new StringBuilder();

                foreach (Page page in document.Pages)
                {
                    foreach (Line line in page.Lines)
                    {
                        if (line.LineType == LineType.Comment)
                        {
                            outputBuilder.AppendLine(line.ToString());
                        }
                        else
                        {
                            foreach (Token token in line.Tokens)
                            {
                                if (token.TokenType == TokenType.Tag)
                                {
                                    foreach (Symbol symbol in token.Symbols)
                                    {
                                        outputBuilder.Append(symbol.Text);
                                    }
                                }
                                else
                                {
                                    foreach (Symbol symbol in token.DecodedSymbols)
                                    {
                                        outputBuilder.Append(symbol.Text);
                                    }
                                    if (_settings.UseOutputSeparators)
                                    {
                                        outputBuilder.Append("|");
                                    }
                                }
                            }
                            outputBuilder.AppendLine();
                        }
                    }
                }
                _outputText = outputBuilder.ToString();
            }
            else
            {
                _outputText = document.ToString();
            }
            OnPropertyChanged("OutputText");
        }

        /// <summary>
        /// Forwards the gui message of the parsers to CrypTool 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ForwardGuiLogNotification(IPlugin sender, GuiLogEventArgs args)
        {
            GuiLogMessage(args.Message, args.NotificationLevel);
        }

        public void Initialize()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {
            DECRYPTTextDocument = null;
            DECRYPTKeyDocument = null;
        }

        public void Stop()
        {

        }

        #endregion

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
    }
}
