using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.ViewModels;

namespace Desktop.Views;

public abstract class ViewModelBase : ObservableObject { }


public partial class MainWindow : Window
{
    static public MainWindow Instance { get; private set; }
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        Instance = this;
    }
}