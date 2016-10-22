using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal struct Context : IContext
	{
		public long Sample { get; set; }
	}
}
