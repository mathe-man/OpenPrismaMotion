namespace CLI;

using FlowResolver;

class Program
{
    static int Main(string[] args)
    {
        if (args.Contains("--help") || args.Contains("-h")) {
            Help();
            return 0;
        }


        var input = GetInputPath(args);
        // We have to stop here if no input as been provided
        if (input == string.Empty)
            return -1;

        var output = GetOutputPath(args);
        var drawOver = GetDrawOver(args);
        var frames = GetFrames(args);

        FlowResolver.GenerateOpticalFlowVideo(
            input, output,
            drawOverFrame: drawOver,
            frameCount: frames
            );

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

        var files = Directory.EnumerateFiles(Directory.GetCurrentDirectory());

        List<string> videos = new();

        // Scan every files in the current directory to find videos
        foreach (var file in files)
            foreach (var extension in VideoExtensions)
                if (file.EndsWith(extension))
                    videos.Add(file);

        
        if (videos.Count() == 1)
        {
            Console.WriteLine($"Found '{videos[0]}', proceed with this video as input ? [Y/n]");
            if (Console.ReadKey().Key == ConsoleKey.Y) {
                Console.WriteLine($"Using '{videos[0]}'");
                return videos[0];
            }
        }
        else if (videos.Count > 1)
        {
            Console.WriteLine("Found multiple videos avaible in the current directory, choose one by typing the correct number");
            
            for (int i = 0; i < videos.Count(); i++)
            {
                Console.WriteLine($" [{i+1}] \t {videos[i]}");

                var answer = Console.ReadLine();


                if (!int.TryParse(answer, out index) || --index >= videos.Count)
                    Console.WriteLine("Unavaible option");

                Console.WriteLine($"Using '{videos[i]}'");
            }
        }

        Console.WriteLine("Couldn't find a video source in the current directory.");

        return string.Empty;
    }

    static string GetOutputPath(string[] args)
    {
        var index = args.IndexOf("-o");
        if (index >= 0)
            if (index + 1 < args.Length)
                return args[index + 1];

        return "Flow Output.mp4";
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
