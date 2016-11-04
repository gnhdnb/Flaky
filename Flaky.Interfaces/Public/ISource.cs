using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface ISource
	{
		Sample Play(IContext context);
		void Initialize(IContext context);
	}
}
