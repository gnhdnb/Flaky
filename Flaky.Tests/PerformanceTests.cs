using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
			var result = compiler.Compile(LoadEmbededResource("StandardWorkload.flk"));

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
