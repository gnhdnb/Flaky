using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Configuration
	{
		private readonly Dictionary<Type, object> factories = new Dictionary<Type, object>();

		internal TFactory Get<TFactory>() where TFactory : class
		{
			return (TFactory)factories[typeof(TFactory)];
		}

		public void Register<TFactory>(TFactory factory)
		{
			factories[typeof(TFactory)] = factory;
		}
	}
}
