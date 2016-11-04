using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IFlakyContext : IContext
	{
		TState GetOrCreateState<TState>(string id) where TState : class, new();

		TFactory Get<TFactory>() where TFactory : class;
	}
}
