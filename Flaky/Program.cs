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

			using (var host = new Host())
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
