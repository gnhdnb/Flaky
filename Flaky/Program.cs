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

		static void Main(string[] args)
        {
			SetProcessPriority();

			host = new Host(1, Path.Combine(GetLocation(), "flaky.wav"));

			Recompile();
			Watch();

			host.Play();
		}

		private static void Watch()
		{
			watcher = new FileSystemWatcher();

			var temporaryCodePath = GetTemporaryCodeFilePath();

			watcher.Path = Path.GetDirectoryName(temporaryCodePath);
			watcher.Filter = Path.GetFileName(temporaryCodePath);

			watcher.Changed += new FileSystemEventHandler(OnCodeChange);

			watcher.EnableRaisingEvents = true;
		}

		private static void OnCodeChange(object source, FileSystemEventArgs e)
		{
			Recompile();
		}

		private static void Recompile()
		{
			var code = Load();
			var errors = host.Recompile(0, code);

			foreach(var error in errors)
			{
				Console.Error.WriteLine(error);
			}
		}

		private static void SetProcessPriority()
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
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
			var codeFilePath = GetTemporaryCodeFilePath();

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

			while (counter < 3)
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

		private static string GetTemporaryCodeFilePath()
		{
			return Path.Combine(GetLocation(), "temp.cs");
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
