using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Compressor
{
    internal class FileManager
    {
        public static void ZipExtract(
            string zipFilePath,
            bool deleteUponFinish = false //*Optional* Delete zip file upon finish.
            )
        {
            ZipFile.ExtractToDirectory(zipFilePath, Path.Combine(Path.GetDirectoryName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath)));

            if (deleteUponFinish)
            {
                File.Delete(zipFilePath);
            }
        }
        private static async Task<JObject> GithubParse(string URL, string versionMatch)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RPGMaker Compressor");

                string json = await client.GetStringAsync(URL);
                JObject URLData = JObject.Parse(json);

                // Sanity check
                if (URLData["status"] != null || URLData["assets"] == null)
                {
                    return null;
                }

                JArray assets = (JArray)URLData["assets"];

                // Find the specific link we are searching for using the versionMatch.
                foreach (JObject asset in assets)
                {
                    string fileName = (string)asset["name"];

                    if (fileName.Contains(versionMatch))
                    {
                        return asset;
                    }
                }

                return null;
            }
        }
        public static async Task<string> Download(
            string URL,
            string saveTo,
            string fileName
            )
        {
            ProgressBar progressBar = new ProgressBar();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage headerResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, URL));
                long fileSize = headerResponse.Content.Headers.ContentLength.Value;

                client.DefaultRequestHeaders.UserAgent.ParseAdd("RPGMaker Compressor");

                HttpResponseMessage response = await client.GetAsync(URL, HttpCompletionOption.ResponseHeadersRead);
                Stream byteStream = await response.Content.ReadAsStreamAsync();
                FileStream fileStream = new FileStream(Path.Combine(saveTo, fileName), FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[8192];
                long totalRead = 0;
                int read;

                while ((read = await byteStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;

                    progressBar.Report((double)totalRead / fileSize);
                }

                response.Dispose();
                byteStream.Dispose();
                fileStream.Dispose();
                progressBar.Dispose();
            }

            return Path.Combine(saveTo, fileName);
        }
        public static async Task<string> GithubDownload(
            string URL,
            string versionMatch,
            string saveTo,
            string fileName
            )
        {
            JObject githubAsset = await GithubParse(URL, versionMatch);

            if (githubAsset == null)
            {
                return "";
            }

            string fileDownloadURL = (string)githubAsset["browser_download_url"];

            return await Download(fileDownloadURL, saveTo, fileName);
        }
        public static async Task<bool> Install(
            string URL,
            string downloadPath,
            string downloadName
            )
        {
            string downloadedFile = await Download(URL, downloadPath, downloadName);

            ZipExtract(downloadedFile, deleteUponFinish: true);

            return true;
        }
        public static async Task<bool> GithubInstall(
            string URL,
            string versionMatch,
            string downloadPath,
            string downloadName
            )
        {
            string downloadedFile = await GithubDownload(URL, versionMatch, downloadPath, downloadName);

            ZipExtract(downloadedFile, deleteUponFinish: true);

            return true;
        }
    }
}
