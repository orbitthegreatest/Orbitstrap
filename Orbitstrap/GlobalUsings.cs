// GlobalUsings.cs — resolves WPF vs System.Windows.Forms ambiguities project-wide.
// WinForms is used by legacy bootstrapper dialogs; WPF is used everywhere else.
// Files that genuinely need WinForms types use the full System.Windows.Forms. qualifier.

global using Application      = System.Windows.Application;
global using Brush             = System.Windows.Media.Brush;
global using Brushes           = System.Windows.Media.Brushes;
global using SolidColorBrush   = System.Windows.Media.SolidColorBrush;
global using Color             = System.Windows.Media.Color;
global using Clipboard         = System.Windows.Clipboard;
global using FlowDirection     = System.Windows.FlowDirection;
global using Image             = System.Windows.Controls.Image;
global using ListBox           = System.Windows.Controls.ListBox;
global using UserControl       = System.Windows.Controls.UserControl;
global using KeyEventArgs      = System.Windows.Input.KeyEventArgs;
global using FontStyle         = System.Windows.FontStyle;
global using ColorConverter    = System.Windows.Media.ColorConverter;
global using PointConverter    = System.Windows.PointConverter;
global using Point             = System.Windows.Point;
global using Rectangle         = System.Windows.Shapes.Rectangle;
global using OpenFileDialog    = Microsoft.Win32.OpenFileDialog;
global using SaveFileDialog    = Microsoft.Win32.SaveFileDialog;
// Button: WPF wins globally; WinForms files must use System.Windows.Forms.Button explicitly
global using Button            = System.Windows.Controls.Button;
// Message: our RPC model wins; WinForms files must use System.Windows.Forms.Message explicitly  
global using Message           = Orbitstrap.Models.OrbitstrapRPC.Message;
// Shortcut: our utility class wins; WinForms Shortcut enum is rarely used
global using Shortcut          = Orbitstrap.Utility.Shortcut;
