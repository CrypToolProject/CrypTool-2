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
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CrypTool.Plugins.DECRYPTTools.Util
{

    /// <summary>
    /// Type of tokens
    /// </summary>
    public enum TokenType
    {
        Unknown = 0,
        NullElement = 1,
        RegularElement = 2,
        NomenclatureElement = 3,
        Tag = 4,
        PlaintextElement = 5
    }

    public enum LineType
    {
        Comment = 0,
        Text = 1
    }

    /// <summary>
    /// A document contains one or more pages
    /// </summary>
    public class TextDocument : ICloneable
    {
        public List<Page> Pages
        {
            get;
            set;
        }

        /// <summary>
        /// Own index of document owner, i.e. file location, e.g. Segr.di Stato Francia 3/1/
        /// </summary>
        public string CatalogName
        {
            get;
            set;
        }

        /// <summary>
        /// Returns all tokens in a single list
        /// </summary>
        /// <returns></returns>
        public List<Token> ToList()
        {
            List<Token> tokenList = new List<Token>();
            foreach (Page page in Pages)
            {
                foreach (Line line in page.Lines)
                {
                    tokenList.AddRange(line.Tokens);
                }
            }
            return tokenList;
        }

        /// <summary>
        /// The name of the image(s) representing the cipher, e.g. 117r.jpg-117v.jpg
        /// </summary>
        public string ImageName
        {
            get;
            set;
        }

        /// <summary>
        /// Full name or initials of the transcriber, e.g. TimB
        /// </summary>
        public string TranscriberName
        {
            get;
            set;
        }

        /// <summary>
        /// The date the transcription was created, e.g. February 3, 2016
        /// </summary>
        public string DateOfTranscription
        {
            get;
            set;
        }

        /// <summary>
        /// The time it took to transcribe all images of a cipher in hours and minutes without counting breaks and quality checks,
        /// e.g. 30+30+60 minutes = 120 minutes
        /// </summary>
        public string TranscriptionTime
        {
            get;
            set;
        }

        /// <summary>
        /// Method, which was used, to create the transcription
        /// </summary>
        public string TranscriptionMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Description of e.g. difficulties, problems
        /// </summary>
        public string Comments
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor to create a TextDocument
        /// </summary>
        public TextDocument()
        {
            Pages = new List<Page>();
        }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Page page in Pages)
            {
                stringBuilder.Append(page.ToString());
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns the number of tokens of this TextDocument
        /// </summary>
        public int TokenCount
        {
            get
            {
                int count = 0;
                foreach (Page page in Pages)
                {
                    count += page.TokenCount;
                }
                return count;
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            StringBuilder builder = new StringBuilder();
            int counter = 0;
            foreach (Page page in Pages)
            {
                foreach (Line line in page.Lines)
                {
                    foreach (Token token in line.Tokens)
                    {
                        foreach (Symbol symbol in token.Symbols)
                        {
                            counter++;
                            hash = ((counter + hash) * 7) + (symbol != null ? symbol.GetHashCode() : 0);
                            builder.Append(hash);
                        }
                    }
                }
            }
            return builder.ToString().GetHashCode();
        }

        /// <summary>
        /// Compares both TextDocuments by comparing their hash code
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            TextDocument document = obj as TextDocument;
            if (document != null)
            {
                return GetHashCode() == document.GetHashCode();
            }
            return false;
        }

        /// <summary>
        /// Calculates the entropy over all regular elements of the given TextDocument
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static double CalculateEntropy(TextDocument document)
        {
            double entropy = 0;
            double N = 0;
            Dictionary<Token, double> absoluteCounts = new Dictionary<Token, double>();
            foreach (Page page in document.Pages)
            {
                foreach (Line line in page.Lines)
                {
                    foreach (Token token in line.Tokens)
                    {
                        if (token.TokenType == TokenType.RegularElement)
                        {
                            if (!absoluteCounts.ContainsKey(token))
                            {
                                absoluteCounts.Add(token, 0);
                            }
                            absoluteCounts[token]++;
                            N++;
                        }
                    }
                }
            }

            foreach (double absoluteValue in absoluteCounts.Values)
            {
                double Pi = absoluteValue / N;
                entropy += Math.Log(Pi, 2) * Pi;
            }

            return -1 * entropy;
        }

        public object Clone()
        {
            TextDocument textDocument = (TextDocument)MemberwiseClone();
            textDocument.Pages = new List<Page>();
            foreach (Page page in Pages)
            {
                Page clone = (Page)page.Clone();
                clone.ParentTextDocument = textDocument;
                textDocument.Pages.Add(clone);
            }
            return textDocument;
        }
    }

    /// <summary>
    /// A page contains several lines of text
    /// </summary>
    public class Page : ICloneable
    {
        public int PageNumber { get; set; }

        public Page(TextDocument textDocument)
        {
            Lines = new List<Line>();
            ParentTextDocument = textDocument;
        }

        public List<Line> Lines
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Line line in Lines)
            {
                stringBuilder.Append(line.ToString());
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        public object Clone()
        {
            Page page = (Page)MemberwiseClone();
            page.Lines = new List<Line>();
            foreach (Line line in Lines)
            {
                Line clone = (Line)line.Clone();
                clone.ParentPage = page;
                page.Lines.Add(clone);
            }
            return page;
        }

        public TextDocument ParentTextDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the number of tokens of this Page
        /// </summary>
        public int TokenCount
        {
            get
            {
                int count = 0;
                foreach (Line line in Lines)
                {
                    if (line.LineType != LineType.Comment)
                    {
                        count += line.TokenCount;
                    }
                }
                return count;
            }
        }
    }

    /// <summary>
    /// A line is a single line of text consisting of tokens
    /// </summary>
    public class Line : ICloneable
    {
        public int LineNumber { get; set; }

        public Line(Page page)
        {
            Tokens = new List<Token>();
            LineType = LineType.Text;
            ParentPage = page;
        }

        public List<Token> Tokens
        {
            get;
            set;
        }

        public LineType LineType
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Token token in Tokens)
            {
                stringBuilder.Append(token.ToString());
            }
            return stringBuilder.ToString();
        }

        public object Clone()
        {
            Line line = (Line)MemberwiseClone();
            line.Tokens = new List<Token>();
            foreach (Token token in Tokens)
            {
                Token clone = (Token)token.Clone();
                clone.ParentLine = line;
                line.Tokens.Add(clone);
            }
            return line;
        }

        public Page ParentPage
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the number of tokens of this line
        /// counts only RegularCodes, Nulls, and NomenclatureElements
        /// </summary>
        public int TokenCount
        {
            get
            {
                int count = 0;
                foreach (Token token in Tokens)
                {
                    if (token.TokenType == TokenType.RegularElement ||
                        token.TokenType == TokenType.NullElement ||
                        token.TokenType == TokenType.NomenclatureElement)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }

    /// <summary>
    /// A Token is a single element of text
    /// </summary>
    public class Token : IComparable, ICloneable
    {
        private List<Symbol> _symbols = new List<Symbol>();
        private List<Symbol> _decodedSymbols = new List<Symbol>();

        public Token(Line line)
        {
            TokenType = TokenType.Unknown;
            ParentLine = line;
            DecodedSymbols = new List<Symbol>();
            Symbols = new List<Symbol>();
        }

        public Token(Line line, string str) : this(line)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                builder.Append("" + c);
                if (builder.Length == 3)
                {
                    if (builder[1] == '^')
                    {
                        Symbol symbolWithTop = new Symbol(this)
                        {
                            Top = "" + builder[2],
                            Text = "" + builder[0]
                        };
                        Symbols.Add(symbolWithTop);
                        builder.Clear();
                        continue;
                    }
                    if (builder[1] == '_')
                    {
                        Symbol symbolWithBottom = new Symbol(this)
                        {
                            Bottom = "" + builder[2],
                            Text = "" + builder[0]
                        };
                        Symbols.Add(symbolWithBottom);
                        builder.Clear();
                        continue;
                    }
                    Symbol symbol = new Symbol(this)
                    {
                        Text = "" + builder[0]
                    };
                    Symbols.Add(symbol);
                    builder.Remove(0, 1);
                    continue;
                }
            }
            while (builder.Length > 0)
            {
                Symbol symbol = new Symbol(this)
                {
                    Text = "" + builder[0]
                };
                Symbols.Add(symbol);
                builder.Remove(0, 1);
                continue;
            }
        }

        /// <summary>
        /// Sets the symbols to the given list
        /// also sets parent token at each element to this token
        /// </summary>
        public List<Symbol> Symbols
        {
            get => _symbols;
            set
            {
                _symbols = value;
                foreach (Symbol symbol in _symbols)
                {
                    symbol.ParentToken = this;
                }
            }
        }

        /// <summary>
        /// Sets the decoded symbols to the given list
        /// also sets parent token at each element to this token
        /// </summary>
        public List<Symbol> DecodedSymbols
        {
            get => _decodedSymbols;
            set
            {
                if (value == null)
                {
                    return;
                }
                _decodedSymbols = value;
                foreach (Symbol symbol in _decodedSymbols)
                {
                    symbol.ParentToken = this;
                }
            }
        }

        public TokenType TokenType
        {
            get; set;
        }

        /// <summary>
        /// Returns the ui color of this token
        /// </summary>
        /// <returns></returns>
        public SolidColorBrush TextColor
        {
            get
            {
                if (ParentLine == null)
                {
                    return null;
                }
                switch (ParentLine.LineType)
                {
                    case LineType.Text:
                        switch (TokenType)
                        {
                            case TokenType.Tag:
                                return new SolidColorBrush(DECRYPTSettingsTab.DrawingToMedia(Properties.Settings.Default.TagElementColor));
                            case TokenType.NullElement:
                                return new SolidColorBrush(DECRYPTSettingsTab.DrawingToMedia(Properties.Settings.Default.NullElementColor));
                            case TokenType.NomenclatureElement:
                                return new SolidColorBrush(DECRYPTSettingsTab.DrawingToMedia(Properties.Settings.Default.NomenclatureElementColor));
                            case TokenType.RegularElement:
                                return new SolidColorBrush(DECRYPTSettingsTab.DrawingToMedia(Properties.Settings.Default.RegularElementColor));
                            default:
                                return Brushes.Black;
                        }
                    case LineType.Comment:
                        return new SolidColorBrush(DECRYPTSettingsTab.DrawingToMedia(Properties.Settings.Default.CommentColor));
                    default:
                        return Brushes.Black;
                }
            }
        }

        public Line ParentLine
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            Symbol symbol = obj as Symbol;
            if (symbol != null)
            {
                if (Symbols.Count != 1)
                {
                    return false;
                }
                return symbol.Equals(Symbols[0]);
            }

            Token token = obj as Token;
            if (token == null)
            {
                return false;
            }
            else
            {
                return token.GetHashCode() == GetHashCode();
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            StringBuilder builder = new StringBuilder();
            int counter = 0;
            foreach (Symbol symbol in Symbols)
            {
                counter++;
                hash = ((counter + hash) * 7) + (symbol != null ? symbol.GetHashCode() : 0);
                builder.Append(hash);
            }
            return builder.ToString().GetHashCode();
        }

        public static Token operator +(Token token, Symbol symbol)
        {
            token.Symbols.Add(symbol);
            symbol.ParentToken = token;
            return token;
        }

        public Symbol this[int index]
        {
            get => Symbols[index];
            set
            {
                Symbols[index] = value;
                value.ParentToken = this;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Symbol symbol in Symbols)
            {
                stringBuilder.Append(symbol.ToString());
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Compares two Tokens by comparing their Symbols
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            Token token = obj as Token;
            if (token == null)
            {
                return -1;
            }
            if (Symbols.Count == 0 || token.Symbols.Count == 0 || Symbols.Count != token.Symbols.Count)
            {
                return -1;
            }
            else
            {
                for (int i = 0; i < Symbols.Count; i++)
                {
                    int compareTo = Symbols[i].CompareTo(token.Symbols[i]);
                    if (compareTo != 0)
                    {
                        //we found two symbols that are not equal
                        return compareTo;
                    }
                }
                //if we are here, all symbols were equal
                return 0;
            }
        }

        public object Clone()
        {
            Token token = (Token)MemberwiseClone();
            token.Symbols = new List<Symbol>();
            token.DecodedSymbols = new List<Symbol>();
            foreach (Symbol symbol in Symbols)
            {
                Symbol clone = (Symbol)symbol.Clone();
                clone.ParentToken = token;
                token.Symbols.Add(clone);
            }
            foreach (Symbol symbol in DecodedSymbols)
            {
                Symbol clone = (Symbol)symbol.Clone();
                clone.ParentToken = token;
                token.DecodedSymbols.Add(clone);
            }
            return token;
        }
    }

    /// <summary>
    /// A symbol is a single symbol of text
    /// examples: A,B,C,...,a,b,c,...,0,1,2,...,0^1,0^2,... etc
    /// </summary>
    public class Symbol : ICloneable, IComparable
    {
        public Symbol(Token token)
        {
            ParentToken = token;
            Top = string.Empty;
            Text = string.Empty;
            Bottom = string.Empty;
            BottomChangesSymbol = true;
            TopChangesSymbol = true;
        }

        private readonly string _top;
        /// <summary>
        /// Top text of symbol
        /// </summary>
        public string Top
        {
            get;
            set;
        }

        /// <summary>
        /// Does the Top text change the meaning of the symbol?
        /// </summary>
        public bool TopChangesSymbol
        {
            get;
            set;
        }

        /// <summary>
        /// Main text of symbol
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Adds Top or Bottom diacritical marks
        /// </summary>
        /*public string VisualizeText
        {
            get
            {
                string ret = Text;
                if (!string.IsNullOrEmpty(Top) && Top.Equals("."))
                {          
                    ret = "\u0307" + ret; //dot on top                
                }
                if (!string.IsNullOrEmpty(Bottom) && Bottom.Equals("."))
                {
                    ret = "\u0323" + ret; //dot on bottom
                }
                if (!string.IsNullOrEmpty(Top) && Top.Equals("_"))
                {
                    ret = "\u0305" + ret; //line on top
                }
                if (!string.IsNullOrEmpty(Bottom) && Bottom.Equals("_"))
                {
                    ret = "\u0332" + ret; //line on bottom
                }
                return ret;
            }
        }*/


        private string _bottom;
        /// <summary>
        /// Bottom text of the symbol
        /// </summary>
        public string Bottom
        {
            get
            {
                if (_bottom != null && _bottom.Equals("_"))
                {
                    return "-";
                }
                return _bottom;
            }
            set => _bottom = value;
        }

        /// <summary>
        /// Does the Bottom text change the meaning of the symbol?
        /// </summary>
        public bool BottomChangesSymbol
        {
            get; set;
        }

        /// <summary>
        /// The parent token which this symbol belongs to
        /// </summary>
        public Token ParentToken
        {
            get; set;
        }

        /// <summary>
        /// Compares this symbol with another one
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            string str = obj as string;
            if (str != null)
            {
                return str.Equals(Text);
            }

            Symbol symbol = obj as Symbol;
            if (symbol == null)
            {
                return false;
            }
            else
            {
                return symbol.GetHashCode() == GetHashCode();
            }
        }

        /// <summary>
        /// Returns the hash code of this symbol
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 13;
            if (TopChangesSymbol)
            {
                hash = (hash * 3) + (Top != null ? Top.GetHashCode() : 0);
            }
            else
            {
                hash = (hash * 3) + string.Empty.GetHashCode();
            }

            hash = (hash * 5) + (Text != null ? Text.GetHashCode() : 0);

            if (BottomChangesSymbol)
            {
                hash = (hash * 7) + (Bottom != null ? Bottom.GetHashCode() : 0);
            }
            else
            {
                hash = (hash * 7) + string.Empty.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Clones this symbol
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Symbol symbol = (Symbol)MemberwiseClone();
            return symbol;
        }

        /// <summary>
        /// Returns the TextColor of this symbol by calling
        /// the property of the parent Token
        /// </summary>
        public SolidColorBrush TextColor
        {
            get
            {
                if (ParentToken != null)
                {
                    return ParentToken.TextColor;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the string representation of this symbol
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Text);

            if (!string.IsNullOrEmpty(Top))
            {
                stringBuilder.Append("^" + Top);
            }
            if (!string.IsNullOrEmpty(Bottom))
            {
                stringBuilder.Append("_" + Bottom);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Compares this Symbol to other objects (Symbols, Tokens, Strings)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            Symbol symobl = obj as Symbol;
            if (symobl != null)
            {
                return symobl.Text.CompareTo(Text);
            }
            Token token = obj as Token;
            if (token != null)
            {
                if (token.Symbols.Count > 0)
                {
                    return token.Symbols[0].CompareTo(Text);
                }
                return -1;
            }
            string str = obj as string;
            if (str != null)
            {

                return str.CompareTo(Text);
            }
            return -1;
        }

        /// <summary>
        /// We make nomenclature elements bold to make it easier when printed out for papers in greyscale
        /// </summary>
        /// <returns></returns>
        public FontWeight TextFontWeight
        {
            get
            {
                if (ParentToken.TokenType == TokenType.NomenclatureElement)
                {
                    return FontWeights.Bold;
                }
                else
                {
                    return FontWeights.Normal;
                }
            }
        }
    }
}
