using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Compressor
{
    internal class GameCompressor
    {
        public static readonly string RPGMakerDecryptor = Path.Combine(Program.librariesDirectory, "RPGMakerDecrypter-cli.exe");
        public static readonly string OxiPNG = Path.Combine(Program.librariesDirectory, "oxipng.exe");
        public static readonly string PNGQuant = Path.Combine(Program.librariesDirectory, "pngquant.exe");
        public static readonly string FFMPEG = Path.Combine(Program.librariesDirectory, "ffmpeg\\ffmpeg.exe");

        private static readonly string _PNGQuantArgs = "--ext .png --quality=80-95 --skip-if-larger --force";
        private static readonly string _OxiPNGArgs = "-o 2 --alpha *.png";
        private static readonly string _FFMPEGArgs = "-c:a libvorbis -q:a 7 -y -loglevel quiet";

        private string gamePath;
        private string[] allFiles = null;
        private int totalImages = 0;
        private int totalAudio = 0;

        public GameCompressor(string gamePath)
        {
            this.gamePath = gamePath;
            this.allFiles = Directory.GetFiles(gamePath, "*.*", SearchOption.AllDirectories);

            // Set up the total counts for progress bars during the compression stage.
            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) == ".ogg")
                {
                    this.totalAudio += 1;
                }
                else if (Path.GetExtension(file) == ".png")
                {
                    this.totalImages += 1;
                }
            }
        }
        public void CompressVideo()
        {
            // Dummy method.
            // To be implemented in the future.
        }
        public void CompressAudio()
        {
            Util.WriteLineMultiColored("[Yellow]Compressing Audio...");

            ProgressBar audioCompressionBar = new ProgressBar();

            int totalProcessed = 0;

            ProcessStartInfo FFMPEGProcessInfo = new ProcessStartInfo()
            {
                FileName = FFMPEG,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) != ".ogg")
                {
                    continue;
                }

                totalProcessed += 1;

                audioCompressionBar.Report((double)(totalProcessed) / (double)(totalAudio));

                string fileName = Path.GetFileNameWithoutExtension(file);
                string newFilePath = Path.Combine(Path.GetDirectoryName(file), $"{fileName}_compressed.ogg");;

                FFMPEGProcessInfo.Arguments = $"-i \"{file}\" {_FFMPEGArgs} \"{newFilePath}\"";
                Process.Start(FFMPEGProcessInfo).WaitForExit();

                int fileKept = Util.KeepSmallestFile(file, newFilePath);

                // If the 2nd file is kept (the compressed file), rename it to the original file's name.
                if (fileKept == 2)
                {
                    File.Move(newFilePath, file);
                }
            }

            audioCompressionBar.Dispose();

            Util.RewriteLastLine("[Green]Compressing Audio... Done!", true);
        }
        public void CompressImages()
        {
            int totalProcessed = 0;

            Util.WriteLineMultiColored("[Yellow]Performing Lossy Image Compression...");

            ProgressBar lossyCompressionBar = new ProgressBar();

            ProcessStartInfo pngQuantProcessInfo = new ProcessStartInfo()
            {
                FileName = PNGQuant,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) != ".png")
                {
                    continue;
                }

                totalProcessed += 1;

                lossyCompressionBar.Report((double)(totalProcessed) / (double)(totalImages));

                pngQuantProcessInfo.Arguments = $"\"{file}\" {_PNGQuantArgs}";
                Process.Start(pngQuantProcessInfo).WaitForExit();
            }

            lossyCompressionBar.Dispose();

            Util.RewriteLastLine("[Green]Performing Lossy Image Compression... Done!", true);
        }
        public void OptimizeImages()
        {
            int totalProcessed = 0;

            ProgressBar losslessCompressionBar = new ProgressBar();

            ProcessStartInfo oxiProcessInfo = new ProcessStartInfo()
            {
                FileName = OxiPNG,
                Arguments = $"\"{gamePath}\" ",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) != ".png")
                {
                    continue;
                }

                totalProcessed += 1;

                losslessCompressionBar.Report((double)(totalProcessed) / (double)(totalImages));

                oxiProcessInfo.Arguments = $"\"{file}\" {_OxiPNGArgs}";
                Process.Start(oxiProcessInfo).WaitForExit();
            }

            losslessCompressionBar.Dispose();
        }
    }
}
