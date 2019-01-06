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
		}

		static void RunRelease()
		{
			BenchmarkRunner.Run<StandardWorkload>();
			BenchmarkRunner.Run<FilterWorkload>();
			BenchmarkRunner.Run<FourierWorkload>();
		}

		static void RunDebug()
		{
			var s = new StandardWorkload();
			s.Setup();
			s.StandardWorkloadTest();

			var filter = new FilterWorkload();
			filter.Setup();
			filter.FilterWorkloadTest();

			var fourier = new FourierWorkload();
			fourier.Setup();
			fourier.FourierWorkloadTest();
		}
	}
}
