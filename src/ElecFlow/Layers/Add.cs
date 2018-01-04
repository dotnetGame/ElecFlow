using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow.Layers
{
    public class Add : Layer
    {
        public Tensor<double> A
        {
            get => GetInput(0);
            set => OfferInput(0, value);
        }

        public Tensor<double> B
        {
            get => GetInput(1);
            set => OfferInput(1, value);
        }

        public Tensor<double> Y
        {
            get => ProvideOutput(0);
            private set => SetOutput(0, value);
        }

        private readonly int _broadcast;
        private readonly int _axis;

        public Add(ReadOnlySpan<int> aDim, ReadOnlySpan<int> bDim, int broadcast, int? axis = default)
        {
            if (broadcast == 0 && !aDim.SequenceEqual(bDim)) throw new ArgumentException("Dimensions of A and B must be same.");

            _broadcast = broadcast;
            _axis = axis ?? aDim.Length - bDim.Length;
            AddInputConnector(aDim);
            AddInputConnector(bDim);
            AddOutputConnector(aDim);
        }

        protected override void CaculateOutputs()
        {
            if (_broadcast == 0 || A.Dimensions.SequenceEqual(B.Dimensions))
            {
                Y = Tensor.Add(A, B);
            }
            else
            {
                var y = A.CloneEmpty();
                var axis = _axis;
                var ranges = new Range[A.Dimensions.Length];
                for (int i = 0; i < B.Dimensions.Length; i++)
                    ranges[axis + i] = new Range(0, (uint)B.Dimensions[i]);

                // FIXME: 只实现了 axis = 1，且 dimA = dimB + 1 的情况
                for (int i = 0; i < A.Dimensions[0]; i++)
                {
                    ranges[0] = new Range(i, 1);
                    var src = A.Slice(ranges);
                    Tensor.Add(src, B, y.Slice(ranges));
                }

                Y = y;
            }
        }
    }
}
