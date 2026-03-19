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
        public static readonly string processingDirectory = Path.Combine(exeDirectory, "Processing");
        public static readonly string librariesDirectory = Path.Combine(exeDirectory, "Libraries");

        static void CreateIfNotExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
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
            CreateIfNotExist(processingDirectory);

            CompressionArgs compressionArgs = new CompressionArgs();

            string gamePath = Util.QuestionInput("[Yellow]Input game path: ", true);

            Console.Clear();

            if (!Directory.Exists(gamePath))
            {
                Console.WriteLine("Invalid game path.");
                Exit();
                return;
            }

            compressionArgs.CompressAudio = Util.QuestionInput("[Yellow]Compress audio? ([Green]Y[Yellow]/[Red]N[Yellow]): ", true) == "y";
            Console.Clear();

            compressionArgs.CompressImages = Util.QuestionInput("[Yellow]Use lossy compression? ([Green]Y[Yellow]/[Red]N[Yellow]): ", true) == "y";
            Console.Clear();

            if (compressionArgs.CompressImages)
            {
                compressionArgs.OptimizeImages = Util.QuestionInput("[Yellow]Optimize results? This process may take a long time. ([Green]Y[Yellow]/[Red]N[Yellow]): ", true) == "y";
                Console.Clear();
            }

            Util.WriteLineMultiColored("[Yellow]Decrypting...");

            string gameFolderName = Path.GetFileName(gamePath);
            string decryptedGamePath = Path.Combine(processingDirectory, gameFolderName);
            string outputGamePath = Path.Combine(outputDirectory, gameFolderName);

            GameHandler.Decrypt(gamePath, decryptedGamePath);

            Util.RewriteLastLine("[Green]Decrypting... Done!", true);

            GameHandler.Compress(decryptedGamePath, compressionArgs);

            Util.WriteLineMultiColored("[Yellow]Finishing up...");

            GameHandler.Encrypt(decryptedGamePath, outputGamePath);

            if (Directory.Exists(decryptedGamePath))
            {
                Directory.Delete(decryptedGamePath, true);
            }

            Util.RewriteLastLine("[Green]Finishing up... Done!", true);

            Util.WriteLineColored("Complete!", ConsoleColor.Green);

            Exit();
        }
    }
}
