using Orbitstrap.UI.ViewModels.Settings;

namespace Orbitstrap.UI.Elements.Settings.Pages
{
    public partial class BehaviourPage
    {
        public BehaviourPage()
        {
            DataContext = new BehaviourViewModel();
            InitializeComponent();
        }
    }
}
