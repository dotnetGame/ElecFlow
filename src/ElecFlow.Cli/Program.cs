using System;
using System.IO;
using System.Numerics;
using ElecFlow.Layers;

namespace ElecFlow.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = @"helloWorld.pb";
            var graph = FlowGraph.Load(File.OpenRead(file));

            var arrA = new DenseTensor<double>(new[] { 1.0, 2, 3, 4, 5, 6 }, new[] { 2, 3 });
            var arrB = new DenseTensor<double>(new[] { 1.0, 4, 2, 5, 3, 6 }, new[] { 3, 2 });
            var arrC = new DenseTensor<double>(new[] { 2.0, 2 }, new[] { 2 });

            var a = Layer.InputVariable<double>("a", arrA.Dimensions);
            var b = Layer.InputVariable<double>("b", arrB.Dimensions);
            var c = Layer.InputVariable<double>("c", arrC.Dimensions);
            var model = Layer.MatMul(a, b) + c;

            var eva = new Evaluator<double>(model.Y);
            var output = eva.Evaluate(new { a = arrA, b = arrB, c = arrC });

            Console.WriteLine(output);
        }
    }
}
