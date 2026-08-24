using System.Windows.Controls;
using Orbitstrap.UI.ViewModels.Pages;

namespace Orbitstrap.UI.Elements.Settings.Pages
{
    public partial class HistoryPage : Page
    {
        private readonly HistoryPageViewModel _viewModel;
        public HistoryPage()
        {
            InitializeComponent();
            _viewModel = new HistoryPageViewModel();
            DataContext = _viewModel;
        }
    }
}