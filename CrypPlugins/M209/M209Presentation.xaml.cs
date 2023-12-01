using System;
using System.Windows.Controls;

namespace CrypTool.M209
{
    /// <summary>
    /// Interaction logic for M209Presentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.M209.Properties.Resources")]
    public partial class M209Presentation : UserControl
    {
        public struct DynamicGridRowValue
        {
            public string Title { set; get; }
            public string W1_value { set; get; }
            public string W2_value { set; get; }
            public string W3_value { set; get; }
            public string W4_value { set; get; }
            public string W5_value { set; get; }
            public string W6_value { set; get; }
        }
        public struct PositionWheelGridRawValue
        {
            public string W1_position { set; get; }
            public string W2_position { set; get; }
            public string W3_position { set; get; }
            public string W4_position { set; get; }
            public string W5_position { set; get; }
            public string W6_position { set; get; }

        }
        public M209Presentation()
        {
            InitializeComponent();
        }

        internal void ShowDisplayedPositionsInGrid(string[,] wheelValues)
        {
            DataGridDynamicInformation.Items[0] = (new DynamicGridRowValue
            {
                Title = Properties.Resources.PositionCurrentlyDisplayedToUser,
                W1_value = wheelValues[0, 2],
                W2_value = wheelValues[1, 2],
                W3_value = wheelValues[2, 2],
                W4_value = wheelValues[3, 2],
                W5_value = wheelValues[4, 2],
                W6_value = wheelValues[5, 2],
            });
        }
        internal void ShowActivePositionsInGrid(string[] wheelValues)
        {
            Console.WriteLine(DataGridDynamicInformation);
            DataGridDynamicInformation.Items[1] = (new DynamicGridRowValue
            {
                Title = Properties.Resources.PositionCurrentlyActive,
                W1_value = wheelValues[0].ToString(),
                W2_value = wheelValues[1].ToString(),
                W3_value = wheelValues[2].ToString(),
                W4_value = wheelValues[3].ToString(),
                W5_value = wheelValues[4].ToString(),
                W6_value = wheelValues[5].ToString(),
            });
        }
        internal void ShowWheelPinActivityInGrid(bool[] wheelValues)
        {
            DataGridDynamicInformation.Items[2] = (new DynamicGridRowValue
            {
                Title = Properties.Resources.WheelsWithActivePins,
                W1_value = wheelValues[0] ? "+" : string.Empty,
                W2_value = wheelValues[1] ? "+" : string.Empty,
                W3_value = wheelValues[2] ? "+" : string.Empty,
                W4_value = wheelValues[3] ? "+" : string.Empty,
                W5_value = wheelValues[4] ? "+" : string.Empty,
                W6_value = wheelValues[5] ? "+" : string.Empty
            });
        }

        internal void CreateDinamicInfoGrid()
        {
            DataGridDynamicInformation.Items.Add(new DynamicGridRowValue
            {
                Title = Properties.Resources.PositionCurrentlyDisplayedToUser,
                W1_value = string.Empty,
                W2_value = string.Empty,
                W3_value = string.Empty,
                W4_value = string.Empty,
                W5_value = string.Empty,
                W6_value = string.Empty
            });

            DataGridDynamicInformation.Items.Add(new DynamicGridRowValue
            {
                Title = Properties.Resources.PositionCurrentlyActive,
                W1_value = string.Empty,
                W2_value = string.Empty,
                W3_value = string.Empty,
                W4_value = string.Empty,
                W5_value = string.Empty,
                W6_value = string.Empty
            });
        }

        internal void CreateWheelsInfoGrids()
        {
            for (int i = 0; i < 5; i++)
            {
                dataGridWheel1.Items.Add(new PositionWheelGridRawValue
                {
                    W1_position = string.Empty,
                }); ;

                dataGridWheel2.Items.Add(new PositionWheelGridRawValue
                {
                    W2_position = string.Empty
                });

                dataGridWheel3.Items.Add(new PositionWheelGridRawValue
                {
                    W3_position = string.Empty
                });

                dataGridWheel4.Items.Add(new PositionWheelGridRawValue
                {
                    W4_position = string.Empty
                });

                dataGridWheel5.Items.Add(new PositionWheelGridRawValue
                {
                    W5_position = string.Empty
                });
                dataGridWheel6.Items.Add(new PositionWheelGridRawValue
                {
                    W6_position = string.Empty
                });
            }

        }

        internal void ShowWheelPositionsinGrid(string[,] wheelValues)
        {
            for (int i = 0; i < 5; i++)
            {
                dataGridWheel1.Items[i] = (new PositionWheelGridRawValue
                {
                    W1_position = wheelValues[0, i],
                });

                dataGridWheel2.Items[i] = (new PositionWheelGridRawValue
                {
                    W2_position = wheelValues[1, i],
                });
                dataGridWheel3.Items[i] = (new PositionWheelGridRawValue
                {
                    W3_position = wheelValues[2, i],
                });
                dataGridWheel4.Items[i] = (new PositionWheelGridRawValue
                {
                    W4_position = wheelValues[3, i],
                });
                dataGridWheel5.Items[i] = (new PositionWheelGridRawValue
                {
                    W5_position = wheelValues[4, i],
                });
                dataGridWheel6.Items[i] = (new PositionWheelGridRawValue
                {
                    W6_position = wheelValues[5, i],
                });
            }
        }
    }
}
