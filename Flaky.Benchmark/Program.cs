using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky.Benchmark
{
	class Program
	{
		static void Main(string[] args)
		{
#if DEBUG
			RunDebug();
#else
			RunRelease();
#endif
			Console.ReadLine();
		}

		static void RunRelease()
		{
			BenchmarkRunner.Run<StandardWorkload>();
		}

		static void RunDebug()
		{
			var s = new StandardWorkload();
			s.Setup();
			s.StandardWorkloadTest();
		}
	}
}
