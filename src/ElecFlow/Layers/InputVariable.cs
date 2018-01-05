using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow.Layers
{
    public sealed class InputVariable<T> : Layer
    {
        public OutputConnector<T> Value { get; }

        public InputVariable(string name, ReadOnlySpan<int> dimensions)
            : base(name)
        {
            Value = AddOutputConnector("Value", dimensions, OnEvaluateValue);
        }

        private Tensor<T> OnEvaluateValue(IReadOnlyDictionary<string, object> evaluationContext)
        {
            var value = (Tensor<T>)evaluationContext[Name];
            if (value == null || !value.Dimensions.SequenceEqual(Value.Dimensions))
                throw new ArgumentException($"Input value of {Name} is not compatible.");
            return value;
        }
    }
}

namespace ElecFlow
{
    using ElecFlow.Layers;

    public partial class Layer
    {
        public static InputVariable<T> InputVariable<T>(string name, ReadOnlySpan<int> dimensions) =>
            new InputVariable<T>(name, dimensions);
    }
}