using Apc40Controller;
using ConsoleDraw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	class Program
	{
		private const int ChannelsCount = 8;
		private static int activeChannel = 0;

		static void Main(string[] args)
		{
			var code =
				Enumerable.Range(0, ChannelsCount)
				.Select(channel => Load(channel))
				.ToList();

			using (var controller = new Controller())
			using (var mixerController = new Apc40MixerAdapter(controller))
			using (var host = new Host(ChannelsCount, GetOutputWaveFilePath(), mixerController))
			{
				Enumerable
					.Range(0, ChannelsCount)
					.ToList()
					.ForEach(channel => host.Recompile(channel, code[channel]));

				WindowManager.SetupWindow();
				WindowManager.SetWindowTitle("F L A K Y");

				var window = new MainWindow();
				window.OnKey += (sender, key) =>
				{
					if (key.Key == ConsoleKey.F5)
						host.Recompile(activeChannel, code[activeChannel]);

					if (key.Key == ConsoleKey.F10)
					{
						window.Exit = true;
					}
				};

				window.SetText(code[activeChannel]);

				window.OnTextChange = (text) =>
				{
					code[activeChannel] = text;
					Save(text, activeChannel);
				};

				controller.OnChannelSelect += (sender, channel) =>
				{
					activeChannel = channel;
					window.SetText(code[activeChannel]);
				};

				host.Play();

				window.MainLoop();

				host.Stop();
			}
		}

		private static void Save(string text, int channel)
		{
			using (var file = File.Open(GetTemporaryCodeFilePath(channel), FileMode.Create))
			using (var writer = new StreamWriter(file))
			{
				writer.Write(text);
				writer.Flush();
			}
		}

		private static string Load(int channel)
		{
			var codeFilePath = GetTemporaryCodeFilePath(channel);

			if (!File.Exists(codeFilePath))
				return GetBasicTemplate(channel);

			using (var file = File.Open(codeFilePath, FileMode.Open))
			using (var reader = new StreamReader(file))
			{
				return reader.ReadToEnd();
			}
		}

		private static string GetBasicTemplate(int channel)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = $"Flaky.Resources.Template{channel}.cs";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		private static string GetTemporaryCodeFilePath(int channel)
		{
			return Path.Combine(GetLocation(), $"temp{channel}.cs");
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
