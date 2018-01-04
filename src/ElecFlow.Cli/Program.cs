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
            var node = new MatMul(arrA.Dimensions, arrB.Dimensions);
            node.A = arrA;
            node.B = arrB;
            var output = node.Y;
            var arrC = new DenseTensor<double>(new[] { 2.0, 2 }, new[] { 2 });
            var node2 = new Add(output.Dimensions, arrC.Dimensions, broadcast: 1, axis: 1);
            node2.A = output;
            node2.B = arrC;
            output = node2.Y;

            Console.WriteLine("Hello World!");
        }
    }
}
