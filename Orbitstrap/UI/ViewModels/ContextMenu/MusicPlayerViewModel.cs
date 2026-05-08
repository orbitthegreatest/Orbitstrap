using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Orbitstrap.UI.ViewModels.ContextMenu;

public class TrackItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private string _title = string.Empty;
    public string Title { get => _title; set { _title = value; OnPropertyChanged(nameof(Title)); } }

    private string _artist = string.Empty;
    public string Artist { get => _artist; set { _artist = value; OnPropertyChanged(nameof(Artist)); } }

    private string _filePath = string.Empty;
    public string FilePath { get => _filePath; set { _filePath = value; OnPropertyChanged(nameof(FilePath)); } }

    private double _duration;
    public double Duration { get => _duration; set { _duration = value; OnPropertyChanged(nameof(Duration)); } }
}

public class MusicPlayerViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<TrackItem> Tracks { get; } = new();

    private TrackItem? _selectedTrack;
    public TrackItem? SelectedTrack { get => _selectedTrack; set { _selectedTrack = value; OnPropertyChanged(nameof(SelectedTrack)); } }

    private string _currentTrackTitle = "No track playing";
    public string CurrentTrackTitle { get => _currentTrackTitle; set { _currentTrackTitle = value; OnPropertyChanged(nameof(CurrentTrackTitle)); } }

    private bool _isPlaying;
    public bool IsPlaying { get => _isPlaying; set { _isPlaying = value; OnPropertyChanged(nameof(IsPlaying)); } }

    private double _volume = 0.5;
    public double Volume { get => _volume; set { _volume = value; OnPropertyChanged(nameof(Volume)); } }
}
