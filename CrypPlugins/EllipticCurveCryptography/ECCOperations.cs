/*                              
   Copyright 2025 Nils Kopal, CrypTool Project

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
using CrypTool.PluginBase;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.EllipticCurveCryptography
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.EllipticCurveCryptography.Properties.Resources", "EccOperationsCaption", "EccOperationsTooltip", "EllipticCurveCryptography/userdoc.xml", new[] { "EllipticCurveCryptography/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class ECCOperations : ICrypComponent
    {
        #region Fields

        private Point _point1;
        private Point _point2;
        private BigInteger? _scalar;
        private Point _result;

        #endregion

        #region Inputs

        [PropertyInfo(Direction.InputData, "Point1Caption", "Point1Tooltip", false)]
        public Point Point1
        {
            get => _point1;
            set
            {
                _point1 = value;
                OnPropertyChanged(nameof(Point1));
            }
        }

        [PropertyInfo(Direction.InputData, "Point2Caption", "Point2Tooltip", false)]
        public Point Point2
        {
            get => _point2;
            set
            {
                _point2 = value;
                OnPropertyChanged(nameof(Point2));
            }
        }

        [PropertyInfo(Direction.InputData, "ScalarCaption", "ScalarTooltip", false)]
        public BigInteger Scalar
        {
            get => _scalar ?? 0;
            set
            {
                _scalar = value;
                OnPropertyChanged(nameof(Scalar));
            }
        }

        #endregion

        #region Output

        [PropertyInfo(Direction.OutputData, "ResultPointCaption", "ResultPointTooltip")]
        public Point Result => _result;

        #endregion

        #region IPlugin Implementation

        public void Execute()
        {
            if (_point1 == null)
            {
                throw new Exception("Point1 not given");
            }

            if (_point2 != null)
            {
                //check, if both points share the same curve
                if (!Point1.Curve.Equals(Point2.Curve))
                {
                    throw new Exception("Points do not share the same curve!");
                }

                _result = Point1.Curve.Add(_point1, _point2);
            }
            else if (_scalar.HasValue)
            {
                _result = Point1.Curve.Multiply(_scalar.Value, _point1);
            }
            else
            {
                return;
            }

            OnPropertyChanged(nameof(Result));
        }

        public void Initialize() { }
        public void Dispose() { }
        public void Stop() { }
        public void Pause() { }
        public void PreExecution()
        {
            _point1 = null;
            _point2 = null;
            _scalar = null;
        }
        public void PostExecution() { }

        public ISettings Settings => null;
        public UserControl Presentation => null;

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}