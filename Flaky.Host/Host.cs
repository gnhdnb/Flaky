﻿	using NAudio.Midi;
	using NAudio.Wave;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	namespace Flaky
	{
	public class Host : IDisposable
	{
		private Compiler Compiler { get; }
		private Mixer Mixer { get; }
		private IAudioDevice Device { get; }

		public Host(
			int channelsCount,
			string libraryPath,
			string outputWaveFilePath = null,
			int bufferSize = 13230)
		{
			var configuration = new Configuration();

			configuration.Register<IWaveReaderFactory>(new WaveReaderFactory(libraryPath));
			configuration.Register<IWaveWriterFactory>(new WaveWriterFactory());
			configuration.Register<IErrorOutput>(new ConsoleErrorOutput());
			configuration.Register<IWebClient>(new WebClient());
			configuration.Register<IAudioStreamReader>(new AudioStreamReader());

			Compiler = new Compiler(new[] {
				typeof(Source).Assembly,
				typeof(Mixer).Assembly
			});

			Device = PlatformDependent.GetAudioDevice();
			Mixer = new Mixer(channelsCount, 44100, bufferSize, 120, configuration);

			Device.Init(Mixer, outputWaveFilePath);
		}

		public string[] SetPlayer(int channel, IPlayer player)
		{
			var initErrors = Mixer.ChangePlayer(channel, player);

			if (initErrors.Any())
				return initErrors;

			return new string[0];
		}

		public string[] Recompile(int channel, string code)
		{
			CompilationResult result;

			try
			{
				result = Compiler.Compile(code);
			}
			catch (Exception ex)
			{
				return new[] { ex.ToString() };
			}

			if (!result.Success)
				return result.Messages;

			return SetPlayer(channel, result.Player);
		}

		public void Play()
		{
			Device.Play();
		}

		public void Stop()
		{
			Device.Stop();
		}

		public void Dispose()
		{
			Device.Dispose();
			Mixer.Dispose();
		}
	}
	}
