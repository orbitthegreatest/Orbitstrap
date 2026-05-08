using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Orbitstrap.UI.ViewModels.Dialogs;

internal class LanguageSelectorViewModel
{
	public ICommand SetLocaleCommand => new RelayCommand(SetLocale);

	public static List<string> Languages => Locale.GetLanguages();

	public string SelectedLanguage { get; set; } = Locale.SupportedLocales[App.Settings.Prop.Locale];

	public event EventHandler? CloseRequestEvent;

	private void SetLocale()
	{
		string identifierFromName = Locale.GetIdentifierFromName(SelectedLanguage);
		Locale.Set(identifierFromName);
		App.Settings.Prop.Locale = identifierFromName;
		this.CloseRequestEvent?.Invoke(this, new EventArgs());
	}
}
