using System;
using System.IO;
using System.Threading.Tasks;
using Orbitstrap.Enums;
using Orbitstrap.Models.APIs.Config;

namespace Orbitstrap;

public class RemoteDataManager : JsonManager<RemoteDataBase>
{
	public GenericTriState LoadedState = GenericTriState.Unknown;

	public override string ClassName => "RemoteDataManager";

	public override string LOG_IDENT_CLASS => ClassName;

	public override string FileLocation => Path.Combine(Paths.Base, "Data.json");

	public bool Changed => !base.OriginalProp.Equals(base.Prop);

	public event EventHandler DataLoaded;

	public void Subscribe(EventHandler Handler)
	{
		switch (LoadedState)
		{
		case GenericTriState.Unknown:
			DataLoaded += Handler;
			break;
		case GenericTriState.Successful:
			Handler(this, EventArgs.Empty);
			break;
		default:
			Handler(this, EventArgs.Empty);
			break;
		}
	}

	public async Task WaitUntilDataFetched()
	{
		while (LoadedState == GenericTriState.Unknown)
		{
			await Task.Delay(100);
		}
	}

	public async Task LoadData()
	{
		Load(alertFailure: false);
		LoadedState = GenericTriState.Successful;
		this.DataLoaded?.Invoke(this, EventArgs.Empty);
		if (LoadedState == GenericTriState.Successful)
		{
			Save();
		}
	}
}
