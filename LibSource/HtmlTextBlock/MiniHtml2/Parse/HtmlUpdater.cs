/*
 * Created by SharpDevelop.
 * User: LYCJ
 * Date: 20/10/2007
 * Time: 23:25
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;

namespace QuickZip.MiniHtml2
{


    /// <summary>
    /// Description of Updater.
    /// </summary>
    public class HtmlUpdater
    {
        private readonly TextBlock textBlock;
        private readonly CurrentStateType currentState = new CurrentStateType();

        private void UpdateStyle(HtmlTag aTag)
        {
            currentState.UpdateStyle(aTag);
        }

        private Inline UpdateElement(HtmlTag aTag)
        {
            Inline retVal = null;

            switch (aTag.Name)
            {
                case "text":
                    retVal = new Run(aTag["value"]);

                    if (currentState.Bold)
                    {
                        retVal = new Bold(retVal);
                    }

                    if (currentState.Italic)
                    {
                        retVal = new Italic(retVal);
                    }

                    if (currentState.Underline)
                    {
                        retVal = new Underline(retVal);
                    }

                    break;
                case "br":
                    retVal = new LineBreak();
                    break;
            }


            if (currentState.HyperLink != null && currentState.HyperLink != "")
            {
                Hyperlink link = new Hyperlink(retVal);
                try
                {
                    link.NavigateUri = new Uri(currentState.HyperLink);
                    link.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(link_RequestNavigate);
                }
                catch
                {
                    link.NavigateUri = null;
                }
                retVal = link;
            }

            return retVal;
        }

        private void link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (e.Uri != null)
            {
                if (!e.Uri.IsAbsoluteUri)
                {
                    throw new InvalidOperationException("An absolute URI is required.");
                }

                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
        }

        public HtmlUpdater(TextBlock aBlock)
        {
            textBlock = aBlock;
        }

        public void Update(HtmlTagTree tagTree)
        {
            List<HtmlTag> tagList = tagTree.ToHtmlTagList();

            foreach (HtmlTag tag in tagList)
            {
                switch (Defines.BuiltinTags[tag.ID].flags)
                {
                    case HTMLFlag.TextFormat: UpdateStyle(tag); break;
                    case HTMLFlag.Element: textBlock.Inlines.Add(UpdateElement(tag)); break;
                }

            }
        }
    }
}
