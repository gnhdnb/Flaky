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
	[MemoryDiagnoser]
	public class StandardWorkload
	{
		private ContextController contextController;
		private ISource source;

		[GlobalSetup]
		public void Setup()
		{
			source = CompilationHelper.CompileFromResource("StandardWorkload.flk");

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
	}
}
