using System.Collections.Generic;
using System.IO;
using System.Xml;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = System.IO.Directory;
using System;

namespace CrypTool.Core
{
    public class SearchResult
    {
        public SearchResult()
        {
            Contexts = new List<string>();
        }

        public string Plugin { get; set; }
        public List<string> Contexts { get; set; }
        public float Score { get; set; }
    }

    public class SearchProvider
    {
        private const string ContentField = "content";
        private const string PluginField = "plugin";
        public string HelpFilePath { get; set; }
        public string IndexPath { get; set; }

        public List<SearchResult> Search(string SearchString)
        {
            var LocalSearch = new LocalSearchTool { IndexPath = IndexPath };
            LocalSearch.Search(SearchString);

            //Hier eventuell dann noch die Online-Suche miteinbauen.
            //Einfach neben der Klasse "LocalSearchTool" noch eine Klasse
            //"OnlineSearchTool" erstellen
            return LocalSearch.SearchResults;
        }

        public void CreateIndexes()
        {
            Lucene.Net.Store.Directory dir = FSDirectory.GetDirectory(IndexPath, true);
            Analyzer analyzer = new StandardAnalyzer();
            var indexWriter = new IndexWriter(dir, analyzer, true, new IndexWriter.MaxFieldLength(25000));

            foreach (var File in Directory.GetFiles(HelpFilePath,"*.xaml"))
            {
                var text = GetTextFromXaml(File).Replace("\r\n"," ").Replace("\n"," ");
                var doc = new Document();

                var fldContent = new Field(ContentField, text, Field.Store.YES, Field.Index.TOKENIZED,
                              Field.TermVector.WITH_OFFSETS);
                var fldName = new Field(PluginField, Path.GetFileNameWithoutExtension(Path.GetFileName(File)), Field.Store.YES, Field.Index.NO,
                              Field.TermVector.NO);
                doc.Add(fldContent);
                doc.Add(fldName);
                indexWriter.AddDocument(doc);
            }
            indexWriter.Optimize();
            indexWriter.Close();
        }

        private static string GetTextFromXaml(string Xaml)
        {
            var XamlDoc = new XmlDocument();
            XamlDoc.Load(Xaml);
            string text = ReadXml(XamlDoc.ChildNodes);
            return text;
        }

        private static string ReadXml(XmlNodeList Nodes)
        {
            string Result = string.Empty;
            if (Nodes.Count > 0)
                foreach (XmlNode o in Nodes)
                {
                    if (!string.IsNullOrEmpty(o.Value))
                        Result += o.Value.Trim(new[] { ' ' });
                    Result += ReadXml(o.ChildNodes);
                }
            return Result;
        }
    }

    public class SearchTool
    {
        public List<SearchResult> SearchResults { get; set; }

        protected const string ContentField = "content";
        protected const string PluginField = "plugin";


        public virtual void Search(string SearchString)
        {
            SearchResults = new List<SearchResult>();
        }
    }

    public class LocalSearchTool : SearchTool
    {
        public string IndexPath { get; set; }

        private const int ContextLeftOffset = 15;
        private const int ContextRightOffset = 35;
        private const float MinSimilarity = 0.7f;

        public override void Search(string SearchString)
        {
            base.Search(SearchString);
            SearchString = SearchString.ToLower();
            var dir = FSDirectory.GetDirectory(IndexPath, false);
            var searcher = new IndexSearcher(dir);
            var parser = new QueryParser(ContentField, new StandardAnalyzer());

            foreach (var s in SearchString.Split(new []{' '}))
            {
                var query = parser.GetFuzzyQuery(ContentField, s, MinSimilarity);

                Hits hits = searcher.Search(query);

                for (int i = 0; i < hits.Length(); i++)
                {
                    Document doc = hits.Doc(i);
                    var result = new SearchResult { Score = hits.Score(i), Plugin = doc.Get(PluginField) };

                    //Text des aktuellen Dokuments auslesen
                    string text = doc.Get(ContentField);
                    //Alle indizierten Wörter dieses Dokumentes auslesen
                    var tpv = (TermPositionVector)IndexReader.Open(dir).GetTermFreqVector(hits.Id(i), ContentField);
                    String[] DocTerms = tpv.GetTerms();
                    //Die Anzahl der Erscheinungen aller Wörter auslesen
                    int[] freq = tpv.GetTermFrequencies();
                    var words = new List<string>(DocTerms);
                    //Hier wollen wir nun die Positionen der Erscheinungen des Suchwortes herausfinden
                    for (int t = 0; t < freq.Length; t++)
                    {
                        //Falls das Suchwort mit dem aktuellen Wort übereinstimmt...
                        if (ContainsSearchString(SearchString, DocTerms[t], words))
                        {
                            //...können wir die Positionen auslesen
                            TermVectorOffsetInfo[] offsets = tpv.GetOffsets(t);
                            //Das Array beinhaltet nun für das Suchwort alle Auftreten mit jeweils Anfang und Ende
                            for (int j = 0; j < offsets.Length; j++)
                            {
                                //Jetz muss nur noch ein kleiner Kontextausschnitt ausgelesen werden, damit der User etwas damit anfangen kann
                                int start = offsets[j].GetStartOffset();
                                int end = offsets[j].GetEndOffset();
                                int contextStart = start - ContextLeftOffset;
                                contextStart = contextStart < 0 ? 0 : contextStart;
                                int contextEnd = end + ContextRightOffset;
                                contextEnd = contextEnd > text.Length ? text.Length : contextEnd;
                                //Nun wollen wir noch bis zum Ende des nächsten Wortes lesen, um das Ergebnis besser lesbar zu machen
                                int nextEndSpace = text.IndexOf(" ", contextEnd);
                                contextEnd = nextEndSpace > 0 ? nextEndSpace : contextEnd;
                                //Maximal so viele Zeichen darf der Text nach einem Leerzeichen links von dem Suchergebnis durchsucht werden
                                int leftSpaceOffset = contextStart;
                                //Finden des nächstenLeerzeichens links vom Suchergebnis
                                int nextStartSpace = text.LastIndexOf(" ", contextStart, leftSpaceOffset);
                                //Falls es kein Space in der Nöhe gibt brauchen wir natürlich auch nichts verändern
                                contextStart = nextStartSpace > 0 ? nextStartSpace : contextStart;
                                int contextLength = contextEnd - contextStart;
                                contextLength = contextLength > text.Length ? text.Length : contextLength;
                                //Kontext auslesen
                                string context = text.Substring(contextStart, contextLength);
                                //und den Searchresults zusammen mit dem zugehörigen PlugInNamen und dem HitScore hinzufügen
                                result.Contexts.Add(context);
                            }
                        }
                    }
                    SearchResults.Add(result);
                }
            }
        }

        private static bool ContainsSearchString(string searchString, string docTerm, List<string> words)
        {
            var searchStrings = searchString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in searchStrings)
            {
                var fuzzyWords = new HashSet<string>(FuzzySearch.Search(s, words, MinSimilarity));
                if (fuzzyWords.Contains(docTerm))
                    return true;
            }

            return false;
        }
    }
}

