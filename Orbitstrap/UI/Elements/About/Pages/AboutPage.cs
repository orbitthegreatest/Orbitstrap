using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using Orbitstrap.UI.ViewModels.About;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.About.Pages;

public partial class AboutPage : UiPage, IComponentConnector
{
	private readonly Queue<Key> _keys = new Queue<Key>();

	private readonly List<Key> _expectedKeys = new List<Key>
	{
		Key.M,
		Key.A,
		Key.T,
		Key.T,
		Key.LeftShift,
		Key.D1
	};

	private bool _triggered;

	public AboutPage()
	{
		base.DataContext = new AboutViewModel();
		InitializeComponent();
	}

	private void UiPage_KeyDown(object sender, KeyEventArgs e)
	{
		if (!_triggered)
		{
			if (_keys.Count >= 6)
			{
				_keys.Dequeue();
			}
			Key key = e.Key;
			if (key == Key.RightShift)
			{
				key = Key.LeftShift;
			}
			_keys.Enqueue(key);
			if (_keys.SequenceEqual(_expectedKeys))
			{
				_triggered = true;
				(base.Resources["EggStoryboard"] as Storyboard).Begin();
			}
		}
	}
}
