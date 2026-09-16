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

public partial class OpticalFlowVideoGeneratorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _videoFilePath = string.Empty;
    [ObservableProperty]
    private string _videoOutputFilePath  = string.Empty;

    [ObservableProperty] 
    private int _frameCount = 0;
    
    [ObservableProperty]
    private bool _drawOver = false;

    [ObservableProperty] private float _farnebackScaling = 0.5f;
    
    [ObservableProperty]
    private float _generationProgress = 0;


    [ObservableProperty]
    private bool isGenerating;

    [RelayCommand]
    public async Task GenerateOpticalFlowVideo()
    {
        IsGenerating = true;
        GenerationProgress = 0;

        try
        {
            var progress = new Progress<float>(p =>
            {
                GenerationProgress = p;
            });

            await Task.Run(() =>
            {
                PrismaFlow.Generator.CreateOpticalFlowVideo(
                    VideoFilePath,
                    VideoOutputFilePath,
                    frameCount: FrameCount,
                    drawOverFrame: DrawOver,
                    farnebackScaling: FarnebackScaling,
                    progress: progress
                );
            });
        }
        finally
        {
            IsGenerating = false;
        }
    }
}
