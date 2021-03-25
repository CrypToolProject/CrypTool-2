using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Resources;
using System.Collections;
using System.Reflection;
using CrypTool.PluginBase;
using System.IO;

namespace CrypTool.Resource
{
    public class ResourceManager
    {
        public static DataTable LoadResourceFile(string fileName)
        {
            ResXResourceReader reader = new ResXResourceReader(fileName);
            DataTable dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Original");
            dt.Columns.Add("Translation");
            dt.Columns["Key"].ReadOnly = true;
            dt.Columns["Original"].ReadOnly = true;

            foreach (DictionaryEntry entry in reader)
            {
                if (entry.Value is ResourceEntry)
                {
                    DataRow dr = dt.NewRow();
                    dr["Key"] = entry.Key;
                    dr["Original"] = ((ResourceEntry)entry.Value).Original;
                    dr["Translation"] = ((ResourceEntry)entry.Value).Translation;
                    dt.Rows.Add(dr);
                }
            }
            dt.AcceptChanges();
            reader.Close();
            return dt; 
        }

        public static void CreateResourceFile(string assemblyFileName, string resourceFileName)
        {
            Assembly asm = Assembly.LoadFile(assemblyFileName);
            Type[] types = asm.GetTypes();

            IResourceWriter writer = new ResXResourceWriter(resourceFileName);

            foreach (Type t in types)
            {
                if (t.GetInterface(typeof(IPlugin).Name) != null)
                {
                    PluginInfoAttribute[] attr = (PluginInfoAttribute[])t.GetCustomAttributes(typeof(PluginInfoAttribute), false);
                    if (attr.Length == 1)
                    {
                        writer.AddResource(string.Format("{0}.{1}.Name", t.Namespace, t.Name), new ResourceEntry(attr[0].Caption, attr[0].Caption));
                    }
                }
            }
            writer.Generate();
            writer.Close();
        }

        public static void SaveResourceFile(string resourceFileName, DataTable dt)
        {
            dt.AcceptChanges();
            IResourceWriter writer = new ResXResourceWriter(resourceFileName);
            foreach (DataRow dr in dt.Rows)
            {
                writer.AddResource(dr["Key"].ToString(), new ResourceEntry(dr["Translation"].ToString(), dr["Original"].ToString()));
            }
            writer.Generate();
            writer.Close();
        }

        public static void UpdateResourceFile(string assemblyFileName, string resourceFileName)
        {
            DataTable resourceTable = LoadResourceFile(resourceFileName);
            CreateResourceFile(assemblyFileName, resourceFileName);
            SaveResourceFile(resourceFileName, resourceTable);
        }
    }
}
