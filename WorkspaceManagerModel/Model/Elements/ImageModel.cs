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
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// This class wraps a image which can be put onto the workspace
    /// </summary>
    [Serializable]
    public class ImageModel : VisualElementModel
    {

        internal ImageModel()
        {

        }

        private readonly byte[] data = null;

        /// <summary>
        /// Get the Image stored by this ImageModel
        /// </summary>
        /// <returns></returns>
        public Image getImage()
        {
            Image image = new Image();
            if (data == null)
            {
                return image;
            }

            MemoryStream stream = new MemoryStream(data);
            JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
            BitmapFrame frame = decoder.Frames.First();
            image.Source = frame;
            return image;
        }

        /// <summary>
        /// Instantiate a new ImageModel
        /// Loads the image from the imgUri and converts it into a jpeg
        /// Afterwards the data are stored in an internal byte array
        /// </summary>
        /// <param name="imageSource"></param>
        public ImageModel(Uri imgUri)
        {
            if (imgUri == null)
            {
                return;
            }
            Width = 0;
            Height = 0;
            BitmapImage bmpImage = new BitmapImage(imgUri);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmpImage));
            MemoryStream stream = new MemoryStream();
            encoder.Save(stream);
            data = stream.ToArray();
            stream.Close();
        }

        /// <summary>
        /// The WorkspaceModel of this ImageModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; set; }

        internal bool HasData()
        {
            return data != null && data.Length != 0;
        }
    }
}
