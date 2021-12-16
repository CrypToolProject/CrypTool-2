/*
 * Created by SharpDevelop.
 * User: LYCJ
 * Date: 19/10/2007
 * Time: 3:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.IO;

namespace QuickZip.MiniHtml2
{
    /// <summary>
    /// MiniHtml internal Html Paraser, used since D7 version of TQzHtmlLabel2,
    /// not too efficient as it does a lot of string swapping.
    /// </summary>
    public class HtmlParser
    {
        private readonly HtmlTagTree tree;
        internal HtmlTagNode previousNode = null;

        /// <summary>
        /// Constructor
        /// </summary>        
        public HtmlParser(HtmlTagTree aTree)
        {
            tree = aTree;
        }

        /// <summary> Return true if both < and > found in input. </summary>        
        private bool haveClosingTag(string input)
        {
            if ((input.IndexOf('[') != -1) && (input.IndexOf(']') != -1))
            {
                return false;
            }

            return true;
        }
        /// <summary> Add a Non TextTag to Tag List </summary>        
        internal void addTag(HtmlTag aTag)
        {
            //            HtmlTagNode newNode = new HtmlTagNode(
            if (previousNode == null) { previousNode = tree; }

            while (!previousNode.CanAdd(aTag))
            {
                previousNode = previousNode.Parent;
            }

            previousNode = previousNode.Add(aTag);
        }
        /// <summary>
        /// Parse a string and return text before a tag, the tag and it's variables, and the string after that tag.
        /// </summary>
        private static void readNextTag(string s, ref string beforeTag, ref string afterTag, ref string tagName,
                                          ref string tagVars, char startBracket, char endBracket)
        {
            int pos1 = s.IndexOf(startBracket);
            int pos2 = s.IndexOf(endBracket);

            if ((pos1 == -1) || (pos2 == -1) || (pos2 < pos1))
            {
                tagVars = "";
                beforeTag = s;
                afterTag = "";
            }
            else
            {
                string tagStr = s.Substring(pos1 + 1, pos2 - pos1 - 1);
                beforeTag = s.Substring(0, pos1);
                afterTag = s.Substring(pos2 + 1, s.Length - pos2 - 1);

                int pos3 = tagStr.IndexOf(' ');
                if ((pos3 != -1) && (tagStr != ""))
                {
                    tagName = tagStr.Substring(0, pos3);
                    tagVars = tagStr.Substring(pos3 + 1, tagStr.Length - pos3 - 1);
                }
                else
                {
                    tagName = tagStr;
                    tagVars = "";
                }

                if (tagName.StartsWith("!--"))
                {
                    if ((tagName.Length < 6) || (!(tagName.EndsWith("--"))))
                    {
                        int pos4 = afterTag.IndexOf("-->");
                        if (pos4 != -1)
                        {
                            afterTag = afterTag.Substring(pos4 + 2, afterTag.Length - pos4 - 1);
                        }
                    }
                    tagName = "";
                    tagVars = "";
                }

            }
        }
        /// <summary>
        /// Parse a string and return text before a tag, the tag and it's variables, and the string after that tag.
        /// </summary>
        private static void readNextTag(string s, ref string beforeTag, ref string afterTag, ref string tagName, ref string tagVars)
        {
            HtmlParser.readNextTag(s, ref beforeTag, ref afterTag, ref tagName, ref tagVars, '[', ']');
        }
        /// <summary>
        /// Recrusive paraser.
        /// </summary>        
        private void parseHtml(ref string s)
        {
            string beforeTag = "", afterTag = "", tagName = "", tagVar = "";
            readNextTag(s, ref beforeTag, ref afterTag, ref tagName, ref tagVar);

            if (beforeTag != "")
            {
                addTag(new HtmlTag(beforeTag));         //Text
            }

            if (tagName != "")
            {
                addTag(new HtmlTag(tagName, tagVar));
            }

            s = afterTag;
        }
        /// <summary>
        /// Parse Html
        /// </summary>        
        public void Parse(TextReader reader)
        {
            previousNode = null;

            string input = reader.ReadToEnd();

            while (input != "")
            {
                parseHtml(ref input);
            }
        }

        public static void DebugUnit()
        {
            //            
            //            mh.parser.Parse((new StringReader(Html)));
            //            mh.masterTag.childTags.PrintItems();
        }
    }
}
