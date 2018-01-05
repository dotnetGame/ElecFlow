using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Onnx;

namespace ElecFlow
{
    public class FlowGraph
    {
        private readonly OutputConnector _output;
        private readonly Evaluator _evaluator;

        public IReadOnlyList<Layer> Layers { get; }

        internal FlowGraph(OutputConnector output)
        {
            _output = output;
            _evaluator = new Evaluator(output);
            Layers = FindLayers();
        }

        private IReadOnlyList<Layer> FindLayers()
        {
            var layers = new HashSet<Layer>();

            void FindLayers(Layer root)
            {
                layers.Add(root);
                foreach (var input in root.Inputs.Values)
                    FindLayers(input.Connection.From.Owner);
            }

            FindLayers(_output.Owner);
            return layers.ToList();
        }

        public object Evaluate(object inputs) => _evaluator.Evaluate(inputs);

        public static FlowGraph From(OutputConnector output) => new FlowGraph(output);
    }
}
