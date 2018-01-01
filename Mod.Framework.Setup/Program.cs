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
				Console.WriteLine("Press any key to exit");
				Console.ReadKey();
			}
		}

		static void PrepareDirectories()
		{
			Console.WriteLine("Preparing folder structure");

			references = Path.Combine(root, "References");
			dnspy = Path.Combine(references, "dnSpy");

			Directory.CreateDirectory(references);

			var dnspy_info = new DirectoryInfo(dnspy);
			if (dnspy_info.Exists)
				dnspy_info.Delete(true);

			dnspy_info.Create();
			dnspy_info.Refresh();
		}

		static async Task SetupFramework()
		{
			PrepareDirectories();

			var release = await FindDnspyRelease("https://api.github.com/repos/0xd4d/dnSpy/releases");
			var path = await DownloadRelease(release);

			ExtractZip(path);

			PatchEditMethodCodeVM();
			PatchUtils();
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

		static void PatchEditMethodCodeVM()
		{
			string path = Path.Combine(dnspy, "dnSpy.AsmEditor.x.dll");
			Console.WriteLine("Patching " + path);

			var assembly = System.IO.File.ReadAllBytes(path);
			Environment.CurrentDirectory = Path.GetDirectoryName(path);
			using (var ms = new MemoryStream(assembly))
			{
				var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(ms);

				foreach (var typeName in new[] {
					"dnSpy.AsmEditor.Compiler.EditMethodCodeVM",
					"dnSpy.AsmEditor.Compiler.EditCodeVMCreator",
					"dnSpy.AsmEditor.Compiler.EditCodeVM",
					"dnSpy.AsmEditor.UndoRedo.IUndoCommandService",
					"dnSpy.AsmEditor.UndoRedo.UndoCommandService",
					//"dnSpy.AsmEditor.Commands.CodeContext",
					//"dnSpy.AsmEditor.Commands.CodeContextMenuHandler"
				})
				{
					var type = asm.Modules
						.SelectMany(m => m.Types)
						.Single(t => t.FullName.Equals(typeName, StringComparison.CurrentCultureIgnoreCase));

					type.IsPublic = true;
					type.IsNotPublic = false;
				}

				asm.Write(path);
			}
		}

		static void PatchUtils()
		{
			string path = Path.Combine(dnspy, "dnSpy.Contracts.DnSpy.dll");
			Console.WriteLine("Patching " + path);

			var assembly = System.IO.File.ReadAllBytes(path);
			Environment.CurrentDirectory = Path.GetDirectoryName(path);
			using (var ms = new MemoryStream(assembly))
			{
				var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(ms);

				foreach (var typeName in new[] {
					"dnSpy.Contracts.Utilities.UIUtilities"
				})
				{
					var type = asm.Modules
						.SelectMany(m => m.Types)
						.Single(t => t.FullName.Equals(typeName, StringComparison.CurrentCultureIgnoreCase));

					type.IsPublic = true;
					type.IsNotPublic = false;
				}

				asm.Write(path);
			}
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
