using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Desktop.ViewModels;


namespace Desktop.Views;

public partial class OpticalFlowVideoGenerator : UserControl
{
    public OpticalFlowVideoGenerator()
    {
        InitializeComponent();
        DataContext = new ViewModels.OpticalFlowVideoGeneratorViewModel();
    }
    
    private async void OpenFilePicker(object? sender, RoutedEventArgs e)
    {
        
        var files = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select a video",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Video Files")
                    {
                        Patterns = new List<string> { "*.mp4" } // Add more video file extensions when needed but for now we will only support mp4 files
                    }
                }
            });

        if (files.Count > 0)
        {
            string path = files[0].Path.LocalPath;
            
            (DataContext as OpticalFlowVideoGeneratorViewModel)?.VideoFilePath = path;
            InputVideoText.Text = ShortenPath(path);
        }

    }
    
    private async void NewFilePicker(object? sender, RoutedEventArgs e)
    {
        var file = await MainWindow.Instance.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save video",
                SuggestedFileName = "output.mp4",
                DefaultExtension = "mp4",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("MP4 Video")
                    {
                        Patterns = new[] { "*.mp4" }
                    }
                }
            });

        if (file is not null)
        {
            (DataContext as OpticalFlowVideoGeneratorViewModel)?.VideoOutputFilePath = file.Path.LocalPath;
            OutputVideoText.Text = ShortenPath(file.Path.LocalPath);
        }
    }
    
    
    private string? ShortenPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        var parts = path.Split(Path.DirectorySeparatorChar);

        if (parts.Length <= 3)
            return path;

        return $"{parts[0]}{Path.DirectorySeparatorChar}...{Path.DirectorySeparatorChar}" +
               $"{parts[^2]}{Path.DirectorySeparatorChar}{parts[^1]}";
    }

}
