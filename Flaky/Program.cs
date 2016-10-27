using ConsoleDraw;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Flaky
{
	class Program
	{
		static void Main(string[] args)
		{
			var code = Load();

			using (var host = new Host(GetOutputWaveFilePath()))
			{

				host.Recompile(code);
				host.Play();

				//WindowManager.UpdateWindow(Console.LargestWindowWidth, Console.LargestWindowHeight);
				WindowManager.SetupWindow();
				WindowManager.SetWindowTitle("F L A K Y");

				var window = new MainWindow();
				window.OnKey += (sender, key) =>
				{
					if (key.Key == ConsoleKey.F5)
						host.Recompile(window.GetText());

					if (key.Key == ConsoleKey.F10)
					{
						window.Exit = true;
					}
				};

				window.SetText(code);

				window.OnTextChange = (text) =>
				{
					Save(text);
				};

				window.MainLoop();

				host.Stop();
			}
		}

		private static void Save(string text)
		{
			using (var file = File.Open(GetTemporaryCodeFilePath(), FileMode.Create))
			using (var writer = new StreamWriter(file))
			{
				writer.Write(text);
				writer.Flush();
			}
		}

		private static string Load()
		{
			var codeFilePath = GetTemporaryCodeFilePath();

			if (!File.Exists(codeFilePath))
				return GetBasicTemplate();

			using (var file = File.Open(codeFilePath, FileMode.Open))
			using (var reader = new StreamReader(file))
			{
				return reader.ReadToEnd();
			}
		}

		private static string GetBasicTemplate()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "Flaky.Resources.template.cs";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		private static string GetTemporaryCodeFilePath()
		{
			return Path.Combine(GetLocation(), "temp.cs");
		}

		private static string GetOutputWaveFilePath()
		{
			return Path.Combine(GetLocation(), "flaky.wav");
		}

		private static string GetLocation()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
	}
}
