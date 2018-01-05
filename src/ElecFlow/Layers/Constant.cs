using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow.Layers
{
    public class Constant<T> : Layer
    {
        public OutputConnector<T> Value { get; }

        private readonly Tensor<T> _value;

        public Constant(Tensor<T> value)
        {
            _value = value;
            Value = AddOutputConnector("Value", value.Dimensions, OnEvaluateValue);
        }

        private Tensor<T> OnEvaluateValue(IReadOnlyDictionary<string, object> evaluationContext)
        {
            return _value.Clone();
        }
    }
}

namespace ElecFlow
{
    using ElecFlow.Layers;

    public partial class Layer
    {
        public static Constant<T> Constant<T>(Tensor<T> value) =>
            new Constant<T>(value);
    }
}
