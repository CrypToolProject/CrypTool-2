/*                              
   Copyright 2010 Nils Kopal

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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using WorkspaceManagerModel.Model.Tools;
using WorkspaceManagerModel.Properties;

namespace XMLSerialization
{
    /// <summary>
    /// Provides static methods for XML serialization and deserialization
    /// </summary>
    public static class XMLSerialization
    {
        private static readonly System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();

        /// <summary>
        /// Serializes the given object and all of its members to the given file using UTF-8 encoding
        /// Works only on objects which are marked as "Serializable"
        /// If compress==true then GZip is used for compressing
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        /// /// <param name="compress"></param>
        public static void Serialize(object obj, string filename, bool compress = false)
        {
            Serialize(obj, filename, Encoding.UTF8, compress);
        }

        /// <summary>
        /// Serializes the given object and all of its members to the given file using
        /// the given encoding
        /// Works only on objects which are marked as "Serializable"
        /// If compress==true then GZip is used for compressing
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        /// <param name="compress"></param>
        public static void Serialize(object obj, string filename, Encoding encoding, bool compress = false)
        {

            FileStream sourceFile = null;
            if (compress)
            {
                GZipStream compStream = null;
                StreamWriter writer = null;
                try
                {
                    sourceFile = File.Create(filename);
                    compStream = new GZipStream(sourceFile, CompressionMode.Compress);
                    writer = new StreamWriter(compStream, encoding);
                    Serialize(obj, writer, compress);
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                    if (compStream != null)
                    {
                        compStream.Dispose();
                    }
                    if (sourceFile != null)
                    {
                        sourceFile.Close();
                    }
                }
            }
            else
            {
                StreamWriter writer = null;
                try
                {
                    sourceFile = File.Create(filename);
                    writer = new StreamWriter(sourceFile, encoding);
                    Serialize(obj, writer);
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                    if (sourceFile != null)
                    {
                        sourceFile.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Serializes the given object and all of its members to the given writer as xml
        /// Works only on objects which are marked as "Serializable"
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="writer"></param>
        public static void Serialize(object obj, StreamWriter writer, bool compress = false)
        {
            HashSet<long> alreadySerializedObjects = new HashSet<long>();
            ObjectIDGenerator objectIDGenerator = new ObjectIDGenerator();
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"" + writer.Encoding.HeaderName + "\"?>");
            writer.WriteLine("<!--");
            writer.WriteLine("     XML serialized C# Objects");
            writer.WriteLine("     File created: " + DateTime.Now);
            writer.WriteLine("     File compressed: " + compress);
            writer.WriteLine("     XMLSerialization created by Nils Kopal");
            writer.WriteLine("     mailto: kopal(AT)cryptool.org");
            writer.WriteLine("-->");
            writer.WriteLine("<objects>");
            SerializeIt(obj, writer, alreadySerializedObjects, objectIDGenerator);
            writer.WriteLine("</objects>");
            writer.Flush();
        }

        /// <summary>
        /// Serializes the given object and all of its members to the given writer as xml
        /// Works only on object which are marked as "Serializable"
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="writer"></param>
        private static void SerializeIt(object obj, StreamWriter writer, HashSet<long> alreadySerializedObjects, ObjectIDGenerator objectIDGenerator)
        {
            //we only work on complex objects which are serializable and we did not see before
            if (obj == null ||
                isPrimitive(obj) ||
                !obj.GetType().IsSerializable ||
                alreadySerializedObjects.Contains(objectIDGenerator.GetID(obj)) ||
                obj is Delegate)
            {
                return;
            }

            MemberInfo[] memberInfos = obj.GetType().FindMembers(
                MemberTypes.All, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, new MemberFilter(DelegateToSearchCriteria), "ReferenceEquals");

            writer.WriteLine("<object>");
            writer.WriteLine("<type>" + obj.GetType().FullName + "</type>");
            writer.WriteLine("<id>" + objectIDGenerator.GetID(obj) + "</id>");

            writer.WriteLine("<members>");

            foreach (MemberInfo memberInfo in memberInfos)
            {
                if (memberInfo.MemberType == MemberTypes.Field && !obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).IsNotSerialized)
                {
                    string type = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FieldType.FullName;
                    object value = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(obj);

                    if (value is System.Collections.IList && value.GetType().IsGenericType)
                    {
                        string gentype = value.GetType().GetGenericArguments()[0].FullName;
                        type = "System.Collections.Generic.List;" + gentype;
                    }

                    writer.WriteLine("<member>");
                    writer.WriteLine("<name>" + ReplaceXMLSymbols(memberInfo.Name) + "</name>");
                    writer.WriteLine("<type>" + ReplaceXMLSymbols(type) + "</type>");

                    if (value is byte[])
                    {
                        byte[] bytes = (byte[])value;
                        writer.WriteLine("<value><![CDATA[" + Convert.ToBase64String(bytes) + "]]></value>");
                    }
                    else if (value is System.Collections.IList)
                    {
                        writer.WriteLine("<list>");
                        foreach (object o in (System.Collections.IList)value)
                        {
                            if (o.GetType().IsSerializable)
                            {
                                writer.WriteLine("<entry>");
                                writer.WriteLine("<type>" + o.GetType().FullName + "</type>");
                                if (isPrimitive(o))
                                {
                                    if (o is Enum)
                                    {
                                        writer.WriteLine("<value>" + o.GetHashCode() + "</value>");
                                    }
                                    else if (o is Point)
                                    {
                                        Point p = (Point)o;
                                        writer.WriteLine("<value><![CDATA[" + p.X + ";" + p.Y + "]]></value>");

                                    }
                                    else if (o is string)
                                    {
                                        byte[] bytes = enc.GetBytes(o.ToString());
                                        writer.WriteLine("<value><![CDATA[" + Convert.ToBase64String(bytes) + "]]></value>");
                                        writer.WriteLine("<B64Encoded/>");
                                    }
                                    else
                                    {
                                        writer.WriteLine("<value><![CDATA[" + o + "]]></value>");
                                    }
                                }
                                else
                                {
                                    writer.WriteLine("<reference>" + objectIDGenerator.GetID(o) + "</reference>");
                                }
                                writer.WriteLine("</entry>");
                            }
                        }
                        writer.WriteLine("</list>");
                    }
                    else if (value == null)
                    {
                        writer.WriteLine("<value></value>");
                    }
                    else if (isPrimitive(value))
                    {
                        if (value is Enum)
                        {
                            writer.WriteLine("<value>" + value.GetHashCode() + "</value>");
                        }
                        else if (value is Point)
                        {
                            Point p = (Point)value;
                            writer.WriteLine("<value><![CDATA[" + p.X + ";" + p.Y + "]]></value>");
                        }
                        else if (value is string)
                        {
                            byte[] bytes = enc.GetBytes(value.ToString());
                            writer.WriteLine("<value><![CDATA[" + Convert.ToBase64String(bytes) + "]]></value>");
                            writer.WriteLine("<B64Encoded/>");
                        }
                        else if (value is Color)
                        {
                            Color c = (Color)value;
                            writer.WriteLine("<value><![CDATA[" + c.R + ";" + c.G + ";" + c.B + ";" + c.A + "]]></value>");
                        }
                        else
                        {
                            writer.WriteLine("<value><![CDATA[" + value.ToString() + "]]></value>");
                        }
                    }
                    else
                    {
                        writer.WriteLine("<reference>" + objectIDGenerator.GetID(value) + "</reference>");
                    }
                    writer.WriteLine("</member>");
                }
            }
            writer.WriteLine("</members>");
            writer.WriteLine("</object>");
            writer.Flush();

            //Save obj so that we will not work on it again
            alreadySerializedObjects.Add(objectIDGenerator.GetID(obj));

            foreach (MemberInfo memberInfo in memberInfos)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    string type = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FieldType.FullName;
                    object value = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(obj);

                    if (value is System.Collections.IList && !(value is byte[]))
                    {
                        foreach (object o in (System.Collections.IList)value)
                        {
                            SerializeIt(o, writer, alreadySerializedObjects, objectIDGenerator);
                        }
                    }
                    else
                    {
                        SerializeIt(value, writer, alreadySerializedObjects, objectIDGenerator);
                    }

                }
            }
        }

        /// <summary>
        /// Check if the given object ist Primitve
        /// Primitive means isPrimitive returns true
        /// or Fullname does not start with "System"
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static bool isPrimitive(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (o is Enum)
            {
                return true;
            }

            return (o.GetType().IsPrimitive || o.GetType().FullName.Substring(0, 6).Equals("System"));
        }

        /// <summary>
        /// Returns true if MemberType is Field or Property
        /// </summary>
        /// <param name="objMemberInfo"></param>
        /// <param name="objSearch"></param>
        /// <returns></returns>
        private static bool DelegateToSearchCriteria(MemberInfo objMemberInfo, object objSearch)
        {

            if (objMemberInfo.MemberType == MemberTypes.Field)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Replaces 
        /// <		with		&lt;
        /// >		with		&gt;
        /// &		with		&amp;
        /// "		with		&quot;
        /// '		with		&apos;
        /// If input string is null it returns "null" string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ReplaceXMLSymbols(string str)
        {
            if (str == null)
            {
                return "null";
            }

            return str.
                Replace("<", "&lt;").
                Replace(">", "&gt").
                Replace("&", "&amp;").
                Replace("\"", "&quot;").
                Replace("'", "&apos;");
        }

        /// <summary>
        /// Inverse to ReplaceXMLSymbols
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string RevertXMLSymbols(string str)
        {
            if (str == null)
            {
                return "null";
            }

            return str.
                Replace("&lt;", "<").
                Replace("&gt", ">").
                Replace("&amp;", "&").
                Replace("&quot;", "\"").
                Replace("&apos;", "'");
        }

        /// <summary>
        /// Deserializes the given XML and returns the root as obj
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="compress"></param>
        /// <param name="workspaceManager"></param>
        /// <returns></returns>
        public static object Deserialize(string filename, bool compress = false)
        {
            FileStream sourceFile = File.OpenRead(filename);
            XmlDocument doc = new XmlDocument();
            GZipStream compStream = null;

            if (compress)
            {
                compStream = new GZipStream(sourceFile, CompressionMode.Decompress);
                doc.Load(compStream);
            }
            else
            {
                doc.Load(sourceFile);
            }

            try
            {
                return XMLSerialization.Deserialize(doc);
            }
            finally
            {
                if (compStream != null)
                {
                    compStream.Close();
                }
            }
        }
        public static object Deserialize(StreamWriter writer)
        {
            XmlDocument doc = new XmlDocument();
            writer.BaseStream.Position = 0;

            doc.Load(writer.BaseStream);

            try
            {
                return XMLSerialization.Deserialize(doc);
            }
            finally
            {
                //writer.Close();
            }
        }

        /// <summary>
        /// Deserializes the given XMLDocument and returns the root as obj
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="workspaceManager"></param>
        /// <returns></returns>
        public static object Deserialize(XmlDocument doc)
        {
            Dictionary<string, object> createdObjects = new Dictionary<string, object>();
            LinkedList<object[]> links = new LinkedList<object[]>();

            XmlElement objects = doc.DocumentElement;

            foreach (XmlNode objct in objects.ChildNodes)
            {
                XmlNode type = objct.ChildNodes[0];
                XmlNode id = objct.ChildNodes[1];
                XmlNode members = objct.ChildNodes[2];

                object newObject = null;
                try
                {
                    //hack: to allow "old" models being loaded (because some model elements were in model namespace before
                    //creating new model)
                    string name = type.InnerText.Replace("WorkspaceManager.View.Container", "WorkspaceManager.Model");
                    newObject = Type.GetType(name).GetConstructor(BindingFlags.NonPublic |
                                    BindingFlags.Instance | BindingFlags.Public,
                                    null, new Type[0], null).Invoke(null);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Could_not_create_instance_of___0_, type.InnerText), ex);
                }
                createdObjects.Add(id.InnerText, newObject);

                foreach (XmlNode member in members.ChildNodes)
                {
                    XmlNode membername = member.ChildNodes[0];
                    XmlNode membertype = member.ChildNodes[1];
                    XmlNode value = member.ChildNodes[2];

                    object newmember;

                    try
                    {
                        if (member.ChildNodes[2].Name.Equals("value"))
                        {
                            if (RevertXMLSymbols(membertype.InnerText).Equals("System.String"))
                            {
                                if (member.ChildNodes.Count > 3 && member.ChildNodes[3].Name.Equals("B64Encoded"))
                                {
                                    byte[] bytes = Convert.FromBase64String(value.InnerText);
                                    newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance).SetValue(newObject,
                                                                                                 enc.GetString(bytes));
                                }
                                else
                                {
                                    newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance).SetValue(newObject,
                                                                                                 value.InnerText);
                                }
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Contains("System.Int"))
                            {
                                int.TryParse(RevertXMLSymbols(value.InnerText), out int result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Double"))
                            {
                                double.TryParse(RevertXMLSymbols(value.InnerText.Replace(',', '.')),
                                                                        NumberStyles.Number,
                                                                        CultureInfo.CreateSpecificCulture("en-Us"),
                                                                        out double result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Single"))
                            {
                                float.TryParse(RevertXMLSymbols(value.InnerText.Replace(',', '.')),
                                                                        NumberStyles.Number,
                                                                        CultureInfo.CreateSpecificCulture("en-Us"),
                                                                        out float result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Char"))
                            {
                                char.TryParse(RevertXMLSymbols(value.InnerText), out char result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Boolean"))
                            {
                                bool.TryParse(RevertXMLSymbols(value.InnerText), out bool result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Windows.Point"))
                            {
                                string[] values = value.InnerText.Split(new char[] { ';' });

                                if (values.Length != 2)
                                {
                                    throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Can_not_create_a_Point_with__0__Coordinates_, values.Length));
                                }

                                double.TryParse(RevertXMLSymbols(values[0].Replace(',', '.')),
                                                                        NumberStyles.Number,
                                                                        CultureInfo.CreateSpecificCulture("en-Us"),
                                                                        out double x);
                                double.TryParse(RevertXMLSymbols(values[1].Replace(',', '.')),
                                                                        NumberStyles.Number,
                                                                        CultureInfo.CreateSpecificCulture("en-Us"),
                                                                        out double y);

                                System.Windows.Point result = new System.Windows.Point(x, y);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Windows.Media.Color"))
                            {
                                string[] values = value.InnerText.Split(new char[] { ';' });

                                if (values.Length != 4)
                                {
                                    throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Can_not_create_a_Color_with__0__Channels_, values.Length));
                                }
                                byte.TryParse(values[0], out byte r);
                                byte.TryParse(values[1], out byte g);
                                byte.TryParse(values[2], out byte b);
                                byte.TryParse(values[3], out byte a);

                                Color result = Color.FromArgb(a, r, g, b);

                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Byte[]"))
                            {
                                byte[] bytearray = Convert.FromBase64String(value.InnerText);

                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, bytearray);
                            }
                            else
                            {
                                //hack: to allow "old" models being loaded (because some model elements were in model namespace before
                                //creating new model)
                                string name = membertype.InnerText.Replace("WorkspaceManager.View.Container", "WorkspaceManager.Model");
                                newmember = Activator.CreateInstance(Type.GetType(RevertXMLSymbols(name)));

                                if (newmember is Enum)
                                {
                                    int.TryParse(RevertXMLSymbols(value.InnerText), out int result);
                                    //hack: to allow "old" models being loaded (because some model elements were in model namespace before
                                    //creating new model)
                                    name = membertype.InnerText.Replace("WorkspaceManager.View.Container", "WorkspaceManager.Model");

                                    object newEnumValue =
                                        Enum.ToObject(Type.GetType(RevertXMLSymbols(name)), result);

                                    newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance).SetValue(newObject, newEnumValue);
                                }
                                else
                                {
                                    newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance).SetValue(newObject, newmember);
                                }

                            }
                        }
                        else if (member.ChildNodes[2].Name.Equals("reference"))
                        {
                            XmlNode reference = member.ChildNodes[2];
                            links.AddLast(new object[]
                                              {
                                                  newObject,
                                                  RevertXMLSymbols(membername.InnerText),
                                                  RevertXMLSymbols(reference.InnerText),
                                                  false
                                              });
                        }
                        else if (member.ChildNodes[2].Name.Equals("list"))
                        {
                            string[] types = RevertXMLSymbols(membertype.InnerText).Split(';');

                            if (types.Length == 1)
                            {
                                newmember = System.Activator.CreateInstance(Type.GetType(types[0]));
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, newmember);
                            }
                            else if (types.Length == 2)
                            {
                                //we have 2 types, that means that we have a generic list with generic type types[1]
                                Type t = typeof(System.Collections.Generic.List<>);
                                Type[] typeArgs;
                                if (types[1].Equals("System.Windows.Point"))
                                {
                                    typeArgs = new Type[] { typeof(System.Windows.Point) };
                                }
                                else
                                {
                                    typeArgs = new Type[] { Type.GetType(types[1]) };
                                }
                                Type constructed = t.MakeGenericType(typeArgs);
                                newmember = Activator.CreateInstance(constructed);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, newmember);
                            }
                            else
                            {
                                throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Expected_1_or_2_types_for_list__But_found___0_, types.Length));
                            }

                            foreach (XmlNode entry in member.ChildNodes[2].ChildNodes)
                            {
                                if (entry.ChildNodes[1].Name.Equals("reference"))
                                {
                                    XmlNode reference = entry.ChildNodes[1];
                                    links.AddLast(new object[]
                                                      {
                                                          newObject,
                                                          RevertXMLSymbols(membername.InnerText),
                                                          RevertXMLSymbols(reference.InnerText),
                                                          true
                                                      });
                                }
                                else
                                {
                                    XmlNode typ = entry.ChildNodes[0];
                                    XmlNode val = entry.ChildNodes[1];

                                    if (RevertXMLSymbols(typ.InnerText).Equals("System.String"))
                                    {
                                        if (entry.ChildNodes.Count > 2 && member.ChildNodes[2].Name.Equals("B64Encoded"))
                                        {
                                            byte[] bytes = Convert.FromBase64String(val.InnerText);
                                            ((IList)newmember).Add(RevertXMLSymbols(enc.GetString(bytes)));
                                        }
                                        else
                                        {
                                            ((IList)newmember).Add(RevertXMLSymbols(val.InnerText));
                                        }
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Int16"))
                                    {
                                        short.TryParse(RevertXMLSymbols(val.InnerText), out short result);
                                        ((IList)newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Int32"))
                                    {
                                        int.TryParse(RevertXMLSymbols(val.InnerText), out int result);
                                        ((IList)newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Int64"))
                                    {
                                        long.TryParse(RevertXMLSymbols(val.InnerText), out long result);
                                        ((IList)newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Double"))
                                    {
                                        double.TryParse(RevertXMLSymbols(val.InnerText.Replace(',', '.')),
                                                                                NumberStyles.Number,
                                                                                CultureInfo.CreateSpecificCulture("en-GB"),
                                                                                out double result);
                                        newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Public |
                                                                     BindingFlags.Instance).SetValue(newObject, result);
                                        ((IList)newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Char"))
                                    {
                                        char.TryParse(RevertXMLSymbols(val.InnerText), out char result);
                                        ((IList)newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Windows.Point"))
                                    {
                                        string[] values = val.InnerText.Split(new char[] { ';' });

                                        if (values.Length != 2)
                                        {
                                            throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Can_not_create_a_Point_with__0__Coordinates_, values.Length));
                                        }

                                        double.TryParse(RevertXMLSymbols(values[0].Replace(',', '.')),
                                                                                NumberStyles.Number,
                                                                                CultureInfo.CreateSpecificCulture("en-GB"),
                                                                                out double x);
                                        double.TryParse(RevertXMLSymbols(values[1].Replace(',', '.')),
                                                                                NumberStyles.Number,
                                                                                CultureInfo.CreateSpecificCulture("en-GB"),
                                                                                out double y);

                                        System.Windows.Point point = new System.Windows.Point(x, y);
                                        ((IList)newmember).Add(point);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Could_not_deserialize_model_element___0___of_type___1__, membername.InnerText, membertype.InnerText), ex);
                    }
                }
            }

            foreach (object[] triple in links)
            {

                object obj = triple[0];
                string membername = (string)triple[1];
                string reference = (string)triple[2];
                bool isList = (bool)triple[3];
                createdObjects.TryGetValue(reference, out object obj2);

                try
                {
                    if (isList)
                    {
                        try
                        {
                            ((IList)obj.GetType().GetField(membername, BindingFlags.NonPublic |
                                                                       BindingFlags.Public |
                                                                       BindingFlags.Instance).GetValue(obj)).Add(obj2);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Can_not_find_list_field___0___of___1__, membername, obj.GetType().FullName), ex);
                        }
                    }
                    else
                    {
                        if (obj != null && obj2 != null)
                        {

                            FieldInfo fieldInfo = null;
                            try
                            {
                                fieldInfo = obj.GetType().GetField(membername,
                                                                             BindingFlags.NonPublic |
                                                                             BindingFlags.Public |
                                                                             BindingFlags.Instance);
                                fieldInfo.SetValue(obj, obj2);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Can_not_find_field___0___of___1__, membername, obj.GetType().FullName), ex);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Resources.XMLSerialization_Deserialize_Could_not_restore_reference_beteen_model_element___0___and_its_reference_with_id___1__, membername, reference), ex);
                }
            }

            return createdObjects.Values.First();
        }
    }
}
