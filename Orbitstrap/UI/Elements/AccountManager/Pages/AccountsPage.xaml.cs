using Orbitstrap.UI.ViewModels.AccountManager;

using AccountMgr = Orbitstrap.Integrations.AccountManager;
namespace Orbitstrap.UI.Elements.AccountManager.Pages
{
    public partial class AccountsPage
    {
        private AccountsViewModel? _viewModel;

        public AccountsPage()
        {
            DataContext = new AccountsViewModel();
            InitializeComponent();
            _viewModel = DataContext as AccountsViewModel;
        }
    }
}