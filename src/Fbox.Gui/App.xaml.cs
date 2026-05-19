using System.Windows;

namespace Fbox.Gui;

public partial class App : System.Windows.Application
{
    private System.Windows.Forms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private bool _isShuttingDown;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        CreateTrayIcon();
        _mainWindow = new MainWindow();
        _mainWindow.Closed += (_, _) => _mainWindow = null;
        _mainWindow.Show();
    }

    private void CreateTrayIcon()
    {
        using var bmp = new System.Drawing.Bitmap(16, 16);
        using var g = System.Drawing.Graphics.FromImage(bmp);
        g.Clear(System.Drawing.Color.FromArgb(26, 115, 232));
        using var font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
        g.DrawString("f", font, System.Drawing.Brushes.White, 4, 2);

        var hIcon = bmp.GetHicon();
        using var tmp = System.Drawing.Icon.FromHandle(hIcon);
        var icon = (System.Drawing.Icon)tmp.Clone();

        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = icon,
            Text = "fbox - 文件收集箱",
            Visible = true
        };

        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("显示窗口", null, (_, _) => ShowWindow());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => RequestShutdown());
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowWindow();
    }

    private void ShowWindow()
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closed += (_, _) => _mainWindow = null;
        }
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    internal void RequestShutdown()
    {
        _isShuttingDown = true;
        _trayIcon?.Dispose();
        Shutdown();
    }

    internal bool IsShuttingDown => _isShuttingDown;

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
