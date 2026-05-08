using System.Windows.Media;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;

namespace Orbitstrap.Models;

public class BootstrapperIconEntry
{
	public BootstrapperIcon IconType { get; set; }

	public ImageSource ImageSource => IconType.GetIcon().GetImageSource();
}
