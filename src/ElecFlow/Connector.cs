using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ElecFlow
{
    public abstract class Connector
    {
        public Layer Owner { get; }

        public string Name { get; }

        public abstract Type ValueType { get; }

        private readonly int[] _dimensions;

        public ReadOnlySpan<int> Dimensions => _dimensions;

        internal Connector(Layer owner, string name, ReadOnlySpan<int> dimensions)
        {
            Owner = owner;
            Name = name;
            _dimensions = dimensions.ToArray();
        }

        protected void CheckConnectorCompatible(Connector other)
        {
            if (other.ValueType != ValueType || !other._dimensions.SequenceEqual(_dimensions))
                throw new InvalidOperationException("Value type and dimensions must be same.");
        }
    }
}
