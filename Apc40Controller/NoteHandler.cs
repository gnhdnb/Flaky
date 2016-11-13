using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apc40Controller
{
	internal class Handler<T>
	{
		private Func<T, bool> canHandle;
		private Action<T> handle;

		internal Handler(Func<T, bool> canHandle, Action<T> handle)
		{
			this.canHandle = canHandle;
			this.handle = handle;
		}

		internal void HandleEvent(T e)
		{
			if (!canHandle(e))
				return;

			handle(e);
		}
	}

	internal class NoteHandler : Handler<NoteEvent>
	{
		internal NoteHandler(Func<NoteEvent, bool> canHandle, Action<NoteEvent> handle) 
			: base(canHandle, handle) { }
	}

	internal class CcHandler : Handler<ControlChangeEvent>
	{
		internal CcHandler(Func<ControlChangeEvent, bool> canHandle, Action<ControlChangeEvent> handle)
			: base(canHandle, handle) { }
	}
}
