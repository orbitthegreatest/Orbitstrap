using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.Dialogs;
using Orbitstrap.Utility;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class AddCustomThemeDialog : WpfUiWindow, IComponentConnector
{
	private const int CreateNewTabId = 0;

	private const int ImportTabId = 1;

	private readonly AddCustomThemeViewModel _viewModel;

	public bool Created { get; private set; }

	public string ThemeName { get; private set; } = "";

	public bool OpenEditor { get; private set; }

	public AddCustomThemeDialog()
	{
		_viewModel = new AddCustomThemeViewModel();
		_viewModel.Name = GenerateRandomName();
		base.DataContext = _viewModel;
		InitializeComponent();
	}

	private static string GetThemePath(string name)
	{
		return Path.Combine(Paths.CustomThemes, name, "Theme.xml");
	}

	private static string GenerateRandomName()
	{
		int num = Directory.GetDirectories(Paths.CustomThemes).Count() + 1;
		string text = string.Format(Strings.CustomTheme_DefaultName, num);
		if (File.Exists(GetThemePath(text)))
		{
			text = string.Format(Strings.CustomTheme_DefaultName, $"{num}-{Random.Shared.Next(1, 100000)}");
		}
		return text;
	}

	private static string GetUniqueName(string name)
	{
		if (!File.Exists(GetThemePath(name)))
		{
			return name;
		}
		for (int i = 1; i <= 100; i++)
		{
			string text = $"{name}_{i}";
			if (!File.Exists(GetThemePath(text)))
			{
				return text;
			}
		}
		return $"{name}_{Random.Shared.Next(101, 1000000)}";
	}

	private static void CreateCustomTheme(string name, CustomThemeTemplate template)
	{
		string text = Path.Combine(Paths.CustomThemes, name);
		if (Directory.Exists(text))
		{
			Directory.Delete(text, recursive: true);
		}
		Directory.CreateDirectory(text);
		string path = Path.Combine(text, "Theme.xml");
		string fileContents = template.GetFileContents();
		File.WriteAllText(path, fileContents);
	}

	private bool ValidateCreateNew()
	{
		if (string.IsNullOrEmpty(_viewModel.Name))
		{
			_viewModel.NameError = Strings.CustomTheme_Add_Errors_NameEmpty;
			return false;
		}
		PathValidator.ValidationResult validationResult = PathValidator.IsFileNameValid(_viewModel.Name);
		if (validationResult != PathValidator.ValidationResult.Ok)
		{
			switch (validationResult)
			{
			case PathValidator.ValidationResult.IllegalCharacter:
				_viewModel.NameError = Strings.CustomTheme_Add_Errors_NameIllegalCharacters;
				break;
			case PathValidator.ValidationResult.ReservedFileName:
				_viewModel.NameError = Strings.CustomTheme_Add_Errors_NameReserved;
				break;
			default:
				App.Logger.WriteLine("AddCustomThemeDialog::ValidateCreateNew", $"Got unhandled PathValidator::ValidationResult {validationResult}");
				_viewModel.NameError = Strings.CustomTheme_Add_Errors_Unknown;
				break;
			}
			return false;
		}
		if (File.Exists(Path.Combine(Paths.CustomThemes, _viewModel.Name, "Theme.xml")))
		{
			_viewModel.NameError = Strings.CustomTheme_Add_Errors_NameTaken;
			return false;
		}
		return true;
	}

	private bool ValidateImport()
	{
		if (!_viewModel.FilePath.EndsWith(".zip"))
		{
			_viewModel.FileError = Strings.CustomTheme_Add_Errors_FileNotZip;
			return false;
		}
		try
		{
			using ZipArchive zipArchive = System.IO.Compression.ZipFile.OpenRead(_viewModel.FilePath);
			ReadOnlyCollection<ZipArchiveEntry> entries = zipArchive.Entries;
			bool flag = false;
			foreach (ZipArchiveEntry item in entries)
			{
				if (item.FullName == "Theme.xml")
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_viewModel.FileError = Strings.CustomTheme_Add_Errors_ZipMissingThemeFile;
				return false;
			}
			return true;
		}
		catch (InvalidDataException ex)
		{
			App.Logger.WriteLine("AddCustomThemeDialog::ValidateImport", "Got invalid data");
			App.Logger.WriteException("AddCustomThemeDialog::ValidateImport", ex);
			_viewModel.FileError = Strings.CustomTheme_Add_Errors_ZipInvalidData;
			return false;
		}
	}

	private void CreateNew()
	{
		if (ValidateCreateNew())
		{
			CreateCustomTheme(_viewModel.Name, _viewModel.Template);
			Created = true;
			ThemeName = _viewModel.Name;
			OpenEditor = true;
			Close();
		}
	}

	private void Import()
	{
		if (ValidateImport())
		{
			string uniqueName = GetUniqueName(Path.GetFileNameWithoutExtension(_viewModel.FilePath));
			string text = Path.Combine(Paths.CustomThemes, uniqueName);
			if (Directory.Exists(text))
			{
				Directory.Delete(text, recursive: true);
			}
			Directory.CreateDirectory(text);
			new FastZip().ExtractZip(_viewModel.FilePath, text, null);
			Created = true;
			ThemeName = uniqueName;
			OpenEditor = false;
			Close();
		}
	}

	private void OnOkButtonClicked(object sender, RoutedEventArgs e)
	{
		if (_viewModel.SelectedTab == 0)
		{
			CreateNew();
		}
		else
		{
			Import();
		}
	}

	private void OnImportButtonClicked(object sender, RoutedEventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = Strings.FileTypes_ZipArchive + "|*.zip"
		};
		if (openFileDialog.ShowDialog() == true)
		{
			_viewModel.FilePath = openFileDialog.FileName;
		}
	}
}
