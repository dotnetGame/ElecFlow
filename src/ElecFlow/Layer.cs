using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElecFlow
{
    public abstract class Layer
    {
        private bool _isDirty = true;

        private readonly List<Tensor<double>> _inputTensors = new List<Tensor<double>>();
        private readonly List<int[]> _inputDimensions = new List<int[]>();

        private readonly List<Tensor<double>> _outputTensors = new List<Tensor<double>>();
        private readonly List<int[]> _outputDimensions = new List<int[]>();

        protected void AddInputConnector(ReadOnlySpan<int> dimensions)
        {
            _inputDimensions.Add(dimensions.ToArray());
            _inputTensors.Add(null);
        }

        protected void AddOutputConnector(ReadOnlySpan<int> dimensions)
        {
            _outputDimensions.Add(dimensions.ToArray());
            _outputTensors.Add(null);
        }

        public void OfferInput(int index, Tensor<double> input)
        {
            if (index < 0 || index >= _inputTensors.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _inputTensors[index] = CheckAndClone(input, _inputDimensions[index]);
            _isDirty = true;
        }

        public Tensor<double> GetInput(int index)
        {
            if (index < 0 || index >= _inputTensors.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _inputTensors[index];
        }

        public Tensor<double> ProvideOutput(int index)
        {
            if (index < 0 || index >= _outputTensors.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_isDirty)
            {
                CaculateOutputs();
                _isDirty = false;
            }

            return _outputTensors[index];
        }

        protected void SetOutput(int index, Tensor<double> value)
        {
            if (!value.Dimensions.SequenceEqual(_outputDimensions[index]))
                throw new ArgumentException("Dimensions is not same.");
            _outputTensors[index] = value;
        }

        public ReadOnlySpan<int> GetOutputDimension(int index) => _outputDimensions[index];

        protected abstract void CaculateOutputs();

        protected static Tensor<double> CheckAndClone(Tensor<double> source, ReadOnlySpan<int> dimensions)
        {
            if (!source.Dimensions.SequenceEqual(dimensions))
                throw new ArgumentException("Dimensions is not same.");
            return source.Clone();
        }
    }
}
