using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ElecFlow.IO;
using ElecFlow.Layers;

namespace ElecFlow.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            //var arrA = new DenseTensor<double>(new[] { 1.0, 2, 3, 4, 5, 6 }, new[] { 2, 3 });
            //var arrB = new DenseTensor<double>(new[] { 1.0, 4, 2, 5, 3, 6 }, new[] { 3, 2 });
            //var arrC = new DenseTensor<double>(new[] { 2.0, 2 }, new[] { 2 });

            //var a = Layer.InputVariable<double>("a", arrA.Dimensions);
            //var b = Layer.Constant(arrB);
            //var c = Layer.Constant(arrC);
            //var model = Layer.MatMul(a, b) + c;

            //var graph = FlowGraph.From(model.Y);
            //var output = graph.Evaluate(new { a = arrA });

            var file = @"helloWorld.pb";
            var graph = OnnxLoader.LoadGraph(File.OpenRead(file));

            var test = new[]
            {
                new []{ 3.76050401, 4.98616695 },
                new []{ 6.62845373, 5.21008635 },
                new []{ 3.11362505, 1.43330753 },
                new []{ 3.18209267, 2.82103515 },
                new []{ 2.1363287 , 2.12190652 },
                new []{ 10.30701065, 4.99226809 },
                new []{ 4.41806793, 2.84903097 },
                new []{ 6.66218519, 5.1756258  },
                new []{ 1.90551507, 6.01617098 },
                new []{ 3.56423473, 3.04456925 },
                new []{ 2.23594642, 2.89287257 },
                new []{ 1.00026858, 2.18650651 },
                new []{ 3.53061914, 2.68027806 },
                new []{ 4.6835227 , 2.8291676  },
                new []{ 2.27399611, 4.18976545 },
                new []{ 2.83831811, 1.75445843 },
                new []{ 3.79490495, 6.85835552 },
                new []{ 6.53413057, 5.20747232 },
                new []{ 3.38053751, 2.87436342 },
                new []{ 5.66439772, 4.65558767 },
                new []{ 8.49361515, 6.00447893 },
                new []{ 3.92734599, 3.78306651 },
                new []{ 5.1521697 , 4.97658062 },
                new []{ 6.53090763, 2.86967635 },
                new []{ 3.63992143, 2.41258216 }
            };

            Console.Write("Predicated: ");
            for (int i = 0; i < test.Length; i++)
            {
                var input = new DenseTensor<double>(test[i], new[] { 1, 2 });
                var output = (IEnumerable<double>)graph.Evaluate(new { Input3 = input });
                var max = output.Select((o, idx) => new { Index = idx, Value = o }).OrderByDescending(o => o.Value).First().Index;
                Console.Write($"{max}, ");
            }

            Console.Read();
        }
    }
}
