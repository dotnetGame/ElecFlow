using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow.Layers
{
    public class MatMul : Layer
    {
        public InputConnector<double> A { get; }

        public InputConnector<double> B { get; }

        public OutputConnector<double> Y { get; }

        public MatMul(ReadOnlySpan<int> aDim, ReadOnlySpan<int> bDim)
        {
            if (aDim.Length != bDim.Length) throw new ArgumentException("Dimensions of A and B must be same.");

            A = AddInputConnector<double>("A", aDim);
            B = AddInputConnector<double>("B", bDim);
            Y = AddOutputConnector("Y", new DenseTensor<double>(aDim).MatrixMultiply(new DenseTensor<double>(bDim)).Dimensions, OnEvaluateY);
        }

        private Tensor<double> OnEvaluateY(IReadOnlyDictionary<string, object> evaluationContext)
        {
            return A.CurrentValue.MatrixMultiply(B.CurrentValue);
        }
    }
}

namespace ElecFlow
{
    using System.Linq;
    using ElecFlow.Layers;

    public partial class Layer
    {
        public static MatMul MatMul(Layer left, Layer right)
        {
            var leftOutput = left.Outputs.First().Value;
            var rightOutput = right.Outputs.First().Value;
            var node = new MatMul(leftOutput.Dimensions, rightOutput.Dimensions);
            leftOutput.Connect(node.A);
            rightOutput.Connect(node.B);
            return node;
        }
    }
}