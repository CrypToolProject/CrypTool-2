/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CrypTool.Plugins.DimCodeEncoder;

namespace DimCodeEncoder.DimCodes
{
    interface IDimCode
    {
        /// <summary>
        /// the dim-code's specific encode methode.
        /// </summary>
        /// <returns> creates based on the given settings an byte array of the image and returns it</returns>
        byte[] Encode(byte[] input, DimCodeEncoderSettings settings);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        UserControl GetPresentation();
    }
}
