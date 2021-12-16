using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CrypTool.T9Code
{
    /// <summary>
    /// Interaction logic for T9CodePresentation.xaml
    /// </summary>
    public partial class T9CodePresentation : UserControl
    {
        private readonly T9Code _t9CodePlugin;

        public T9CodePresentation(T9Code t9CodePlugin)
        {
            _t9CodePlugin = t9CodePlugin;
            InitializeComponent();
            List<(Shape, int)> list = new List<(Shape, int)>
            {
                (PathFor2, 2),
                (PathFor3, 3),
                (PathFor4, 4),
                (PathFor5, 5),
                (PathFor6, 6),
                (PathFor7, 7),
                (PathFor8, 8),
                (PathFor9, 9),
                (PathFor0, 0),
            };
            foreach ((Shape, int) shape in list)
            {
                AddListenersWithValues(shape);
            }

            PathForDelete.MouseDown += (sender, args) =>
            {
                if (Display.Text.Length > 0)
                {
                    Display.Text = Display.Text.Remove(Display.Text.Length - 1);
                }
            };
            AddListenersForCursor(PathForDelete);
        }

        private static void AddListenersForCursor(Shape shape)
        {
            shape.MouseEnter += (sender, args) => shape.Cursor = Cursors.Hand;
            shape.MouseLeave += (sender, args) => shape.Cursor = Cursors.Arrow;
        }

        private void AddListenersWithValues((Shape, int) s)
        {
            (Shape shape, int value) = s;
            shape.MouseDown += (sender, args) => { HandleMouseClick(value); };
            AddListenersForCursor(shape);
        }

        private void HandleMouseClick(int value)
        {
            Display.Text += value;
            _t9CodePlugin.InputText = Display.Text;
            _t9CodePlugin.Execute();
        }

        public void SetNumbersToDisplay(string numbers)
        {
            Display.Text = numbers;
        }
    }
}