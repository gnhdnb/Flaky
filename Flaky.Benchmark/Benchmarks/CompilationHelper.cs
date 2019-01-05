using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky.Benchmark
{
	public static class CompilationHelper
	{
		public static ISource Compile(string code)
		{
			var compiler = new Compiler(new[] {
				typeof(Source).Assembly,
				typeof(Mixer).Assembly
			});

			var result = compiler.Compile(code);

			return result.Player.CreateSource();
		}

		public static ISource CompileFromResource(string resourceName)
		{
			return Compile(LoadEmbededResource(resourceName));
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
