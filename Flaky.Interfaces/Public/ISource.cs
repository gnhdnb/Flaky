using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface ISource : IDisposable
	{
		Sample Play(IContext context);
		void Init(IContext context);
	}
}
