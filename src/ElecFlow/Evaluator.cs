using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ElecFlow.Layers;

namespace ElecFlow
{
    internal class Evaluator
    {
        private readonly OutputConnector _output;

        public OutputConnector Output => _output;

        public IReadOnlyList<Layer> InputVariables { get; }

        public Evaluator(OutputConnector output)
        {
            _output = output;
            InputVariables = FindInputVariables();
        }

        public object Evaluate(object inputs)
        {
            IReadOnlyDictionary<string, object> ConvertToContext()
            {
                if (!(inputs is IReadOnlyDictionary<string, object> value))
                {
                    value = (from p in inputs.GetType().GetProperties()
                             select new
                             {
                                 Key = p.Name,
                                 Value = p.GetValue(inputs)
                             }).ToDictionary(o => o.Key, o => o.Value);
                }

                return value;
            }

            return Evaluate(ConvertToContext());
        }

        public object Evaluate(IReadOnlyDictionary<string, object> inputs)
        {
            var deferReset = new List<InputConnector>();
            var evaluateStack = new Stack<Layer>();
            var evaluatedLayers = new HashSet<Layer>();

            void EvaluateTop()
            {
                var node = evaluateStack.Peek();
                if (node.Inputs.Values.All(o => o.CurrentValue != null))
                {
                    if (!evaluatedLayers.Add(node)) return;

                    foreach (var output in node.Outputs.Values)
                    {
                        var outValue = output.Evaluate(inputs);
                        var connections = output.Connections;
                        for (int i = 0; i < connections.Count; i++)
                        {
                            var to = connections[i].To;
                            if (i == connections.Count - 1)
                                to.SetCurrentValue(outValue);
                            else
                                to.SetCurrentValue(output.CloneOutputValue(outValue));
                            deferReset.Add(to);
                        }
                    }

                    evaluateStack.Pop();
                }
                else
                {
                    foreach (var input in node.Inputs.Values.Where(o => o.CurrentValue == null))
                    {
                        var sourceNode = input.Connection.From.Owner;
                        evaluateStack.Push(sourceNode);
                    }
                }
            }

            try
            {
                foreach (var input in _output.Owner.Inputs.Values)
                {
                    var sourceNode = input.Connection.From.Owner;
                    evaluateStack.Push(sourceNode);
                }

                while (evaluateStack.Count != 0)
                    EvaluateTop();

                return _output.Evaluate(inputs);
            }
            finally
            {
                foreach (var input in deferReset)
                    input.ResetCurrentValue();
            }
        }

        private IReadOnlyList<Layer> FindInputVariables()
        {
            var inputs = new List<Layer>();

            void FindInputVariables(Layer output)
            {
                var type = output.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(InputVariable<>))
                {
                    inputs.Add(output);
                    return;
                }

                foreach (var input in output.Inputs.Values)
                {
                    if (input.Connection != null)
                        FindInputVariables(input.Connection.From.Owner);
                }
            }

            FindInputVariables(_output.Owner);
            return inputs;
        }
    }
}
