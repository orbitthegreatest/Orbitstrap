using Orbitstrap.Models.Attributes;

namespace Orbitstrap.Enums;

public enum BootstrapperStyle
{
	VistaDialog,
	LegacyDialog2008,
	LegacyDialog2011,
	ProgressDialog,
	ClassicFluentDialog,
	ByfronDialog,
	[EnumName(StaticName = "Orbitstrap")]
	FluentDialog,
	FluentAeroDialog,
	CustomDialog
}
