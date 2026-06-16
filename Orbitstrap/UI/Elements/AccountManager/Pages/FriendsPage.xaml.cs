using Orbitstrap.UI.ViewModels.AccountManager;

namespace Orbitstrap.UI.Elements.AccountManager.Pages
{
    /// <summary>
    /// Interaction logic for FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage
    {
        public FriendsPage()
        {
            DataContext = new FriendsViewModel();
            InitializeComponent();
        }
    }
}
