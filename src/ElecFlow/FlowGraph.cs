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
        private readonly ModelProto _onnxModel;
        private readonly IReadOnlyCollection<ValueInfoProto> _inputsNoInit;
        private readonly IReadOnlyCollection<ValueInfoProto> _outputs;

        internal FlowGraph(ModelProto modelProto)
        {
            _onnxModel = modelProto;
            _inputsNoInit = FindInputsNoInit();
            _outputs = modelProto.Graph.Output;
        }

        private IReadOnlyCollection<ValueInfoProto> FindInputsNoInit()
        {
            var init = _onnxModel.Graph.Initializer;
            return _onnxModel.Graph.Input.Where(o => !init.Any(i => i.Name == o.Name)).ToList();
        }

        public static FlowGraph Load(Stream input)
        {
            return new FlowGraph(ModelProto.Parser.ParseFrom(input));
        }
    }
}
