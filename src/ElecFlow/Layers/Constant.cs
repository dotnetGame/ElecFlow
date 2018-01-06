using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ElecFlow.CodeGeneration;

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

        internal override Task GenerateHDLAsync(VerilogCodeGenContext context)
        {
            var id = Name;
            var model = new { Id = id, Bits = Value.Dimensions.ToArray().Sum() * Unsafe.SizeOf<T>() * 8 };
            return context.WriteTemplateFileAsync("Constant_" + id, "Verilog.Constant.v", model);
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
