using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Flaky
{
	class Program
	{
		private static Host host;
		private static FileSystemWatcher watcher;
		private static string codeFilePath;
		private static Timer codeWatch;
		private static DateTime lastRecompilationDateTime = new DateTime(0);

		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine(@"Usage: flaky.exe [sampleLibrariesPath] [codefile]");
				return;
			}

			var libraryPath = args[0];
			codeFilePath = args[1];

			PlatformDependant.SetProcessPriority();

			try
			{
				using (host = new Host(1, libraryPath ?? GetLocation(), Path.Combine(GetLocation(), "flaky.wav")))
				{
					Recompile();
					Watch();

					host.Play();

					while(true)
					{
						var key = Console.ReadKey();

						if (key.Key == ConsoleKey.E
							&& (key.Modifiers & ConsoleModifiers.Control) != 0)
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static void Watch()
		{
			lastRecompilationDateTime = DateTime.UtcNow;
			codeWatch = new Timer(CheckForCodeChange, null, 0, 100);
		}

		private static void CheckForCodeChange(object state)
		{
			var lastWriteTime = File.GetLastWriteTimeUtc(codeFilePath);

			if (lastRecompilationDateTime < lastWriteTime)
			{
				lastRecompilationDateTime = lastWriteTime;
				Recompile();
			}
		}

		private static void Recompile()
		{
			Console.Clear();

			var code = Load();
			var errors = host.Recompile(0, code);

			foreach(var error in errors)
			{
				Console.Error.WriteLine(error);
			}
		}

		private static string GetDemoSong()
		{
			using (Stream s = LoadFile("demo.flk"))
			using (var reader = new StreamReader(s))
			{
				return reader.ReadToEnd();
			}
		}

		private static string Load()
		{
			if (!File.Exists(codeFilePath))
			{
				using (var file = File.Open(codeFilePath, FileMode.Create))
				using (var writer = new StreamWriter(file))
				{
					writer.Write(GetDemoSong());

					writer.Flush();
				}
			}

			Exception exception = null;
			int counter = 0;

			while (counter < 25)
			{
				try
				{
					using (var file = File.Open(codeFilePath, FileMode.Open))
					using (var reader = new StreamReader(file))
					{
						return reader.ReadToEnd();
					}
				}
				catch (Exception ex)
				{
					exception = ex;

					Thread.Sleep(20);
				}

				counter++;
			}

			throw exception;
		}

		private static string GetLocation()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		private static Stream LoadFile(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			var resources = assembly.GetManifestResourceNames();
			var resourceName = resources.Single(r => r.EndsWith(fileName));

			return assembly.GetManifestResourceStream(resourceName);
		}
	}
}
