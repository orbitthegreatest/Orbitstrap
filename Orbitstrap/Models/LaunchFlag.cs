namespace Orbitstrap.Models;

public class LaunchFlag
{
	public bool Active;

	public string? Data;

	public string Identifiers { get; private set; }

	public LaunchFlag(string identifiers)
	{
		Identifiers = identifiers;
	}
}
