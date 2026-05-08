using System.Windows;
using System.Windows.Controls;

namespace Orbitstrap.UI.Elements.Controls;

[System.Windows.Markup.ContentProperty("InnerContent")]
public partial class OptionControl : UserControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(OptionControl),
            new PropertyMetadata(null, OnHeaderChanged));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register("Description", typeof(string), typeof(OptionControl),
            new PropertyMetadata(null, OnDescriptionChanged));

    public static readonly DependencyProperty HelpLinkProperty =
        DependencyProperty.Register("HelpLink", typeof(string), typeof(OptionControl),
            new PropertyMetadata(null, OnHelpLinkChanged));

    public static readonly DependencyProperty InnerContentProperty =
        DependencyProperty.Register("InnerContent", typeof(object), typeof(OptionControl));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string HelpLink
    {
        get => (string)GetValue(HelpLinkProperty);
        set => SetValue(HelpLinkProperty, value);
    }

    public object InnerContent
    {
        get => GetValue(InnerContentProperty);
        set => SetValue(InnerContentProperty, value);
    }

    public OptionControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Sync all values after the visual tree is built (catches properties set before InitializeComponent)
        SyncHeader(Header);
        SyncDescription(Description);
        SyncHelpLink(HelpLink);
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OptionControl oc && oc.PART_Header != null)
            oc.SyncHeader((string?)e.NewValue);
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OptionControl oc && oc.PART_Description != null)
            oc.SyncDescription((string?)e.NewValue);
    }

    private static void OnHelpLinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OptionControl oc && oc.PART_HelpLinkContainer != null)
            oc.SyncHelpLink((string?)e.NewValue);
    }

    private void SyncHeader(string? value)
    {
        PART_Header.Text = value ?? string.Empty;
    }

    private void SyncDescription(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            PART_Description.Visibility = Visibility.Collapsed;
            PART_Description.MarkdownText = string.Empty;
        }
        else
        {
            PART_Description.MarkdownText = value;
            PART_Description.Visibility = Visibility.Visible;
        }
    }

    private void SyncHelpLink(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            PART_HelpLinkContainer.Tag = null;
            PART_HelpLinkContainer.Visibility = Visibility.Collapsed;
        }
        else
        {
            PART_HelpLinkContainer.Tag = value;
            PART_HelpLinkContainer.Visibility = Visibility.Visible;
        }
    }
}
