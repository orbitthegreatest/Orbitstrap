using Orbitstrap.Integrations;
using Orbitstrap.UI.ViewModels.ContextMenu;

namespace Orbitstrap.UI.Elements.ContextMenu
{
    public partial class ChatLogs
    {
        public ChatLogs(ActivityWatcher watcher)
        {
            var viewModel = new ChatLogsViewModel(watcher);

            viewModel.RequestCloseEvent += (_, _) => Close();

            DataContext = viewModel;
            InitializeComponent();
        }

        private void ChatLogsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}