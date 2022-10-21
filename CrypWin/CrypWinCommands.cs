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
using DevComponents.WpfRibbon;

namespace CrypTool.CrypWin
{
    public class CrypWinCommands
    {
        public static ButtonDropDownCommand ContextHelp = new ButtonDropDownCommand(Properties.Resources.ContextHelp, "ContextHelp", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Play = new ButtonDropDownCommand(Properties.Resources.Play, "Play", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Pause = new ButtonDropDownCommand(Properties.Resources.Pause, "Pause", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Stop = new ButtonDropDownCommand(Properties.Resources.Stop, "Stop", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Undo = new ButtonDropDownCommand(Properties.Resources.Undo, "Undo", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Redo = new ButtonDropDownCommand(Properties.Resources.Redo, "Redo", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Cut = new ButtonDropDownCommand(Properties.Resources.Cut, "Cut", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Copy = new ButtonDropDownCommand(Properties.Resources.Copy, "Copy", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Paste = new ButtonDropDownCommand(Properties.Resources.Paste, "Paste", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Remove = new ButtonDropDownCommand(Properties.Resources.Remove, "Remove", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand AddImage = new ButtonDropDownCommand(Properties.Resources.AddImage, "AddImage", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand AddText = new ButtonDropDownCommand(Properties.Resources.AddText, "AddText", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand PlayDemo = new ButtonDropDownCommand(Properties.Resources.PlayDemo, "PlayDemo", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand StopDemo = new ButtonDropDownCommand(Properties.Resources.StopDemo, "StopDemo", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Maximize = new ButtonDropDownCommand(Properties.Resources.Maximize, "Maximize", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Fullscreen = new ButtonDropDownCommand(Properties.Resources.Fullscreen, "Fullscreen", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand P2P = new ButtonDropDownCommand(Properties.Resources.Network, "P2P", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Startcenter = new ButtonDropDownCommand(Properties.Resources.Startcenter, "Startcenter", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand CrypToolStore = new ButtonDropDownCommand(Properties.Resources.CrypToolStore, "CrypToolStore", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand AutoUpdater = new ButtonDropDownCommand(Properties.Resources.AutoUpdater, "AutoUpdater", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand Settings = new ButtonDropDownCommand(Properties.Resources.Settings, "Settings", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand F2S = new ButtonDropDownCommand(Properties.Resources.Settings, "FitToScreen", typeof(CrypWin.MainWindow));
        public static ButtonDropDownCommand ShowHideLog = new ButtonDropDownCommand(Properties.Resources.Settings, "ShowHideLog", typeof(CrypWin.MainWindow));
    }
}
