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
	public class Filter
	{
		private ContextController contextController;
		private ISource source;

		[GlobalSetup]
		public void Setup()
		{
			source = CompilationHelper.Compile(
			@"
				return 3000 % Osc(1) % LP(0.2, 0.1, ""lp1"");
			");

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
