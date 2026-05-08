using System.IO;
using System.Linq;
using System.Reflection;
using Orbitstrap.Models.SettingTasks.Base;
using Orbitstrap.Resources;
using Orbitstrap.Utility;

namespace Orbitstrap.Models.SettingTasks;

public class ExtractIconsTask : BoolBaseTask
{
	private string _path => Path.Combine(Paths.Base, Strings.Paths_Icons);

	public ExtractIconsTask()
		: base("ExtractIcons")
	{
		OriginalState = Directory.Exists(_path);
	}

	public override void Execute()
	{
		if (NewState)
		{
			Directory.CreateDirectory(_path);
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			foreach (string item in from x in executingAssembly.GetManifestResourceNames()
				where x.EndsWith(".ico")
				select x)
			{
				string text = Path.Combine(_path, item.Replace("Orbitstrap.Resources.", ""));
				Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(item);
				using MemoryStream memoryStream = new MemoryStream();
				manifestResourceStream.CopyTo(memoryStream);
				Filesystem.AssertReadOnly(text);
				File.WriteAllBytes(text, memoryStream.ToArray());
			}
		}
		else if (Directory.Exists(_path))
		{
			Directory.Delete(_path, recursive: true);
		}
		OriginalState = NewState;
	}
}
