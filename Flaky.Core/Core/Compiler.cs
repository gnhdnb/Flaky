﻿using Microsoft.CodeAnalysis;
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

		private readonly MetadataReference[] references;

		public Compiler(Assembly sourcesAssembly)
		{
			references = new MetadataReference[]
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(IPlayer).Assembly.Location),
				MetadataReference.CreateFromFile(sourcesAssembly.Location)
			};
		}

		public CompilationResult Compile(string code)
		{
			string assemblyName = Path.GetRandomFileName();

			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

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

	public class CompilationResult
	{
		public bool Success { get; set; }
		public string[] Messages { get; set; }
		public IPlayer Player { get; set; }
	}
}
