using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow
{
    public abstract class InputConnector : Connector
    {
        public Connection Connection { get; internal set; }

        public object CurrentValue => GetCurrentValue();

        internal InputConnector(Layer owner, string name, ReadOnlySpan<int> dimensions)
            : base(owner, name, dimensions)
        {
        }

        protected abstract object GetCurrentValue();

        internal abstract void SetCurrentValue(object value);

        internal abstract void ResetCurrentValue();

        public void Disconnect()
        {
            if (Connection != null)
            {
                var output = Connection.From;
                Connection = null;
                output.Disconnect(this);
            }
        }
    }

    public class InputConnector<T> : InputConnector
    {
        public override Type ValueType => typeof(T);

        public new Tensor<T> CurrentValue { get; private set; }

        internal InputConnector(Layer owner, string name, ReadOnlySpan<int> dimensions)
            : base(owner, name, dimensions)
        {
        }

        protected override object GetCurrentValue() => CurrentValue;

        internal override void SetCurrentValue(object value)
        {
            CurrentValue = (Tensor<T>)value;
        }

        internal override void ResetCurrentValue()
        {
            CurrentValue = null;
        }
    }
}
