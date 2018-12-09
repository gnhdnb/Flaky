using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky.Tests
{
	[TestClass]
	public class PerformanceTests
	{
		[TestMethod]
		public void StandardWorkloadTest()
		{
			var compiler = new Compiler(typeof(Source).Assembly);
			var result = compiler.Compile(
			@"
				return 35 % Osc(1);
			"
			);

			var source = result.Player.CreateSource();

			var contextController = new ContextController(44100, 120, new Configuration());

			source.Initialize(new Context(contextController));


			var timer = new Stopwatch();

			timer.Start();
			for (long i = 0; i < 44100; i++)
			{
				contextController.NextSample();

				source.Play(new Context(contextController));
			}
			timer.Stop();

			Assert.Inconclusive($"{timer.ElapsedMilliseconds} ms");
		}
	}
}
