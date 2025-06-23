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
using CrypTool.Plugins.EllipticCurveCryptography.Views;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.EllipticCurveCryptography
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.EllipticCurveCryptography.Properties.Resources", "EccInputCaption", "EccInputTooltip", "EllipticCurveCryptography/userdoc.xml", new[] { "EllipticCurveCryptography/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class ECCInput : ICrypComponent
    {
        #region Private Fields

        private ECCInputSettings _settings = new ECCInputSettings();
        private EccCurveVisualizer _visualizer = new EccCurveVisualizer();
        private Point _point;

        #endregion

        #region IPlugin Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => _settings;

        [PropertyInfo(Direction.OutputData, "PointCaption", "PointTooltip")]
        public Point Point => _point;

        public UserControl Presentation => _visualizer;

        public void Execute()
        {
            switch (_settings.Mode)
            {
                default:
                case ModeEnum.ReturnGenerator:
                    ReturnGenerator();
                    break;
                case ModeEnum.ReturnCustomPoint:
                    ReturnCustomPoint();
                    break;
            }
            _visualizer.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                int max = 1024;
                if(_point.Curve.P < 1024)
                {
                    max = (int)_point.Curve.P;
                }
                _visualizer.MaxRange = max;
                _visualizer.UpdateCurve(_point.Curve);                
            }, null);
           
        }

        /// <summary>
        /// Returns the point defined by the user
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void ReturnCustomPoint()
        {
            BigInteger a = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.A);
            BigInteger b = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.B);
            BigInteger mod = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.Mod);

            if (_settings.SelectedCurve != 0)
            {
                a = PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].A;
                b = PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].B;
                mod = PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].P;
            }

            _point = new Point
            {
                X = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.X),
                Y = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.Y),
                Curve = new WeierstraßCurve(a, b, mod),
                IsInfinity = false
            };

            if (!_point.Curve.IsPointOnCurve(_point))
            {
                throw new Exception(string.Format("Point ({0},{1}) is not on the curve", _settings.X, _settings.Y));
            }

            OnPropertyChanged(nameof(Point));
        }

        /// <summary>
        /// Returns a generator point
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void ReturnGenerator()
        {
            EllipticCurve curve;

            if (_settings.SelectedCurve != 0)
            {
                switch (PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].CurveType)
                {
                    default:
                    case CurveType.Weierstraß:
                        curve = new WeierstraßCurve(
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].A,
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].B,
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].P);
                        break;

                    case CurveType.Montgomery:
                        curve = new MontgomeryCurve(
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].A,
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].B,
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].P);
                        break;

                    case CurveType.TwistedEdwards:
                        curve = new TwistedEdwardsCurve(
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].A,
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].D,
                            PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].P);
                        break;
                }

                _point = new Point
                {
                    X = PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].Gx,
                    Y = PredefinedEllipticCurves.Curves[_settings.PredefinedCurvesNames[_settings.SelectedCurve]].Gy,
                    Curve = curve,
                    IsInfinity = false
                };
                OnPropertyChanged(nameof(Point));
                return;
            }

            BigInteger a = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.A);
            BigInteger b = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.B);
            BigInteger mod = PredefinedEllipticCurves.EllipticCurveDefinition.ParseStringToBiginteger(_settings.Mod);
            curve = new WeierstraßCurve(a, b, mod);

            const int MAX_X_Y_VALUES = 1024;
            for (BigInteger x = 0; x < MAX_X_Y_VALUES; x++)
            {
                BigInteger rhs = x * x * x + a * x + b;
                rhs = rhs.Mod(mod);

                for (BigInteger y = 0; y < MAX_X_Y_VALUES; y++)
                {
                    if (y * y % mod == rhs)
                    {
                        _point = new Point
                        {
                            X = x,
                            Y = y,
                            Curve = curve,
                            IsInfinity = false
                        };
                        OnPropertyChanged(nameof(Point));
                        return;
                    }
                }
            }
            throw new Exception("Did not find any valid point. Please check a, b, p!");
        }

        public void Initialize() { }
        public void Dispose() { }
        public void Stop() { }
        public void Pause() { }
        public void PreExecution() { }
        public void PostExecution() { }

        #endregion

        #region Helper

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}