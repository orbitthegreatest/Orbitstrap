using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Orbitstrap.Models;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Dialogs;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class FastFlagEditorPage : UiPage, IComponentConnector
{
	private readonly ObservableCollection<FastFlag> _fastFlagList = new ObservableCollection<FastFlag>();

	private bool _showPresets;

	private string _searchFilter = "";

	public FastFlagEditorPage()
	{
		InitializeComponent();
	}

	private void ReloadList()
	{
		FastFlag selectedEntry = DataGrid.SelectedItem as FastFlag;
		_fastFlagList.Clear();
		IEnumerable<string> values = FastFlagManager.PresetFlags.Values;
		foreach (KeyValuePair<string, object> item2 in App.FastFlags.Prop.OrderBy<KeyValuePair<string, object>, string>((KeyValuePair<string, object> x) => x.Key))
		{
			if ((_showPresets || !values.Contains(item2.Key)) && item2.Key.ToLower().Contains(_searchFilter.ToLower()))
			{
				FastFlag item = new FastFlag
				{
					Name = item2.Key,
					Value = item2.Value.ToString()
				};
				_fastFlagList.Add(item);
			}
		}
		if (DataGrid.ItemsSource == null)
		{
			DataGrid.ItemsSource = _fastFlagList;
		}
		if (selectedEntry != null)
		{
			FastFlag fastFlag = _fastFlagList.Where((FastFlag x) => x.Name == selectedEntry.Name).FirstOrDefault();
			if (fastFlag != null)
			{
				DataGrid.SelectedItem = fastFlag;
				DataGrid.ScrollIntoView(fastFlag);
			}
		}
	}

	private void ClearSearch(bool refresh = true)
	{
		SearchTextBox.Text = "";
		_searchFilter = "";
		if (refresh)
		{
			ReloadList();
		}
	}

	private void ShowAddDialog()
	{
		AddFastFlagDialog addFastFlagDialog = new AddFastFlagDialog();
		addFastFlagDialog.ShowDialog();
		if (addFastFlagDialog.Result == MessageBoxResult.OK)
		{
			if (addFastFlagDialog.Tabs.SelectedIndex == 0)
			{
				AddSingle(addFastFlagDialog.FlagNameTextBox.Text.Trim(), addFastFlagDialog.FlagValueTextBox.Text);
			}
			else if (addFastFlagDialog.Tabs.SelectedIndex == 1)
			{
				ImportJSON(addFastFlagDialog.JsonTextBox.Text);
			}
		}
	}

	private void AddSingle(string name, string value)
	{
		FastFlag fastFlag;
		if (App.FastFlags.GetValue(name) == null)
		{
			fastFlag = new FastFlag
			{
				Name = name,
				Value = value
			};
			if (!name.Contains(_searchFilter))
			{
				ClearSearch();
			}
			_fastFlagList.Add(fastFlag);
			App.FastFlags.SetValue(fastFlag.Name, fastFlag.Value);
		}
		else
		{
			Frontend.ShowMessageBox(Strings.Menu_FastFlagEditor_AlreadyExists, MessageBoxImage.Asterisk);
			bool flag = false;
			if (!_showPresets && FastFlagManager.PresetFlags.Values.Contains(name))
			{
				TogglePresetsButton.IsChecked = true;
				_showPresets = true;
				flag = true;
			}
			if (!name.Contains(_searchFilter))
			{
				ClearSearch(refresh: false);
				flag = true;
			}
			if (flag)
			{
				ReloadList();
			}
			fastFlag = _fastFlagList.Where((FastFlag x) => x.Name == name).FirstOrDefault();
		}
		DataGrid.SelectedItem = fastFlag;
		DataGrid.ScrollIntoView(fastFlag);
	}

	private void ImportJSON(string json)
	{
		Dictionary<string, object> list = null;
		json = json.Trim();
		if (!json.StartsWith('{'))
		{
			json = "{" + json;
		}
		if (!json.EndsWith('}'))
		{
			int num = json.LastIndexOf('}');
			json = ((num != -1) ? json.Substring(0, num + 1) : (json + "}"));
		}
		try
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true
			};
			list = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
			if (list == null)
			{
				throw new Exception("JSON deserialization returned null");
			}
		}
		catch (Exception ex)
		{
			Frontend.ShowMessageBox(string.Format(Strings.Menu_FastFlagEditor_InvalidJSON, ex.Message), MessageBoxImage.Hand);
			ShowAddDialog();
			return;
		}
		if (list.Count > 16 && Frontend.ShowMessageBox(Strings.Menu_FastFlagEditor_LargeConfig, MessageBoxImage.Exclamation, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
		{
			return;
		}
		IEnumerable<string> source = from x in App.FastFlags.Prop
			where list.ContainsKey(x.Key)
			select x.Key;
		bool flag = false;
		if (source.Any())
		{
			int num2 = source.Count();
			string text = string.Format(Strings.Menu_FastFlagEditor_ConflictingImport, num2, string.Join(", ", source.Take(25)));
			if (num2 > 25)
			{
				text += "...";
			}
			flag = Frontend.ShowMessageBox(text, MessageBoxImage.Question, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
		}
		foreach (KeyValuePair<string, object> item in list)
		{
			if ((!App.FastFlags.Prop.ContainsKey(item.Key) || flag) && item.Value != null && item.Value.ToString() != null)
			{
				App.FastFlags.SetValue(item.Key, item.Value);
			}
		}
		ClearSearch();
	}

	private void Page_Loaded(object sender, RoutedEventArgs e)
	{
		ReloadList();
	}

	private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
	{
		if (!(e.Row.DataContext is FastFlag fastFlag) || !(e.EditingElement is System.Windows.Controls.TextBox textBox))
		{
			return;
		}
		switch (e.Column.Header as string)
		{
		case "Name":
		{
			string name = fastFlag.Name;
			string text2 = textBox.Text;
			if (text2 == name)
			{
				break;
			}
			if (App.FastFlags.GetValue(text2) != null)
			{
				Frontend.ShowMessageBox(Strings.Menu_FastFlagEditor_AlreadyExists, MessageBoxImage.Asterisk);
				e.Cancel = true;
				textBox.Text = name;
				break;
			}
			App.FastFlags.SetValue(name, null);
			App.FastFlags.SetValue(text2, fastFlag.Value);
			if (!text2.Contains(_searchFilter))
			{
				ClearSearch();
			}
			fastFlag.Name = text2;
			break;
		}
		case "Value":
		{
			_ = fastFlag.Value;
			string text = textBox.Text;
			App.FastFlags.SetValue(fastFlag.Name, text);
			break;
		}
		}
	}

	private void BackButton_Click(object sender, RoutedEventArgs e)
	{
		if (Window.GetWindow(this) is INavigationWindow navigationWindow)
		{
			navigationWindow.Navigate(typeof(FastFlagsPage));
		}
	}

	private void AddButton_Click(object sender, RoutedEventArgs e)
	{
		ShowAddDialog();
	}

	private void DeleteButton_Click(object sender, RoutedEventArgs e)
	{
		List<FastFlag> list = new List<FastFlag>();
		foreach (FastFlag selectedItem in DataGrid.SelectedItems)
		{
			list.Add(selectedItem);
		}
		foreach (FastFlag item in list)
		{
			_fastFlagList.Remove(item);
			App.FastFlags.SetValue(item.Name, null);
		}
	}

	private void ToggleButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is ToggleButton toggleButton)
		{
			_showPresets = toggleButton.IsChecked == true;
			ReloadList();
		}
	}

	private void ExportJSONButton_Click(object sender, RoutedEventArgs e)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		MessageBoxResult messageBoxResult = Frontend.ShowMessageBox(Strings.Menu_FastFlagEditor_ExportJson_IncludePresets, MessageBoxImage.Question, MessageBoxButton.YesNo);
		foreach (KeyValuePair<string, object> item in App.FastFlags.Prop)
		{
			if (!App.FastFlags.IsPreset(item.Key) || messageBoxResult == MessageBoxResult.Yes)
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		Clipboard.SetDataObject(JsonSerializer.Serialize(dictionary, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
		Frontend.ShowMessageBox(Strings.Menu_FastFlagEditor_JsonCopiedToClipboard, MessageBoxImage.Asterisk);
	}

	private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is System.Windows.Controls.TextBox textBox)
		{
			_searchFilter = textBox.Text;
			ReloadList();
		}
	}
}
