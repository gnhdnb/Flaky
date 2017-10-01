using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class NoteSource : Source
	{
		protected NoteSource() : base() { }

		protected NoteSource(string id) : base(id) { }

		public abstract PlayingNote GetNote(IContext context);

		protected sealed override Sample NextSample(IContext context)
		{
			return GetNote(context);
		}
	}
}
