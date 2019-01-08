using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Flaky.Adapters")]
[assembly: InternalsVisibleTo("Flaky.Core")]
[assembly: InternalsVisibleTo("Flaky.Tests")]
[assembly: InternalsVisibleTo("Flaky.Sources")]
namespace Flaky
{
	public interface ISource : IDisposable
	{
		Vector2 Play(IContext context);
		void Initialize(ISource parent, IContext context);
	}
}
