using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PrismaFlow;

namespace Desktop.ViewModels;

public partial class OpticalFlowVideoGeneratorViewModel : ObservableObject
{
    [ObservableProperty]
    private string _videoFilePath = string.Empty;
    [ObservableProperty]
    private string _videoOutputFilePath  = string.Empty;

    [ObservableProperty] 
    private int _frameCount = 0;
    
    [ObservableProperty]
    private bool _drawOver = false;
    
    [ObservableProperty]
    private float _generationProgress = 0;

    [RelayCommand]
    public void Generate()
    {
        PrismaFlow.PrismaFlow.GenerateOpticalFlowVideo(
            VideoFilePath,
            VideoOutputFilePath,
            frameCount: FrameCount,
            drawOverFrame: DrawOver,
            progress: new Progress<float>(p =>
            {
                GenerationProgress = p;
            })
        );
    }
}
