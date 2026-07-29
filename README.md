# OpenPrismaMotion
An open source software to visualize pixels movement along time from a video


# CLI
The command line interface allow you to generate an Optical Flow video from a source video.

## How to use it

 - Install the executable version corresponding to your operating system (Windows is the only one avaible yet)
 
 - Move the executable in a folder containing at least one video you want to use as a source.

 - Open the folder in wich you placed the executable and your video.

You can directly start by double-clicking it and then choosing your video.

You can also open a terminal (e.g. `cmd` or a `PowerShell`) to pass some arguments.

## Arguments
| Argument     | Following value | Expected type         | Note   |
|:------------:|:---------------:|:---------------------:|:------:|
| `-i`         | input video     | Absolute/Relative path|        |
| `-o`         | output video    | Absolute/Relative path| If an already existing file is given it'll be overwritten|
| `-h` `--help`|                 |                       | Display an help message with the existing arguments|
| `--drawOver` |                 |                       | Use the source in the output, draw the arrows over the source frames, otherwise arrows will be drawed on an empty frame|
| `--frame`    | number of frame | Unsigned int          | Specify how many frame should be generated |

> The `--drawOver` argument won't affect the source video which is never modified

> The `--frame` argument is usefull to avoid generating an entire video wich can take a long time.
> The process still need multiple optimization

## After generation
Once the output is ready it'll be directly viewable using a compatible video viewer.
You'll be notified by a prompt asking if you wan't to open it directly