using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RazorLight;

namespace ElecFlow.CodeGeneration
{
    internal class VerilogCodeGenContext
    {
        private readonly RazorLightEngine _engine;
        private readonly string _outputDirectory;

        public VerilogCodeGenContext(RazorLightEngine engine, string outputDirectory)
        {
            _engine = engine;
            _outputDirectory = outputDirectory;
        }

        public async Task<string> CompileRenderAsync<T>(string fileName, T model)
        {
            return await _engine.CompileRenderAsync(fileName, model);
        }

        public async Task WriteTemplateFileAsync<T>(string fileName, string templateFile, T model)
        {
            var content = await CompileRenderAsync(templateFile, model);
            File.WriteAllText(Path.Combine(_outputDirectory, fileName) + ".v", content);
        }
    }
}
