using System.Text;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.IO;

namespace WebService
{
   public class Canonicalizator
   {
       #region Fields

       private Transform transform;
       private XmlDocument inputString;

       #endregion

       #region Constructor

       public Canonicalizator(XmlDocument inputString)
       {
       
           this.inputString = inputString;
       }

       #endregion

       #region Methods

       public Stream CanonicalizeNode(XmlElement nodeToCanon)
       {
           XmlNode node = (XmlNode)nodeToCanon;
           XmlNodeReader reader = new XmlNodeReader(node);
           Stream stream = new MemoryStream();
           XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
           writer.WriteNode(reader, false);
           writer.Flush();
           stream.Position = 0;
           XmlDsigExcC14NTransform transform = new XmlDsigExcC14NTransform();
           transform.LoadInput(stream);
           return transform.GetOutput() as Stream;
       }

       #endregion
   }
}
