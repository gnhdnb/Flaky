using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
    internal class Mdct
    {
        private const float Pi3Eighths = .38268343236508977175f;
        private const float Pi2Eighths = .70710678118654752441f;
        private const float Pi1Eighth = .92387953251128675613f;
        private readonly int[] _bitReverse;
        private int _n;
        private int _log2N;
        private float[] _trig;
        float _scale;

        internal Mdct(int n, bool flag)
        {
            _n = n;
            _bitReverse = new int[n / 4];
            _trig = new float[n + n / 4];
            int n2 = (int)((uint)n >> 1);
            _log2N = (int)Math.Round(Math.Log(n) / Math.Log(2));

            int AE = 0;
            int AO = 1;
            int BE = AE + n / 2;
            int BO = BE + 1;
            int CE = BE + n / 2;
            int CO = CE + 1;

            // trig lookups...
            for (int i = 0; i < n / 4; i++)
            {
                _trig[AE + i * 2] = (float)Math.Cos((Math.PI / n) * (4 * i));
                _trig[AO + i * 2] = (float)-Math.Sin((Math.PI / n) * (4 * i));
                _trig[BE + i * 2] = (float)Math.Cos((Math.PI / (2 * n)) * (2 * i + 1));
                _trig[BO + i * 2] = (float)Math.Sin((Math.PI / (2 * n)) * (2 * i + 1));
            }
            for (int i = 0; i < n / 8; i++)
            {
                _trig[CE + i * 2] = (float)Math.Cos((Math.PI / n) * (4 * i + 2));
                _trig[CO + i * 2] = (float)-Math.Sin((Math.PI / n) * (4 * i + 2));
            }

            {
                int mask = (1 << (_log2N - 1)) - 1;
                int msb = 1 << (_log2N - 2);
                for (int i = 0; i < n / 8; i++)
                {
                    int acc = 0;
                    for (int j = 0; (((uint)msb) >> j) != 0; j++)
                        if (((((uint)msb >> j)) & i) != 0)
                            acc |= 1 << j;
                    if (flag)
                        _bitReverse[i * 2] = ((~acc) & mask) - 1;
                    else
                        _bitReverse[i * 2] = ((~acc) & mask);
                    //	bitrev[i*2]=((~acc)&mask)-1;
                    _bitReverse[i * 2 + 1] = acc;
                }
            }
            _scale = 4.0f / n;
        }

        public void Forward(IList<float> input, float[] output)
        {
            var n = _n;
            var n2 = n >> 1;
            var n4 = n >> 2;
            var n8 = n >> 3;
            var work = new float[n];
            var w2 = new OffsetArray<float>(work, n2);

            // rotate 
            // window + rotate + step 1 
            var x0 = n2 + n4;
            var x1 = x0 + 1;
            var t = n2;

            var i = 0;
            for (; i < n8; i += 2)
            {
                x0 -= 4;
                t -= 2;
                var r0 = input[x0 + 2] + input[x1 + 0];
                var r1 = input[x0 + 0] + input[x1 + 2];
                w2[i] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                w2[i + 1] = r1 * _trig[t + 0] - r0 * _trig[t + 1];
                x1 += 4;
            }

            x1 = 1;

            for (; i < n2 - n8; i += 2)
            {
                t -= 2;
                x0 -= 4;
                var r0 = input[x0 + 2] - input[x1 + 0];
                var r1 = input[x0 + 0] - input[x1 + 2];
                w2[i] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                w2[i + 1] = r1 * _trig[t + 0] - r0 * _trig[t + 1];
                x1 += 4;
            }

            x0 = n;

            for (; i < n2; i += 2)
            {
                t -= 2;
                x0 -= 4;
                var r0 = -input[x0 + 2] - input[x1 + 0];
                var r1 = -input[x0 + 0] - input[x1 + 2];
                w2[i] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                w2[i + 1] = r1 * _trig[t + 0] - r0 * _trig[t + 1];
                x1 += 4;
            }


            Butterflies(w2, n2);
            ReverseBits(work);

            // rotate + window 
            t = n2;
            x0 = n2;
            var offset = 0;
            for (i = 0; i < n4; i++)
            {
                x0--;
                output[i] = (work[offset + 0] * _trig[t + 0]
                             + work[offset + 1] * _trig[t + 1]) * _scale;

                output[x0 + 0] = (work[offset + 0] * _trig[t + 1]
                                  - work[offset + 1] * _trig[t + 0]) * _scale;
                offset += 2;
                t += 2;
            }
        }

        private void Butterflies(IList<float> data, int points)
        {
            var stages = _log2N - 5;

            if (--stages > 0)
                ButterflyFirst(data, points);

            for (var i = 1; --stages > 0; i++)
                for (var j = 0; j < 1 << i; j++)
                    ButterflyGeneric(data, (points >> i) * j, points >> i, 4 << i);

            for (var j = 0; j < points; j += 32)
                Butterfly32(data, j);
        }

        private static void Butterfly32(IList<float> data, int offset)
        {
            var r0 = data[offset + 30] - data[offset + 14];
            var r1 = data[offset + 31] - data[offset + 15];

            data[offset + 30] += data[offset + 14];
            data[offset + 31] += data[offset + 15];
            data[offset + 14] = r0;
            data[offset + 15] = r1;

            r0 = data[offset + 28] - data[offset + 12];
            r1 = data[offset + 29] - data[offset + 13];
            data[offset + 28] += data[offset + 12];
            data[offset + 29] += data[offset + 13];
            data[offset + 12] = r0 * Pi1Eighth - r1 * Pi3Eighths;
            data[offset + 13] = r0 * Pi3Eighths + r1 * Pi1Eighth;

            r0 = data[offset + 26] - data[offset + 10];
            r1 = data[offset + 27] - data[offset + 11];
            data[offset + 26] += data[offset + 10];
            data[offset + 27] += data[offset + 11];
            data[offset + 10] = (r0 - r1) * Pi2Eighths;
            data[offset + 11] = (r0 + r1) * Pi2Eighths;

            r0 = data[offset + 24] - data[offset + 8];
            r1 = data[offset + 25] - data[offset + 9];
            data[offset + 24] += data[offset + 8];
            data[offset + 25] += data[offset + 9];
            data[offset + 8] = r0 * Pi3Eighths - r1 * Pi1Eighth;
            data[offset + 9] = r1 * Pi3Eighths + r0 * Pi1Eighth;

            r0 = data[offset + 22] - data[offset + 6];
            r1 = data[offset + 7] - data[offset + 23];
            data[offset + 22] += data[offset + 6];
            data[offset + 23] += data[offset + 7];
            data[offset + 6] = r1;
            data[offset + 7] = r0;

            r0 = data[offset + 4] - data[offset + 20];
            r1 = data[offset + 5] - data[offset + 21];
            data[offset + 20] += data[offset + 4];
            data[offset + 21] += data[offset + 5];
            data[offset + 4] = r1 * Pi1Eighth + r0 * Pi3Eighths;
            data[offset + 5] = r1 * Pi3Eighths - r0 * Pi1Eighth;

            r0 = data[offset + 2] - data[offset + 18];
            r1 = data[offset + 3] - data[offset + 19];
            data[offset + 18] += data[offset + 2];
            data[offset + 19] += data[offset + 3];
            data[offset + 2] = (r1 + r0) * Pi2Eighths;
            data[offset + 3] = (r1 - r0) * Pi2Eighths;

            r0 = data[offset + 0] - data[offset + 16];
            r1 = data[offset + 1] - data[offset + 17];
            data[offset + 16] += data[offset + 0];
            data[offset + 17] += data[offset + 1];
            data[offset + 0] = r1 * Pi3Eighths + r0 * Pi1Eighth;
            data[offset + 1] = r1 * Pi1Eighth - r0 * Pi3Eighths;

            Butterfly16(data, offset);
            Butterfly16(data, offset + 16);
        }

        private static void Butterfly16(IList<float> data, int offset)
        {
            var r0 = data[offset + 1] - data[offset + 9];
            var r1 = data[offset + 0] - data[offset + 8];

            data[offset + 8] += data[offset + 0];
            data[offset + 9] += data[offset + 1];
            data[offset + 0] = (r0 + r1) * Pi2Eighths;
            data[offset + 1] = (r0 - r1) * Pi2Eighths;

            r0 = data[offset + 3] - data[offset + 11];
            r1 = data[offset + 10] - data[offset + 2];
            data[offset + 10] += data[offset + 2];
            data[offset + 11] += data[offset + 3];
            data[offset + 2] = r0;
            data[offset + 3] = r1;

            r0 = data[offset + 12] - data[offset + 4];
            r1 = data[offset + 13] - data[offset + 5];
            data[offset + 12] += data[offset + 4];
            data[offset + 13] += data[offset + 5];
            data[offset + 4] = (r0 - r1) * Pi2Eighths;
            data[offset + 5] = (r0 + r1) * Pi2Eighths;

            r0 = data[offset + 14] - data[offset + 6];
            r1 = data[offset + 15] - data[offset + 7];
            data[offset + 14] += data[offset + 6];
            data[offset + 15] += data[offset + 7];
            data[offset + 6] = r0;
            data[offset + 7] = r1;

            Butterfly8(data, offset);
            Butterfly8(data, offset + 8);
        }

        private static void Butterfly8(IList<float> data, int offset)
        {
            var r0 = data[offset + 6] + data[offset + 2];
            var r1 = data[offset + 6] - data[offset + 2];
            var r2 = data[offset + 4] + data[offset + 0];
            var r3 = data[offset + 4] - data[offset + 0];

            data[offset + 6] = r0 + r2;
            data[offset + 4] = r0 - r2;

            r0 = data[offset + 5] - data[offset + 1];
            r2 = data[offset + 7] - data[offset + 3];
            data[offset + 0] = r1 + r0;
            data[offset + 2] = r1 - r0;

            r0 = data[offset + 5] + data[offset + 1];
            r1 = data[offset + 7] + data[offset + 3];
            data[offset + 3] = r2 + r3;
            data[offset + 1] = r2 - r3;
            data[offset + 7] = r1 + r0;
            data[offset + 5] = r1 - r0;
        }

        private void ButterflyGeneric(IList<float> data, int offset, int points, int trigIncrement)
        {
            var t = 0;
            var x1 = offset + points - 8;
            var x2 = offset + (points >> 1) - 8;

            do
            {
                var r0 = data[x1 + 6] - data[x2 + 6];
                var r1 = data[x1 + 7] - data[x2 + 7];
                data[x1 + 6] += data[x2 + 6];
                data[x1 + 7] += data[x2 + 7];
                data[x2 + 6] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                data[x2 + 7] = r1 * _trig[t + 0] - r0 * _trig[t + 1];

                t += trigIncrement;

                r0 = data[x1 + 4] - data[x2 + 4];
                r1 = data[x1 + 5] - data[x2 + 5];
                data[x1 + 4] += data[x2 + 4];
                data[x1 + 5] += data[x2 + 5];
                data[x2 + 4] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                data[x2 + 5] = r1 * _trig[t + 0] - r0 * _trig[t + 1];

                t += trigIncrement;

                r0 = data[x1 + 2] - data[x2 + 2];
                r1 = data[x1 + 3] - data[x2 + 3];
                data[x1 + 2] += data[x2 + 2];
                data[x1 + 3] += data[x2 + 3];
                data[x2 + 2] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                data[x2 + 3] = r1 * _trig[t + 0] - r0 * _trig[t + 1];

                t += trigIncrement;

                r0 = data[x1 + 0] - data[x2 + 0];
                r1 = data[x1 + 1] - data[x2 + 1];
                data[x1 + 0] += data[x2 + 0];
                data[x1 + 1] += data[x2 + 1];
                data[x2 + 0] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                data[x2 + 1] = r1 * _trig[t + 0] - r0 * _trig[t + 1];

                t += trigIncrement;
                x1 -= 8;
                x2 -= 8;
            } while (x2 >= offset);
        }

        private void ButterflyFirst(IList<float> data, int points)
        {
            var x1 = points - 8;
            var x2 = (points >> 1) - 8;
            var t = 0;

            do
            {
                var r0 = data[x1 + 6] - data[x2 + 6];
                var r1 = data[x1 + 7] - data[x2 + 7];
                data[x1 + 6] += data[x2 + 6];
                data[x1 + 7] += data[x2 + 7];
                data[x2 + 6] = r1 * _trig[t + 1] + r0 * _trig[t + 0];
                data[x2 + 7] = r1 * _trig[t + 0] - r0 * _trig[t + 1];

                r0 = data[x1 + 4] - data[x2 + 4];
                r1 = data[x1 + 5] - data[x2 + 5];
                data[x1 + 4] += data[x2 + 4];
                data[x1 + 5] += data[x2 + 5];
                data[x2 + 4] = r1 * _trig[t + 5] + r0 * _trig[t + 4];
                data[x2 + 5] = r1 * _trig[t + 4] - r0 * _trig[t + 5];

                r0 = data[x1 + 2] - data[x2 + 2];
                r1 = data[x1 + 3] - data[x2 + 3];
                data[x1 + 2] += data[x2 + 2];
                data[x1 + 3] += data[x2 + 3];
                data[x2 + 2] = r1 * _trig[t + 9] + r0 * _trig[t + 8];
                data[x2 + 3] = r1 * _trig[t + 8] - r0 * _trig[t + 9];

                r0 = data[x1 + 0] - data[x2 + 0];
                r1 = data[x1 + 1] - data[x2 + 1];
                data[x1 + 0] += data[x2 + 0];
                data[x1 + 1] += data[x2 + 1];
                data[x2 + 0] = r1 * _trig[t + 13] + r0 * _trig[t + 12];
                data[x2 + 1] = r1 * _trig[t + 12] - r0 * _trig[t + 13];

                x1 -= 8;
                x2 -= 8;
                t += 16;
            } while (x2 >= 0);
        }

        private void ReverseBits(IList<float> data)
        {
            var n = _n;
            var bit = 0;
            var w0 = 0;
            var w1 = w0 + (n >> 1);
            var x = w1;
            var t = n;

            do
            {
                var x0 = x + _bitReverse[bit + 0];
                var x1 = x + _bitReverse[bit + 1];

                var r0 = data[x0 + 1] - data[x1 + 1];
                var r1 = data[x0 + 0] + data[x1 + 0];
                var r2 = r1 * _trig[t + 0] + r0 * _trig[t + 1];
                var r3 = r1 * _trig[t + 1] - r0 * _trig[t + 0];

                w1 -= 4;

                r0 = 0.5f * (data[x0 + 1] + data[x1 + 1]);
                r1 = 0.5f * (data[x0 + 0] - data[x1 + 0]);

                data[w0 + 0] = r0 + r2;
                data[w1 + 2] = r0 - r2;
                data[w0 + 1] = r1 + r3;
                data[w1 + 3] = r3 - r1;

                x0 = x + _bitReverse[bit + 2];
                x1 = x + _bitReverse[bit + 3];

                r0 = data[x0 + 1] - data[x1 + 1];
                r1 = data[x0 + 0] + data[x1 + 0];
                r2 = r1 * _trig[t + 2] + r0 * _trig[t + 3];
                r3 = r1 * _trig[t + 3] - r0 * _trig[t + 2];

                r0 = 0.5f * (data[x0 + 1] + data[x1 + 1]);
                r1 = 0.5f * (data[x0 + 0] - data[x1 + 0]);

                data[w0 + 2] = r0 + r2;
                data[w1 + 0] = r0 - r2;
                data[w0 + 3] = r1 + r3;
                data[w1 + 1] = r3 - r1;

                t += 4;
                bit += 4;
                w0 += 4;
            } while (w0 < w1);
        }

        float[] _x = new float[1024];
        float[] _w = new float[1024];

        internal void Backward(IList<float> fin, float[] fout)
        {
            if (_x.Length < _n / 2) { _x = new float[_n / 2]; }
            if (_w.Length < _n / 2) { _w = new float[_n / 2]; }
            float[] x = _x;
            float[] w = _w;
            int n2 = (int)((uint)_n >> 1);
            int n4 = (int)((uint)_n >> 2);
            int n8 = (int)((uint)_n >> 3);

            // rotate + step 1
            {
                int inO = 1;
                int xO = 0;
                int A = n2;

                int i;
                for (i = 0; i < n8; i++)
                {
                    A -= 2;
                    x[xO++] = -fin[inO + 2] * _trig[A + 1] - fin[inO] * _trig[A];
                    x[xO++] = fin[inO] * _trig[A + 1] - fin[inO + 2] * _trig[A];
                    inO += 4;
                }

                inO = n2 - 4;

                for (i = 0; i < n8; i++)
                {
                    A -= 2;
                    x[xO++] = fin[inO] * _trig[A + 1] + fin[inO + 2] * _trig[A];
                    x[xO++] = fin[inO] * _trig[A] - fin[inO + 2] * _trig[A + 1];
                    inO -= 4;
                }
            }

            float[] xxx = MdctKernel(x, w, _n, n2, n4, n8);
            int xx = 0;

            // step 8

            {
                int B = n2;
                int o1 = n4, o2 = o1 - 1;
                int o3 = n4 + n2, o4 = o3 - 1;

                for (int i = 0; i < n4; i++)
                {
                    float temp1 = (xxx[xx] * _trig[B + 1] - xxx[xx + 1] * _trig[B]);
                    float temp2 = -(xxx[xx] * _trig[B] + xxx[xx + 1] * _trig[B + 1]);

                    fout[o1] = -temp1;
                    fout[o2] = temp1;
                    fout[o3] = temp2;
                    fout[o4] = temp2;

                    o1++;
                    o2--;
                    o3++;
                    o4--;
                    xx += 2;
                    B += 2;
                }
            }
        }
        internal float[] MdctKernel(float[] x, float[] w,
            int n, int n2, int n4, int n8)
        {
            // step 2

            int xA = n4;
            int xB = 0;
            int w2 = n4;
            int A = n2;

            for (int i = 0; i < n4;)
            {
                float x0 = x[xA] - x[xB];
                float x1;
                w[w2 + i] = x[xA++] + x[xB++];

                x1 = x[xA] - x[xB];
                A -= 4;

                w[i++] = x0 * _trig[A] + x1 * _trig[A + 1];
                w[i] = x1 * _trig[A] - x0 * _trig[A + 1];

                w[w2 + i] = x[xA++] + x[xB++];
                i++;
            }

            // step 3

            {
                for (int i = 0; i < _log2N - 3; i++)
                {
                    int k0 = (int)((uint)n >> (i + 2));
                    int k1 = 1 << (i + 3);
                    int wbase = n2 - 2;

                    A = 0;
                    float[] temp;

                    for (int r = 0; r < ((uint)k0 >> 2); r++)
                    {
                        int w1 = wbase;
                        w2 = w1 - (k0 >> 1);
                        float AEv = _trig[A], wA;
                        float AOv = _trig[A + 1], wB;
                        wbase -= 2;

                        k0++;
                        for (int s = 0; s < (2 << i); s++)
                        {
                            wB = w[w1] - w[w2];
                            x[w1] = w[w1] + w[w2];

                            wA = w[++w1] - w[++w2];
                            x[w1] = w[w1] + w[w2];

                            x[w2] = wA * AEv - wB * AOv;
                            x[w2 - 1] = wB * AEv + wA * AOv;

                            w1 -= k0;
                            w2 -= k0;
                        }
                        k0--;
                        A += k1;
                    }

                    temp = w;
                    w = x;
                    x = temp;
                }
            }

            // step 4, 5, 6, 7
            {
                int C = n;
                int bit = 0;
                int x1 = 0;
                int x2 = n2 - 1;

                for (int i = 0; i < n8; i++)
                {
                    int t1 = _bitReverse[bit++];
                    int t2 = _bitReverse[bit++];

                    float wA = w[t1] - w[t2 + 1];
                    float wB = w[t1 - 1] + w[t2];
                    float wC = w[t1] + w[t2 + 1];
                    float wD = w[t1 - 1] - w[t2];

                    float wACE = wA * _trig[C];
                    float wBCE = wB * _trig[C++];
                    float wACO = wA * _trig[C];
                    float wBCO = wB * _trig[C++];

                    x[x1++] = (wC + wACO + wBCE) * .5f;
                    x[x2--] = (-wD + wBCO - wACE) * .5f;
                    x[x1++] = (wD + wBCO - wACE) * .5f;
                    x[x2--] = (wC - wACO - wBCE) * .5f;
                }
            }
            return (x);
        }
    }
}
