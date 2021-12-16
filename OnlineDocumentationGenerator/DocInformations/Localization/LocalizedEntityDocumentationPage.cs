using System.Windows.Media.Imaging;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public abstract class LocalizedEntityDocumentationPage
    {
        public EntityDocumentationPage DocumentationPage { get; protected set; }
        public string Name { get; protected set; }
        public string Lang { get; protected set; }
        public abstract string FilePath { get; }

        public Reference.ReferenceList References => DocumentationPage.References;

        public BitmapFrame Icon
        {
            get; protected set;
        }
    }
}