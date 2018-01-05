using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow.Layers
{
    public class Add : Layer
    {
        public InputConnector<double> A { get; }

        public InputConnector<double> B { get; }

        public OutputConnector<double> Y { get; }

        private readonly int _broadcast;
        private readonly int _axis;

        public Add(ReadOnlySpan<int> aDim, ReadOnlySpan<int> bDim, int broadcast, int? axis = default)
        {
            if (broadcast == 0 && !aDim.SequenceEqual(bDim)) throw new ArgumentException("Dimensions of A and B must be same.");

            _broadcast = broadcast;
            _axis = axis ?? aDim.Length - bDim.Length;
            A = AddInputConnector<double>("A", aDim);
            B = AddInputConnector<double>("B", bDim);
            Y = AddOutputConnector("Y", aDim, OnEvaluateY);
        }

        private Tensor<double> OnEvaluateY(IReadOnlyDictionary<string, object> evaluationContext)
        {
            if (_broadcast == 0 || A.Dimensions.SequenceEqual(B.Dimensions))
            {
                return Tensor.Add(A.CurrentValue, B.CurrentValue);
            }
            else
            {
                var y = A.CurrentValue.CloneEmpty();
                var axis = _axis;
                var ranges = new Range[A.Dimensions.Length];
                for (int i = 0; i < B.Dimensions.Length; i++)
                    ranges[axis + i] = new Range(0, (uint)B.Dimensions[i]);

                // FIXME: 只实现了 axis = 1，且 dimA = dimB + 1 的情况
                for (int i = 0; i < A.Dimensions[0]; i++)
                {
                    ranges[0] = new Range(i, 1);
                    var src = A.CurrentValue.Slice(ranges);
                    Tensor.Add(src, B.CurrentValue, y.Slice(ranges));
                }

                return y;
            }
        }
    }
}

namespace ElecFlow
{
    using System.Linq;
    using ElecFlow.Layers;

    public partial class Layer
    {
        public static Add Add(Layer left, Layer right, int broadcast = 1, int? axis = null)
        {
            var leftOutput = left.Outputs.First().Value;
            var rightOutput = right.Outputs.First().Value;
            var node = new Add(leftOutput.Dimensions, rightOutput.Dimensions, broadcast, axis);
            leftOutput.Connect(node.A);
            rightOutput.Connect(node.B);
            return node;
        }

        public static Add operator +(Layer left, Layer right) => Add(left, right);
    }
}