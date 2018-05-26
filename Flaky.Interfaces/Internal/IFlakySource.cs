using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	interface IFlakySource : ISource
	{
		void SetExternalProcessor(IExternalSourceProcessor processor);
		Sample PlayInCurrentThread(IContext context);
	}
}
