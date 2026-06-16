using Orbitstrap.Integrations;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.ContextMenu;

namespace Orbitstrap.UI.Elements.ContextMenu
{
    public partial class BetterBloxDataCenterConsole
    {
        public BetterBloxDataCenterConsole()
        {
            InitializeComponent();
            var vm = new BetterBloxDataCenterConsoleViewModel();
            DataContext = vm;
        }
    }
}
