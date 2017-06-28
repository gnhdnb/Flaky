using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class PlayerBase : IPlayer
	{
		protected Chr Chr(Source source, string id)
			{ return new Chr(source, id); }
		protected Delay Delay(Source sound, Source time)
			{ return new Delay(sound, time); }
		protected Delay Delay(Source sound, Source time, string id)
			{ return new Delay(sound, time, id); }
		protected LPFilter LPFilter(Source source, Source cutoff, Source resonance)
			{ return new LPFilter(source, cutoff, resonance); }
		protected BPFilter BPFilter(Source source, Source cutoff, Source resonance)
			{ return new BPFilter(source, cutoff, resonance); }
		protected HPFilter HPFilter(Source source, Source cutoff, Source resonance)
			{ return new HPFilter(source, cutoff, resonance); }
		protected OnePoleLPFilter OnePoleLPFilter(Source source, Source cutoff)
			{ return new OnePoleLPFilter(source, cutoff); }
		protected OnePoleHPFilter OnePoleHPFilter(Source source, Source cutoff)
			{ return new OnePoleHPFilter(source, cutoff); }
		protected MatrixVerb MatrixVerb(Source source)
			{ return new MatrixVerb(source); }
		protected MatrixVerb MatrixVerb(Source source, Source viscosity)
			{ return new MatrixVerb(source, viscosity); }
		protected Overdrive Overdrive(Source source, Source overdrive)
			{ return new Overdrive(source, overdrive); }
		protected Pan Pan(Source sound, Source position)
			{ return new Pan(sound, position); }
		protected Pan Pan(Source sound, Source position, Source width)
			{ return new Pan(sound, position, width); }
		protected Rep Rep(Source source, NoteSource feed, string id)
			{ return new Rep(source, feed, id); }
		protected AD AD(NoteSource source, Source decay)
			{ return new AD(source, decay); }
		protected AD AD(NoteSource source, Source attack, Source decay)
			{ return new AD(source, attack, decay); }
		protected FI FI(float time, string id)
			{ return new FI(time, id); }
		protected FO FO(float time, string id)
			{ return new FO(time, id); }

		protected Seq Seq(IEnumerable<int> notes, Source length)
			{ return new Seq(notes, length); }
		protected Seq Seq(IEnumerable<int> notes, Source length, string id)
			{ return new Seq(notes, length, id); }
		protected Seq Seq(IEnumerable<int> notes, int size, string id)
			{ return new Seq(notes, size, id); }
		protected Seq Seq(IEnumerable<Note> notes, Source length)
			{ return new Seq(notes, length); }
		protected Seq Seq(IEnumerable<Note> notes, Source length, string id)
			{ return new Seq(notes, length, id); }
		protected Seq Seq(IEnumerable<Note> notes, int size, string id)
			{ return new Seq(notes, size, id); }
		protected Seq Seq(string sequence, Source length, string id)
			{ return new Seq(sequence, length, id); }
		protected Seq Seq(string sequence, int size, string id)
			{ return new Seq(sequence, size, id); }

		protected RSeq RSeq(string sequence, Source length, string id)
			{ return new RSeq(sequence, length, id); }
		protected RSeq RSeq(string sequence, int size, string id)
			{ return new RSeq(sequence, size, id); }
		protected RSeq RSeq(IEnumerable<int> notes, Source length, string id)
			{ return new RSeq(notes, length, id); }
		protected RSeq RSeq(IEnumerable<int> notes, int size, string id)
			{ return new RSeq(notes, size, id); }

		protected DoublePendulum DoublePendulum()
			{ return new DoublePendulum(); }
		protected DR DR(NoteSource noteSource, params string[] samples)
			{ return new DR(noteSource, samples); }
		protected Met Met()
			{ return new Met(); }
		protected Noise Noise()
			{ return new Noise(); }

		protected Osc Osc()
			{ return new Osc(); }
		protected Osc Osc(Source frequency, Source amplitude)
			{ return new Osc(frequency, amplitude); }
		protected Osc Osc(Source frequency, Source amplitude, string id)
			{ return new Osc(frequency, amplitude, id); }

		protected Sampler Sampler(string sample, NoteSource noteSource, string id)
			{ return new Sampler(sample, noteSource, id); }

		protected Saw Saw(Source frequency, Source amplitude)
			{ return new Saw(frequency, amplitude); }
		protected Saw Saw(Source frequency, Source amplitude, string id)
			{ return new Saw(frequency, amplitude, id); }
	}
}
