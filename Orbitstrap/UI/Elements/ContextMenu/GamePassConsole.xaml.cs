using Orbitstrap.Integrations;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.ContextMenu;

namespace Orbitstrap.UI.Elements.ContextMenu
{
    public partial class GamePassConsole
    {
        public GamePassConsole(long userId)
        {
            InitializeComponent();
            var vm = new GamePassConsoleViewModel();
            DataContext = vm;
            vm.RequestCloseEvent += (_, _) => Close();
            vm.LoadGamePassesCommand.Execute(userId);
        }
    }
}
