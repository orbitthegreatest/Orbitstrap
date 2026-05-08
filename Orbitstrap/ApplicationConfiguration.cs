using System.Runtime.CompilerServices;
using System.Windows.Forms;
using WinFormsApp = System.Windows.Forms.Application;

namespace Orbitstrap;

[CompilerGenerated]
internal static class ApplicationConfiguration
{
	public static void Initialize()
	{
		WinFormsApp.EnableVisualStyles();
		WinFormsApp.SetCompatibleTextRenderingDefault(defaultValue: false);
		WinFormsApp.SetHighDpiMode(HighDpiMode.SystemAware);
	}
}
