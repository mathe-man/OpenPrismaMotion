namespace CLI;

using FlowResolver;
using System.Diagnostics;
using ShellProgressBar;

class Program
{
    static string input;
    static string output;
    static bool drawOver;
    static int frames;

    static int Main(string[] args)
    {


        if (args.Contains("--help") || args.Contains("-h")) {
            Help();
            return 0;
        }


        input = GetInputPath(args);
        // We have to stop here if no input as been provided
        if (string.IsNullOrEmpty(input))
            return -1;

        // Retrieve parameters
        output = GetOutputPath(args);
        drawOver = GetDrawOver(args);
        frames = GetFrames(args);

        Console.WriteLine("Optical flow video generation...");

        // Visual shell progress bar
        var pBar = new ProgressBar
        (
            1000,       // For a 100.0 precision
            "Generation",
            new ProgressBarOptions
            {
                EnableTaskBarProgress = true,
                ForegroundColor = ConsoleColor.Cyan,
                ForegroundColorDone = ConsoleColor.Green
            }
        );


        FlowResolver.GenerateOpticalFlowVideo(
            input, output,
            drawOverFrame: drawOver,
            frameCount: frames,

            progress: new Progress<float>(p =>
                {
                    pBar.Tick((int)(p * 1000));
                })
            );



        // Jump over the progress bar to write after it once the process will be done
        Console.Write("\n\n\n");
        Console.WriteLine("Generation done.");

        Console.Write("Would you like to read the video directly? [Y/n] : ");
        if (Console.ReadKey().Key != ConsoleKey.Y)
            return 0;

        // Open the video using the user's default app associated with the extension
        Process.Start(new ProcessStartInfo
        {
            FileName = output, // Path to the video
            UseShellExecute = true
        });

        return 0;
    }

    static string[] VideoExtensions =
        [
        ".mp4",
        ".mkv",
        ];

    static string GetInputPath(string[] args)
    {

        var index = args.IndexOf("-i");
        if (index >= 0)
            if (index + 1 < args.Length)
                return args[index + 1];

        Console.WriteLine("No video source specified");

        Console.WriteLine("Searching in current directory...");

        var files = Directory.EnumerateFiles(Directory.GetCurrentDirectory()).ToList();

        List<string> videos = new();

        // Scan every files in the current directory to find videos
        foreach (var file in files)
        {
            foreach (var extension in VideoExtensions)
                if (file.EndsWith(extension))
                    videos.Add(file);
        }

        
        if (videos.Count() == 1)
        {
            Console.Write($"Found '{videos[0]}', proceed with this video as input ? [Y/n] : ");
            if (Console.ReadKey().Key == ConsoleKey.Y) {
                Console.WriteLine($"\nUsing '{videos[0]}'");
                return videos[0];
            }
        }
        else if (videos.Count > 1)
        {
            Console.WriteLine("Found multiple videos avaible in the current directory, choose one by typing the correct number");

            for (int i = 0; i < videos.Count(); i++)
            {
                Console.WriteLine($" [{i + 1}] \t {Path.GetFileName(videos[i])}");
            }
            var answer = Console.ReadLine();


            if (!int.TryParse(answer, out index) || --index >= videos.Count) {
                Console.WriteLine("Unavaible option");
                return string.Empty;
            }

            Console.WriteLine($"Using '{Path.GetFileName(videos[index])}'");

            return videos[index];
        }

        Console.WriteLine("Couldn't find any video source in the current directory.");

        return string.Empty;
    }

    static string GetOutputPath(string[] args)
    {
        var index = args.IndexOf("-o");
        if (index >= 0)
            if (index + 1 < args.Length)
                return args[index + 1];

        // If there is an source and no output we name the output after the source
        if (string.IsNullOrEmpty(input))
        {
            int fileExtension = input.LastIndexOf(".");
            return input.Insert(fileExtension - 1, " Optical_flow");    // Add right before the extension
        }

        return string.Empty;
    }

    static bool GetDrawOver(string[] args)
        => args.Contains("--drawOver");

    static int GetFrames(string[] args)
    {
        var index = args.IndexOf("--frame");
        if (index >= 0)
            if (index + 1 < args.Length)
                if (int.TryParse(args[index + 1], out int frames))
                    return frames;

        return -1;
    }

    static void Help()
    {
        Console.WriteLine(
            """
            Usage:
                ./CLI.exe -i inputPath -o outputPath --drawOver --frame [int]

            Arguments:
                --drawOver:       Reuse the content of the source video then draw the arrows over
                                  (The source video won't be modified)
                --frame [int]:    The amount of frame to generate, (added to the output)

                -h --help:        Show this help message

            Examples:
                ./CLI.exe cat.mp4 "cat flow.mp4"
                ./ClI.exe my_video.mp4 out.mp4 --frame 60 (only 1 second of video output for a 60fps input video)
            """);
    }
}
