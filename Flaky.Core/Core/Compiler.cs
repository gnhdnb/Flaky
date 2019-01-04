using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Compiler
	{
#if DEBUG
		private const OptimizationLevel optimizationLevel = OptimizationLevel.Debug;
#else
		private const OptimizationLevel optimizationLevel = OptimizationLevel.Release;
#endif

		private readonly IEnumerable<MetadataReference> references;

		private readonly ClassTemplate classTemplate;

		public Compiler(Assembly sourcesAssembly)
		{
			var assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

			references = new List<MetadataReference>()
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(sourcesAssembly.Location)
			}
			.Concat(assemblies
				.Select(a => Assembly.ReflectionOnlyLoad(a.FullName).Location)
				.Select(l => MetadataReference.CreateFromFile(l)));

			classTemplate = ClassTemplate.FromEmbededResource("Player.tmp");
		}

		public CompilationResult Compile(string code)
		{
			string assemblyName = Path.GetRandomFileName();

			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(classTemplate.Render("Player", code));

			CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName,
				syntaxTrees: new[] { syntaxTree },
				references: references,
				options: new CSharpCompilationOptions(
					OutputKind.DynamicallyLinkedLibrary, 
					optimizationLevel: optimizationLevel));

			var result = new CompilationResult();

			using (var ms = new MemoryStream())
			{
				EmitResult emitResult = compilation.Emit(ms);

				result.Success = emitResult.Success;

				if (!result.Success)
				{
					IEnumerable<Diagnostic> failures = emitResult.Diagnostics.Where(diagnostic =>
						diagnostic.IsWarningAsError ||
						diagnostic.Severity == DiagnosticSeverity.Error);

					result.Messages = failures
						.Select(diagnostic => $"{diagnostic.Id}: {diagnostic.GetMessage()}")
						.ToArray();
				}
				else
				{
					ms.Seek(0, SeekOrigin.Begin);

					Assembly assembly = Assembly.Load(ms.ToArray());

					Type type = assembly.GetType("Flaky.Player");
					result.Player = (IPlayer)Activator.CreateInstance(type);
				}
			}

			return result;
		}
	}

	internal class ClassTemplate
	{
		private string template;

		internal ClassTemplate(string template)
		{
			this.template = template;
		}

		internal string Render(string className, string code)
		{
			return template.Replace("%CLASSNAME%", className).Replace("%CODE%", code);
		}

		internal static ClassTemplate FromEmbededResource(string resourceName)
		{
			return new ClassTemplate(LoadEmbededResource(resourceName));
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

	public class CompilationResult
	{
		public bool Success { get; set; }
		public string[] Messages { get; set; }
		public IPlayer Player { get; set; }
	}
}
