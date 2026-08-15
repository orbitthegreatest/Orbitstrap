using System.Windows;
using Orbitstrap.UI.ViewModels;

namespace Orbitstrap.UI.Elements.ContextMenu
{
    public partial class AccountManagerWindow
    {
        public AccountManagerWindow()
        {
            InitializeComponent();
            DataContext = new AccountBackupsViewModel();
        }
    }
}
