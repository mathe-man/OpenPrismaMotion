using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Desktop.ViewModels;

public abstract class ViewModelBase : ObservableObject { }

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    public OpticalFlowVideoGeneratorViewModel GeneratorVM { get; } = new();
    public ViewportViewModel ViewportVM { get; } = new();
    
    
    public bool IsGeneratorActive => CurrentPage == GeneratorVM;
    public bool IsViewportActive => CurrentPage == ViewportVM;

    
    public MainWindowViewModel()
    {
        CurrentPage = ViewportVM;
    }

    [RelayCommand]
    private void GoToGenerator() => CurrentPage = GeneratorVM;

    [RelayCommand]
    private void GoToViewport() => CurrentPage = ViewportVM;
    
    partial void OnCurrentPageChanged(ViewModelBase value)
    {
        OnPropertyChanged(nameof(IsGeneratorActive));
        OnPropertyChanged(nameof(IsViewportActive));
    }
}