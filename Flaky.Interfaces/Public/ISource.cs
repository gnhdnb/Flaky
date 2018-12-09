using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface ISource : IDisposable
	{
		Vector2 Play(IContext context);
		void Initialize(IContext context);
	}
}
