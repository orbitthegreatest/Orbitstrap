using System.Windows;
using Orbitstrap.Resources;

namespace Orbitstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for DataTransferDialog.xaml
    /// </summary>
    public partial class DataTransferDialog
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public bool ExportAllData => AllDataRadio.IsChecked == true;

        public bool ExportMods => ExportAllData || ModsCheck.IsChecked == true;

        public bool ExportFastFlags => ExportAllData || FastFlagsCheck.IsChecked == true;

        public bool ExportSettings => ExportAllData || SettingsCheck.IsChecked == true;

        public bool ExportGlobalSettings => ExportAllData || GlobalSettingsCheck.IsChecked == true;

        public DataTransferDialog()
        {
            InitializeComponent();
        }

        private void ScopeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (CustomOptions is null)
                return;

            CustomOptions.IsEnabled = CustomRadio.IsChecked == true;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomRadio.IsChecked == true &&
                ModsCheck.IsChecked != true &&
                FastFlagsCheck.IsChecked != true &&
                SettingsCheck.IsChecked != true &&
                GlobalSettingsCheck.IsChecked != true)
            {
                Frontend.ShowMessageBox(Strings.Dialog_DataTransfer_NoSelection, MessageBoxImage.Warning);
                return;
            }

            Result = MessageBoxResult.OK;
            Close();
        }
    }
}