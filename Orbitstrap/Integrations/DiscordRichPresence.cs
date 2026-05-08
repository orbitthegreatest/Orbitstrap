using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DiscordRPC;
using DiscordButton = DiscordRPC.Button;
using DiscordRPC.Message;
using Orbitstrap.Enums;
using Orbitstrap.Models;
using Orbitstrap.Models.APIs.Roblox;
using Orbitstrap.Models.Entities;
using Orbitstrap.Models.OrbitstrapRPC;
using Orbitstrap.Resources;
using Orbitstrap.UI;
using Orbitstrap.Utility;

namespace Orbitstrap.Integrations;

public class DiscordRichPresence : IDisposable
{
	private readonly DiscordRpcClient _rpcClient = new DiscordRpcClient("1005469189907173486");

	private readonly ActivityWatcher _activityWatcher;

	private readonly Queue<Message> _messageQueue = new Queue<Message>();

	private DiscordRPC.RichPresence? _currentPresence;

	private DiscordRPC.RichPresence? _originalPresence;

	private FixedSizeList<ThumbnailCacheEntry> _thumbnailCache = new FixedSizeList<ThumbnailCacheEntry>(20);

	private ulong? _smallImgBeingFetched;

	private ulong? _largeImgBeingFetched;

	private CancellationTokenSource? _fetchThumbnailsToken;

	private bool _visible = true;

	public DiscordRichPresence(ActivityWatcher activityWatcher)
	{
		_activityWatcher = activityWatcher;
		_activityWatcher.OnGameJoin += delegate
		{
			Task.Run(() => SetCurrentGame());
		};
		_activityWatcher.OnGameLeave += delegate
		{
			Task.Run(() => SetCurrentGame());
		};
		_activityWatcher.OnRPCMessage += delegate(object? _, Message message)
		{
			ProcessRPCMessage(message);
		};
		_rpcClient.OnReady += delegate(object _, ReadyMessage e)
		{
			App.Logger.WriteLine("DiscordRichPresence", $"Received ready from user {e.User} ({e.User.ID})");
		};
		_rpcClient.OnPresenceUpdate += delegate
		{
			App.Logger.WriteLine("DiscordRichPresence", "Presence updated");
		};
		_rpcClient.OnError += delegate(object _, ErrorMessage e)
		{
			App.Logger.WriteLine("DiscordRichPresence", "An RPC error occurred - " + e.Message);
		};
		_rpcClient.OnConnectionEstablished += delegate
		{
			App.Logger.WriteLine("DiscordRichPresence", "Established connection with Discord RPC");
		};
		_rpcClient.OnClose += delegate(object _, CloseMessage e)
		{
			App.Logger.WriteLine("DiscordRichPresence", $"Lost connection to Discord RPC - {e.Reason} ({e.Code})");
		};
		_rpcClient.Initialize();
	}

	public void ProcessRPCMessage(Message message, bool implicitUpdate = true)
	{
		if (message.Command != "SetRichPresence" && message.Command != "SetLaunchData")
		{
			return;
		}
		if (_currentPresence == null || _originalPresence == null)
		{
			App.Logger.WriteLine("DiscordRichPresence::ProcessRPCMessage", "Presence is not set, enqueuing message");
			_messageQueue.Enqueue(message);
			return;
		}
		if (message.Command == "SetLaunchData")
		{
			_currentPresence.Buttons = GetButtons();
		}
		else if (message.Command == "SetRichPresence")
		{
			ProcessSetRichPresence(message, implicitUpdate);
		}
		if (implicitUpdate)
		{
			UpdatePresence();
		}
	}

	private void AddToThumbnailCache(ulong id, string? url)
	{
		if (url != null)
		{
			_thumbnailCache.Add(new ThumbnailCacheEntry
			{
				Id = id,
				Url = url
			});
		}
	}

	private async Task UpdatePresenceIconsAsync(ulong? smallImg, ulong? largeImg, bool implicitUpdate, CancellationToken token)
	{
		if (smallImg.HasValue && largeImg.HasValue)
		{
			string[] obj = await Thumbnails.GetThumbnailUrlsAsync(new List<ThumbnailRequest>
			{
				new ThumbnailRequest
				{
					TargetId = smallImg.Value,
					Type = "Asset",
					Size = "512x512",
					IsCircular = false
				},
				new ThumbnailRequest
				{
					TargetId = largeImg.Value,
					Type = "Asset",
					Size = "512x512",
					IsCircular = false
				}
			}, token);
			string text = obj[0];
			string text2 = obj[1];
			AddToThumbnailCache(smallImg.Value, text);
			AddToThumbnailCache(largeImg.Value, text2);
			if (_currentPresence != null)
			{
				_currentPresence.Assets.SmallImageKey = text;
				_currentPresence.Assets.LargeImageKey = text2;
			}
		}
		else if (smallImg.HasValue)
		{
			string text3 = await Thumbnails.GetThumbnailUrlAsync(new ThumbnailRequest
			{
				TargetId = smallImg.Value,
				Type = "Asset",
				Size = "512x512",
				IsCircular = false
			}, token);
			AddToThumbnailCache(smallImg.Value, text3);
			if (_currentPresence != null)
			{
				_currentPresence.Assets.SmallImageKey = text3;
			}
		}
		else if (largeImg.HasValue)
		{
			string text4 = await Thumbnails.GetThumbnailUrlAsync(new ThumbnailRequest
			{
				TargetId = largeImg.Value,
				Type = "Asset",
				Size = "512x512",
				IsCircular = false
			}, token);
			AddToThumbnailCache(largeImg.Value, text4);
			if (_currentPresence != null)
			{
				_currentPresence.Assets.LargeImageKey = text4;
			}
		}
		_smallImgBeingFetched = null;
		_largeImgBeingFetched = null;
		if (implicitUpdate)
		{
			UpdatePresence();
		}
	}

	private void ProcessSetRichPresence(Message message, bool implicitUpdate)
	{
		if (_fetchThumbnailsToken != null)
		{
			_fetchThumbnailsToken.Cancel();
			_fetchThumbnailsToken = null;
		}
		Orbitstrap.Models.OrbitstrapRPC.RichPresence presenceData;
		try
		{
			presenceData = message.Data.Deserialize<Orbitstrap.Models.OrbitstrapRPC.RichPresence>();
		}
		catch (Exception)
		{
			App.Logger.WriteLine("DiscordRichPresence::ProcessSetRichPresence", "Failed to parse message! (JSON deserialization threw an exception)");
			return;
		}
		if (presenceData == null)
		{
			App.Logger.WriteLine("DiscordRichPresence::ProcessSetRichPresence", "Failed to parse message! (JSON deserialization returned null)");
			return;
		}
		if (presenceData.Details != null)
		{
			if (presenceData.Details.Length > 128)
			{
				App.Logger.WriteLine("DiscordRichPresence::ProcessSetRichPresence", "Details cannot be longer than 128 characters");
			}
			else if (presenceData.Details == "<reset>")
			{
				_currentPresence.Details = _originalPresence.Details;
			}
			else
			{
				_currentPresence.Details = presenceData.Details;
			}
		}
		if (presenceData.State != null)
		{
			if (presenceData.State.Length > 128)
			{
				App.Logger.WriteLine("DiscordRichPresence::ProcessSetRichPresence", "State cannot be longer than 128 characters");
			}
			else if (presenceData.State == "<reset>")
			{
				_currentPresence.State = _originalPresence.State;
			}
			else
			{
				_currentPresence.State = presenceData.State;
			}
		}
		if (presenceData.TimestampStart == 0)
		{
			_currentPresence.Timestamps.Start = null;
		}
		else if (presenceData.TimestampStart.HasValue)
		{
			_currentPresence.Timestamps.StartUnixMilliseconds = presenceData.TimestampStart * 1000;
		}
		if (presenceData.TimestampEnd == 0)
		{
			_currentPresence.Timestamps.End = null;
		}
		else if (presenceData.TimestampEnd.HasValue)
		{
			_currentPresence.Timestamps.EndUnixMilliseconds = presenceData.TimestampEnd * 1000;
		}
		ulong? smallImgBeingFetched = null;
		ulong? largeImgBeingFetched = null;
		if (presenceData.SmallImage != null)
		{
			if (presenceData.SmallImage.Clear)
			{
				_currentPresence.Assets.SmallImageKey = "";
				_smallImgBeingFetched = null;
			}
			else if (presenceData.SmallImage.Reset)
			{
				_currentPresence.Assets.SmallImageText = _originalPresence.Assets.SmallImageText;
				_currentPresence.Assets.SmallImageKey = _originalPresence.Assets.SmallImageKey;
				_smallImgBeingFetched = null;
			}
			else
			{
				if (presenceData.SmallImage.AssetId.HasValue)
				{
					ThumbnailCacheEntry thumbnailCacheEntry = _thumbnailCache.FirstOrDefault((ThumbnailCacheEntry x) => x.Id == presenceData.SmallImage.AssetId);
					if (thumbnailCacheEntry == null)
					{
						smallImgBeingFetched = presenceData.SmallImage.AssetId;
					}
					else
					{
						_currentPresence.Assets.SmallImageKey = thumbnailCacheEntry.Url;
						_smallImgBeingFetched = null;
					}
				}
				if (presenceData.SmallImage.HoverText != null)
				{
					_currentPresence.Assets.SmallImageText = presenceData.SmallImage.HoverText;
				}
			}
		}
		if (presenceData.LargeImage != null)
		{
			if (presenceData.LargeImage.Clear)
			{
				_currentPresence.Assets.LargeImageKey = "";
				_largeImgBeingFetched = null;
			}
			else if (presenceData.LargeImage.Reset)
			{
				_currentPresence.Assets.LargeImageText = _originalPresence.Assets.LargeImageText;
				_currentPresence.Assets.LargeImageKey = _originalPresence.Assets.LargeImageKey;
				_largeImgBeingFetched = null;
			}
			else
			{
				if (presenceData.LargeImage.AssetId.HasValue)
				{
					ThumbnailCacheEntry thumbnailCacheEntry2 = _thumbnailCache.FirstOrDefault((ThumbnailCacheEntry x) => x.Id == presenceData.LargeImage.AssetId);
					if (thumbnailCacheEntry2 == null)
					{
						largeImgBeingFetched = presenceData.LargeImage.AssetId;
					}
					else
					{
						_currentPresence.Assets.LargeImageKey = thumbnailCacheEntry2.Url;
						_largeImgBeingFetched = null;
					}
				}
				if (presenceData.LargeImage.HoverText != null)
				{
					_currentPresence.Assets.LargeImageText = presenceData.LargeImage.HoverText;
				}
			}
		}
		if (smallImgBeingFetched.HasValue)
		{
			_smallImgBeingFetched = smallImgBeingFetched;
		}
		if (largeImgBeingFetched.HasValue)
		{
			_largeImgBeingFetched = largeImgBeingFetched;
		}
		if (_smallImgBeingFetched.HasValue || _largeImgBeingFetched.HasValue)
		{
			_fetchThumbnailsToken = new CancellationTokenSource();
			Task.Run(() => UpdatePresenceIconsAsync(_smallImgBeingFetched, _largeImgBeingFetched, implicitUpdate, _fetchThumbnailsToken.Token));
		}
	}

	public void SetVisibility(bool visible)
	{
		App.Logger.WriteLine("DiscordRichPresence::SetVisibility", $"Setting presence visibility ({visible})");
		_visible = visible;
		if (_visible)
		{
			UpdatePresence();
		}
		else
		{
			_rpcClient.ClearPresence();
		}
	}

	public async Task<bool> SetCurrentGame()
	{
		if (!_activityWatcher.InGame)
		{
			App.Logger.WriteLine("DiscordRichPresence::SetCurrentGame", "Not in game, clearing presence");
			_currentPresence = (_originalPresence = null);
			_messageQueue.Clear();
			UpdatePresence();
			return true;
		}
		string smallImageText = "Roblox";
		string smallImage = "roblox";
		ActivityData activity = _activityWatcher.Data;
		long placeId = activity.PlaceId;
		App.Logger.WriteLine("DiscordRichPresence::SetCurrentGame", $"Setting presence for Place ID {placeId}");
		DateTime timeStarted = activity.TimeJoined;
		if (activity.RootActivity != null)
		{
			timeStarted = activity.RootActivity.TimeJoined;
		}
		if (activity.UniverseDetails == null)
		{
			try
			{
				await UniverseDetails.FetchSingle(activity.UniverseId);
			}
			catch (Exception ex)
			{
				App.Logger.WriteException("DiscordRichPresence::SetCurrentGame", ex);
				Frontend.ShowMessageBox(Strings.ActivityWatcher_RichPresenceLoadFailed + "\n\n" + ex.Message, MessageBoxImage.Exclamation);
				return false;
			}
			activity.UniverseDetails = UniverseDetails.LoadFromCache(activity.UniverseId);
		}
		UniverseDetails universeDetails = activity.UniverseDetails;
		string icon = universeDetails.Thumbnail.ImageUrl;
		if (App.Settings.Prop.ShowAccountOnRichPresence)
		{
			UserDetails userDetails = await UserDetails.Fetch(activity.UserId);
			smallImage = userDetails.Thumbnail.ImageUrl;
			smallImageText = $"Playing on {userDetails.Data.DisplayName} (@{userDetails.Data.Name})";
		}
		if (!_activityWatcher.InGame || placeId != activity.PlaceId)
		{
			App.Logger.WriteLine("DiscordRichPresence::SetCurrentGame", "Aborting presence set because game activity has changed");
			return false;
		}
		string state = _activityWatcher.Data.ServerType switch
		{
			ServerType.Private => "In a private server", 
			ServerType.Reserved => "In a reserved server", 
			_ => "by " + universeDetails.Data.Creator.Name + (universeDetails.Data.Creator.HasVerifiedBadge ? " ☑\ufe0f" : ""), 
		};
		string text = universeDetails.Data.Name;
		if (text.Length < 2)
		{
			text += "⠀⠀⠀";
		}
		_currentPresence = new DiscordRPC.RichPresence
		{
			Details = text,
			State = state,
			Timestamps = new Timestamps
			{
				Start = timeStarted.ToUniversalTime()
			},
			Buttons = GetButtons(),
			Assets = new Assets
			{
				LargeImageKey = icon,
				LargeImageText = universeDetails.Data.Name,
				SmallImageKey = smallImage,
				SmallImageText = smallImageText
			}
		};
		_originalPresence = _currentPresence.Clone();
		if (_messageQueue.Any())
		{
			App.Logger.WriteLine("DiscordRichPresence::SetCurrentGame", "Processing queued messages");
			ProcessRPCMessage(_messageQueue.Dequeue(), implicitUpdate: false);
		}
		UpdatePresence();
		return true;
	}

	public DiscordButton[] GetButtons()
	{
		List<DiscordButton> list = new List<DiscordButton>();
		ActivityData data = _activityWatcher.Data;
		if (!App.Settings.Prop.HideRPCButtons)
		{
			bool flag = false;
			if (data.ServerType == ServerType.Public)
			{
				flag = true;
			}
			else if (data.ServerType == ServerType.Reserved && !string.IsNullOrEmpty(data.RPCLaunchData))
			{
				flag = true;
			}
			if (flag)
			{
				list.Add(new DiscordButton
				{
					Label = "Join server",
					Url = data.GetInviteDeeplink()
				});
			}
		}
		list.Add(new DiscordButton
		{
			Label = "See game page",
			Url = $"https://www.roblox.com/games/{data.PlaceId}"
		});
		return list.ToArray();
	}

	public void UpdatePresence()
	{
		if (_currentPresence == null)
		{
			App.Logger.WriteLine("DiscordRichPresence::UpdatePresence", "Presence is empty, clearing");
			_rpcClient.ClearPresence();
			return;
		}
		App.Logger.WriteLine("DiscordRichPresence::UpdatePresence", "Updating presence");
		if (_visible)
		{
			_rpcClient.SetPresence(_currentPresence);
		}
	}

	public void Dispose()
	{
		App.Logger.WriteLine("DiscordRichPresence::Dispose", "Cleaning up Discord RPC and Presence");
		_rpcClient.ClearPresence();
		_rpcClient.Dispose();
		GC.SuppressFinalize(this);
	}
}
