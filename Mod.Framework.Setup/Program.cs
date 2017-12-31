using NDesk.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Framework.Setup
{
	class Program
	{
		static string root;
		static string references;
		static string dnspy;

		static void Main(string[] args)
		{
			try
			{
				var options = new OptionSet();
				options.Add("root=", "Specifies where the root of the framework is located",
					 opt => root = opt);

				options.Parse(args);

				if (String.IsNullOrWhiteSpace(root) == true)
				{
					options.WriteOptionDescriptions(Console.Out);
					Environment.ExitCode = 1;
					return;
				}

				SetupFramework().Wait();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex);
			}
		}

		static void PrepareDirectories()
		{
			Console.WriteLine("Preparing folder structure");

			references = Path.Combine(root, "References");
			dnspy = Path.Combine(references, "dnSpy");

			Directory.CreateDirectory(references);
			Directory.CreateDirectory(dnspy);
		}

		static async Task SetupFramework()
		{
			PrepareDirectories();

			var release = await FindDnspyRelease("https://api.github.com/repos/0xd4d/dnSpy/releases");
			var path = await DownloadRelease(release);

			ExtractZip(path);
		}

		static async Task<GitHubRelease> FindDnspyRelease(string url)
		{
			Console.WriteLine("Finding latest dnSpy release");
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.UserAgent.Clear();
				client.DefaultRequestHeaders.Add("User-Agent", "ModFrameworkNET");

				var json = await client.GetStringAsync(url);
				var releases = JsonConvert.DeserializeObject<GitHubRelease[]>(json);

				return releases.FirstOrDefault();
			}
		}

		static async Task<FileInfo> DownloadRelease(GitHubRelease release)
		{
			ConsoleColor preserve = Console.ForegroundColor;

			var asset = release.Assets.Single(a => a.Name.Equals("dnSpy.zip", StringComparison.CurrentCultureIgnoreCase));
			Console.WriteLine("Downloading dnSpy release " + asset.BrowserDownloadUrl);

			var savePath = new FileInfo(Path.Combine(dnspy, asset.Name));
			if (savePath.Exists)
			{
				savePath.Delete();
				savePath.Refresh();
			}

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.UserAgent.Clear();
				client.DefaultRequestHeaders.Add("User-Agent", "ModFrameworkNET");

				using (var response = await client.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead))
				{
					// You must use as stream to have control over buffering and number of bytes read/received
					using (var stream = await response.Content.ReadAsStreamAsync())
					using (var writer = savePath.OpenWrite())
					{
						long? max_length = response.Content.Headers.ContentLength;
						if (max_length == null)
						{
							throw new EndOfStreamException();
						}

						var buffer = new byte[2048];
						long position = 0;
						while (position < max_length)
						{
							int read = stream.Read(buffer, 0, buffer.Length);
							writer.Write(buffer, 0, read);
							position += read;

							double progress = Math.Round((position / (double)max_length.Value) * 100.0, 2);

							var line = $" -> {progress:###.00}%";
							Console.CursorLeft = 0;
							Console.Write(line);

							preserve = Console.ForegroundColor;
							Console.ForegroundColor = ConsoleColor.DarkGray;
							Console.Write(" [IN PROGRESS]");
							Console.ForegroundColor = preserve;
						}
					}
				}
			}

			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Write(" [COMPLETED]");
			Console.ForegroundColor = preserve;
			Console.WriteLine();

			return savePath;
		}

		static void ExtractZip(FileInfo file)
		{
			Console.WriteLine($"Extracting {file.FullName} to {file.Directory.FullName}");
			ZipFile.ExtractToDirectory(file.FullName, file.Directory.FullName);
		}
	}

	class GitHubRelease
	{
		[JsonProperty("assets")]
		public IEnumerable<GitHubReleaseAsset> Assets { get; set; }
	}

	class GitHubReleaseAsset
	{
		[JsonProperty("browser_download_url")]
		public string BrowserDownloadUrl { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
