using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class NoteSource : Source, IFlakyNoteSource
	{
		private IExternalNoteSourceProcessor externalProcessor;

		protected NoteSource() : base() { }

		protected NoteSource(string id) : base(id) { }

		protected abstract PlayingNote NextNote(IContext context);

		public PlayingNote GetNote(IContext context)
		{
			if (externalProcessor != null)
				return externalProcessor.GetNote(context);

			return GetNoteInCurrentThread(context);
		}

		public PlayingNote GetNoteInCurrentThread(IContext context)
		{
			return NextNote(context);
		}

		void IFlakyNoteSource.SetExternalProcessor(IExternalNoteSourceProcessor processor)
		{
			this.externalProcessor = processor;
		}

		protected sealed override Sample NextSample(IContext context)
		{
			return GetNote(context);
		}
	}
}
