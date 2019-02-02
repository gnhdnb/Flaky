using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public interface INoteSource : ISource
	{
		PlayingNote GetNote(IContext context);
	}
}
