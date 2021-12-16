using CrypTool.MD5.Algorithm;
using CrypTool.PluginBase;
using System;
using System.Windows.Data;

namespace CrypTool.MD5.Presentation.Converters
{
    internal class MD5StateDescriptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //typeof(MD5).GetPluginStringResource("PluginCaption");
            //PluginInfoAttribute[] attributes = (PluginInfoAttribute[])type.GetCustomAttributes(typeof(PluginInfoAttribute), false);

            MD5StateDescription state = (MD5StateDescription)value;
            switch (state)
            {
                case MD5StateDescription.UNINITIALIZED:
                    return typeof(MD5).GetPluginStringResource("MD5State_Algorithm_uninitialized");
                case MD5StateDescription.INITIALIZED:
                    return typeof(MD5).GetPluginStringResource("MD5State_Initialization");
                case MD5StateDescription.READING_DATA:
                    return typeof(MD5).GetPluginStringResource("MD5State_Reading_data");
                case MD5StateDescription.READ_DATA:
                    return typeof(MD5).GetPluginStringResource("MD5State_Read_data");
                case MD5StateDescription.STARTING_PADDING:
                    return typeof(MD5).GetPluginStringResource("MD5State_Beginning_padding_process");
                case MD5StateDescription.ADDING_PADDING_BYTES:
                    return typeof(MD5).GetPluginStringResource("MD5State_Adding_the_padding_bytes");
                case MD5StateDescription.ADDED_PADDING_BYTES:
                    return typeof(MD5).GetPluginStringResource("MD5State_Added_the_padding_bytes");
                case MD5StateDescription.ADDING_LENGTH:
                    return typeof(MD5).GetPluginStringResource("MD5State_Adding_the_data_length");
                case MD5StateDescription.ADDED_LENGTH:
                    return typeof(MD5).GetPluginStringResource("MD5State_Added_the_data_length");
                case MD5StateDescription.FINISHED_PADDING:
                    return typeof(MD5).GetPluginStringResource("MD5State_Finished_padding");
                case MD5StateDescription.STARTING_COMPRESSION:
                    return typeof(MD5).GetPluginStringResource("MD5State_Starting_the_compression");
                case MD5StateDescription.STARTING_ROUND:
                    return typeof(MD5).GetPluginStringResource("MD5State_Starting_a_compression_round");
                case MD5StateDescription.STARTING_ROUND_STEP:
                    return typeof(MD5).GetPluginStringResource("MD5State_Before_compression_step");
                case MD5StateDescription.FINISHED_ROUND_STEP:
                    return typeof(MD5).GetPluginStringResource("MD5State_Performing_compression_step");
                case MD5StateDescription.FINISHED_ROUND:
                    return typeof(MD5).GetPluginStringResource("MD5State_Finished_compression_round");
                case MD5StateDescription.FINISHING_COMPRESSION:
                    return typeof(MD5).GetPluginStringResource("MD5State_Finalizing_compression");
                case MD5StateDescription.FINISHED_COMPRESSION:
                    return typeof(MD5).GetPluginStringResource("MD5State_Finished_compression");
                case MD5StateDescription.FINISHED:
                    return typeof(MD5).GetPluginStringResource("MD5State_Finished");
                default:
                    return typeof(MD5).GetPluginStringResource("MD5State_Unknown_state");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
