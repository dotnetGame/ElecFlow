using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ElecFlow.Layers;

namespace ElecFlow
{
    public class Evaluator<T>
    {
        private readonly OutputConnector<T> _output;

        public IReadOnlyList<Layer> InputVariables { get; }

        public Evaluator(OutputConnector<T> output)
        {
            _output = output;
            InputVariables = FindInputVariables();
        }

        public Tensor<T> Evaluate(object inputs)
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

        public Tensor<T> Evaluate(IReadOnlyDictionary<string, object> inputs)
        {
            var deferReset = new List<InputConnector>();
            var evaluateQueue = new Queue<Layer>();
            var evaluateLayers = new HashSet<Layer>();

            void OfferValue(Layer node)
            {
                if (node.Inputs.Values.Any(o => o.CurrentValue == null)) return;

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

                foreach (var output in node.Outputs.Values)
                {
                    foreach (var conn in output.Connections)
                    {
                        var layer = conn.To.Owner;
                        if (evaluateLayers.Add(layer))
                            evaluateQueue.Enqueue(layer);
                    }
                }
            }

            foreach (var inputVar in InputVariables)
            {
                evaluateQueue.Enqueue(inputVar);
                evaluateLayers.Add(inputVar);
            }

            try
            {
                while (evaluateQueue.Count != 0)
                {
                    OfferValue(evaluateQueue.Dequeue());
                }

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
