using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utility;

namespace Compressor
{
    internal class Installer
    {
        private static readonly string ffmpegGithubURL = "https://api.github.com/repos/BtbN/FFmpeg-Builds/releases/latest";
        private static readonly string oxipngGithubURL = "https://api.github.com/repos/oxipng/oxipng/releases/latest";
        private static readonly string pngquantURL = "https://pngquant.org/pngquant-windows.zip";
        private static readonly string rpgMakerDecrypterURL = "https://api.github.com/repos/Hexnet111/RPGMakerDecrypter/releases/latest";

        private static bool CheckLibrariesFolder()
        {
            if (!Directory.Exists(Program.librariesDirectory))
            {
                return false;
            }

            if (!File.Exists(GameCompressor.RPGMakerDecryptor) || !File.Exists(GameCompressor.PNGQuant) || !File.Exists(GameCompressor.OxiPNG) || !File.Exists(GameCompressor.FFMPEG))
            {
                return false;
            }

            return true;
        }

        public async static Task<bool> IsValid()
        {
            bool InstallationValid = CheckLibrariesFolder();

            if (!InstallationValid)
            {
                Console.WriteLine("Preparing first time install...");

                // Folder setup.
                if (Directory.Exists(Program.librariesDirectory))
                {
                    Directory.Delete(Program.librariesDirectory, true);
                }

                Directory.CreateDirectory(Program.librariesDirectory);

                // FFMPEG Installation
                Util.WriteLineMultiColored("[Yellow]Installing ffmpeg...");

                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    await FileManager.GithubInstall(ffmpegGithubURL, downloadName: "ffmpeg.zip", downloadPath: Program.librariesDirectory, versionMatch: "win64-gpl");
                }
                else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                {
                    await FileManager.GithubInstall(ffmpegGithubURL, downloadName: "ffmpeg.zip", downloadPath: Program.librariesDirectory, versionMatch: "winarm64-gpl");
                }

                string ffmpegLibraryFolder = Path.Combine(Program.librariesDirectory, "ffmpeg");
                string ffmpegExtracted = Directory.GetDirectories(ffmpegLibraryFolder)[0];

                foreach (string file in Directory.GetFiles(ffmpegExtracted, "bin/*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file) == ".exe" && Path.GetFileNameWithoutExtension(file) != "ffmpeg")
                    {
                        continue;
                    }

                    File.Move(file, Path.Combine(ffmpegLibraryFolder, Path.GetFileName(file)));
                }

                Directory.Delete(ffmpegExtracted, true);

                Util.RewriteLastLine("[Green]Installing ffmpeg... Done!", true);

                // OxiPNG Installation
                Util.WriteLineMultiColored("[Yellow]Installing oxipng...");

                await FileManager.GithubInstall(oxipngGithubURL, downloadName: "oxipng.zip", downloadPath: Program.librariesDirectory, versionMatch: "pc-windows-msvc");

                string oxipngLibraryFolder = Path.Combine(Program.librariesDirectory, "oxipng");
                string oxipngExtracted = Directory.GetDirectories(oxipngLibraryFolder)[0];

                File.Move(Path.Combine(oxipngExtracted, "oxipng.exe"), Path.Combine(Program.librariesDirectory, "oxipng.exe"));

                Directory.Delete(oxipngLibraryFolder, true);

                Util.RewriteLastLine("[Green]Installing oxipng... Done!", true);

                // PNGQuant Installation
                Util.WriteLineMultiColored("[Yellow]Installing pngquant2...");

                await FileManager.Install(pngquantURL, downloadPath: Program.librariesDirectory, downloadName: "pngquant.zip");

                string pngquantLibraryFolder = Path.Combine(Program.librariesDirectory, "pngquant");
                string pngquantExtracted = Directory.GetDirectories(pngquantLibraryFolder)[0];

                File.Move(Path.Combine(pngquantExtracted, "pngquant.exe"), Path.Combine(Program.librariesDirectory, "pngquant.exe"));

                Directory.Delete(pngquantLibraryFolder, true);

                Util.RewriteLastLine("[Green]Installing pngquant2... Done!", true);

                // RPGMakerDecrypter Installation
                Util.WriteLineMultiColored("[Yellow]Installing RPGMakerDecryptor...");

                await FileManager.GithubDownload(rpgMakerDecrypterURL, versionMatch: "cli.exe", Program.librariesDirectory, "RPGMakerDecrypter-cli.exe");

                Util.RewriteLastLine("[Green]Installing RPGMakerDecryptor... Done!", true);

                return false;
            }

            return true;
        }
    }
}
