using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui.Hardware;
using Orbitstrap.UI.Elements.Dialogs;
using Orbitstrap.UI.ViewModels.Settings;
using System.Collections.ObjectModel;

namespace Orbitstrap.UI.Elements.Settings.Pages
{
    public partial class PluginsPage
    {
        public PluginsPage()
        {
            InitializeComponent();
            DataContext = new PluginsViewModel();
        }
    }
}

