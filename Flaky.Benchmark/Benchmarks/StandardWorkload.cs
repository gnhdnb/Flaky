using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky.Benchmark
{
	public class StandardWorkload
	{
		private ContextController contextController;
		private ISource source;

		[GlobalSetup]
		public void Setup()
		{
			var compiler = new Compiler(typeof(Source).Assembly);
			var result = compiler.Compile(LoadEmbededResource("StandardWorkload.flk"));

			source = result.Player.CreateSource();

			contextController = new ContextController(44100, 120, new Configuration());

			source.Initialize(new Context(contextController));
		}

		[Benchmark]
		public void StandardWorkloadTest()
		{

			for (long i = 0; i < 44100; i++)
			{
				contextController.NextSample();

				source.Play(new Context(contextController));
			}
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			source.Dispose();
		}

		private static string LoadEmbededResource(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			var resources = assembly.GetManifestResourceNames();
			var resourceName = resources.Single(r => r.EndsWith(fileName));

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
