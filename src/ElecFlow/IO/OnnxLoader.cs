using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using ElecFlow.Layers;
using Google.Protobuf.Collections;
using Onnx;

namespace ElecFlow.IO
{
    public static class OnnxLoader
    {
        public static FlowGraph LoadGraph(Stream stream)
        {
            var onnx = ModelProto.Parser.ParseFrom(stream);
            return LoadGraph(onnx.Graph);
        }

        private static FlowGraph LoadGraph(GraphProto graph)
        {
            var nodes = new Dictionary<string, Layer>();
            var typeDic = new Dictionary<string, TensorType>();
            var outputConn = new Dictionary<string, OutputConnector>();
            foreach (var init in graph.Initializer)
                nodes.Add(init.Name, ParseConstantNode(init, typeDic, outputConn));

            foreach (var input in graph.Input.Where(o => !nodes.ContainsKey(o.Name)))
                nodes.Add(input.Name, ParseInputVariableNode(input, typeDic, outputConn));

            foreach (var node in graph.Node)
                nodes.Add(node.Name, ParseNode(node, typeDic, outputConn));
            return FlowGraph.From(outputConn[graph.Output[0].Name]);
        }

        private static Layer ParseNode(NodeProto node, Dictionary<string, TensorType> types, Dictionary<string, OutputConnector> outputConns)
        {
            switch (node.OpType)
            {
                case "Constant":
                    return ParseConstantNode(node, types, outputConns);
                case "MatMul":
                    return ParseMatMulNode(node, types, outputConns);
                case "Add":
                    return ParseAddNode(node, types, outputConns);
                case "Softmax":
                    return ParseSoftmaxNode(node, types, outputConns);
                default:
                    break;
            }

            throw new NotSupportedException($"Node of OpType: {node.OpType} is not supported.");
        }

        private static Layer ParseSoftmaxNode(NodeProto node, Dictionary<string, TensorType> types, Dictionary<string, OutputConnector> outputConns)
        {
            var inputType = types[node.Input[0]];
            var axis = node.Attribute.SingleOrDefault(o => o.Name == "axis");
            var layer = new Softmax(inputType.Dimensions, (int?)axis?.I ?? 1) { Name = node.Name };
            outputConns[node.Input[0]].Connect(layer.Input);

            types.Add(node.Output[0], new TensorType { ElementType = typeof(double), Dimensions = layer.Output.Dimensions.ToArray() });
            outputConns.Add(node.Output[0], layer.Output);
            return layer;
        }

        private static Layer ParseAddNode(NodeProto node, Dictionary<string, TensorType> types, Dictionary<string, OutputConnector> outputConns)
        {
            var aType = types[node.Input[0]];
            var bType = types[node.Input[1]];
            var broadcast = node.Attribute.Single(o => o.Name == "broadcast");
            var axis = node.Attribute.SingleOrDefault(o => o.Name == "axis");
            var layer = new Add(aType.Dimensions, bType.Dimensions, (int)broadcast.I, (int?)axis?.I) { Name = node.Name };
            outputConns[node.Input[0]].Connect(layer.A);
            outputConns[node.Input[1]].Connect(layer.B);

            types.Add(node.Output[0], new TensorType { ElementType = typeof(double), Dimensions = layer.Y.Dimensions.ToArray() });
            outputConns.Add(node.Output[0], layer.Y);
            return layer;
        }

        private static Layer ParseMatMulNode(NodeProto node, Dictionary<string, TensorType> types, Dictionary<string, OutputConnector> outputConns)
        {
            var aType = types[node.Input[0]];
            var bType = types[node.Input[1]];
            var layer = new MatMul(aType.Dimensions, bType.Dimensions) { Name = node.Name };
            outputConns[node.Input[0]].Connect(layer.A);
            outputConns[node.Input[1]].Connect(layer.B);

            types.Add(node.Output[0], new TensorType { ElementType = typeof(double), Dimensions = layer.Y.Dimensions.ToArray() });
            outputConns.Add(node.Output[0], layer.Y);
            return layer;
        }

        private static Layer ParseConstantNode(NodeProto node, Dictionary<string, TensorType> types, Dictionary<string, OutputConnector> outputConns)
        {
            var value = node.Attribute.Single(o => o.Name == "value").T;
            var desiredType = TensorType.From(value);
            types.Add(node.Output[0], desiredType);

            if (desiredType.ElementType == typeof(double))
            {
                Memory<double> mem;
                switch (value.DataType)
                {
                    case TensorProto.Types.DataType.Float:
                        mem = (from f in value.FloatData select (double)f).ToArray();
                        break;
                    case TensorProto.Types.DataType.Double:
                        mem = value.DoubleData.ToArray();
                        break;
                    default:
                        throw new NotSupportedException();
                }

                var tensor = new DenseTensor<double>(mem, desiredType.Dimensions);
                var layer = new Constant<double>(tensor) { Name = node.Name };
                outputConns.Add(node.Output[0], layer.Value);
                return layer;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private static Layer ParseInputVariableNode(ValueInfoProto node, Dictionary<string, TensorType> types, Dictionary<string, OutputConnector> outputConns)
        {
            var desiredType = TensorType.From(node.Type);
            types.Add(node.Name, desiredType);

            if (desiredType.ElementType == typeof(double))
            {
                var layer = new InputVariable<double>(node.Name, desiredType.Dimensions);
                outputConns.Add(node.Name, layer.Value);
                return layer;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private static Layer ParseConstantNode(TensorProto node, Dictionary<string, TensorType> types, Dictionary<string, OutputConnector> outputConns)
        {
            return null;
        }

        private struct TensorType
        {
            public Type ElementType;

            public int[] Dimensions;

            public static TensorType From(TensorProto tensor)
            {
                return new TensorType
                {
                    ElementType = ParseElementType(tensor.DataType),
                    Dimensions = ParseDimensions(tensor.Dims)
                };
            }

            public static TensorType From(TypeProto type)
            {
                return new TensorType
                {
                    ElementType = ParseElementType(type.TensorType.ElemType),
                    Dimensions = ParseDimensions(type.TensorType.Shape.Dim)
                };
            }

            private static int[] ParseDimensions(RepeatedField<TensorShapeProto.Types.Dimension> dim)
            {
                return (from d in dim select (int)d.DimValue).ToArray();
            }

            private static int[] ParseDimensions(RepeatedField<long> dims)
            {
                return (from d in dims select (int)d).ToArray();
            }

            private static Type ParseElementType(TensorProto.Types.DataType dataType)
            {
                switch (dataType)
                {
                    case TensorProto.Types.DataType.Float:
                    case TensorProto.Types.DataType.Float16:
                    case TensorProto.Types.DataType.Double:
                        return typeof(double);
                }

                throw new NotSupportedException();
            }
        }
    }
}
