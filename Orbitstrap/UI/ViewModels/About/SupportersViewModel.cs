using System;
using System.Windows;
using Orbitstrap.Enums;
using Orbitstrap.Models.APIs.Config;
using Orbitstrap.Utility;

namespace Orbitstrap.UI.ViewModels.About;

public class SupportersViewModel : NotifyPropertyChangedViewModel
{
	public SizeChangedEventHandler? WindowResizeEvent;

	public SupporterData? SupporterData { get; private set; }

	public GenericTriState LoadedState { get; set; } = GenericTriState.Unknown;

	public string LoadError { get; set; } = "";

	public int Columns { get; set; } = 3;

	public SupportersViewModel()
	{
		WindowResizeEvent = (SizeChangedEventHandler)Delegate.Combine(WindowResizeEvent, new SizeChangedEventHandler(OnWindowResize));
		LoadSupporterData();
	}

	private void OnWindowResize(object sender, SizeChangedEventArgs e)
	{
		if (e.WidthChanged)
		{
			int num = (int)Math.Floor(e.NewSize.Width / 200.0);
			if (Columns != num)
			{
				Columns = num;
				OnPropertyChanged("Columns");
			}
		}
	}

	public async void LoadSupporterData()
	{
		try
		{
			SupporterData = await Http.GetJson<SupporterData>("https://raw.githubusercontent.com/Orbitstraplabs/config/main/supporters.json");
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("AboutViewModel::LoadSupporterData", "Could not load supporter data");
			App.Logger.WriteException("AboutViewModel::LoadSupporterData", ex);
			LoadedState = GenericTriState.Failed;
			LoadError = ex.Message;
			OnPropertyChanged("LoadError");
		}
		if (SupporterData != null)
		{
			LoadedState = GenericTriState.Successful;
			OnPropertyChanged("SupporterData");
		}
		OnPropertyChanged("LoadedState");
	}
}
