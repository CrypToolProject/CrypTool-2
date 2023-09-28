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
namespace CrypTool.PluginBase.IO
{
    /// <summary>
    /// ICrypToolStream reference to membuffer/swapfile.
    /// ICrypToolStream instances can be passed safely to other plugins or components.
    /// To create a new ICrypToolStream, please use CStreamWriter.
    /// 
    /// <para>You MAY create 1 to n readers upon one ICrypToolStream. They do not interfere.
    /// You MAY pass the ICrypToolStream object safely to other components. They can create new
    /// readers, but not modify the stream.</para>
    /// 
    /// <para>For more usage notes, see CStreamWriter and CStreamReader.</para>
    /// </summary>
    public interface ICrypToolStream
    {
        CStreamReader CreateReader();

        long Length
        {
            get;
        }
    }
}
