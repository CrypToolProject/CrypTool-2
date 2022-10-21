/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypTool.PluginBase.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class DataManager
    {
        private const string DataDirecory = "Data";

        private const string MetaSuffix = ".metainfo";

        private readonly string globalDataStore;

        public DataManager()
        {
            globalDataStore = Path.Combine(DirectoryHelper.DirectoryCrypPlugins, DataDirecory);
        }

        /// <summary>
        /// Maps filename to metainfo.
        /// </summary>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public IDictionary<string, DataFileMetaInfo> LoadDirectory(string datatype)
        {
            Dictionary<string, DataFileMetaInfo> filesDict = new Dictionary<string, DataFileMetaInfo>();

            // TODO: enable custom data store
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(globalDataStore, datatype));
            if (dir.Exists)
            {
                // TODO: read subdirectories

                LoadFiles(dir, filesDict);
            }

            return filesDict;
        }

        private void LoadFiles(DirectoryInfo dir, Dictionary<string, DataFileMetaInfo> filesDict)
        {
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Name.EndsWith(MetaSuffix))
                {
                    continue;
                }

                if (filesDict.ContainsKey(file.Name.ToLower()))
                {
                    // TODO: throw warning about duplicate file
                }
                else
                {
                    filesDict.Add(file.Name.ToLower(), GetFileMeta(file));
                }
            }
        }

        private DataFileMetaInfo GetFileMeta(FileInfo dataFile)
        {
            DataFileMetaInfo metaInfo = new DataFileMetaInfo
            {
                DataFile = dataFile
            };

            FileInfo metaFile = new FileInfo(string.Concat(dataFile.FullName, MetaSuffix));
            if (metaFile.Exists)
            {
                KeyValueReader keyValues = new KeyValueReader(metaFile);

                metaInfo.MetaFile = metaFile;
                metaInfo.KeyValues = keyValues;

                // TODO: should ignore case
                if (keyValues.ContainsKey("textencoding"))
                {
                    try
                    {
                        metaInfo.TextEncoding = Encoding.GetEncoding(keyValues["textencoding"]);
                    }
                    catch (ArgumentException)
                    {
                        // Encoding error, defaulting to null
                        metaInfo.TextEncoding = null;
                        // TODO: throw warning about unknown encoding
                    }
                }
            }

            return metaInfo;
        }
    }

    public class DataFileMetaInfo
    {
        public FileInfo DataFile
        {
            get;
            internal set;
        }

        public FileInfo MetaFile
        {
            get;
            internal set;
        }

        public Encoding TextEncoding
        {
            get;
            internal set;
        }

        public string Name
        {
            get
            {
                if (KeyValues.ContainsKey("name"))
                {
                    return KeyValues["name"];
                }
                else
                {
                    return DataFile.Name;
                }
            }
        }

        public IDictionary<string, string> KeyValues
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
