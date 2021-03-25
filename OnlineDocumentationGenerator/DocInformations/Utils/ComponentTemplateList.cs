using System.Collections.Generic;

namespace OnlineDocumentationGenerator.DocInformations.Utils
{
    public class ComponentTemplateList
    {
        private readonly List<TemplateDocumentationPage> _templates = new List<TemplateDocumentationPage>();
        public List<TemplateDocumentationPage> Templates
        {
            get { return _templates; }
        }

        public void Add(TemplateDocumentationPage templateDocumentationPage)
        {
            Templates.Add(templateDocumentationPage);
        }
    }
}
