using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Xml.Linq;
// SharpVectors removed — not net8 compatible. SVG elements fall back to WPF Image.
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.Elements.Bootstrapper.Base;
using Orbitstrap.UI.Elements.Controls;
using Orbitstrap.UI.ViewModels.Bootstrapper;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Markup;
using XamlAnimatedGif;

namespace Orbitstrap.UI.Elements.Bootstrapper;

public partial class CustomDialog : WpfUiWindow, IBootstrapperDialog, IComponentConnector
{
	private class DummyFrameworkElement : FrameworkElement
	{
	}

	private delegate object HandleXmlElementDelegate(CustomDialog dialog, XElement xmlElement);

	private struct GetImageSourceDataResult
	{
		public bool IsIcon = false;

		public Uri? Uri = null;

		public GetImageSourceDataResult()
		{
		}
	}

	private static GeometryConverter? _geometryConverter = null;

	private static RectConverter? _rectConverter = null;

	private const int Version = 1;

	private const int MaxElements = 100;

	private bool _initialised;

	private static Dictionary<string, HandleXmlElementDelegate> _elementHandlerMap = new Dictionary<string, HandleXmlElementDelegate>
	{
		["OrbitstrapCustomBootstrapper"] = HandleXmlElement_OrbitstrapCustomBootstrapper_Fake,
		["TitleBar"] = HandleXmlElement_TitleBar,
		["Button"] = HandleXmlElement_Button,
		["ProgressBar"] = HandleXmlElement_ProgressBar,
		["ProgressRing"] = HandleXmlElement_ProgressRing,
		["TextBlock"] = HandleXmlElement_TextBlock,
		["MarkdownTextBlock"] = HandleXmlElement_MarkdownTextBlock,
		["Image"] = HandleXmlElement_Image,
		["Grid"] = HandleXmlElement_Grid,
		["StackPanel"] = HandleXmlElement_StackPanel,
		["Border"] = HandleXmlElement_Border,
		["MediaElement"] = HandleXmlElement_MediaElement,
		["SolidColorBrush"] = HandleXmlElement_SolidColorBrush,
		["ImageBrush"] = HandleXmlElement_ImageBrush,
		["LinearGradientBrush"] = HandleXmlElement_LinearGradientBrush,
		["RadialGradientBrush"] = HandleXmlElement_RadialGradientBrush,
		["GradientStop"] = HandleXmlElement_GradientStop,
		["ScaleTransform"] = HandleXmlElement_ScaleTransform,
		["SkewTransform"] = HandleXmlElement_SkewTransform,
		["RotateTransform"] = HandleXmlElement_RotateTransform,
		["TranslateTransform"] = HandleXmlElement_TranslateTransform,
		["BlurEffect"] = HandleXmlElement_BlurEffect,
		["DropShadowEffect"] = HandleXmlElement_DropShadowEffect,
		["SvgViewbox"] = HandleXmlElement_SvgViewbox,
		["SvgIcon"] = HandleXmlElement_SvgIcon,
		["SvgBitmap"] = HandleXmlElement_SvgBitmap,
		["Path"] = HandleXmlElement_Path,
		["Ellipse"] = HandleXmlElement_Ellipse,
		["Line"] = HandleXmlElement_Line,
		["Rectangle"] = HandleXmlElement_Rectangle,
		["RowDefinition"] = HandleXmlElement_RowDefinition,
		["ColumnDefinition"] = HandleXmlElement_ColumnDefinition
	};

	private readonly BootstrapperDialogViewModel _viewModel;

	private bool _isClosing;

	private static ThicknessConverter ThicknessConverter { get; } = new ThicknessConverter();

	private static GeometryConverter GeometryConverter => _geometryConverter ?? (_geometryConverter = new GeometryConverter());

	public static RectConverter RectConverter => _rectConverter ?? (_rectConverter = new RectConverter());

	private static ColorConverter ColorConverter { get; } = new ColorConverter();

	private static PointConverter PointConverter { get; } = new PointConverter();

	private static CornerRadiusConverter CornerRadiusConverter { get; } = new CornerRadiusConverter();

	private static GridLengthConverter GridLengthConverter { get; } = new GridLengthConverter();

	private static BrushConverter BrushConverter { get; } = new BrushConverter();

	private List<string> UsedNames { get; } = new List<string>();

	private string ThemeDir { get; set; } = "";

	public Orbitstrap.Bootstrapper? Bootstrapper { get; set; }

	public string Message
	{
		get
		{
			return _viewModel.Message;
		}
		set
		{
			_viewModel.Message = value;
			_viewModel.OnPropertyChanged("Message");
		}
	}

	public ProgressBarStyle ProgressStyle
	{
		get
		{
			if (!_viewModel.ProgressIndeterminate)
			{
				return ProgressBarStyle.Continuous;
			}
			return ProgressBarStyle.Marquee;
		}
		set
		{
			_viewModel.ProgressIndeterminate = value == ProgressBarStyle.Marquee;
			_viewModel.OnPropertyChanged("ProgressIndeterminate");
		}
	}

	public int ProgressMaximum
	{
		get
		{
			return _viewModel.ProgressMaximum;
		}
		set
		{
			_viewModel.ProgressMaximum = value;
			_viewModel.OnPropertyChanged("ProgressMaximum");
		}
	}

	public int ProgressValue
	{
		get
		{
			return _viewModel.ProgressValue;
		}
		set
		{
			_viewModel.ProgressValue = value;
			_viewModel.OnPropertyChanged("ProgressValue");
		}
	}

	public TaskbarItemProgressState TaskbarProgressState
	{
		get
		{
			return _viewModel.TaskbarProgressState;
		}
		set
		{
			_viewModel.TaskbarProgressState = value;
			_viewModel.OnPropertyChanged("TaskbarProgressState");
		}
	}

	public double TaskbarProgressValue
	{
		get
		{
			return _viewModel.TaskbarProgressValue;
		}
		set
		{
			_viewModel.TaskbarProgressValue = value;
			_viewModel.OnPropertyChanged("TaskbarProgressValue");
		}
	}

	public bool CancelEnabled
	{
		get
		{
			return _viewModel.CancelEnabled;
		}
		set
		{
			_viewModel.CancelEnabled = value;
			_viewModel.OnPropertyChanged("CancelButtonVisibility");
			_viewModel.OnPropertyChanged("CancelEnabled");
		}
	}

	private static T? ConvertValue<T>(string input) where T : struct
	{
		try
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			if (converter != null)
			{
				return (T?)converter.ConvertFromInvariantString(input);
			}
			return null;
		}
		catch (NotSupportedException)
		{
			return null;
		}
	}

	private static object? GetTypeFromXElement(TypeConverter converter, XElement xmlElement, string attributeName)
	{
		string text = xmlElement.Attribute(attributeName)?.Value?.ToString();
		if (text == null)
		{
			return null;
		}
		try
		{
			return converter.ConvertFromInvariantString(text);
		}
		catch (Exception ex)
		{
			throw new Exception($"{xmlElement.Name} has invalid {attributeName}: {ex.Message}", ex);
		}
	}

	private static object? GetThicknessFromXElement(XElement xmlElement, string attributeName)
	{
		return GetTypeFromXElement(ThicknessConverter, xmlElement, attributeName);
	}

	private static object? GetGeometryFromXElement(XElement xmlElement, string attributeName)
	{
		return GetTypeFromXElement(GeometryConverter, xmlElement, attributeName);
	}

	private static object? GetRectFromXElement(XElement xmlElement, string attributeName)
	{
		return GetTypeFromXElement(RectConverter, xmlElement, attributeName);
	}

	private static object? GetColorFromXElement(XElement xmlElement, string attributeName)
	{
		return GetTypeFromXElement(ColorConverter, xmlElement, attributeName);
	}

	private static object? GetPointFromXElement(XElement xmlElement, string attributeName)
	{
		return GetTypeFromXElement(PointConverter, xmlElement, attributeName);
	}

	private static object? GetCornerRadiusFromXElement(XElement xmlElement, string attributeName)
	{
		return GetTypeFromXElement(CornerRadiusConverter, xmlElement, attributeName);
	}

	private static object? GetGridLengthFromXElement(XElement xmlElement, string attributeName)
	{
		return GetTypeFromXElement(GridLengthConverter, xmlElement, attributeName);
	}

	private static object? GetBrushFromXElement(XElement element, string attributeName)
	{
		string text = element.Attribute(attributeName)?.Value?.ToString();
		if (text == null)
		{
			return null;
		}
		if (text.StartsWith('{') && text.EndsWith('}'))
		{
			string text2 = text;
			return text2.Substring(1, text2.Length - 1 - 1);
		}
		try
		{
			return BrushConverter.ConvertFromInvariantString(text);
		}
		catch (Exception ex)
		{
			throw new Exception($"{element.Name} has invalid {attributeName}: {ex.Message}", ex);
		}
	}

	private static T HandleXml<T>(CustomDialog dialog, XElement xmlElement) where T : class
	{
		if (!_elementHandlerMap.ContainsKey(xmlElement.Name.ToString()))
		{
			throw new Exception($"Unknown element {xmlElement.Name}");
		}
		object obj = _elementHandlerMap[xmlElement.Name.ToString()](dialog, xmlElement);
		if (!(obj is T))
		{
			throw new Exception($"{xmlElement.Parent.Name} cannot have a child of {xmlElement.Name}");
		}
		return (T)obj;
	}

	private static void AddXml(CustomDialog dialog, XElement xmlElement)
	{
		if (!xmlElement.Name.ToString().StartsWith($"{xmlElement.Parent.Name}."))
		{
			UIElement uIElement = HandleXml<UIElement>(dialog, xmlElement);
			if (!(uIElement is DummyFrameworkElement))
			{
				dialog.ElementGrid.Children.Add(uIElement);
			}
		}
	}

	private void HandleXmlBase(XElement xml)
	{
		if (_initialised)
		{
			throw new Exception("Custom dialog has already been initialised");
		}
		if (xml.Name != "OrbitstrapCustomBootstrapper")
		{
			throw new Exception("XML root is not a OrbitstrapCustomBootstrapper");
		}
		if (xml.Attribute("Version")?.Value != 1.ToString())
		{
			throw new Exception("Unknown OrbitstrapCustomBootstrapper version");
		}
		if (xml.Descendants().Count() > 100)
		{
			throw new Exception($"Custom bootstrappers can have a maximum of {100} elements");
		}
		_initialised = true;
		HandleXmlElement_OrbitstrapCustomBootstrapper(this, xml);
		foreach (XElement item in xml.Elements())
		{
			AddXml(this, item);
		}
	}

	public void ApplyCustomTheme(string name, string contents)
	{
		ThemeDir = System.IO.Path.Combine(Paths.CustomThemes, name);
		XElement xml;
		try
		{
			using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
			xml = XElement.Load(stream);
		}
		catch (Exception ex)
		{
			throw new Exception("XML parse failed: " + ex.Message, ex);
		}
		HandleXmlBase(xml);
	}

	public void ApplyCustomTheme(string name)
	{
		string path = System.IO.Path.Combine(Paths.CustomThemes, name, "Theme.xml");
		ApplyCustomTheme(name, File.ReadAllText(path));
	}

	private static Transform HandleXmlElement_ScaleTransform(CustomDialog dialog, XElement xmlElement)
	{
		return new ScaleTransform
		{
			ScaleX = ParseXmlAttribute<double>(xmlElement, "ScaleX", 1.0),
			ScaleY = ParseXmlAttribute<double>(xmlElement, "ScaleY", 1.0),
			CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0.0),
			CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0.0)
		};
	}

	private static Transform HandleXmlElement_SkewTransform(CustomDialog dialog, XElement xmlElement)
	{
		return new SkewTransform
		{
			AngleX = ParseXmlAttribute<double>(xmlElement, "AngleX", 0.0),
			AngleY = ParseXmlAttribute<double>(xmlElement, "AngleY", 0.0),
			CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0.0),
			CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0.0)
		};
	}

	private static Transform HandleXmlElement_RotateTransform(CustomDialog dialog, XElement xmlElement)
	{
		return new RotateTransform
		{
			Angle = ParseXmlAttribute<double>(xmlElement, "Angle", 0.0),
			CenterX = ParseXmlAttribute<double>(xmlElement, "CenterX", 0.0),
			CenterY = ParseXmlAttribute<double>(xmlElement, "CenterY", 0.0)
		};
	}

	private static Transform HandleXmlElement_TranslateTransform(CustomDialog dialog, XElement xmlElement)
	{
		return new TranslateTransform
		{
			X = ParseXmlAttribute<double>(xmlElement, "X", 0.0),
			Y = ParseXmlAttribute<double>(xmlElement, "Y", 0.0)
		};
	}

	private static BlurEffect HandleXmlElement_BlurEffect(CustomDialog dialog, XElement xmlElement)
	{
		return new BlurEffect
		{
			KernelType = ParseXmlAttribute<KernelType>(xmlElement, "KernelType", KernelType.Gaussian),
			Radius = ParseXmlAttribute<double>(xmlElement, "Radius", 5.0),
			RenderingBias = ParseXmlAttribute<RenderingBias>(xmlElement, "RenderingBias", RenderingBias.Performance)
		};
	}

	private static DropShadowEffect HandleXmlElement_DropShadowEffect(CustomDialog dialog, XElement xmlElement)
	{
		DropShadowEffect dropShadowEffect = new DropShadowEffect();
		dropShadowEffect.BlurRadius = ParseXmlAttribute<double>(xmlElement, "BlurRadius", 5.0);
		dropShadowEffect.Direction = ParseXmlAttribute<double>(xmlElement, "Direction", 315.0);
		dropShadowEffect.Opacity = ParseXmlAttribute<double>(xmlElement, "Opacity", 1.0);
		dropShadowEffect.ShadowDepth = ParseXmlAttribute<double>(xmlElement, "ShadowDepth", 5.0);
		dropShadowEffect.RenderingBias = ParseXmlAttribute<RenderingBias>(xmlElement, "RenderingBias", RenderingBias.Performance);
		object colorFromXElement = GetColorFromXElement(xmlElement, "Color");
		if (colorFromXElement is Color)
		{
			dropShadowEffect.Color = (Color)colorFromXElement;
		}
		return dropShadowEffect;
	}

	private static Brush HandleXmlElement_RadialGradientBrush(CustomDialog dialog, XElement xmlElement)
	{
		RadialGradientBrush radialGradientBrush = new RadialGradientBrush();
		HandleXml_Brush(radialGradientBrush, xmlElement);
		object pointFromXElement = GetPointFromXElement(xmlElement, "GradientOrigin");
		if (pointFromXElement is Point)
		{
			radialGradientBrush.GradientOrigin = (Point)pointFromXElement;
		}
		object pointFromXElement2 = GetPointFromXElement(xmlElement, "Center");
		if (pointFromXElement2 is Point)
		{
			radialGradientBrush.Center = (Point)pointFromXElement2;
		}
		radialGradientBrush.ColorInterpolationMode = ParseXmlAttribute<ColorInterpolationMode>(xmlElement, "ColorInterpolationMode", ColorInterpolationMode.SRgbLinearInterpolation);
		radialGradientBrush.MappingMode = ParseXmlAttribute<BrushMappingMode>(xmlElement, "MappingMode", BrushMappingMode.RelativeToBoundingBox);
		radialGradientBrush.SpreadMethod = ParseXmlAttribute<GradientSpreadMethod>(xmlElement, "SpreadMethod", GradientSpreadMethod.Pad);
		foreach (XElement item in xmlElement.Elements())
		{
			radialGradientBrush.GradientStops.Add(HandleXml<GradientStop>(dialog, item));
		}
		return radialGradientBrush;
	}

	private static void HandleXml_Brush(Brush brush, XElement xmlElement)
	{
		brush.Opacity = ParseXmlAttribute<double>(xmlElement, "Opacity", 1.0);
	}

	private static Brush HandleXmlElement_SolidColorBrush(CustomDialog dialog, XElement xmlElement)
	{
		SolidColorBrush solidColorBrush = new SolidColorBrush();
		HandleXml_Brush(solidColorBrush, xmlElement);
		object colorFromXElement = GetColorFromXElement(xmlElement, "Color");
		if (colorFromXElement is Color)
		{
			solidColorBrush.Color = (Color)colorFromXElement;
		}
		return solidColorBrush;
	}

	private static Brush HandleXmlElement_ImageBrush(CustomDialog dialog, XElement xmlElement)
	{
		ImageBrush imageBrush = new ImageBrush();
		HandleXml_Brush(imageBrush, xmlElement);
		imageBrush.AlignmentX = ParseXmlAttribute<AlignmentX>(xmlElement, "AlignmentX", AlignmentX.Center);
		imageBrush.AlignmentY = ParseXmlAttribute<AlignmentY>(xmlElement, "AlignmentY", AlignmentY.Center);
		imageBrush.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Fill);
		imageBrush.TileMode = ParseXmlAttribute<TileMode>(xmlElement, "TileMode", TileMode.None);
		imageBrush.ViewboxUnits = ParseXmlAttribute<BrushMappingMode>(xmlElement, "ViewboxUnits", BrushMappingMode.RelativeToBoundingBox);
		imageBrush.ViewportUnits = ParseXmlAttribute<BrushMappingMode>(xmlElement, "ViewportUnits", BrushMappingMode.RelativeToBoundingBox);
		object rectFromXElement = GetRectFromXElement(xmlElement, "Viewbox");
		if (rectFromXElement is Rect)
		{
			imageBrush.Viewbox = (Rect)rectFromXElement;
		}
		object rectFromXElement2 = GetRectFromXElement(xmlElement, "Viewport");
		if (rectFromXElement2 is Rect)
		{
			imageBrush.Viewport = (Rect)rectFromXElement2;
		}
		GetImageSourceDataResult imageSourceData = GetImageSourceData(dialog, "ImageSource", xmlElement);
		if (imageSourceData.IsIcon)
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Icon")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(imageBrush, ImageBrush.ImageSourceProperty, binding);
		}
		else
		{
			BitmapImage imageSource;
			try
			{
				imageSource = new BitmapImage(imageSourceData.Uri);
			}
			catch (Exception ex)
			{
				throw new Exception("ImageBrush Failed to create BitmapImage: " + ex.Message, ex);
			}
			imageBrush.ImageSource = imageSource;
		}
		return imageBrush;
	}

	private static GradientStop HandleXmlElement_GradientStop(CustomDialog dialog, XElement xmlElement)
	{
		GradientStop gradientStop = new GradientStop();
		object colorFromXElement = GetColorFromXElement(xmlElement, "Color");
		if (colorFromXElement is Color)
		{
			gradientStop.Color = (Color)colorFromXElement;
		}
		gradientStop.Offset = ParseXmlAttribute<double>(xmlElement, "Offset", 0.0);
		return gradientStop;
	}

	private static Brush HandleXmlElement_LinearGradientBrush(CustomDialog dialog, XElement xmlElement)
	{
		LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
		HandleXml_Brush(linearGradientBrush, xmlElement);
		object pointFromXElement = GetPointFromXElement(xmlElement, "StartPoint");
		if (pointFromXElement is Point)
		{
			linearGradientBrush.StartPoint = (Point)pointFromXElement;
		}
		object pointFromXElement2 = GetPointFromXElement(xmlElement, "EndPoint");
		if (pointFromXElement2 is Point)
		{
			linearGradientBrush.EndPoint = (Point)pointFromXElement2;
		}
		linearGradientBrush.ColorInterpolationMode = ParseXmlAttribute<ColorInterpolationMode>(xmlElement, "ColorInterpolationMode", ColorInterpolationMode.SRgbLinearInterpolation);
		linearGradientBrush.MappingMode = ParseXmlAttribute<BrushMappingMode>(xmlElement, "MappingMode", BrushMappingMode.RelativeToBoundingBox);
		linearGradientBrush.SpreadMethod = ParseXmlAttribute<GradientSpreadMethod>(xmlElement, "SpreadMethod", GradientSpreadMethod.Pad);
		foreach (XElement item in xmlElement.Elements())
		{
			linearGradientBrush.GradientStops.Add(HandleXml<GradientStop>(dialog, item));
		}
		return linearGradientBrush;
	}

	private static void ApplyBrush_UIElement(CustomDialog dialog, FrameworkElement uiElement, string name, DependencyProperty dependencyProperty, XElement xmlElement)
	{
		object brushFromXElement = GetBrushFromXElement(xmlElement, name);
		if (brushFromXElement is Brush)
		{
			uiElement.SetValue(dependencyProperty, brushFromXElement);
			return;
		}
		if (brushFromXElement is string)
		{
			uiElement.SetResourceReference(dependencyProperty, brushFromXElement);
			return;
		}
		XElement xElement = xmlElement.Element($"{xmlElement.Name}.{name}");
		if (xElement != null)
		{
			if (!(xElement.FirstNode is XElement xmlElement2))
			{
				throw new Exception($"{xmlElement.Name} {name} is missing the brush");
			}
			Brush value = HandleXml<Brush>(dialog, xmlElement2);
			uiElement.SetValue(dependencyProperty, value);
		}
	}

		// SvgViewbox replaced with WPF Image — SharpVectors not compatible with net8.
	private static Image HandleXmlElement_SvgViewbox(CustomDialog dialog, XElement xmlElement)
	{
		Image image = new Image();
		HandleXmlElement_FrameworkElement(dialog, image, xmlElement);
		image.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
		image.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);
		if (xmlElement.Attribute("Source")?.Value != null)
		{
			try { image.Source = new BitmapImage(GetSourceData(dialog, "Source", xmlElement)); } catch { }
		}
		return image;
	}

	// SvgIcon replaced with WPF Image — SharpVectors not compatible with net8.
	private static Image HandleXmlElement_SvgIcon(CustomDialog dialog, XElement xmlElement)
	{
		Image image = new Image();
		HandleXmlElement_FrameworkElement(dialog, image, xmlElement);
		image.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
		image.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);
		return image;
	}

	private static Image HandleXmlElement_SvgBitmap(CustomDialog dialog, XElement xmlElement)
	{
		Image image = new Image();
		HandleXmlElement_FrameworkElement(dialog, image, xmlElement);
		image.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
		image.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);
		if (xmlElement.Attribute("Source")?.Value != null)
		{
			try { image.Source = new BitmapImage(GetSourceData(dialog, "Source", xmlElement)); } catch { }
		}
		return image;
	}

	private static void HandleXmlElement_Shape(CustomDialog dialog, Shape shape, XElement xmlElement)
	{
		HandleXmlElement_FrameworkElement(dialog, shape, xmlElement);
		ApplyBrush_UIElement(dialog, shape, "Fill", Shape.FillProperty, xmlElement);
		ApplyBrush_UIElement(dialog, shape, "Stroke", Shape.StrokeProperty, xmlElement);
		shape.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Fill);
		shape.StrokeDashCap = ParseXmlAttribute<PenLineCap>(xmlElement, "StrokeDashCap", PenLineCap.Flat);
		shape.StrokeDashOffset = ParseXmlAttribute<double>(xmlElement, "StrokeDashOffset", 0.0);
		shape.StrokeEndLineCap = ParseXmlAttribute<PenLineCap>(xmlElement, "StrokeEndLineCap", PenLineCap.Flat);
		shape.StrokeLineJoin = ParseXmlAttribute<PenLineJoin>(xmlElement, "StrokeLineJoin", PenLineJoin.Miter);
		shape.StrokeMiterLimit = ParseXmlAttribute<double>(xmlElement, "StrokeMiterLimit", 10.0);
		shape.StrokeStartLineCap = ParseXmlAttribute<PenLineCap>(xmlElement, "StrokeStartLineCap", PenLineCap.Flat);
		shape.StrokeThickness = ParseXmlAttribute<double>(xmlElement, "StrokeThickness", 1.0);
	}

	private static System.Windows.Shapes.Path HandleXmlElement_Path(CustomDialog dialog, XElement xmlElement)
	{
		System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
		HandleXmlElement_Shape(dialog, path, xmlElement);
		object geometryFromXElement = GetGeometryFromXElement(xmlElement, "Data");
		if (geometryFromXElement is Geometry)
		{
			path.Data = (Geometry)geometryFromXElement;
		}
		return path;
	}

	private static Ellipse HandleXmlElement_Ellipse(CustomDialog dialog, XElement xmlElement)
	{
		Ellipse ellipse = new Ellipse();
		HandleXmlElement_Shape(dialog, ellipse, xmlElement);
		return ellipse;
	}

	private static Line HandleXmlElement_Line(CustomDialog dialog, XElement xmlElement)
	{
		Line line = new Line();
		HandleXmlElement_Shape(dialog, line, xmlElement);
		line.X1 = ParseXmlAttribute<double>(xmlElement, "X1", 0.0);
		line.X2 = ParseXmlAttribute<double>(xmlElement, "X2", 0.0);
		line.Y1 = ParseXmlAttribute<double>(xmlElement, "Y1", 0.0);
		line.Y2 = ParseXmlAttribute<double>(xmlElement, "Y2", 0.0);
		return line;
	}

	private static Rectangle HandleXmlElement_Rectangle(CustomDialog dialog, XElement xmlElement)
	{
		Rectangle rectangle = new Rectangle();
		HandleXmlElement_Shape(dialog, rectangle, xmlElement);
		rectangle.RadiusX = ParseXmlAttribute<double>(xmlElement, "RadiusX", 0.0);
		rectangle.RadiusY = ParseXmlAttribute<double>(xmlElement, "RadiusY", 0.0);
		return rectangle;
	}

	private static void HandleXmlElement_FrameworkElement(CustomDialog dialog, FrameworkElement uiElement, XElement xmlElement)
	{
		string text = xmlElement.Attribute("Name")?.Value?.ToString();
		if (text != null)
		{
			if (dialog.UsedNames.Contains(text))
			{
				throw new Exception($"{xmlElement.Name} has duplicate name {text}");
			}
			dialog.UsedNames.Add(text);
		}
		uiElement.Name = text;
		uiElement.Visibility = ParseXmlAttribute<Visibility>(xmlElement, "Visibility", Visibility.Visible);
		uiElement.IsEnabled = ParseXmlAttribute<bool>(xmlElement, "IsEnabled", true);
		object thicknessFromXElement = GetThicknessFromXElement(xmlElement, "Margin");
		if (thicknessFromXElement != null)
		{
			uiElement.Margin = (Thickness)thicknessFromXElement;
		}
		uiElement.Height = ParseXmlAttribute<double>(xmlElement, "Height", double.NaN);
		uiElement.Width = ParseXmlAttribute<double>(xmlElement, "Width", double.NaN);
		uiElement.HorizontalAlignment = ParseXmlAttribute<System.Windows.HorizontalAlignment>(xmlElement, "HorizontalAlignment", System.Windows.HorizontalAlignment.Left);
		uiElement.VerticalAlignment = ParseXmlAttribute<VerticalAlignment>(xmlElement, "VerticalAlignment", VerticalAlignment.Top);
		uiElement.Opacity = ParseXmlAttribute<double>(xmlElement, "Opacity", 1.0);
		ApplyBrush_UIElement(dialog, uiElement, "OpacityMask", UIElement.OpacityMaskProperty, xmlElement);
		object pointFromXElement = GetPointFromXElement(xmlElement, "RenderTransformOrigin");
		if (pointFromXElement is Point)
		{
			uiElement.RenderTransformOrigin = (Point)pointFromXElement;
		}
		int value = ParseXmlAttributeClamped(xmlElement, "Panel.ZIndex", 0, 0, 1000);
		System.Windows.Controls.Panel.SetZIndex(uiElement, value);
		int value2 = ParseXmlAttribute<int>(xmlElement, "Grid.Row", 0);
		Grid.SetRow(uiElement, value2);
		int value3 = ParseXmlAttribute<int>(xmlElement, "Grid.RowSpan", 1);
		Grid.SetRowSpan(uiElement, value3);
		int value4 = ParseXmlAttribute<int>(xmlElement, "Grid.Column", 0);
		Grid.SetColumn(uiElement, value4);
		int value5 = ParseXmlAttribute<int>(xmlElement, "Grid.ColumnSpan", 1);
		Grid.SetColumnSpan(uiElement, value5);
		ApplyTransformations_UIElement(dialog, uiElement, xmlElement);
		ApplyEffects_UIElement(dialog, uiElement, xmlElement);
	}

	private static void HandleXmlElement_Control(CustomDialog dialog, System.Windows.Controls.Control uiElement, XElement xmlElement)
	{
		HandleXmlElement_FrameworkElement(dialog, uiElement, xmlElement);
		object thicknessFromXElement = GetThicknessFromXElement(xmlElement, "Padding");
		if (thicknessFromXElement != null)
		{
			uiElement.Padding = (Thickness)thicknessFromXElement;
		}
		object thicknessFromXElement2 = GetThicknessFromXElement(xmlElement, "BorderThickness");
		if (thicknessFromXElement2 != null)
		{
			uiElement.BorderThickness = (Thickness)thicknessFromXElement2;
		}
		ApplyBrush_UIElement(dialog, uiElement, "Foreground", System.Windows.Controls.Control.ForegroundProperty, xmlElement);
		ApplyBrush_UIElement(dialog, uiElement, "Background", System.Windows.Controls.Control.BackgroundProperty, xmlElement);
		ApplyBrush_UIElement(dialog, uiElement, "BorderBrush", System.Windows.Controls.Control.BorderBrushProperty, xmlElement);
		double? num = ParseXmlAttributeNullable<double>(xmlElement, "FontSize");
		if (num is double)
		{
			uiElement.FontSize = num.Value;
		}
		uiElement.FontWeight = GetFontWeightFromXElement(xmlElement);
		uiElement.FontStyle = GetFontStyleFromXElement(xmlElement);
		string fullPath = GetFullPath(dialog, xmlElement.Attribute("FontFamily")?.Value);
		if (fullPath != null)
		{
			uiElement.FontFamily = new System.Windows.Media.FontFamily(fullPath);
		}
	}

	private static UIElement HandleXmlElement_OrbitstrapCustomBootstrapper(CustomDialog dialog, XElement xmlElement)
	{
		xmlElement.SetAttributeValue("Visibility", "Collapsed");
		xmlElement.SetAttributeValue("IsEnabled", "True");
		HandleXmlElement_Control(dialog, dialog, xmlElement);
		dialog.AllowsTransparency = ParseXmlAttribute<bool>(xmlElement, "AllowsTransparency", true);
		dialog.WindowCornerPreference = ParseXmlAttribute<WindowCornerPreference>(xmlElement, "WindowCornerPreference", WindowCornerPreference.Default);
		dialog.WindowBackdropType = ParseXmlAttribute<BackgroundType>(xmlElement, "WindowBackdropType", BackgroundType.None);
		dialog.Opacity = 1.0;
		dialog.ElementGrid.RenderTransform = dialog.RenderTransform;
		dialog.RenderTransform = null;
		dialog.ElementGrid.LayoutTransform = dialog.LayoutTransform;
		dialog.LayoutTransform = null;
		dialog.ElementGrid.Effect = dialog.Effect;
		dialog.Effect = null;
		Orbitstrap.Enums.Theme theme = ParseXmlAttribute<Orbitstrap.Enums.Theme>(xmlElement, "Theme", Orbitstrap.Enums.Theme.Default);
		if (theme == Orbitstrap.Enums.Theme.Default)
		{
			theme = App.Settings.Prop.Theme;
		}
		ThemeType themeType = ((theme.GetFinal() == Orbitstrap.Enums.Theme.Dark) ? ThemeType.Dark : ThemeType.Light);
		dialog.Resources.MergedDictionaries.Clear();
		dialog.Resources.MergedDictionaries.Add(new ThemesDictionary
		{
			Theme = themeType
		});
		dialog.DefaultBorderThemeOverwrite = themeType;
		if (xmlElement.Attribute("BorderBrush") != null || xmlElement.Attribute("BorderThickness") != null)
		{
			dialog.DefaultBorderEnabled = false;
		}
		dialog.ElementGrid.Margin = dialog.Margin;
		dialog.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
		dialog.Padding = new Thickness(0.0, 0.0, 0.0, 0.0);
		string title = xmlElement.Attribute("Title")?.Value?.ToString() ?? "Orbitstrap";
		dialog.Title = title;
		if (ParseXmlAttribute<bool>(xmlElement, "IgnoreTitleBarInset", false))
		{
			Grid.SetRow(dialog.ElementGrid, 0);
			Grid.SetRowSpan(dialog.ElementGrid, 2);
		}
		return new DummyFrameworkElement();
	}

	private static UIElement HandleXmlElement_OrbitstrapCustomBootstrapper_Fake(CustomDialog dialog, XElement xmlElement)
	{
		throw new Exception($"{xmlElement.Parent.Name} cannot have a child of {xmlElement.Name}");
	}

	private static DummyFrameworkElement HandleXmlElement_TitleBar(CustomDialog dialog, XElement xmlElement)
	{
		xmlElement.SetAttributeValue("Name", "TitleBar");
		xmlElement.SetAttributeValue("IsEnabled", "True");
		HandleXmlElement_Control(dialog, dialog.RootTitleBar, xmlElement);
		dialog.RootTitleBar.RenderTransform = null;
		dialog.RootTitleBar.LayoutTransform = null;
		dialog.RootTitleBar.Effect = null;
		System.Windows.Controls.Panel.SetZIndex(dialog.RootTitleBar, 1001);
		dialog.RootTitleBar.Height = double.NaN;
		dialog.RootTitleBar.Width = double.NaN;
		dialog.RootTitleBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
		dialog.RootTitleBar.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
		dialog.RootTitleBar.ShowMinimize = ParseXmlAttribute<bool>(xmlElement, "ShowMinimize", true);
		dialog.RootTitleBar.ShowClose = ParseXmlAttribute<bool>(xmlElement, "ShowClose", true);
		string title = xmlElement.Attribute("Title")?.Value?.ToString() ?? "Orbitstrap";
		dialog.RootTitleBar.Title = title;
		return new DummyFrameworkElement();
	}

	private static UIElement HandleXmlElement_Button(CustomDialog dialog, XElement xmlElement)
	{
		System.Windows.Controls.Button button = new System.Windows.Controls.Button();
		HandleXmlElement_Control(dialog, button, xmlElement);
		button.Content = GetContentFromXElement(dialog, xmlElement);
		if (xmlElement.Attribute("Name")?.Value == "CancelButton")
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding("CancelEnabled")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(button, UIElement.IsEnabledProperty, binding);
			System.Windows.Data.Binding binding2 = new System.Windows.Data.Binding("CancelInstallCommand");
			BindingOperations.SetBinding(button, System.Windows.Controls.Primitives.ButtonBase.CommandProperty, binding2);
		}
		return button;
	}

	private static void HandleXmlElement_RangeBase(CustomDialog dialog, RangeBase rangeBase, XElement xmlElement)
	{
		HandleXmlElement_Control(dialog, rangeBase, xmlElement);
		rangeBase.Value = ParseXmlAttribute<double>(xmlElement, "Value", 0.0);
		rangeBase.Maximum = ParseXmlAttribute<double>(xmlElement, "Maximum", 100.0);
	}

	private static UIElement HandleXmlElement_ProgressBar(CustomDialog dialog, XElement xmlElement)
	{
		System.Windows.Controls.ProgressBar progressBar = new System.Windows.Controls.ProgressBar();
		HandleXmlElement_RangeBase(dialog, progressBar, xmlElement);
		progressBar.IsIndeterminate = ParseXmlAttribute<bool>(xmlElement, "IsIndeterminate", false);
		if (xmlElement.Attribute("Name")?.Value == "PrimaryProgressBar")
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding("ProgressIndeterminate")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(progressBar, System.Windows.Controls.ProgressBar.IsIndeterminateProperty, binding);
			System.Windows.Data.Binding binding2 = new System.Windows.Data.Binding("ProgressMaximum")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(progressBar, RangeBase.MaximumProperty, binding2);
			System.Windows.Data.Binding binding3 = new System.Windows.Data.Binding("ProgressValue")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(progressBar, RangeBase.ValueProperty, binding3);
		}
		return progressBar;
	}

	private static UIElement HandleXmlElement_ProgressRing(CustomDialog dialog, XElement xmlElement)
	{
		ProgressRing progressRing = new ProgressRing();
		progressRing.IsIndeterminate = ParseXmlAttribute<bool>(xmlElement, "IsIndeterminate", false);
		if (xmlElement.Attribute("Name")?.Value == "PrimaryProgressRing")
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding("ProgressIndeterminate")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(progressRing, ProgressRing.IsIndeterminateProperty, binding);
			// ProgressRing does not extend RangeBase — Maximum/Value bindings not applicable
		}
		return progressRing;
	}

	private static void HandleXmlElement_TextBlock_Base(CustomDialog dialog, TextBlock textBlock, XElement xmlElement)
	{
		HandleXmlElement_FrameworkElement(dialog, textBlock, xmlElement);
		ApplyBrush_UIElement(dialog, textBlock, "Foreground", TextBlock.ForegroundProperty, xmlElement);
		ApplyBrush_UIElement(dialog, textBlock, "Background", TextBlock.BackgroundProperty, xmlElement);
		double? num = ParseXmlAttributeNullable<double>(xmlElement, "FontSize");
		if (num is double)
		{
			textBlock.FontSize = num.Value;
		}
		textBlock.FontWeight = GetFontWeightFromXElement(xmlElement);
		textBlock.FontStyle = GetFontStyleFromXElement(xmlElement);
		textBlock.LineHeight = ParseXmlAttribute<double>(xmlElement, "LineHeight", double.NaN);
		textBlock.LineStackingStrategy = ParseXmlAttribute<LineStackingStrategy>(xmlElement, "LineStackingStrategy", LineStackingStrategy.MaxHeight);
		textBlock.TextAlignment = ParseXmlAttribute<TextAlignment>(xmlElement, "TextAlignment", TextAlignment.Center);
		textBlock.TextTrimming = ParseXmlAttribute<TextTrimming>(xmlElement, "TextTrimming", TextTrimming.None);
		textBlock.TextWrapping = ParseXmlAttribute<TextWrapping>(xmlElement, "TextWrapping", TextWrapping.NoWrap);
		textBlock.TextDecorations = GetTextDecorationsFromXElement(xmlElement);
		textBlock.IsHyphenationEnabled = ParseXmlAttribute<bool>(xmlElement, "IsHyphenationEnabled", false);
		textBlock.BaselineOffset = ParseXmlAttribute<double>(xmlElement, "BaselineOffset", double.NaN);
		string fullPath = GetFullPath(dialog, xmlElement.Attribute("FontFamily")?.Value);
		if (fullPath != null)
		{
			textBlock.FontFamily = new System.Windows.Media.FontFamily(fullPath);
		}
		object thicknessFromXElement = GetThicknessFromXElement(xmlElement, "Padding");
		if (thicknessFromXElement != null)
		{
			textBlock.Padding = (Thickness)thicknessFromXElement;
		}
	}

	private static UIElement HandleXmlElement_TextBlock(CustomDialog dialog, XElement xmlElement)
	{
		TextBlock textBlock = new TextBlock();
		HandleXmlElement_TextBlock_Base(dialog, textBlock, xmlElement);
		textBlock.Text = GetTranslatedText(xmlElement.Attribute("Text")?.Value);
		if (xmlElement.Attribute("Name")?.Value == "StatusText")
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Message")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, binding);
		}
		return textBlock;
	}

	private static UIElement HandleXmlElement_MarkdownTextBlock(CustomDialog dialog, XElement xmlElement)
	{
		MarkdownTextBlock markdownTextBlock = new MarkdownTextBlock();
		HandleXmlElement_TextBlock_Base(dialog, markdownTextBlock, xmlElement);
		string translatedText = GetTranslatedText(xmlElement.Attribute("Text")?.Value);
		if (translatedText != null)
		{
			markdownTextBlock.MarkdownText = translatedText;
		}
		return markdownTextBlock;
	}

	private static UIElement HandleXmlElement_Image(CustomDialog dialog, XElement xmlElement)
	{
		Image image = new Image();
		HandleXmlElement_FrameworkElement(dialog, image, xmlElement);
		image.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
		image.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);
		RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
		GetImageSourceDataResult imageSourceData = GetImageSourceData(dialog, "Source", xmlElement);
		if (imageSourceData.IsIcon)
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Icon")
			{
				Mode = BindingMode.OneWay
			};
			BindingOperations.SetBinding(image, Image.SourceProperty, binding);
		}
		else if (!ParseXmlAttribute<bool>(xmlElement, "IsAnimated", false))
		{
			BitmapImage source;
			try
			{
				source = new BitmapImage(imageSourceData.Uri);
			}
			catch (Exception ex)
			{
				throw new Exception("Image Failed to create BitmapImage: " + ex.Message, ex);
			}
			image.Source = source;
		}
		else
		{
			RepeatBehavior imageRepeatBehaviourData = GetImageRepeatBehaviourData(xmlElement);
			AnimationBehavior.SetRepeatBehavior(image, imageRepeatBehaviourData);
			AnimationBehavior.SetSourceUri(image, imageSourceData.Uri);
		}
		return image;
	}

	private static UIElement HandleXmlElement_MediaElement(CustomDialog dialog, XElement xmlElement)
	{
		MediaElement media = new MediaElement();
		HandleXmlElement_FrameworkElement(dialog, media, xmlElement);
		RenderOptions.SetBitmapScalingMode(media, BitmapScalingMode.HighQuality);
		media.LoadedBehavior = ParseXmlAttribute<MediaState>(xmlElement, "LoadedBehaviour", MediaState.Play);
		media.UnloadedBehavior = ParseXmlAttribute<MediaState>(xmlElement, "UnloadedBehaviour", MediaState.Close);
		media.Volume = ParseXmlAttribute<double>(xmlElement, "Volume", 0.5);
		media.Stretch = ParseXmlAttribute<Stretch>(xmlElement, "Stretch", Stretch.Uniform);
		media.StretchDirection = ParseXmlAttribute<StretchDirection>(xmlElement, "StretchDirection", StretchDirection.Both);
		media.Source = GetSourceData(dialog, "Source", xmlElement);
		if (ParseXmlAttribute<bool>(xmlElement, "Looped", true))
		{
			media.MediaEnded += delegate
			{
				media.Position = TimeSpan.Zero;
			};
		}
		return media;
	}

	private static RowDefinition HandleXmlElement_RowDefinition(CustomDialog dialog, XElement xmlElement)
	{
		RowDefinition rowDefinition = new RowDefinition();
		object gridLengthFromXElement = GetGridLengthFromXElement(xmlElement, "Height");
		if (gridLengthFromXElement != null)
		{
			rowDefinition.Height = (GridLength)gridLengthFromXElement;
		}
		rowDefinition.MinHeight = ParseXmlAttribute<double>(xmlElement, "MinHeight", 0.0);
		rowDefinition.MaxHeight = ParseXmlAttribute<double>(xmlElement, "MaxHeight", double.PositiveInfinity);
		return rowDefinition;
	}

	private static ColumnDefinition HandleXmlElement_ColumnDefinition(CustomDialog dialog, XElement xmlElement)
	{
		ColumnDefinition columnDefinition = new ColumnDefinition();
		object gridLengthFromXElement = GetGridLengthFromXElement(xmlElement, "Width");
		if (gridLengthFromXElement != null)
		{
			columnDefinition.Width = (GridLength)gridLengthFromXElement;
		}
		columnDefinition.MinWidth = ParseXmlAttribute<double>(xmlElement, "MinWidth", 0.0);
		columnDefinition.MaxWidth = ParseXmlAttribute<double>(xmlElement, "MaxWidth", double.PositiveInfinity);
		return columnDefinition;
	}

	private static void HandleXmlElement_Grid_RowDefinitions(Grid grid, CustomDialog dialog, XElement xmlElement)
	{
		foreach (XElement item in xmlElement.Elements())
		{
			RowDefinition value = HandleXml<RowDefinition>(dialog, item);
			grid.RowDefinitions.Add(value);
		}
	}

	private static void HandleXmlElement_Grid_ColumnDefinitions(Grid grid, CustomDialog dialog, XElement xmlElement)
	{
		foreach (XElement item in xmlElement.Elements())
		{
			ColumnDefinition value = HandleXml<ColumnDefinition>(dialog, item);
			grid.ColumnDefinitions.Add(value);
		}
	}

	private static Grid HandleXmlElement_Grid(CustomDialog dialog, XElement xmlElement)
	{
		Grid grid = new Grid();
		HandleXmlElement_FrameworkElement(dialog, grid, xmlElement);
		bool flag = false;
		bool flag2 = false;
		foreach (XElement item in xmlElement.Elements())
		{
			if (item.Name == "Grid.RowDefinitions")
			{
				if (flag)
				{
					throw new Exception("Grid can only have one RowDefinitions defined");
				}
				flag = true;
				HandleXmlElement_Grid_RowDefinitions(grid, dialog, item);
			}
			else if (item.Name == "Grid.ColumnDefinitions")
			{
				if (flag2)
				{
					throw new Exception("Grid can only have one ColumnDefinitions defined");
				}
				flag2 = true;
				HandleXmlElement_Grid_ColumnDefinitions(grid, dialog, item);
			}
			else if (!item.Name.ToString().StartsWith("Grid."))
			{
				FrameworkElement element = HandleXml<FrameworkElement>(dialog, item);
				grid.Children.Add(element);
			}
		}
		return grid;
	}

	private static StackPanel HandleXmlElement_StackPanel(CustomDialog dialog, XElement xmlElement)
	{
		StackPanel stackPanel = new StackPanel();
		HandleXmlElement_FrameworkElement(dialog, stackPanel, xmlElement);
		stackPanel.Orientation = ParseXmlAttribute<System.Windows.Controls.Orientation>(xmlElement, "Orientation", System.Windows.Controls.Orientation.Vertical);
		foreach (XElement item in xmlElement.Elements())
		{
			FrameworkElement element = HandleXml<FrameworkElement>(dialog, item);
			stackPanel.Children.Add(element);
		}
		return stackPanel;
	}

	private static Border HandleXmlElement_Border(CustomDialog dialog, XElement xmlElement)
	{
		Border border = new Border();
		HandleXmlElement_FrameworkElement(dialog, border, xmlElement);
		ApplyBrush_UIElement(dialog, border, "Background", Border.BackgroundProperty, xmlElement);
		ApplyBrush_UIElement(dialog, border, "BorderBrush", Border.BorderBrushProperty, xmlElement);
		object thicknessFromXElement = GetThicknessFromXElement(xmlElement, "BorderThickness");
		if (thicknessFromXElement != null)
		{
			border.BorderThickness = (Thickness)thicknessFromXElement;
		}
		object thicknessFromXElement2 = GetThicknessFromXElement(xmlElement, "Padding");
		if (thicknessFromXElement2 != null)
		{
			border.Padding = (Thickness)thicknessFromXElement2;
		}
		object cornerRadiusFromXElement = GetCornerRadiusFromXElement(xmlElement, "CornerRadius");
		if (cornerRadiusFromXElement != null)
		{
			border.CornerRadius = (CornerRadius)cornerRadiusFromXElement;
		}
		IEnumerable<XElement> source = from x in xmlElement.Elements()
			where !x.Name.ToString().StartsWith("Border.")
			select x;
		if (source.Any())
		{
			if (source.Count() > 1)
			{
				throw new Exception("Border can only have one child");
			}
			border.Child = HandleXml<UIElement>(dialog, source.FirstOrDefault());
		}
		return border;
	}

	private static string GetXmlAttribute(XElement element, string attributeName, string? defaultValue = null)
	{
		XAttribute xAttribute = element.Attribute(attributeName);
		if (xAttribute == null)
		{
			if (defaultValue != null)
			{
				return defaultValue;
			}
			throw new Exception($"Element {element.Name} is missing the {attributeName} attribute");
		}
		return xAttribute.Value.ToString();
	}

	private static T ParseXmlAttribute<T>(XElement element, string attributeName, T? defaultValue = null) where T : struct
	{
		XAttribute xAttribute = element.Attribute(attributeName);
		if (xAttribute == null)
		{
			if (defaultValue.HasValue)
			{
				return defaultValue.Value;
			}
			throw new Exception($"Element {element.Name} is missing the {attributeName} attribute");
		}
		T? val = ConvertValue<T>(xAttribute.Value);
		if (!val.HasValue)
		{
			throw new Exception($"{element.Name} {attributeName} is not a valid {typeof(T).Name}");
		}
		return val.Value;
	}

	private static T? ParseXmlAttributeNullable<T>(XElement element, string attributeName) where T : struct
	{
		XAttribute xAttribute = element.Attribute(attributeName);
		if (xAttribute == null)
		{
			return null;
		}
		T? val = ConvertValue<T>(xAttribute.Value);
		if (!val.HasValue)
		{
			throw new Exception($"{element.Name} {attributeName} is not a valid {typeof(T).Name}");
		}
		return val.Value;
	}

	private static void ValidateXmlElement(string elementName, string attributeName, int value, int? min = null, int? max = null)
	{
		if (min.HasValue && value < min)
		{
			throw new Exception($"{elementName} {attributeName} must be larger than {min}");
		}
		if (max.HasValue && value > max)
		{
			throw new Exception($"{elementName} {attributeName} must be smaller than {max}");
		}
	}

	private static void ValidateXmlElement(string elementName, string attributeName, double value, double? min = null, double? max = null)
	{
		if (min.HasValue && value < min)
		{
			throw new Exception($"{elementName} {attributeName} must be larger than {min}");
		}
		if (max.HasValue && value > max)
		{
			throw new Exception($"{elementName} {attributeName} must be smaller than {max}");
		}
	}

	private static int ParseXmlAttributeClamped(XElement element, string attributeName, int? defaultValue = null, int? min = null, int? max = null)
	{
		int num = ParseXmlAttribute(element, attributeName, defaultValue);
		ValidateXmlElement(element.Name.ToString(), attributeName, num, min, max);
		return num;
	}

	private static FontWeight GetFontWeightFromXElement(XElement element)
	{
		string text = element.Attribute("FontWeight")?.Value?.ToString();
		if (string.IsNullOrEmpty(text))
		{
			text = "Normal";
		}
		switch (text)
		{
		case "Thin":
			return FontWeights.Thin;
		case "ExtraLight":
		case "UltraLight":
			return FontWeights.ExtraLight;
		case "Medium":
			return FontWeights.Medium;
		case "Normal":
		case "Regular":
			return FontWeights.Normal;
		case "DemiBold":
		case "SemiBold":
			return FontWeights.DemiBold;
		case "Bold":
			return FontWeights.Bold;
		case "ExtraBold":
		case "UltraBold":
			return FontWeights.ExtraBold;
		case "Black":
		case "Heavy":
			return FontWeights.Black;
		case "ExtraBlack":
		case "UltraBlack":
			return FontWeights.UltraBlack;
		default:
			throw new Exception($"{element.Name} Unknown FontWeight {text}");
		}
	}

	private static FontStyle GetFontStyleFromXElement(XElement element)
	{
		string text = element.Attribute("FontStyle")?.Value?.ToString();
		if (string.IsNullOrEmpty(text))
		{
			text = "Normal";
		}
		return text switch
		{
			"Normal" => FontStyles.Normal, 
			"Italic" => FontStyles.Italic, 
			"Oblique" => FontStyles.Oblique, 
			_ => throw new Exception($"{element.Name} Unknown FontStyle {text}"), 
		};
	}

	private static TextDecorationCollection? GetTextDecorationsFromXElement(XElement element)
	{
		string text = element.Attribute("TextDecorations")?.Value?.ToString();
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return text switch
		{
			"Baseline" => TextDecorations.Baseline, 
			"OverLine" => TextDecorations.OverLine, 
			"Strikethrough" => TextDecorations.Strikethrough, 
			"Underline" => TextDecorations.Underline, 
			_ => throw new Exception($"{element.Name} Unknown TextDecorations {text}"), 
		};
	}

	private static string? GetTranslatedText(string? text)
	{
		if (text == null || !text.StartsWith('{') || !text.EndsWith('}'))
		{
			return text;
		}
		string name = text.Substring(1, text.Length - 1 - 1);
		return Strings.ResourceManager.GetStringSafe(name);
	}

	private static string? GetFullPath(CustomDialog dialog, string? sourcePath)
	{
		return sourcePath?.Replace("theme://", dialog.ThemeDir + "\\");
	}

	private static GetImageSourceDataResult GetImageSourceData(CustomDialog dialog, string name, XElement xmlElement)
	{
		string xmlAttribute = GetXmlAttribute(xmlElement, name);
		GetImageSourceDataResult result;
		if (xmlAttribute == "{Icon}")
		{
			result = new GetImageSourceDataResult();
			result.IsIcon = true;
			return result;
		}
		xmlAttribute = GetFullPath(dialog, xmlAttribute);
		if (!Uri.TryCreate(xmlAttribute, UriKind.RelativeOrAbsolute, out Uri result2))
		{
			throw new Exception($"{xmlElement.Name} failed to parse {name} as Uri");
		}
		if (result2 == null)
		{
			throw new Exception($"{xmlElement.Name} {name} Uri is null");
		}
		if (result2.Scheme != "file")
		{
			throw new Exception($"{xmlElement.Name} most be linked to a file");
		}
		result = new GetImageSourceDataResult();
		result.Uri = result2;
		return result;
	}

	private static Uri GetSourceData(CustomDialog dialog, string name, XElement xmlElement)
	{
		string xmlAttribute = GetXmlAttribute(xmlElement, name);
		xmlAttribute = GetFullPath(dialog, xmlAttribute);
		if (!Uri.TryCreate(xmlAttribute, UriKind.RelativeOrAbsolute, out Uri result))
		{
			throw new Exception($"{xmlElement.Name} failed to parse Source as Uri");
		}
		if (result == null)
		{
			throw new Exception($"{xmlElement.Name} Source Uri is null");
		}
		return result;
	}

	private static RepeatBehavior GetImageRepeatBehaviourData(XElement element)
	{
		string text = element.Attribute("RepeatBehaviour")?.Value?.ToString();
		RepeatBehavior result = RepeatBehavior.Forever;
		if (string.IsNullOrEmpty(text) || text == "Forever")
		{
			return result;
		}
		Match match = new Regex("([0-9]+)x").Match(text);
		Match match2 = new Regex("[0-9][0-9]:[0-9][0-9]:[0-9][0-9]").Match(text);
		if (match.Success)
		{
			int.TryParse(match.Groups[1].Value, out var result2);
			result = new RepeatBehavior(result2);
		}
		if (match2.Success)
		{
			TimeSpan duration = TimeSpan.Parse(text);
			result = new RepeatBehavior(duration);
		}
		return result;
	}

	private static object? GetContentFromXElement(CustomDialog dialog, XElement xmlElement)
	{
		XAttribute xAttribute = xmlElement.Attribute("Content");
		XElement xElement = xmlElement.Element($"{xmlElement.Name}.Content");
		if (xAttribute != null && xElement != null)
		{
			throw new Exception($"{xmlElement.Name} can only have one Content defined");
		}
		if (xAttribute != null)
		{
			return GetTranslatedText(xAttribute.Value);
		}
		if (xElement == null)
		{
			return null;
		}
		if (xElement.Elements().Count() > 1)
		{
			throw new Exception($"{xmlElement.Name}.Content can only have one child");
		}
		if (!(xElement.FirstNode is XElement xmlElement2))
		{
			throw new Exception($"{xmlElement.Name} Content is missing the content");
		}
		return HandleXml<UIElement>(dialog, xmlElement2);
	}

	private static void ApplyEffects_UIElement(CustomDialog dialog, UIElement uiElement, XElement xmlElement)
	{
		XElement xElement = xmlElement.Element($"{xmlElement.Name}.Effect");
		if (xElement != null)
		{
			IEnumerable<XElement> source = xElement.Elements();
			if (source.Count() > 1)
			{
				throw new Exception($"{xmlElement.Name}.Effect can only have one child");
			}
			XElement xElement2 = source.FirstOrDefault();
			if (xElement2 != null)
			{
				Effect effect = HandleXml<Effect>(dialog, xElement2);
				uiElement.Effect = effect;
			}
		}
	}

	private static void ApplyTransformation_UIElement(CustomDialog dialog, string name, DependencyProperty property, UIElement uiElement, XElement xmlElement)
	{
		XElement xElement = xmlElement.Element($"{xmlElement.Name}.{name}");
		if (xElement == null)
		{
			return;
		}
		TransformGroup transformGroup = new TransformGroup();
		foreach (XElement item in xElement.Elements())
		{
			Transform value = HandleXml<Transform>(dialog, item);
			transformGroup.Children.Add(value);
		}
		uiElement.SetValue(property, transformGroup);
	}

	private static void ApplyTransformations_UIElement(CustomDialog dialog, UIElement uiElement, XElement xmlElement)
	{
		ApplyTransformation_UIElement(dialog, "RenderTransform", UIElement.RenderTransformProperty, uiElement, xmlElement);
		ApplyTransformation_UIElement(dialog, "LayoutTransform", FrameworkElement.LayoutTransformProperty, uiElement, xmlElement);
	}

	public CustomDialog()
	{
		InitializeComponent();
		_viewModel = new BootstrapperDialogViewModel(this);
		base.DataContext = _viewModel;
		base.Title = App.Settings.Prop.BootstrapperTitle;
		base.Icon = App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();
	}

	private void UiWindow_Closing(object sender, CancelEventArgs e)
	{
		if (!_isClosing)
		{
			Bootstrapper?.Cancel();
		}
	}

	public void ShowBootstrapper()
	{
		ShowDialog();
	}

	public void CloseBootstrapper()
	{
		_isClosing = true;
		base.Dispatcher.BeginInvoke(new Action(base.Close));
	}

	public void ShowSuccess(string message, Action? callback)
	{
		BaseFunctions.ShowSuccess(message, callback);
	}
}
