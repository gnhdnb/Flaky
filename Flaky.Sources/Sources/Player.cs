using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class PlayerBase : IPlayer
	{
		protected Recorder Recorder(string id, params Source[] sources)
			{ return new Recorder(id, sources); }
		protected Looper Looper(string sample, string id)
			{ return new Looper(sample, id); }

		protected ThreeBodiesOscillator TBO()
			{ return new ThreeBodiesOscillator(); }
		protected ThreeBodiesOscillator TBO(Source timeFactor)
			{ return new ThreeBodiesOscillator(timeFactor); }

		protected DoublePendulum DoublePendulum()
			{ return new DoublePendulum(); }

		protected PipingSourceWrapper Fourier(float effect, string id)
			{ return Pipe(new Fourier(effect, id)); }
		protected PipingSourceWrapper Trans(Source pitch, string id)
			{ return Pipe(new Transient(pitch, id)); }
		protected PipingSourceWrapper Trans(NoteSource pitch, string id)
			{ return Pipe(new Transient(pitch, id)); }
		protected PipingSourceWrapper Trans(NoteSource pitch, Source sensitivity, string id)
			{ return Pipe(new Transient(pitch, sensitivity, id)); }

		protected PipingSourceWrapper Trans(NoteSource pitch, Source sensitivity, Source trigger, string id)
			{ return Pipe(new Transient(pitch, sensitivity, trigger, id)); }
		protected PipingSourceWrapper Trans(Source pitch, Source sensitivity, string id)
			{ return Pipe(new Transient(pitch, sensitivity, id)); }
		protected PipingSourceWrapper Trans(Source pitch, Source sensitivity, Source trigger, string id)
			{ return Pipe(new Transient(pitch, sensitivity, trigger, id)); }

		protected PipingSourceWrapper Chr(string id)
			{ return Pipe(new Chr(id)); }

		protected PipingSourceWrapper Delay(Source time, string id)
			{ return Pipe(new Delay(time, id)); }
		protected PipingSourceWrapper Delay(Source time, Func<Source, Source> transform, string id)
			{ return Pipe(new Delay(time, transform, id)); }
		protected PipingSourceWrapper Delay(Source time, Func<Source, Source> transform, Source dryWet, string id)
			{ return Pipe(new Delay(time, transform, dryWet, id)); }

		protected PipingSourceWrapper LP(Source cutoff, Source resonance, string id)
			{ return Pipe(new LPFilter(cutoff, resonance, id)); }
		protected PipingSourceWrapper BP(Source cutoff, Source resonance, string id)
			{ return Pipe(new BPFilter(cutoff, resonance, id)); }
		protected PipingSourceWrapper HP(Source cutoff, Source resonance, string id)
			{ return Pipe(new HPFilter(cutoff, resonance, id)); }
		protected PipingSourceWrapper LP1(Source cutoff, string id)
			{ return Pipe(new OnePoleLPFilter(cutoff, id)); }
		protected PipingSourceWrapper HP1(Source cutoff, string id)
			{ return Pipe(new OnePoleHPFilter(cutoff, id)); }
		protected PipingSourceWrapper MatrixVerb()
			{ return Pipe(new MatrixVerb()); }
		protected PipingSourceWrapper MatrixVerb(Source viscosity)
			{ return Pipe(new MatrixVerb(viscosity)); }
		protected PipingSourceWrapper Overdrive(Source overdrive, string id)
			{ return Pipe(new Overdrive(overdrive, id)); }
		protected PipingSourceWrapper OD(Source overdrive, string id)
			{ return Pipe(new Overdrive(overdrive, id)); }
		protected PipingSourceWrapper Tape(string id)
			{ return Pipe(new Tape(id)); }
		protected PipingSourceWrapper Tape(float noiseLevel, string id)
			{ return Pipe(new Tape(noiseLevel, id)); }
		protected PipingSourceWrapper Pan(Source position)
			{ return Pipe(new Pan(position)); }
		protected PipingSourceWrapper Pan(Source position, Source width)
			{ return Pipe(new Pan(position, width)); }
		protected PipingSourceWrapper Rep(NoteSource feed, string id)
			{ return Pipe(new Rep(feed, id)); }
		protected AD AD(NoteSource source, Source decay)
			{ return new AD(source, decay); }
		protected AD AD(NoteSource source, Source attack, Source decay)
			{ return new AD(source, attack, decay); }
		protected FI FI(float time, string id)
			{ return new FI(time, id); }
		protected FO FO(float time, string id)
			{ return new FO(time, id); }

		protected PSeq PSeq(string sequence, int size, string id)
			{ return new PSeq(sequence, size, id); }
		protected Groover Groover(string sequence, string id)
			{ return new Groover(sequence, id); }
		protected SequenceWrapper Seq(string sequence)
			{ return new SequenceWrapper(sequence); }
		protected Sequence Seq(IEnumerable<int> notes, Source length)
			{ return new Sequence(notes, length); }
		protected Sequence Seq(IEnumerable<int> notes, Source length, string id)
			{ return new Sequence(notes, length, id); }
		protected Sequence Seq(IEnumerable<int> notes, int size, string id)
			{ return new Sequence(notes, size, id); }
		protected Sequence Seq(IEnumerable<Note> notes, Source length)
			{ return new Sequence(notes, length); }
		protected Sequence Seq(IEnumerable<Note> notes, Source length, string id)
			{ return new Sequence(notes, length, id); }
		protected Sequence Seq(IEnumerable<Note> notes, int size, string id)
			{ return new Sequence(notes, size, id); }
		protected Sequence Seq(string sequence, Source length, string id)
			{ return new Sequence(sequence, length, id); }
		protected Sequence Seq(string sequence, Source length, bool skipSilentNotes, string id)
			{ return new Sequence(sequence, length, skipSilentNotes, id); }
		protected Sequence Seq(string sequence, int size, string id)
			{ return new Sequence(sequence, size, id); }
		protected Sequence Seq(string sequence, int size, bool skipSilentNotes, string id)
			{ return new Sequence(sequence, size, skipSilentNotes, id); }
		protected Sequence S(string sequence, Source length, string id)
			{ return new Sequence(sequence, length, id); }
		protected Sequence S(string sequence, Source length, bool skipSilentNotes, string id)
			{ return new Sequence(sequence, length, skipSilentNotes, id); }
		protected Sequence S(string sequence, int size, string id)
			{ return new Sequence(sequence, size, id); }
		protected Sequence S(string sequence, int size, bool skipSilentNotes, string id)
			{ return new Sequence(sequence, size, skipSilentNotes, id); }

		protected RandomSeqence RSeq(string sequence, Source length, string id)
			{ return new RandomSeqence(sequence, length, id); }
		protected RandomSeqence RSeq(string sequence, Source length, bool skipSilentNotes, string id)
			{ return new RandomSeqence(sequence, length, skipSilentNotes, id); }
		protected RandomSeqence RSeq(string sequence, int size, string id)
			{ return new RandomSeqence(sequence, size, id); }

		protected RandomSeqence RS(string sequence, Source length, string id)
			{ return new RandomSeqence(sequence, length, id); }
		protected RandomSeqence RS(string sequence, Source length, bool skipSilentNotes, string id)
			{ return new RandomSeqence(sequence, length, skipSilentNotes, id); }
		protected RandomSeqence RS(string sequence, int size, string id)
			{ return new RandomSeqence(sequence, size, id); }

		protected RandomWalkSequence RW(string sequence, Source deviation, Source length, string id)
			{ return new RandomWalkSequence(sequence, deviation, length, id); }
		protected RandomWalkSequence RW(string sequence, Source deviation, int size, string id)
			{ return new RandomWalkSequence(sequence, deviation, size, id); }
		protected RandomWalkSequence RW(string sequence, Source deviation, Source length, bool skipSilentNotes, string id)
			{ return new RandomWalkSequence(sequence, deviation, length, skipSilentNotes, id); }

		protected RandomSeqence RSeq(IEnumerable<int> notes, Source length, string id)
			{ return new RandomSeqence(notes, length, id); }
		protected RandomSeqence RSeq(IEnumerable<int> notes, int size, string id)
			{ return new RandomSeqence(notes, size, id); }

		protected PipingSourceWrapper<NoteSource, NoteSource> DSeq(int delay, string id)
			{ return Pipe<NoteSource, NoteSource>(new SequenceDelay(delay, id)); }

		protected PipingSourceWrapper<NoteSource, NoteSource> SD(int delay, string id)
			{ return Pipe<NoteSource, NoteSource>(new SequenceDelay(delay, id)); }

		protected PipingSourceWrapper<NoteSource, NoteSource> ST(int delta)
			{ return Pipe<NoteSource, NoteSource>(new SequenceTranspose(delta)); }

		protected PipingSourceWrapper<NoteSource, NoteSource> ST(NoteSource delta)
			{ return Pipe<NoteSource, NoteSource>(new SequenceTranspose(delta)); }

		protected PipingSourceWrapper<NoteSource, Source> DR(params string[] samples)
			{ return Pipe<NoteSource, Source>(new DR(samples)); }
		protected PipingSourceWrapper<NoteSource, Source> Sampler(string sample, string id)
			{ return Pipe<NoteSource, Source>(new Sampler(sample, id)); }

		protected Met Met()
			{ return new Met(); }
		protected Noise Noise()
			{ return new Noise(); }

		protected PipingSourceWrapper SH(Source trigger, string id)
			{ return Pipe(new SampleAndHold(trigger, id)); }

		protected PipingSourceWrapper Osc()
			{ return Pipe(new Osc()); }
		protected PipingSourceWrapper Osc(Source amplitude)
			{ return Pipe(new Osc(amplitude)); }
		protected PipingSourceWrapper Osc(Source amplitude, string id)
			{ return Pipe(new Osc(amplitude, id)); }

		protected PipingSourceWrapper WT(string pack, Source selector, string id)
			{ return Pipe(new WaveTable(pack, selector, id)); }

		protected PipingSourceWrapper Saw()
			{ return Pipe(new Saw()); }
		protected PipingSourceWrapper Saw(Source amplitude)
			{ return Pipe(new Saw(amplitude)); }
		protected PipingSourceWrapper Saw(Source amplitude, string id)
			{ return Pipe(new Saw(amplitude, id)); }

		protected PipingSourceWrapper Sq()
			{ return Pipe(new Sq()); }
		protected PipingSourceWrapper Sq(Source amplitude)
			{ return Pipe(new Sq(amplitude)); }
		protected PipingSourceWrapper Sq(Source amplitude, Source pwm)
			{ return Pipe(new Sq(amplitude, pwm)); }
		protected PipingSourceWrapper Sq(Source amplitude, Source pwm, string id)
			{ return Pipe(new Sq(amplitude, pwm, id)); }

		private PipingSourceWrapper Pipe(IPipingSource source) 
		{
			return new PipingSourceWrapper(source);
		}

		private PipingSourceWrapper<TSource, TResult> Pipe<TSource, TResult>(IPipingSource<TSource> source) 
			where TSource : Source
			where TResult : Source
		{
			return new PipingSourceWrapper<TSource, TResult>(source);
		}
	}
}
