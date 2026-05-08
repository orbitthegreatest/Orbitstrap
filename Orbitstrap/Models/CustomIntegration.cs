namespace Orbitstrap.Models;

public class CustomIntegration
{
	public string Name { get; set; } = "";

	public string Location { get; set; } = "";

	public string LaunchArgs { get; set; } = "";

	public int Delay { get; set; }

	public bool PreLaunch { get; set; }

	public bool AutoClose { get; set; } = true;
}
