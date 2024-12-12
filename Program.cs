using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        string path = AppDomain.CurrentDomain.BaseDirectory;
        Console.WriteLine($"Current directory: {path}");
        // Remove the path of the bin folder
        string badPath = "bin\\Debug\\net8.0\\";
        path = path.Replace(badPath, "");
        path = path + "Gifs\\";
        Console.WriteLine($"New directory: {path}");

        // Search for all GIF files in the directory
        string[] files = Directory.GetFiles(path, "*.gif");
        if (files.Length == 0)
        {
            Console.WriteLine("No GIF files found in the directory.");
            return;
        }

        int selectedIndex = 0;
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Select a GIF to play:");
            for (int i = 0; i < files.Length; i++)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
                if (i == selectedIndex)
                {
                    Console.WriteLine($"> {fileNameWithoutExtension}");
                }
                else
                {
                    Console.WriteLine($"  {fileNameWithoutExtension}");
                }
            }

            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex == 0) ? files.Length - 1 : selectedIndex - 1;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex == files.Length - 1) ? 0 : selectedIndex + 1;
            }
            else if (key == ConsoleKey.Enter)
            {
                Console.Clear();
                PlayGif(files[selectedIndex], 10); // 10 FPS
                break;
            }
        }
    }

    public static void PlayGif(string filepath, int fps)
    {
        if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            using (Image gifImage = Image.FromFile(filepath))
            {
                FrameDimension dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
                int frameCount = gifImage.GetFrameCount(dimension);

                // Preload all frames into memory
                List<Bitmap> frames = new List<Bitmap>();
                for (int i = 0; i < frameCount; i++)
                {
                    gifImage.SelectActiveFrame(dimension, i);
                    frames.Add(new Bitmap(gifImage));
                }

                // Calculate refresh rate in milliseconds
                int refreshRate = 1000 / fps;

                // Initialize previous frame buffer
                string[] previousFrame = new string[Console.WindowHeight];
                for (int i = 0; i < previousFrame.Length; i++)
                {
                    previousFrame[i] = new string(' ', Console.WindowWidth);
                }

                while (true)
                {
                    foreach (var frame in frames)
                    {
                        Console.SetCursorPosition(0, 0);
                        string[] currentFrame = PrintImage(frame);

                        // Update only the parts that have changed
                        for (int y = 0; y < currentFrame.Length; y++)
                        {
                            if (currentFrame[y] != previousFrame[y])
                            {
                                Console.SetCursorPosition(0, y);
                                Console.Write(currentFrame[y]);
                            }
                        }

                        previousFrame = currentFrame;
                        Thread.Sleep(refreshRate);
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Bitmap processing is only supported on Windows.");
        }
    }

    public static string[] PrintImage(Bitmap bitmap)
    {
        StringBuilder sb = new StringBuilder();
        int scaledHeight = bitmap.Height / 2; // Adjust this value to maintain aspect ratio
        string[] frameBuffer = new string[scaledHeight];
        for (int y = 0; y < scaledHeight; y++)
        {
            sb.Clear();
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixelColor = bitmap.GetPixel(x, y * 2); // Skip every other row
                sb.Append(GetAnsiColor(pixelColor) + "█" + "\u001b[0m");
            }
            frameBuffer[y] = sb.ToString();
        }
        return frameBuffer;
    }

    public static string GetAnsiColor(Color color)
    {
        int r = color.R;
        int g = color.G;
        int b = color.B;
        return $"\u001b[38;2;{r};{g};{b}m";
    }
}
