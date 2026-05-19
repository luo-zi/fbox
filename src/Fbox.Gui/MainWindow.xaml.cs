using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Fbox.Shared.Models;
using Fbox.Shared.Services;

namespace Fbox.Gui;

public partial class MainWindow : Window
{
    private readonly FileCollectionService _service = new();
    private readonly ObservableCollection<FileEntry> _files = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _files;
        LoadFiles();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RestorePosition();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SavePosition();
        var app = (App)Application.Current;
        if (!app.IsShuttingDown)
        {
            e.Cancel = true;
            Hide();
        }
    }

    private void LoadFiles()
    {
        var files = _service.GetAllFilesUnmarked();
        _files.Clear();
        foreach (var f in files)
            _files.Add(f);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var (total, unseen) = _service.GetCount();
        CountBadge.Text = $"{total}";
        StatusText.Text = unseen > 0
            ? $"共 {total} 个文件 ({unseen} 个新)"
            : $"共 {total} 个文件";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadFiles();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_files.Count == 0) return;
        var result = MessageBox.Show(
            "确定清空收集箱中的所有文件引用？",
            "清空确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _service.Clear();
            _files.Clear();
            UpdateStatus();
        }
    }

    private void DropBorder_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            DropBorder.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
            DropBorder.Background = System.Windows.Media.Brushes.AliceBlue;
            DropHint.Text = "松开以添加文件";
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void DropBorder_DragLeave(object sender, DragEventArgs e)
    {
        DropBorder.BorderBrush = System.Windows.Media.Brushes.LightGray;
        DropBorder.Background = System.Windows.Media.Brushes.Transparent;
        DropHint.Text = "拖拽文件到此处\n或双击粘贴剪贴板中的文件路径";
        e.Handled = true;
    }

    private void DropBorder_Drop(object sender, DragEventArgs e)
    {
        DropBorder.BorderBrush = System.Windows.Media.Brushes.LightGray;
        DropBorder.Background = System.Windows.Media.Brushes.Transparent;
        DropHint.Text = "拖拽文件到此处\n或双击粘贴剪贴板中的文件路径";

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddFiles(files);
        }
        e.Handled = true;
    }

    private void DropBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
            PasteFromClipboard();
    }

    private void PasteFromClipboard()
    {
        var paths = new List<string>();

        if (Clipboard.ContainsFileDropList())
        {
            var dropList = Clipboard.GetFileDropList();
            paths.AddRange(dropList.Cast<string>());
        }
        else if (Clipboard.ContainsText())
        {
            var text = Clipboard.GetText();
            var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim().Trim('"');
                if (File.Exists(trimmed) || Directory.Exists(trimmed))
                    paths.Add(trimmed);
            }
        }

        if (paths.Count > 0)
            AddFiles(paths);
        else
            StatusText.Text = "剪贴板中没有有效的文件路径";
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        var list = paths.ToList();
        _service.AddFiles(list);
        LoadFiles();
        StatusText.Text = $"已添加 {list.Count} 个文件";
    }

    private void RemoveItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement el && el.Tag is int id)
        {
            _service.Remove(id);
            LoadFiles();
        }
    }

    private void SavePosition()
    {
        var settings = new SettingsData
        {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height
        };
        SaveSettings(settings);
    }

    private void RestorePosition()
    {
        var settings = LoadSettings();
        if (settings is not null)
        {
            Left = settings.Left;
            Top = settings.Top;
            Width = settings.Width;
            Height = settings.Height;
        }
        else
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 20;
            Top = workArea.Bottom - Height - 20;
        }
    }

    private static string SettingsPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "fbox", "window.json");

    private record SettingsData
    {
        public double Left { get; init; }
        public double Top { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
    }

    private static void SaveSettings(SettingsData data)
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }

    private static SettingsData? LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return null;
            var json = File.ReadAllText(SettingsPath);
            return System.Text.Json.JsonSerializer.Deserialize<SettingsData>(json);
        }
        catch
        {
            return null;
        }
    }
}
