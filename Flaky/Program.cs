using ConsoleDraw;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Flaky
{
	class Program
	{
		static void Main(string[] args)
		{
			var code = Load();

			var host = new Host();

			host.Recompile(code);
			//host.Play();

			WindowManager.UpdateWindow(Console.LargestWindowWidth, Console.LargestWindowHeight);
			WindowManager.SetWindowTitle("F L A K Y");

			//Start Program
			var window = new MainWindow();
			window.OnKey += (sender, key) =>
			{
				if (key.Key == ConsoleKey.F5)
					host.Recompile(window.GetText());
			};

			window.SetText(code);

			window.OnTextChange = () =>
			{
				Save(window.GetText());
			};

			window.MainLoop();

			host.Stop();
		}

		public static void Save(string text)
		{
			using (var file = File.Open(@"D:\temp\flaky.cs", FileMode.Create))
			using (var writer = new StreamWriter(file))
			{
				writer.Write(text);
				writer.Flush();
			}
		}

		public static string Load()
		{
			using (var file = File.Open(@"D:\temp\flaky.cs", FileMode.Open))
			using (var reader = new StreamReader(file))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
