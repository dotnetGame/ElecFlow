using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ElecFlow.Layers
{
    public class Softmax : Layer
    {
        public InputConnector<double> Input { get; }

        public OutputConnector<double> Output { get; }

        private readonly int _axis;

        public Softmax(ReadOnlySpan<int> inputDim, int axis = 1)
        {
            _axis = axis;
            Input = AddInputConnector<double>("input", inputDim);
            Output = AddOutputConnector("output", inputDim, OnEvaluateOutput);
        }

        private Tensor<double> OnEvaluateOutput(IReadOnlyDictionary<string, object> evaluationContext)
        {
            var y = Input.CurrentValue;
            for (int i = 0; i < y.Dimensions[0]; i++)
            {
                var slice = y.Slice(new[] { Range.Construct(i, i + 1), Range.Construct(0, y.Dimensions[1]) });
                for (int j = 0; j < slice.Length; j++)
                {
                    var value = slice.GetValue(j);
                    slice.SetValue(j, Math.Exp(value));
                }

                var sum = slice.Sum();
                for (int j = 0; j < slice.Length; j++)
                {
                    var value = slice.GetValue(j);
                    slice.SetValue(j, value / sum);
                }
            }

            return y;
        }
    }
}

namespace ElecFlow
{
    using System.Linq;
    using ElecFlow.Layers;

    public partial class Layer
    {
        public static Softmax Softmax(Layer input, int axis = 1)
        {
            var inputConn = input.Outputs.First().Value;
            var node = new Softmax(inputConn.Dimensions, axis);
            inputConn.Connect(node.Input);
            return node;
        }
    }
}