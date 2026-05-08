using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using Orbitstrap.Extensions;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.Editor;
using Wpf.Ui.Common;

namespace Orbitstrap.UI.Elements.Editor;

public partial class BootstrapperEditorWindow : WpfUiWindow, IComponentConnector
{
	private static class CustomBootstrapperSchema
	{
		private class Schema
		{
			public Dictionary<string, Element> Elements { get; set; } = new Dictionary<string, Element>();

			public Dictionary<string, Type> Types { get; set; } = new Dictionary<string, Type>();
		}

		private class Element
		{
			public string? SuperClass { get; set; }

			public bool IsCreatable { get; set; }

			public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
		}

		public class Type
		{
			public bool CanHaveElement { get; set; }

			public List<string>? Values { get; set; }
		}

		private static Schema? _schema;

		public static SortedDictionary<string, SortedDictionary<string, string>> ElementInfo { get; set; } = new SortedDictionary<string, SortedDictionary<string, string>>();

		public static Dictionary<string, List<string>> PropertyElements { get; set; } = new Dictionary<string, List<string>>();

		public static SortedDictionary<string, Type> Types { get; set; } = new SortedDictionary<string, Type>();

		public static void ParseSchema()
		{
			if (_schema != null)
			{
				return;
			}
			string schemaJson = Resource.GetString("CustomBootstrapperSchema.json").Result;
			if (string.IsNullOrEmpty(schemaJson)) { _schema = new Schema(); return; }
			_schema = JsonSerializer.Deserialize<Schema>(schemaJson);
			if (_schema == null)
			{
				throw new Exception("Deserialised CustomBootstrapperSchema is null");
			}
			foreach (KeyValuePair<string, Type> type in _schema.Types)
			{
				Types.Add(type.Key, type.Value);
			}
			PopulateElementInfo();
		}

		private static (SortedDictionary<string, string>, List<string>) GetElementAttributes(string name, Element element)
		{
			if (ElementInfo.ContainsKey(name))
			{
				return (ElementInfo[name], PropertyElements[name]);
			}
			List<string> list = new List<string>();
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			foreach (KeyValuePair<string, string> attribute in element.Attributes)
			{
				sortedDictionary.Add(attribute.Key, attribute.Value);
				if (!Types.ContainsKey(attribute.Value))
				{
					throw new Exception("Schema for type " + attribute.Value + " is missing. Blame Matt!");
				}
				if (Types[attribute.Value].CanHaveElement)
				{
					list.Add(attribute.Key);
				}
			}
			if (element.SuperClass != null)
			{
				var (sortedDictionary2, list2) = GetElementAttributes(element.SuperClass, _schema.Elements[element.SuperClass]);
				foreach (KeyValuePair<string, string> item in sortedDictionary2)
				{
					sortedDictionary.Add(item.Key, item.Value);
				}
				foreach (string item2 in list2)
				{
					list.Add(item2);
				}
			}
			list.Sort();
			ElementInfo[name] = sortedDictionary;
			PropertyElements[name] = list;
			return (sortedDictionary, list);
		}

		private static void PopulateElementInfo()
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, Element> element in _schema.Elements)
			{
				GetElementAttributes(element.Key, element.Value);
				if (!element.Value.IsCreatable)
				{
					list.Add(element.Key);
				}
			}
			foreach (string item in list)
			{
				ElementInfo.Remove(item);
			}
		}
	}

	private BootstrapperEditorWindowViewModel _viewModel;

	private CompletionWindow? _completionWindow;

	public BootstrapperEditorWindow(string name)
	{
		CustomBootstrapperSchema.ParseSchema();
		string text = Path.Combine(Paths.CustomThemes, name);
		string text2 = File.ReadAllText(Path.Combine(text, "Theme.xml"));
		text2 = ToCRLF(text2);
		_viewModel = new BootstrapperEditorWindowViewModel();
		_viewModel.ThemeSavedCallback = ThemeSavedCallback;
		_viewModel.Directory = text;
		_viewModel.Name = name;
		_viewModel.Title = string.Format(Strings.CustomTheme_Editor_Title, name);
		_viewModel.Code = text2;
		base.DataContext = _viewModel;
		InitializeComponent();
		UIXML.Text = _viewModel.Code;
		UIXML.TextChanged += OnCodeChanged;
		UIXML.TextArea.TextEntered += OnTextAreaTextEntered;
		LoadHighlightingTheme();
	}

	private void LoadHighlightingTheme()
	{
		using Stream? input = Resource.GetStream($"Editor-Theme-{App.Settings.Prop.Theme.GetFinal()}.xshd");
		if (input == null) return; // theme xshd not embedded — skip highlighting, editor still works
		using XmlReader reader = XmlReader.Create(input);
		UIXML.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
		UIXML.TextArea.TextView.SetResourceReference(TextView.LinkTextForegroundBrushProperty, "NewTextEditorLink");
	}

	private void ThemeSavedCallback(bool success, string message)
	{
		if (success)
		{
			Snackbar.Show(Strings.CustomTheme_Editor_Save_Success, message, SymbolRegular.CheckmarkCircle24, ControlAppearance.Success);
		}
		else
		{
			Snackbar.Show(Strings.CustomTheme_Editor_Save_Error, message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
		}
	}

	private static string ToCRLF(string text)
	{
		return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
	}

	private void OnCodeChanged(object? sender, EventArgs e)
	{
		_viewModel.Code = UIXML.Text;
		_viewModel.CodeChanged = true;
	}

	private void OnClosing(object sender, CancelEventArgs e)
	{
		if (_viewModel.CodeChanged)
		{
			switch (Frontend.ShowMessageBox(string.Format(Strings.CustomTheme_Editor_ConfirmSave, _viewModel.Name), MessageBoxImage.Asterisk, MessageBoxButton.YesNoCancel))
			{
			case MessageBoxResult.Cancel:
				e.Cancel = true;
				break;
			case MessageBoxResult.Yes:
				_viewModel.SaveCommand.Execute(null);
				break;
			}
		}
	}

	private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
	{
		switch (e.Text)
		{
		case "<":
			OpenElementAutoComplete();
			break;
		case " ":
			OpenAttributeAutoComplete();
			break;
		case ".":
			OpenPropertyElementAutoComplete();
			break;
		case "/":
			AddEndTag();
			break;
		case ">":
			CloseCompletionWindow();
			break;
		case "!":
			CloseCompletionWindow();
			break;
		}
	}

	private (string, int) GetLineAndPosAtCaretPosition()
	{
		int num = UIXML.CaretOffset - 1;
		int num2 = UIXML.Text.LastIndexOf('\n', num);
		int num3 = UIXML.Text.IndexOf('\n', num);
		string item;
		int item2;
		if (num2 == -1 && num3 == -1)
		{
			item = UIXML.Text;
			item2 = num;
		}
		else if (num2 == -1)
		{
			item = UIXML.Text.Substring(0, num3 - 1);
			item2 = num;
		}
		else if (num3 == -1)
		{
			string text = UIXML.Text;
			int num4 = num2 + 1;
			item = text.Substring(num4, text.Length - num4);
			item2 = num - num2 - 2;
		}
		else
		{
			string text2 = UIXML.Text;
			int num4 = num2 + 1;
			item = text2.Substring(num4, num3 - 1 - num4);
			item2 = num - num2 - 2;
		}
		return (item, item2);
	}

	public static string? GetElementAtCursor(string xml, int offset, bool onlyAllowInside = false)
	{
		if (offset == xml.Length)
		{
			offset--;
		}
		int num = xml.LastIndexOf('<', offset);
		if (num < 0)
		{
			return null;
		}
		if (num < xml.Length && xml[num + 1] == '/')
		{
			num++;
		}
		int num2 = xml.IndexOf(' ', num);
		if (num2 == -1)
		{
			num2 = int.MaxValue;
		}
		int num3 = xml.IndexOf('>', num);
		if (num3 == -1)
		{
			num3 = int.MaxValue;
		}
		else
		{
			if (onlyAllowInside && num3 < offset)
			{
				return null;
			}
			if (num3 < xml.Length && xml[num3 - 1] == '/')
			{
				num3--;
			}
		}
		int num4 = Math.Min(num2, num3);
		if (num3 > 0 && num3 < int.MaxValue && num4 > num)
		{
			string text = xml.Substring(num + 1, num4 - num - 1);
			if (!(text == "!--"))
			{
				return text;
			}
			return null;
		}
		return null;
	}

	private string? GetElementAtCursorNoSpaces(string xml, int offset)
	{
		(string, int) lineAndPosAtCaretPosition = GetLineAndPosAtCaretPosition();
		string item = lineAndPosAtCaretPosition.Item1;
		int num = lineAndPosAtCaretPosition.Item2;
		string text = "";
		while (num != -1)
		{
			char c = item[num];
			switch (c)
			{
			case '\t':
			case ' ':
				return null;
			case '<':
				return text;
			}
			text = c + text;
			num--;
		}
		return null;
	}

	private string? ShowAttributesForElementName()
	{
		var (text, num) = GetLineAndPosAtCaretPosition();
		if (text.Count((char x) => x == '"') % 2 == 0)
		{
			int num2 = -1;
			int num3 = num;
			int num4 = text.Length - 1;
			while (num3 != -1)
			{
				num2++;
				num3 = ((num4 <= num3 + 1) ? (-1) : text.IndexOf('"', num3 + 1));
			}
			if (num2 % 2 != 0)
			{
				return null;
			}
		}
		return GetElementAtCursor(UIXML.Text, UIXML.CaretOffset, onlyAllowInside: true);
	}

	private void AddEndTag()
	{
		CloseCompletionWindow();
		if (UIXML.Text.Length > 2 && UIXML.Text[UIXML.CaretOffset - 2] == '<')
		{
			string elementAtCursor = GetElementAtCursor(UIXML.Text, UIXML.CaretOffset - 3);
			if (elementAtCursor != null)
			{
				UIXML.TextArea.Document.Insert(UIXML.CaretOffset, elementAtCursor + ">");
			}
		}
		else if ((UIXML.Text.Length <= UIXML.CaretOffset || UIXML.Text[UIXML.CaretOffset] != '>') && ShowAttributesForElementName() != null)
		{
			UIXML.TextArea.Document.Insert(UIXML.CaretOffset, ">");
		}
	}

	private void OpenElementAutoComplete()
	{
		List<ICompletionData> list = new List<ICompletionData>();
		foreach (string key in CustomBootstrapperSchema.ElementInfo.Keys)
		{
			list.Add(new ElementCompletionData(key));
		}
		ShowCompletionWindow(list);
	}

	private void OpenAttributeAutoComplete()
	{
		string text = ShowAttributesForElementName();
		if (text == null)
		{
			CloseCompletionWindow();
			return;
		}
		if (!CustomBootstrapperSchema.ElementInfo.ContainsKey(text))
		{
			CloseCompletionWindow();
			return;
		}
		SortedDictionary<string, string> sortedDictionary = CustomBootstrapperSchema.ElementInfo[text];
		List<ICompletionData> list = new List<ICompletionData>();
		foreach (KeyValuePair<string, string> attribute in sortedDictionary)
		{
			list.Add(new AttributeCompletionData(attribute.Key, delegate
			{
				OpenTypeValueAutoComplete(attribute.Value);
			}));
		}
		ShowCompletionWindow(list);
	}

	private void OpenTypeValueAutoComplete(string typeName)
	{
		List<string> values = CustomBootstrapperSchema.Types[typeName].Values;
		if (values == null)
		{
			return;
		}
		List<ICompletionData> list = new List<ICompletionData>();
		foreach (string item in values)
		{
			list.Add(new TypeValueCompletionData(item));
		}
		ShowCompletionWindow(list);
	}

	private void OpenPropertyElementAutoComplete()
	{
		string elementAtCursorNoSpaces = GetElementAtCursorNoSpaces(UIXML.Text, UIXML.CaretOffset);
		if (elementAtCursorNoSpaces == null)
		{
			CloseCompletionWindow();
			return;
		}
		if (!CustomBootstrapperSchema.PropertyElements.ContainsKey(elementAtCursorNoSpaces))
		{
			CloseCompletionWindow();
			return;
		}
		List<string> list = CustomBootstrapperSchema.PropertyElements[elementAtCursorNoSpaces];
		List<ICompletionData> list2 = new List<ICompletionData>();
		foreach (string item in list)
		{
			list2.Add(new TypeValueCompletionData(item));
		}
		ShowCompletionWindow(list2);
	}

	private void CloseCompletionWindow()
	{
		if (_completionWindow != null)
		{
			_completionWindow.Close();
			_completionWindow = null;
		}
	}

	private void ShowCompletionWindow(List<ICompletionData> completionData)
	{
		CloseCompletionWindow();
		if (!completionData.Any())
		{
			return;
		}
		_completionWindow = new CompletionWindow(UIXML.TextArea);
		IList<ICompletionData> completionData2 = _completionWindow.CompletionList.CompletionData;
		foreach (ICompletionData completionDatum in completionData)
		{
			completionData2.Add(completionDatum);
		}
		_completionWindow.Show();
		_completionWindow.Closed += delegate
		{
			_completionWindow = null;
		};
	}
}
