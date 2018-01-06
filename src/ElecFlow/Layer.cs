using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ElecFlow.CodeGeneration;

namespace ElecFlow
{
    public abstract partial class Layer
    {
        private readonly Dictionary<string, InputConnector> _inputConnectors = new Dictionary<string, InputConnector>();
        private readonly Dictionary<string, OutputConnector> _outputConnectors = new Dictionary<string, OutputConnector>();

        public IReadOnlyDictionary<string, InputConnector> Inputs => _inputConnectors;

        public IReadOnlyDictionary<string, OutputConnector> Outputs => _outputConnectors;

        public string Name { get; set; }

        public Layer(string name = null)
        {
            Name = name ?? GetType().ToString();
        }

        protected InputConnector<T> AddInputConnector<T>(string name, ReadOnlySpan<int> dimensions)
        {
            var input = new InputConnector<T>(this, name, dimensions);
            _inputConnectors.Add(name, input);
            return input;
        }

        protected OutputConnector<T> AddOutputConnector<T>(string name, ReadOnlySpan<int> dimensions, Func<IReadOnlyDictionary<string, object>, Tensor<T>> evaluator)
        {
            var output = new OutputConnector<T>(this, name, dimensions, evaluator);
            _outputConnectors.Add(name, output);
            return output;
        }

        internal virtual Task GenerateHDLAsync(VerilogCodeGenContext context) => Task.CompletedTask;
    }
}
