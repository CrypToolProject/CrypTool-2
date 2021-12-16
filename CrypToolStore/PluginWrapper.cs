/*
   Copyright 2019 Nils Kopal <kopal<AT>CrypTool.org>

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
using CrypToolStoreLib.DataObjects;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CrypTool.CrypToolStore
{

    /// <summary>
    /// This class wraps a PluginAndData object, offering comfort functions, thus, it can be easier shown in the UI
    /// </summary>
    public class PluginWrapper
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public int PluginId { get; set; }
        public int PluginVersion { get; set; }
        public string LongDescription { get; set; }
        public string Authornames { get; set; }
        public string Authoremails { get; set; }
        public string Authorinstitutes { get; set; }
        public int BuildVersion { get; set; }
        public DateTime BuildDate { get; set; }
        public bool IsInstalled { get; set; }
        public bool UpdateAvailable { get; set; }
        public string FileSize { get; set; }
        private byte[] IconData { get; set; }
        public Brush BackgroundColor { get; set; }

        public PluginWrapper()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pluginAndSource"></param>
        public PluginWrapper(PluginAndSource pluginAndSource)
        {
            Plugin plugin = pluginAndSource.Plugin;
            Source source = pluginAndSource.Source;

            PluginId = plugin.Id;
            PluginVersion = source.PluginVersion;
            Name = plugin.Name;
            ShortDescription = plugin.ShortDescription;
            LongDescription = plugin.LongDescription;
            Authornames = plugin.Authornames;
            Authoremails = plugin.Authoremails;
            Authorinstitutes = plugin.Authorinstitutes;
            BuildVersion = pluginAndSource.Source.BuildVersion;
            BuildDate = pluginAndSource.Source.BuildDate;
            IconData = plugin.Icon;
            FileSize = CrypToolStoreLib.Tools.Tools.FormatFileSizeString(pluginAndSource.FileSize);

            Color color = Color.FromArgb(0xFF, 0xC8, 0xDC, 0xF5);
            BackgroundColor = new SolidColorBrush(color);
        }

        /// <summary>
        /// Get icon of the plugin as BitmapFrame for displaying it in the UI
        /// </summary>
        /// <returns></returns>
        public BitmapFrame Icon
        {
            get
            {
                byte[] data;
                if (this is ResourceWrapper)
                {
                    //this is a resource, thus, we use the icon_resource
                    MemoryStream stream = new MemoryStream();
                    Properties.Resources.icon_resource.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    data = stream.ToArray();
                    stream.Close();
                }
                else if (IconData == null || IconData.Length == 0)
                {
                    //we have no icon, thus, we display the default icon
                    MemoryStream stream = new MemoryStream();
                    Properties.Resources._default.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    data = stream.ToArray();
                    stream.Close();
                }
                else
                {
                    data = IconData;
                }
                BitmapDecoder decoder = BitmapDecoder.Create(new MemoryStream(data),
                                        BitmapCreateOptions.PreservePixelFormat,
                                        BitmapCacheOption.None);
                if (decoder.Frames.Count > 0)
                {
                    return decoder.Frames[0];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get installation icon of the plugin as BitmapFrame for displaying it in the UI
        /// </summary>
        /// <returns></returns>
        public BitmapFrame InstallationIcon
        {
            get
            {
                byte[] data;
                MemoryStream stream = new MemoryStream();
                if (IsInstalled)
                {
                    if (UpdateAvailable)
                    {
                        Properties.Resources.updateavailable.Save(stream, ImageFormat.Png);
                    }
                    else
                    {
                        Properties.Resources.downloaded.Save(stream, ImageFormat.Png);
                    }
                }
                else
                {
                    Properties.Resources.download.Save(stream, ImageFormat.Png);
                }
                stream.Position = 0;
                data = stream.ToArray();
                stream.Close();
                BitmapDecoder decoder = BitmapDecoder.Create(new MemoryStream(data),
                                        BitmapCreateOptions.PreservePixelFormat,
                                        BitmapCacheOption.None);
                if (decoder.Frames.Count > 0)
                {
                    return decoder.Frames[0];
                }
                else
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// This class wraps a ResourceAndResourceData object, offering comfort functions, thus, it can be easier shown in the UI
    /// Actually, we "misuse" the PluginWrapper by using it as a ResourceWrapper, thus, some of the namings are not 100% correct
    /// e.g. PluginId is the ResourceId
    /// </summary>
    public class ResourceWrapper : PluginWrapper
    {
        public ResourceWrapper(ResourceAndResourceData resourceAndResource)
        {
            Resource resource = resourceAndResource.Resource;
            ResourceData resourceData = resourceAndResource.ResourceData;
            PluginId = resource.Id;
            PluginVersion = resourceData.ResourceVersion;
            Name = resource.Name;
            ShortDescription = resource.Description;
            LongDescription = string.Empty;
            FileSize = CrypToolStoreLib.Tools.Tools.FormatFileSizeString(resourceAndResource.FileSize);
            Color color = Color.FromArgb(0xFF, 0xEB, 0xEF, 0xF6);
            BackgroundColor = new SolidColorBrush(color);
        }
    }

}
