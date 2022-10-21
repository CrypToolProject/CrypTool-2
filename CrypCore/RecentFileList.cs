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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrypTool.Core
{
    public class RecentFileList
    {
        private static RecentFileList _recentFileList = null;
        private readonly List<string> recentFiles = new List<string>();
        private readonly string RegistryKey = "Software\\CrypTool2.0";
        private readonly string valueKey = "recentFileList";

        public int ListLength { get; private set; }

        public delegate void ListChangedEventHandler(List<string> recentFiles);
        public event ListChangedEventHandler ListChanged;

        public static RecentFileList GetSingleton()
        {
            try
            {
                if (_recentFileList == null)
                {
                    _recentFileList = new RecentFileList(Properties.Settings.Default.RecentFileListSize);
                }
            }
            catch (Exception)
            {
                return new RecentFileList();
            }
            return _recentFileList;
        }

        private RecentFileList() : this(10)
        {
        }

        public void ChangeListLength(int listLength)
        {
            Properties.Settings.Default.RecentFileListSize = listLength;
            try
            {
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                //if saving failed try one more time
                try
                {
                    Properties.Settings.Default.Save();
                }
                catch (Exception)
                {
                    //if saving failed again we do not try it again
                }
            }
            ListLength = listLength;
            if (ListLength < recentFiles.Count)
            {
                recentFiles.RemoveRange(ListLength, recentFiles.Count - ListLength);
            }
            Store();
            ListChanged(recentFiles);
        }

        private RecentFileList(int listLength)
        {
            ListLength = listLength;
            Load();
        }

        public void AddRecentFile(string recentFile)
        {
            if (Path.GetFileName(recentFile).StartsWith("."))
            {
                return;
            }

            recentFiles.Remove(recentFile);
            recentFiles.Add(recentFile);
            if (recentFiles.Count > ListLength)
            {
                recentFiles.RemoveAt(0);
            }

            Store();
            ListChanged(recentFiles);
        }

        public void RemoveFile(string fileName)
        {
            recentFiles.Remove(fileName);
            Store();
            ListChanged(recentFiles);
        }

        public List<string> GetRecentFiles()
        {
            return recentFiles;
        }

        public void Clear()
        {
            recentFiles.Clear();
            Store();
            ListChanged(recentFiles);
        }

        private void Store()
        {
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (k == null)
            {
                k = Registry.CurrentUser.CreateSubKey(RegistryKey);
            }

            k = Registry.CurrentUser.OpenSubKey(RegistryKey, true);

            k.SetValue(valueKey, recentFiles.ToArray());
        }

        private void Load()
        {
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (k == null)
            {
                k = Registry.CurrentUser.CreateSubKey(RegistryKey);
            }

            if (k.GetValue(valueKey) != null && k.GetValueKind(valueKey) == RegistryValueKind.MultiString)
            {
                string[] list = (string[])(k.GetValue(valueKey));
                for (int i = list.Length - ListLength; i < list.Length; i++)
                {
                    if ((i >= 0) && File.Exists(list[i]))
                    {
                        recentFiles.Add(list[i]);
                    }
                }
            }
        }

        public int Count
        {
            get => recentFiles.Count;
        }        
    }
}
