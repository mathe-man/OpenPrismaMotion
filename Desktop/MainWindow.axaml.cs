using Avalonia.Controls;

namespace Desktop;

public partial class MainWindow : Window
{
    static public MainWindow Instance { get; private set; }
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }
}