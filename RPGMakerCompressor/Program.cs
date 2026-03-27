using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Utility;

namespace Compressor
{
    internal class Program
    {
        public static readonly string exeDirectory = AppContext.BaseDirectory;
        public static readonly string outputDirectory = Path.Combine(exeDirectory, "Output");
        public static readonly string processingDirectory = Path.Combine(Path.GetTempPath(), "RPGMakerCompressor");
        public static readonly string librariesDirectory = Path.Combine(exeDirectory, "Libraries");

        static void CreateIfNotExist(string directoryPath, bool DeleteIfExists=false)
        {
            if (DeleteIfExists && Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
            else if (Directory.Exists(directoryPath))
            {
                return;
            }

            Directory.CreateDirectory(directoryPath);
        }

        static void Exit()
        {
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            // Install dependencies on first boot.
            bool firstTimeSetup = !Installer.IsValid().GetAwaiter().GetResult();

            if (firstTimeSetup)
            {
                Console.Clear();
                Console.WriteLine("Please re-launch the program to finish the installation.");

                Exit();
                return;
            }

            // Create required directories on first use.
            CreateIfNotExist(outputDirectory);
            CreateIfNotExist(processingDirectory, true);

            CompressionArgs compressionArgs = new CompressionArgs();

            string gamePath = Util.QuestionInput("[Gray]Input game path: ", true);

            if (!Directory.Exists(gamePath))
            {
                Console.WriteLine("Invalid game path.");
                Exit();
                return;
            }

            compressionArgs.CompressAudio = Util.QuestionInput("[Gray]Compress audio? ([Green]Y[Yellow]/[Red]N[Gray]): ", true, true).ToLower() == "y";
            compressionArgs.CompressImages = Util.QuestionInput("[Gray]Use lossy compression? ([Green]Y[Yellow]/[Red]N[Gray]): ", true, true).ToLower() == "y";

            if (compressionArgs.CompressImages)
            {
                compressionArgs.OptimizeImages = Util.QuestionInput("[Gray]Optimize results? This process may take a long time. ([Green]Y[Yellow]/[Red]N[Gray]): ", true, true).ToLower() == "y";
            }

            Console.Clear();

            string gameFolderName = Path.GetFileName(gamePath);
            string decryptedGamePath = Path.Combine(processingDirectory, gameFolderName);
            string outputGamePath = Path.Combine(outputDirectory, gameFolderName);

            GameHandler.Decrypt(gamePath, decryptedGamePath);

            long preCompressionSize = Util.GetFolderSize(decryptedGamePath);

            //GameHandler.Compress(decryptedGamePath, compressionArgs);

            long postCompressionSize = Util.GetFolderSize(decryptedGamePath);

            GameHandler.Encrypt(decryptedGamePath, outputGamePath);

            if (Directory.Exists(decryptedGamePath))
            {
                Directory.Delete(decryptedGamePath, true);
            }

            Util.WriteLineMultiColored("[Green]Complete!");
            Util.WriteLineMultiColored($"Size reduced by [Green]{Math.Floor((decimal)(postCompressionSize / preCompressionSize * 100.0))}%");

            Exit();
        }
    }
}
