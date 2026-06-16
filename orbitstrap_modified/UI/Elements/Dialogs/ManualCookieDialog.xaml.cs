using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.Dialogs;

namespace Orbitstrap.UI.Elements.Dialogs
{
    public partial class ManualCookieDialog : WpfUiWindow
    {
        public ManualCookieDialogViewModel ViewModel { get; }

        public ManualCookieDialog()
        {
            ViewModel = new ManualCookieDialogViewModel(this);
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}