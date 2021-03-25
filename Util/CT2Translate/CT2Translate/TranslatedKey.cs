using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Xml;

namespace WindowsFormsApplication1
{
    class TranslatedKey
    {
        Dictionary<string, string> translations = new Dictionary<string, string>();

        public Dictionary<string, string> Translations
        {
            get
            {
                return translations;
            }
        }

        public void Add(string lang, string text)
        {
            translations[lang] = text;
        }
    }

    class TranslatedResource
    {
        // maps a resource key to a dictionary with texts in different languages for this key
        Dictionary<string, TranslatedKey> translatedKey = new Dictionary<string, TranslatedKey>();

        public Dictionary<string, string> files = new Dictionary<string, string>();
        public Dictionary<string, bool> modified = new Dictionary<string, bool>();
        public string relpath; // filename ohne basepath, language und .resx
        public LogDelegate log;

        public TranslatedResource(string basepath, string relpath, LogDelegate del = null)
        {
            this.relpath = relpath;
            log = del;
        }

        private void Log(string msg)
        {
            log?.Invoke(msg);
        }

        public Dictionary<string, TranslatedKey> TranslatedKey
        {
            get
            {
                return translatedKey;
            }
        }

        public bool Modified
        {
            get
            {
                return modified.ContainsValue(true);
            }
        }

        public void ClearModified()
        {
            modified.Clear();
        }

        public void SaveAs(string basepath, string lang, string modification)
        {
            Save(basepath + "." + modification + "." + AllResources.langext[lang] + ".resx", lang);
        }

        public void Save(string basepath)
        {
            foreach (string lang in files.Keys)
                Save(basepath, files[lang], lang);
        }

        public void Save(string basepath, string lang)
        {
            if (files.ContainsKey(lang))
                Save(basepath, files[lang], lang);
        }

        public void Save(string basepath, string filename, string lang)
        {
            IResourceWriter writer = new ResXResourceWriter(basepath + filename);

            foreach (string key in translatedKey.Keys)
            {
                if (translatedKey[key].Translations.ContainsKey(lang))
                {
                    string value = translatedKey[key].Translations[lang];
                    writer.AddResource(key, value);
                }
            }

            writer.Generate();
            writer.Close();
        }

        public bool Update(string basepath, string lang)
        {
            List<ResXDataNode> nodelist = new List<ResXDataNode>();
            string filename = basepath + files[lang];

            // read nodes
            ResXResourceReader reader = new ResXResourceReader(filename);
            
            reader.BasePath = Path.GetDirectoryName(filename);
            reader.UseResXDataNodes = true;

            bool modified = false;
            var done = new HashSet<string>();

            // aktualisiere alle bereits in der .resx-Datei vorhandenen Keys und übernehme Nicht-Text-Einträge (z.B. FileRefs)
            foreach (DictionaryEntry entry in reader)
            {
                ResXDataNode dataNode = (ResXDataNode)entry.Value;
                string key, value;
                try
                {
                    if (dataNode.FileRef == null)
                    {
                        key = entry.Key.ToString();
                        var v = dataNode.GetValue((System.ComponentModel.Design.ITypeResolutionService)null);
                        value = (v == null) ? "" : v.ToString();

                        if (translatedKey.ContainsKey(key))
                            if (translatedKey[key].Translations.ContainsKey(lang))
                            {
                                done.Add(key);  // key wurde beim durchlaufen der vorhandenen Einträge behandelt
                                if (translatedKey[key].Translations[lang] != value)
                                {
                                    dataNode = new ResXDataNode(key, translatedKey[key].Translations[lang]);
                                    modified = true;
                                }
                            }
                    }
                    nodelist.Add(dataNode);
                }
                catch (Exception ex)
                {
                }
            }

            // füge die neu hinzugekommenen Keys dazu
            foreach(var key in translatedKey.Keys)
            {
                if(!done.Contains(key))
                    if (translatedKey[key].Translations.ContainsKey(lang))
                    {
                        var dataNode = new ResXDataNode(key, translatedKey[key].Translations[lang]);
                        nodelist.Add(dataNode);
                        modified = true;
                    }
            }

            reader.Close();

            // write nodes if resource was modified
            if (modified)
            {
                ResXResourceWriter writer = new ResXResourceWriter(filename);
                writer.BasePath = Path.GetDirectoryName(filename);
                nodelist.ForEach(n => writer.AddResource(n));
                writer.Generate();
                writer.Close();

                Log(filename + " updated");
            }

            this.modified.Remove(lang);

            return modified;
        }

        void CreateEmptyResourceFile(string basepath, string lang)
        {
            string resource = relpath + "." + lang + ".resx";
            files.Add(lang, resource);
            string fname = basepath + resource;
            File.WriteAllText(fname, Properties.Resources.Resource_template);
            Log("Missing resource file " + fname + " created");
        }

        public int Update(string basepath)
        {
            int cnt = 0;

            // Check if a resource file for a new language has to be generated.
            // This is necessary if a key is added to a language that previously contained no keys.
            var langs = translatedKey.Values.SelectMany(tk => tk.Translations.Keys).Distinct();
            var missingLangs = langs.Where(lang => !files.ContainsKey(lang));
            foreach (var lang in missingLangs)
                CreateEmptyResourceFile(basepath, lang);

            foreach (string lang in files.Keys)
                if (Update(basepath, lang)) cnt++;

            return cnt;
        }

        public void Load(string lang, string filename)
        {
            ResXResourceReader reader = new ResXResourceReader(filename);
            reader.BasePath = Path.GetDirectoryName(filename);
            reader.UseResXDataNodes = true;

            try
            {
                foreach (DictionaryEntry entry in reader)
                {
                    string key = entry.Key.ToString();
                    string value = entry.Value.ToString();
                    if (!translatedKey.ContainsKey(key))
                        translatedKey.Add(key, new TranslatedKey());
                    translatedKey[key].Add(lang, value);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            reader.Close();

            files[lang] = filename;
        }

        /* lädt die resx-Dateien ohne die referenzierten externen Keys */
        public void LoadXML(string basepath, string lang, string filename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(basepath + filename);

            try
            {
                XmlNodeList nodelist = xml.DocumentElement.SelectNodes("data[not(@type or @mimetype)]");
                foreach (XmlNode n in nodelist)
                {
                    //string key = n.SelectSingleNode("@name").Value;
                    //string value = n.SelectSingleNode(".//value").InnerText;
                    string key = n.Attributes["name"].Value;
                    string value = n.ChildNodes[1].InnerText;

                    if (!translatedKey.ContainsKey(key))
                        translatedKey.Add(key, new TranslatedKey());
                    translatedKey[key].Add(lang, value);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            
            files[lang] = filename;
        }
    }

    class AllResources
    {
        public static string[] cultures = new string[] { "en", "de", "ru" };
        public static string defaultculture = "en";
        public static Dictionary<string, string> langext = new Dictionary<string, string> { { "en", "en" }, { "de", "de" }, { "ru", "ru" } };
        public static HashSet<string> ignorePaths = new HashSet<string> { "obj" };

        public Dictionary<string, TranslatedResource> Resources = new Dictionary<string, TranslatedResource>();
        public string basepath;
        public LogDelegate log;

        private void Log(string msg)
        {
            log?.Invoke(msg);
        }

        public AllResources(LogDelegate del=null)
        {
            log = del;
        }

        public bool Modified
        {
            get
            {
                return Resources.Values.Any(tr => tr.Modified);
            }
        }

        public string getKey(string fname)
        {
            if (fname.IndexOf(basepath) == 0)
                fname = fname.Remove(0, basepath.Length);

            if (Path.GetExtension(fname) != ".resx") return null;
            string basename = Path.Combine(Path.GetDirectoryName(fname), Path.GetFileNameWithoutExtension(fname));

            foreach (string culture in cultures)
            {
                Match match = Regex.Match(basename, $"^(.*)\\.{langext[culture]}$");
                if (match.Success) return match.Groups[1].Value;
            }

            return basename;
        }

        public static string getCulture(string fname)
        {
            foreach (string culture in cultures)
                if (Regex.Match(fname, "^(.*)." + langext[culture] + ".resx$").Success)
                    return culture;

            return AllResources.defaultculture;
        }

        public Dictionary<string, string> getExistingFileNames(string relpath)
        {
            Dictionary<string, string> result = new Dictionary<string, string> { };

            string f;
            foreach (string culture in cultures)
            {
                f = relpath + "." + langext[culture] + ".resx";
                if (File.Exists(basepath + f)) result[culture] = f;
            }

            f = relpath + ".resx";
            if (File.Exists(basepath + f)) result[defaultculture] = f;

            return result;
        }

        public TranslatedResource getResources(string basepath, string relpath)
        {
            Dictionary<string, string> fnames = getExistingFileNames(relpath);

            TranslatedResource result = new TranslatedResource(basepath, relpath, log);

            foreach (string lang in fnames.Keys)
                result.LoadXML(basepath, lang, fnames[lang]);

            return result;
        }

        public void Add(string basepath, string fname)
        {
            string relpath = getKey(fname);
            if (relpath != null && !Resources.ContainsKey(relpath))
                Resources[relpath] = getResources(basepath, relpath);
        }

        public void Add(string basepath, string[] fname)
        {
            foreach (string f in fname)
                Add(basepath, f);
        }

        public void Clear()
        {
            Resources.Clear();
        }

        public void Update(string basepath)
        {
            try
            {
                int cnt = Resources.Values.Sum(tr => tr.Update(basepath));
                Log(cnt + " file(s) updated.");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while updating:\n" + e.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SaveText(string filename, string lang)
        {
            using (StreamWriter w = new StreamWriter(filename))
            {
                foreach (string res in Resources.Keys)
                    foreach (string key in Resources[res].TranslatedKey.Keys)
                        if (Resources[res].TranslatedKey[key].Translations.ContainsKey(lang))
                            w.WriteLine(Resources[res].TranslatedKey[key].Translations[lang]);
            }
        }

        public void SaveXML(string filename)
        {
            XmlTextWriter w = new XmlTextWriter(filename, null);
            w.Formatting = Formatting.Indented;

            w.WriteStartElement("root");

            w.WriteStartElement("basepath");
            w.WriteAttributeString("name", basepath);
            w.WriteEndElement();

            foreach (string res in Resources.Keys)
            {
                w.WriteStartElement("resource");
                w.WriteAttributeString("base", res);

                w.WriteStartElement("files");
                foreach (string lang in Resources[res].files.Keys)
                {
                    w.WriteStartElement("file");
                    w.WriteAttributeString("lang", lang);
                    w.WriteString(Resources[res].files[lang]);
                    w.WriteEndElement();
                }
                w.WriteEndElement();

                foreach (string key in Resources[res].TranslatedKey.Keys)
                {
                    w.WriteStartElement("key");
                    w.WriteAttributeString("value", key);
                    TranslatedKey t = Resources[res].TranslatedKey[key];
                    foreach (string lang in t.Translations.Keys)
                    {
                        w.WriteStartElement("value");
                        w.WriteAttributeString("lang", lang);
                        w.WriteString(t.Translations[lang]);
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }

            // Clear all modified flags if resources were saved as merged XML
            foreach(var res in Resources.Keys)
                Resources[res].ClearModified();

            w.Close();
        }

        public void LoadXML(string filename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(filename);

            XmlNode n = xml.SelectSingleNode("/root/basepath");
            this.basepath = n.Attributes["name"].Value;
            basepath = new Regex("[/\\\\]+$").Replace(basepath, "");

            XmlNodeList nl = xml.SelectNodes("/root/resource");
            foreach (XmlNode r in nl)
            {
                string relpath = r.Attributes["base"].Value;
                if (relpath.IndexOf(basepath) == 0) relpath = relpath.Remove(0, basepath.Length);

                TranslatedResource tr = new TranslatedResource(basepath, relpath, log);

                XmlNode f = r.SelectSingleNode("files");
                foreach (XmlNode l in f.SelectNodes("file"))
                {
                    string lang = l.Attributes["lang"].Value;
                    tr.files[lang] = l.FirstChild.InnerText;
                    if (tr.files[lang].IndexOf(basepath) == 0) tr.files[lang] = tr.files[lang].Remove(0, basepath.Length);
                }

                foreach (XmlNode k in r.SelectNodes("key"))
                {
                    string key = k.Attributes["value"].Value;
                    TranslatedKey tk = new TranslatedKey();
                    foreach (XmlNode t in k.ChildNodes)
                    {
                        string lang = t.Attributes["lang"].Value;
                        tk.Translations[lang] = t.InnerText;
                    }
                    tr.TranslatedKey[key] = tk;
                }

                Resources[relpath] = tr;
            }
        }

        public string[] MatchFiles(string pathprefix)
        {
            if (Path.GetExtension(pathprefix) == ".resx")
                pathprefix = Path.GetDirectoryName(pathprefix);
            return Resources.Keys.Where(path => path.IndexOf(pathprefix) == 0).ToArray();
        }

        public TreeNode GetTree()
        {
            TreeNode root = new TreeNode(this.basepath);
            root.Name = "";

            foreach (string path in this.Resources.Keys)
            {
                foreach (string file in this.Resources[path].files.Values)
                {
                    string[] dirs = file.Split(new char[] { '/', '\\' });
                    TreeNode t = root;
                    foreach (string dir in dirs)
                    {
                        if (dir.Length == 0) continue;
                        string name = t.Name + "\\" + dir;
                        t = t.Nodes.ContainsKey(name) ? t.Nodes[name] : t.Nodes.Add(name, dir);
                    }
                    t.ToolTipText = this.basepath + file;
                }
            }

            //root.Tag = "\\";    // must be here for tag creation to work properly
            return root;
        }

        public void ScanDir(string basepath)
        {
            this.basepath = basepath;
            Clear();
            DirSearch(basepath);
        }

        int DirSearch(string sDir)
        {
            int cnt = 0;

            try
            {
                foreach (string f in Directory.GetFiles(sDir, "*.resx"))
                {
                    Add(basepath, f);
                    cnt++;
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    string dir = new DirectoryInfo(d).Name;
                    if (ignorePaths.Contains(dir)) continue;
                    int found = DirSearch(d);
                    if (found > 0) cnt += found;
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return cnt;
        }

    }
}