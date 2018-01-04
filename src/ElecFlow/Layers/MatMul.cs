using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow.Layers
{
    public class MatMul : Layer
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

        public MatMul(ReadOnlySpan<int> aDim, ReadOnlySpan<int> bDim)
        {
            if (aDim.Length != bDim.Length) throw new ArgumentException("Dimensions of A and B must be same.");

            AddInputConnector(aDim);
            AddInputConnector(bDim);
            AddOutputConnector(new DenseTensor<double>(aDim).MatrixMultiply(new DenseTensor<double>(bDim)).Dimensions);
        }

        protected override void CaculateOutputs()
        {
            Y = A.MatrixMultiply(B);
        }
    }
}
