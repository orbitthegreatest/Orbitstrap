using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Wpf.Ui.Common;

namespace Orbitstrap.UI.Elements.Controls;

[ContentProperty("InnerContent")]
public partial class Expander : UserControl, IComponentConnector
{
	public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Expander));

	public static readonly DependencyProperty HeaderIconProperty = DependencyProperty.Register("HeaderIcon", typeof(SymbolRegular), typeof(Expander));

	public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(Expander));

	public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register("InnerContent", typeof(object), typeof(Expander));

	public bool IsExpanded
	{
		get
		{
			return (bool)GetValue(IsExpandedProperty);
		}
		set
		{
			SetValue(IsExpandedProperty, value);
		}
	}

	public string HeaderText
	{
		get
		{
			return (string)GetValue(HeaderTextProperty);
		}
		set
		{
			SetValue(HeaderTextProperty, value);
		}
	}

	public SymbolRegular HeaderIcon
	{
		get
		{
			return (SymbolRegular)GetValue(HeaderIconProperty);
		}
		set
		{
			SetValue(HeaderTextProperty, value);
		}
	}

	public object InnerContent
	{
		get
		{
			return GetValue(InnerContentProperty);
		}
		set
		{
			SetValue(InnerContentProperty, value);
		}
	}

	public Expander()
	{
		InitializeComponent();
	}
}
