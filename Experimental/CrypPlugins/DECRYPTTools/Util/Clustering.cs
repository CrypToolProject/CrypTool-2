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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace CrypTool.Plugins.DECRYPTTools.Util
{
    /// <summary>
    /// A ClusterSet is a self-generating set of different Clusters using a dedicated matchBorder (threshold)
    /// </summary>
    public class ClusterSet : INotifyPropertyChanged
    {
        private readonly double _matchThreshold;
        private readonly ObservableCollection<Cluster> _clusters = new ObservableCollection<Cluster>();
        private readonly ObservableCollection<TextDocument> _documents = new ObservableCollection<TextDocument>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ClusterSet(double matchThreshold = 15)
        {
            _matchThreshold = matchThreshold;
        }

        /// <summary>
        /// Adds this document to our ClusterSet
        /// </summary>
        /// <param name="document"></param>
        public void AddDocument(TextDocument document)
        {

            //Step 0: Check, if document is already in the internal document list
            if (_documents.Contains(document))
            {
                return;
            }

            //Step 1: create frequencies of document
            TextDocumentWithFrequencies textDocumentWithFrequencies = new TextDocumentWithFrequencies
            {
                TextDocument = document //this creates the frequencies
            };

            //Step 2: check, if this belongs to any of our clusters
            Cluster bestMatchingCluster = null;
            double currentBestMatchingValue = double.MaxValue;
            foreach (Cluster cluster in _clusters)
            {
                double matchValue = cluster.GetMatchValue(textDocumentWithFrequencies);
                if (matchValue < _matchThreshold && matchValue < currentBestMatchingValue)
                {
                    currentBestMatchingValue = matchValue;
                    bestMatchingCluster = cluster;
                }
            }
            if (bestMatchingCluster != null)
            {
                //Step 2.1: we found a best-matching cluster; thus, we add the document
                bestMatchingCluster.AddTextDocumentWithFrequencies(textDocumentWithFrequencies);
            }
            else
            {
                //Step 2.1: we did not find a best-matching cluster; thus, we create a new one
                Cluster cluster = new Cluster();
                cluster.AddTextDocumentWithFrequencies(textDocumentWithFrequencies);
                Application.Current.Dispatcher.Invoke(new Action(() => _clusters.Add(cluster)));
            }

            //Store document in the overall list of all documents
            Application.Current.Dispatcher.Invoke(new Action(() => _documents.Add(document)));

            //Notify everyone that our clusters and documents have been changed
            OnPropertyChanged("Clusters");
            OnPropertyChanged("Documents");
            OnPropertyChanged("DocumentCount");
            OnPropertyChanged("ClusterCount");
        }

        /// <summary>
        /// Returns all clusters of this cluster set
        /// </summary>
        public ObservableCollection<Cluster> Clusters => _clusters;

        /// <summary>
        /// Returns all documents of this cluster set
        /// </summary>
        public ObservableCollection<TextDocument> Documents => _documents;

        /// <summary>
        /// Returns the number of documents stored in this ClusterSet
        /// </summary>
        public int DocumentCount => _documents.Count;

        /// <summary>
        /// Returns the number of clusters stored in this ClusterSet
        /// </summary>
        public int ClusterCount => _clusters.Count;

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// A cluster is a set of text document with similiar symbol frequencies
    /// </summary>
    public class Cluster : INotifyPropertyChanged
    {
        private readonly ObservableCollection<TextDocumentWithFrequencies> _documents = new ObservableCollection<TextDocumentWithFrequencies>();
        private readonly Dictionary<Symbol, double> _frequencies = new Dictionary<Symbol, double>();
        private string _name;
        private int _symbolCount = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Returns the name of this cluster
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Returns a list of all stored documents of this cluster
        /// </summary>
        public ObservableCollection<TextDocumentWithFrequencies> Documents => _documents;

        /// <summary>
        /// Returns the number of documents in this cluster
        /// </summary>
        public int DocumentCount => _documents.Count;


        /// <summary>
        /// Returns the sum of symbols of all documents in this cluster
        /// </summary>
        public int SymbolCount => _symbolCount;

        /// <summary>
        /// Returns the number of "different symbols"
        /// e.g. for a digit-based cipher this is usally 10
        /// </summary>
        public int DifferentSymbols
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<Symbol, double> keyvaluepair in _frequencies)
                {
                    if (keyvaluepair.Value >= 1)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Returns the match value of the given document and this cluster
        /// A "perfect match" is equal to 0.0
        /// </summary>
        /// <param name="textDocumentWithFrequencies"></param>
        /// <returns></returns>
        public double GetMatchValue(TextDocumentWithFrequencies textDocumentWithFrequencies)
        {
            double matchValue = 0;
            foreach (Symbol key in textDocumentWithFrequencies.Frequencies.Keys)
            {
                double frequencyDocument = textDocumentWithFrequencies.Frequencies[key];
                double frequencyCluster = 0;
                if (_frequencies.ContainsKey(key))
                {
                    frequencyCluster = _frequencies[key];
                }
                matchValue += Math.Abs(frequencyCluster - frequencyDocument);
            }
            return matchValue;
        }

        /// <summary>
        /// Adds a text document to this Cluster,
        /// also updates frequencies of the cluster
        /// </summary>
        /// <param name="textDocumentWithFrequencies"></param>
        public void AddTextDocumentWithFrequencies(TextDocumentWithFrequencies textDocumentWithFrequencies)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => _documents.Add(textDocumentWithFrequencies)));
            UpdateFrequencies();
            if (_documents.Count == 1)
            {
                _name = textDocumentWithFrequencies.TextDocument.CatalogName;
                OnPropertyChanged("Name");
            }

            _symbolCount += textDocumentWithFrequencies.TextDocument.TokenCount;

            OnPropertyChanged("Frequencies");
            OnPropertyChanged("FrequenciesSortedBySymbol");
            OnPropertyChanged("Documents");
            OnPropertyChanged("DocumentCount");
            OnPropertyChanged("ClusterInfo");
            OnPropertyChanged("SymbolCount");
            OnPropertyChanged("DifferentSymbols");
        }

        /// <summary>
        /// Updates the symbol frequencies of this cluster
        /// </summary>
        private void UpdateFrequencies()
        {
            _frequencies.Clear();

            Dictionary<Symbol, int> _absoluteValues = new Dictionary<Symbol, int>();
            int totalSymbols = 0;
            foreach (TextDocumentWithFrequencies document in _documents)
            {
                foreach (Page page in document.TextDocument.Pages)
                {
                    foreach (Line line in page.Lines)
                    {
                        // we don't count frequencies of the comments
                        if (line.LineType == LineType.Comment)
                        {
                            continue;
                        }
                        foreach (Token token in line.Tokens)
                        {
                            // we don't count frequencies of tags inside the text
                            if (token.TokenType == TokenType.Tag)
                            {
                                continue;
                            }
                            foreach (Symbol symbol in token.Symbols)
                            {
                                if (!_absoluteValues.ContainsKey(symbol))
                                {
                                    _absoluteValues[symbol] = 0;
                                }
                                _absoluteValues[symbol] = _absoluteValues[symbol] + 1;
                                totalSymbols++;
                            }
                        }
                    }
                }
            }
            foreach (Symbol key in _absoluteValues.Keys)
            {
                double frequency = _absoluteValues[key] / ((double)totalSymbols) * 100;
                _frequencies.Add(key, frequency);
            }
        }

        /// <summary>
        /// Returns all symbol frequencies of this Cluster
        /// </summary>
        public Dictionary<Symbol, double> Frequencies => _frequencies;

        /// <summary>
        /// Returns all symbol frequencies of this Cluster
        /// </summary>
        public List<KeyValuePair<Symbol, double>> SortedFrequencies
        {
            get
            {
                List<KeyValuePair<Symbol, double>> frequencies = _frequencies.ToList();
                frequencies.Sort(delegate (KeyValuePair<Symbol, double> a, KeyValuePair<Symbol, double> b)
                {
                    if (a.Value == b.Value)
                    {
                        return 0;
                    }
                    return a.Value < b.Value ? 1 : -1;
                });
                return frequencies;
            }
        }

        /// <summary>
        /// Returns the "info" of this cluster. Meaning, it returns a string representation
        /// containing the frequencies of all symbols
        /// </summary>
        public string ClusterInfo
        {
            get
            {
                List<KeyValuePair<Symbol, double>> frequencies = SortedFrequencies;
                if (frequencies == null || frequencies.Count == 0)
                {
                    return base.ToString();
                }
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < frequencies.Count; i++)
                {
                    KeyValuePair<Symbol, double> keyvaluepair = frequencies[i];
                    if (keyvaluepair.Value >= 1)
                    {
                        builder.Append(keyvaluepair.Key.Text);
                        if (!string.IsNullOrEmpty(keyvaluepair.Key.Top))
                        {
                            builder.Append("^" + keyvaluepair.Key.Top);
                        }
                        if (!string.IsNullOrEmpty(keyvaluepair.Key.Bottom))
                        {
                            builder.Append("_" + keyvaluepair.Key.Bottom);
                        }

                        builder.Append("=" + Math.Round(keyvaluepair.Value, 0));

                        if (i < frequencies.Count - 1)
                        {
                            builder.Append(", ");
                        }
                    }
                }
                string retString = builder.ToString();
                if (retString.EndsWith(", "))
                {
                    retString = retString.Substring(0, retString.Length - 2);
                }
                return retString;
            }
        }

        /// <summary>
        /// Returns the string representation of this Cluster
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ClusterInfo;
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

    }

    /// <summary>
    /// A TextDocumentWithFrequencies is a wrapper for a TextDocument
    /// It also contains the relative frequencies of the symbols of the TextDocument
    /// </summary>
    public class TextDocumentWithFrequencies
    {
        private readonly Dictionary<Symbol, double> _frequencies = new Dictionary<Symbol, double>();
        private TextDocument _textDocument;

        /// <summary>
        /// The text document of this TextDocumentWithFrequencies
        /// </summary>
        public TextDocument TextDocument
        {
            get => _textDocument;
            set
            {
                _textDocument = value;
                UpdateFrequencies();
            }
        }

        /// <summary>
        /// Computes the symbol frequencies of all symbols in this document
        /// </summary>
        private void UpdateFrequencies()
        {
            _frequencies.Clear();

            Dictionary<Symbol, int> _absoluteValues = new Dictionary<Symbol, int>();
            int totalSymbols = 0;

            foreach (Page page in _textDocument.Pages)
            {
                foreach (Line line in page.Lines)
                {
                    // we don't count frequencies of the comments
                    if (line.LineType == LineType.Comment)
                    {
                        continue;
                    }
                    foreach (Token token in line.Tokens)
                    {
                        // we don't count frequencies of tags inside the text
                        if (token.TokenType == TokenType.Tag)
                        {
                            continue;
                        }
                        foreach (Symbol symbol in token.Symbols)
                        {
                            if (!_absoluteValues.ContainsKey(symbol))
                            {
                                _absoluteValues[symbol] = 0;
                            }
                            _absoluteValues[symbol] = _absoluteValues[symbol] + 1;
                            totalSymbols++;
                        }
                    }
                }
            }
            foreach (Symbol key in _absoluteValues.Keys)
            {
                double frequency = _absoluteValues[key] / ((double)totalSymbols) * 100;
                _frequencies.Add(key, frequency);
            }
        }

        /// <summary>
        /// Returns all frequencies of the TextDocument of this TextDocumentWithFrequencies
        /// </summary>
        public Dictionary<Symbol, double> Frequencies => _frequencies;

        /// <summary>
        /// Returns all symbol frequencies of this Cluster
        /// </summary>
        public List<KeyValuePair<Symbol, double>> FrequenciesSortedBySymbol
        {
            get
            {
                List<KeyValuePair<Symbol, double>> frequencies = _frequencies.ToList();
                frequencies.Sort(delegate (KeyValuePair<Symbol, double> a, KeyValuePair<Symbol, double> b)
                {
                    return a.Key.CompareTo(b.Key);
                });
                return frequencies;
            }
        }

        /// <summary>
        /// Returns the ToString of the internal text document if it is not equal to null
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_textDocument != null)
            {
                return _textDocument.ToString();
            }
            else
            {
                return base.ToString();
            }
        }
    }

}
