using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Orbitstrap.Integrations;
using Orbitstrap.RobloxInterfaces;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Interfaces;

namespace Orbitstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage
    {
        private string? CustomLogoPath = null;
        private string? CustomSpinnerPath = null;

        private ModsViewModel ViewModel;


        public ModsPage()
        {
            SetupViewModel();
            InitializeComponent();
            InitializePreview();

            ViewModel = new ModsViewModel();
            DataContext = ViewModel;

            Loaded += async (s, e) =>
            {
                await ViewModel.InitializeAsync();
            };

            // Mod Generator removed
            ViewModel.GradientStops.Add(new GradientStopViewModel { Offset = 0, ColorHex = "#FFFFFF" });
        }

        private void SetupViewModel()
        {
            ViewModel = new ModsViewModel();
            DataContext = ViewModel;
        }

        private async void ModGenerator_Click(object sender, RoutedEventArgs e)
        { /* Mod Generator removed */ }

        private void OnSelectCustomLogo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select Custom Roblox Logo"
            };

            if (dlg.ShowDialog() == true)
            {
                CustomLogoPath = dlg.FileName;

                _ = UpdatePreviewAsync();
            }
        }

        private void OnClearCustomLogo_Click(object sender, RoutedEventArgs e)
        {
            CustomLogoPath = null;

            _ = UpdatePreviewAsync();
        }

        private void OnSelectCustomSpinner_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select Custom Spinner"
            };

            if (dlg.ShowDialog() == true)
            {
                CustomSpinnerPath = dlg.FileName;
                _ = UpdatePreviewAsync();
            }
        }

        private void OnClearCustomSpinner_Click(object sender, RoutedEventArgs e)
        {
            CustomSpinnerPath = null;
            _ = UpdatePreviewAsync();
        }

        private Bitmap? _sheetOriginalBitmap = null;
        private CancellationTokenSource? _previewCts;
        private readonly List<ModGenerator.SpriteDef> _previewSprites = ParseHardcodedSpriteList();

        private void InitializePreview()
        {
            LoadEmbeddedPreviewSheet();
            // PopulateSpriteSelector removed (Mod Generator tab removed)
            _ = UpdatePreviewAsync();
        }

        private void LoadEmbeddedPreviewSheet()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                string? resName = asm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && n.Contains("Orbitstrap.Resources"));
                if (resName == null)
                    resName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
                if (resName == null) return;
                using var rs = asm.GetManifestResourceStream(resName);
                if (rs == null) return;
                using var src = new Bitmap(rs);
                _sheetOriginalBitmap?.Dispose();
                _sheetOriginalBitmap = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(_sheetOriginalBitmap)) g.DrawImage(src, 0, 0, src.Width, src.Height);
            }
            catch (Exception ex)
            {
                App.Logger?.WriteException("ModsPage::LoadEmbeddedPreviewSheet", ex);
            }
        }

        private static List<ModGenerator.SpriteDef> ParseHardcodedSpriteList()
        {
            string[] lines = new[]
            {
                "like_off 0x0 72x72","rocket_off 74x0 72x72","heart_on 148x0 72x72","trophy_off 222x0 72x72","heart_off 296x0 72x72","report_off 370x0 72x72",
                "dislike_off 0x74 72x72","music 74x74 72x72","player_count 148x74 72x72","selfview_on 222x74 72x72","notification_off 296x74 72x72","send 370x74 72x72",
                "like_on 0x148 72x72","robux 74x148 72x72","backpack_on 148x148 72x72","report_on 222x148 72x72","search 296x148 72x72","notification_on 370x148 72x72",
                "dislike_on 0x222 72x72","chat_on 74x222 72x72","backpack_off 148x222 72x72","fingerprint 222x222 72x72","roblox 296x222 72x72","roblox_studio 370x222 72x72",
                "notepad 0x296 72x72","chat_off 74x296 72x72","close 148x296 72x72","add 222x296 72x72","sync 296x296 72x72","pin 370x296 72x72",
                "picture 0x370 72x72","enlarge 74x370 72x72","headset_locked 148x370 72x72","friends_off 222x370 72x72","friends_on 296x370 72x72","person_camera 370x370 72x72"
            };

            var list = new List<ModGenerator.SpriteDef>();
            foreach (var l in lines)
            {
                var parts = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                string name = parts[0];
                var coords = parts[1].Split('x');
                int x = int.Parse(coords[0]);
                int y = int.Parse(coords[1]);
                var size = parts[2].Split('x');
                int w = int.Parse(size[0]);
                int h = int.Parse(size[1]);
                list.Add(new ModGenerator.SpriteDef(name, x, y, w, h));
            }
            return list;
        }

        private void PopulateSpriteSelector()
        { /* Mod Generator removed */ }

        private async Task UpdatePreviewAsync()
        { /* Mod Generator removed */ }

        private Bitmap RenderPreviewSheet(Bitmap sheetBmp, Color? solidColor, List<ModGenerator.GradientStop>? gradient, string? customRobloxPath, float gradientAngleDeg)
        {
            if (sheetBmp == null) throw new InvalidOperationException("sheetBmp is null.");

            Bitmap? customRoblox = null;

            try
            {
                if (!string.IsNullOrEmpty(customRobloxPath) && File.Exists(customRobloxPath))
                {
                    using var fs = new FileStream(customRobloxPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var tmp = new Bitmap(fs);
                    customRoblox = new Bitmap(tmp.Width, tmp.Height, PixelFormat.Format32bppArgb);
                    using var g = Graphics.FromImage(customRoblox);
                    g.DrawImage(tmp, 0, 0, tmp.Width, tmp.Height);
                }

                var output = new Bitmap(sheetBmp.Width, sheetBmp.Height, PixelFormat.Format32bppArgb);
                using var gOutput = Graphics.FromImage(output);
                gOutput.CompositingMode = CompositingMode.SourceOver;
                gOutput.CompositingQuality = CompositingQuality.HighQuality;
                gOutput.SmoothingMode = SmoothingMode.HighQuality;

                gOutput.DrawImage(sheetBmp, 0, 0);

                foreach (var def in _previewSprites)
                {
                    if (def.W <= 0 || def.H <= 0) continue;
                    var rect = new Rectangle(def.X, def.Y, def.W, def.H);

                    if (string.Equals(def.Name, "roblox", StringComparison.OrdinalIgnoreCase) && customRoblox != null)
                    {
                        using var brush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
                        gOutput.CompositingMode = CompositingMode.SourceCopy;
                        gOutput.FillRectangle(brush, rect);
                        gOutput.CompositingMode = CompositingMode.SourceOver;
                        gOutput.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gOutput.DrawImage(customRoblox, rect);
                        continue;
                    }

                    try
                    {
                        using var cropped = sheetBmp.Clone(rect, PixelFormat.Format32bppArgb);
                        using var recolored = ApplyMaskPreview(cropped, solidColor, gradient, gradientAngleDeg);
                        gOutput.DrawImage(recolored, rect);
                    }
                    catch (OutOfMemoryException)
                    {
                        continue;
                    }
                }

                return output;
            }
            finally
            {
                customRoblox?.Dispose();
            }
        }

        private Bitmap ApplyMaskPreview(Bitmap original, Color? solidColor, List<ModGenerator.GradientStop>? gradient, float gradientAngleDeg)
        {
            if (original.Width == 0 || original.Height == 0)
                return new Bitmap(original);

            Bitmap recolored = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);

            double theta = gradientAngleDeg * Math.PI / 180.0;
            double cos = Math.Cos(theta);
            double sin = Math.Sin(theta);
            double w = original.Width - 1;
            double h = original.Height - 1;

            double[] projs = new double[]
            {
        0 * cos + 0 * sin,
        w * cos + 0 * sin,
        0 * cos + h * sin,
        w * cos + h * sin
            };
            double minProj = projs.Min();
            double maxProj = projs.Max();
            double denom = Math.Abs(maxProj - minProj) < 1e-6 ? 1.0 : (maxProj - minProj);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color src = original.GetPixel(x, y);
                    if (src.A <= 5)
                    {
                        recolored.SetPixel(x, y, Color.Transparent);
                        continue;
                    }

                    Color applyColor;
                    if (gradient != null && gradient.Count > 0)
                    {
                        double proj = x * cos + y * sin;
                        float t = (float)((proj - minProj) / denom);
                        t = Math.Clamp(t, 0f, 1f);
                        applyColor = InterpolateGradientPreview(gradient, t);
                    }
                    else
                    {
                        applyColor = solidColor ?? Color.White;
                    }

                    float alphaFactor = src.A / 255f;
                    Color finalColor = Color.FromArgb(
                        src.A,
                        (byte)(applyColor.R * alphaFactor),
                        (byte)(applyColor.G * alphaFactor),
                        (byte)(applyColor.B * alphaFactor)
                    );

                    recolored.SetPixel(x, y, finalColor);
                }
            }

            return recolored;
        }

        private static Color InterpolateGradientPreview(List<ModGenerator.GradientStop> gradient, float t)
        {
            if (gradient == null || gradient.Count == 0)
                return Color.White;

            var stops = gradient.OrderBy(s => s.Stop).ToList();

            if (t <= stops[0].Stop) return stops[0].Color;
            if (t >= stops[^1].Stop) return stops[^1].Color;

            ModGenerator.GradientStop left = stops[0];
            ModGenerator.GradientStop right = stops[^1];
            for (int i = 0; i < stops.Count - 1; i++)
            {
                if (t >= stops[i].Stop && t <= stops[i + 1].Stop)
                {
                    left = stops[i];
                    right = stops[i + 1];
                    break;
                }
            }

            float span = right.Stop - left.Stop;
            float localT = span > 0 ? (t - left.Stop) / span : 0f;
            localT = Math.Clamp(localT, 0f, 1f);

            int r = (int)Math.Round(left.Color.R + (right.Color.R - left.Color.R) * localT);
            int g = (int)Math.Round(left.Color.G + (right.Color.G - left.Color.G) * localT);
            int b = (int)Math.Round(left.Color.B + (right.Color.B - left.Color.B) * localT);

            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            return Color.FromArgb(r, g, b);
        }

        private void UpdateSpritePreviewFromBitmap(Bitmap sheetBmp)
        { /* Mod Generator removed */ }

        private BitmapImage BitmapToImageSource(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        private void OnSpriteSelectorChanged(object sender, SelectionChangedEventArgs e)
        { /* Mod Generator removed */ }

        private double _gradientAngle = 0.0;

        private void ApplyGradientAngleFromTextBox()
        { /* Mod Generator removed */ }

        private static readonly Regex _angleInputRegex = new Regex("^[0-9]*\\.?[0-9]*$");

        private void GradientAngleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        { /* Mod Generator removed */ }

        private void GradientAngleTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        { /* Mod Generator removed */ }

        private void GradientAngleTextBox_KeyDown(object sender, KeyEventArgs e)
        { /* Mod Generator removed */ }

        private void GradientAngleTextBox_LostFocus(object sender, RoutedEventArgs e)
        { /* Mod Generator removed */ }

        #region Gradient Color Stuff
        public class GradientStopViewModel : INotifyPropertyChanged
        {
            private float offset;
            private string colorHex = "#FFFFFF";

            public float Offset
            {
                get => offset;
                set { offset = value; OnPropertyChanged(); }
            }

            public string ColorHex
            {
                get => colorHex;
                set { colorHex = value; OnPropertyChanged(); }
            }

            public Color Color
            {
                get
                {
                    try { return ColorTranslator.FromHtml(colorHex); }
                    catch { return Color.White; }
                }
                set { ColorHex = $"#{value.R:X2}{value.G:X2}{value.B:X2}"; }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OnAddGradientStop_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GradientStops.Add(new GradientStopViewModel { Offset = 1f, ColorHex = "#FFFFFF" });
            _ = UpdatePreviewAsync();
        }

        private void OnRemoveGradientStop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is GradientStopViewModel stop)
            {
                ViewModel.GradientStops.Remove(stop);
                _ = UpdatePreviewAsync();
            }
        }

        private void OnMoveUpGradientStop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is GradientStopViewModel stop)
            {
                int idx = ViewModel.GradientStops.IndexOf(stop);
                if (idx > 0)
                    ViewModel.GradientStops.Move(idx, idx - 1);
                _ = UpdatePreviewAsync();
            }
        }

        private void OnMoveDownGradientStop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is GradientStopViewModel stop)
            {
                int idx = ViewModel.GradientStops.IndexOf(stop);
                if (idx < ViewModel.GradientStops.Count - 1)
                    ViewModel.GradientStops.Move(idx, idx + 1);
                _ = UpdatePreviewAsync();
            }
        }

        private void OnGradientOffsetChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Two-way binding handles the value change; refresh preview so it updates immediately.
            _ = UpdatePreviewAsync();
        }

        private void OnGradientColorHexChanged(object sender, TextChangedEventArgs e)
        {
            // Two-way binding updates the ColorHex; refresh preview so it updates immediately.
            _ = UpdatePreviewAsync();
        }

        private void OnChangeGradientColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is GradientStopViewModel stop)
            {
                var dlg = new System.Windows.Forms.ColorDialog
                {
                    AllowFullOpen = true,
                    FullOpen = true,
                    Color = ColorTranslator.FromHtml(stop.ColorHex)
                };

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    stop.ColorHex = $"#{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
                    _ = UpdatePreviewAsync();
                }
            }
        }
        #endregion
    }

    public class ModInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string FolderPath { get; set; }
    }
    public class GitHubContent
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Download_Url { get; set; }
    }
}