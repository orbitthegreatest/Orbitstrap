using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Settings.Pages;
using Wpf.Ui.Mvvm.Contracts;

namespace Orbitstrap.UI.ViewModels.Settings;

internal class FastFlagEditorWarningViewModel : NotifyPropertyChangedViewModel
{
	private Page _page;

	private CancellationTokenSource? _cancellationTokenSource;

	public string ContinueButtonText { get; set; } = "";

	public bool CanContinue { get; set; }

	public ICommand GoBackCommand => new RelayCommand(GoBack);

	public ICommand ContinueCommand => new RelayCommand(Continue);

	public FastFlagEditorWarningViewModel(Page page)
	{
		_page = page;
	}

	public void StopCountdown()
	{
		_cancellationTokenSource?.Cancel();
		_cancellationTokenSource = null;
	}

	public void StartCountdown()
	{
		StopCountdown();
		_cancellationTokenSource = new CancellationTokenSource();
		DoCountdown(_cancellationTokenSource.Token);
	}

	private async void DoCountdown(CancellationToken token)
	{
		CanContinue = false;
		OnPropertyChanged("CanContinue");
		for (int i = 10; i > 0; i--)
		{
			ContinueButtonText = $"({i}) {Strings.Menu_FastFlagEditor_Warning_Continue}";
			OnPropertyChanged("ContinueButtonText");
			try
			{
				await Task.Delay(1000, token);
			}
			catch (TaskCanceledException)
			{
				return;
			}
		}
		ContinueButtonText = Strings.Menu_FastFlagEditor_Warning_Continue;
		OnPropertyChanged("ContinueButtonText");
		CanContinue = true;
		OnPropertyChanged("CanContinue");
	}

	private void Continue()
	{
		if (CanContinue)
		{
			App.State.Save();
			if (Window.GetWindow(_page) is INavigationWindow navigationWindow)
			{
				navigationWindow.Navigate(typeof(FastFlagEditorPage));
			}
		}
	}

	private void GoBack()
	{
		if (Window.GetWindow(_page) is INavigationWindow navigationWindow)
		{
			navigationWindow.Navigate(typeof(FastFlagsPage));
		}
	}
}
