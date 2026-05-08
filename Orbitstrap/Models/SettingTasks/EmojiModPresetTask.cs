using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Models.SettingTasks.Base;
using Orbitstrap.Resources;
using Orbitstrap.UI;
using Orbitstrap.Utility;

namespace Orbitstrap.Models.SettingTasks;

public class EmojiModPresetTask : EnumBaseTask<EmojiType>
{
	private string _filePath => Path.Combine(Paths.Modifications, "content\\fonts\\TwemojiMozilla.ttf");

	private IEnumerable<KeyValuePair<EmojiType, string>>? QueryCurrentValue()
	{
		if (!File.Exists(_filePath))
		{
			return null;
		}
		using FileStream inputStream = File.OpenRead(_filePath);
		string hash = MD5Hash.Stringify(App.MD5Provider.ComputeHash(inputStream));
		return EmojiTypeEx.Hashes.Where<KeyValuePair<EmojiType, string>>((KeyValuePair<EmojiType, string> x) => x.Value == hash);
	}

	public EmojiModPresetTask()
		: base("ModPreset", "EmojiFont")
	{
		IEnumerable<KeyValuePair<EmojiType, string>> enumerable = QueryCurrentValue();
		if (enumerable != null)
		{
			OriginalState = enumerable.FirstOrDefault().Key;
		}
	}

	public override async void Execute()
	{
		IEnumerable<KeyValuePair<EmojiType, string>> enumerable = QueryCurrentValue();
		if (NewState != EmojiType.Default && (enumerable == null || enumerable.FirstOrDefault().Key != NewState))
		{
			try
			{
				HttpResponseMessage httpResponseMessage = await App.HttpClient.GetAsync(NewState.GetUrl());
				httpResponseMessage.EnsureSuccessStatusCode();
				Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
				await using (FileStream fileStream = new FileStream(_filePath, FileMode.Create))
				{
					await httpResponseMessage.Content.CopyToAsync(fileStream);
					OriginalState = NewState;
				}
				return;
			}
			catch (Exception ex)
			{
				App.Logger.WriteException("EmojiModPresetTask::Execute", ex);
				Frontend.ShowConnectivityDialog(string.Format(Strings.Dialog_Connectivity_UnableToConnect, "GitHub"), Strings.Menu_Mods_Presets_EmojiType_Error + "\n\n" + Strings.Dialog_Connectivity_TryAgainLater, MessageBoxImage.Exclamation, ex);
				return;
			}
		}
		if (enumerable != null && enumerable.Any())
		{
			Filesystem.AssertReadOnly(_filePath);
			File.Delete(_filePath);
			OriginalState = NewState;
		}
	}
}
