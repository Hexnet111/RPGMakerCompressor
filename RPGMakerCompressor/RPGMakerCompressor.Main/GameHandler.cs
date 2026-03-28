using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using static System.Net.Mime.MediaTypeNames;

namespace Compressor
{
    public class CompressionArgs
    {
        public bool CompressAudio = true;
        public bool CompressImages = true;
        public bool OptimizeImages = true;
    }

    public class GameHandler
    {
        public static readonly string RPGMakerDecryptor = Path.Combine(Program.librariesDirectory, "RPGMakerDecrypter-cli.exe");

        // Decrypts RPG Maker games and moves them to the outputPath provided. 
        public static void Decrypt(string gamePath, string outputPath)
        {
            // TODO: Add support for more than rpg maker mv/mz games.
            Util.WriteLineMultiColored("[Yellow]Decrypting...");

            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                FileName = RPGMakerDecryptor,
                Arguments = $"\"{gamePath}\" --output=\"{outputPath}\" --directories=\"img,audio,data\" --files=\"none\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process decryptor = Process.Start(processInfo);
            decryptor.WaitForExit();

            Util.RewriteLastLine("[Green]Decrypting... Done!", true);
        }
        // Encrypts RPG Maker files back to their original file format and outputs the files into the encrpytedPath provided.
        public static void Encrypt(string gamePath, string outputPath)
        {
            Util.WriteLineMultiColored("[Yellow]Finishing up...");

            // TODO: Add functionality to properly encrypt files.
            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                FileName = RPGMakerDecryptor,
                Arguments = $"\"{gamePath}\" --output=\"{outputPath}\" --reconstruct-project",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // This only works with older rpg maker games (which are currently not supported but i will keep the code here.)
            Process encryptor = Process.Start(processInfo);
            encryptor.WaitForExit();

            Util.RewriteLastLine("[Green]Finishing up... Done!", true);
        }
        // Compresses all png files inside the given game folder.
        public static void Compress(string gamePath, CompressionArgs args)
        {
            // System.json manipulation.
            string dataFolder = Path.Combine(gamePath, "data");
            string systemFile = Path.Combine(dataFolder, "System.json");

            if (Directory.Exists(dataFolder))
            {
                // Remove every other file except the system.json file.
                foreach (string file in Directory.GetFiles(dataFolder))
                {
                    if (Path.GetFileName(file) == "System.json")
                    {
                        continue;
                    }

                    File.Delete(file);
                }

                // Perform edits.
                Dictionary<string, object> systemJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(systemFile));

                systemJson["hasEncryptedImages"] = false;
                systemJson["hasEncryptedAudio"] = !args.CompressAudio;

                // Save to disk.
                File.WriteAllText(systemFile, JsonConvert.SerializeObject(systemJson));
            }

            // Create new compressor object for the current game.
            GameCompressor compressor = new GameCompressor(gamePath);

            // Use FFMPEG to lossly compress audio.
            if (args.CompressAudio)
            {
                compressor.CompressAudio();
            }
            else
            {
                // No need to keep the audio folder if the audio isn't compressed.
                string audioFolder = Path.Combine(gamePath, "audio");

                if (Directory.Exists(audioFolder))
                {
                    Directory.Delete(audioFolder, true);
                }
            }

            // Use PNGQuant to lossly compress the image files.
            if (args.CompressImages)
            {
                compressor.CompressImages();
            }

            // If lossy compression was used, oxipng is optional as it already takes a really long time by itself
            if (args.CompressImages && !args.OptimizeImages)
            {
                return;
            }

            if (args.CompressImages)
            {
                Util.WriteLineMultiColored("[Yellow]Optimizing Results...");
            }
            else
            {
                Util.WriteLineMultiColored("[Yellow]Performing Lossless Image Compression...");
            }

            // Optimize whatever's left using OxiPNG.
            compressor.OptimizeImages();

            if (args.CompressImages)
            {
                Util.RewriteLastLine("[Green]Optimizing Results... Done!", true);
            }
            else
            {
                Util.RewriteLastLine("[Green]Performing Lossless Image Compression... Done!", true);
            }
        }
    }
}
