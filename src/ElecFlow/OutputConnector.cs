using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ElecFlow
{
    public abstract class OutputConnector : Connector
    {
        internal List<Connection> MutableConnections { get; } = new List<Connection>();

        public IReadOnlyList<Connection> Connections => MutableConnections;

        internal OutputConnector(Layer owner, string name, ReadOnlySpan<int> dimensions)
            : base(owner, name, dimensions)
        {
        }

        public object Evaluate(IReadOnlyDictionary<string, object> evaluationContext) => EvaluateCore(evaluationContext);

        protected abstract object EvaluateCore(IReadOnlyDictionary<string, object> evaluationContext);

        public void Connect(InputConnector to) => ConnectCore(to);

        protected abstract void ConnectCore(InputConnector to);

        public void Disconnect(InputConnector to)
        {
            MutableConnections.RemoveAll(o => o.To == to);
            to.Disconnect();
        }

        internal abstract object CloneOutputValue(object value);
    }

    public class OutputConnector<T> : OutputConnector
    {
        public override Type ValueType => typeof(T);

        private readonly Func<IReadOnlyDictionary<string, object>, Tensor<T>> _evaluator;

        internal OutputConnector(Layer owner, string name, ReadOnlySpan<int> dimensions, Func<IReadOnlyDictionary<string, object>, Tensor<T>> evaluator)
            : base(owner, name, dimensions)
        {
            _evaluator = evaluator;
        }

        protected override object EvaluateCore(IReadOnlyDictionary<string, object> evaluationContext) => Evaluate(evaluationContext);

        public new Tensor<T> Evaluate(IReadOnlyDictionary<string, object> evaluationContext) => _evaluator(evaluationContext);

        protected override void ConnectCore(InputConnector to) => Connect((InputConnector<T>)to);

        public void Connect(InputConnector<T> to)
        {
            if (!Connections.Any(o => o.To == to))
            {
                to.Disconnect();
                var connection = new Connection(this, to);
                MutableConnections.Add(connection);
                to.Connection = connection;
            }
        }

        internal override object CloneOutputValue(object value) => ((Tensor<T>)value).Clone();
    }
}
