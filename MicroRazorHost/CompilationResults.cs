using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Razor;

namespace MicroRazorHost
{
    public class CompilationResults : GeneratorResults
    {
        public TemplateBase Compiled { get; private set; }
        public IList<CompilerError> Errors { get; private set; }
        
        public CompilationResults(GeneratorResults generatorResults, IList<CompilerError> errors)
            : base(false, generatorResults.Document, generatorResults.ParserErrors, generatorResults.GeneratedCode, generatorResults.DesignTimeLineMappings)
        {
            Errors = errors;
        }

        public CompilationResults(GeneratorResults generatorResults, TemplateBase compiled)
            : base(true, generatorResults.Document, generatorResults.ParserErrors, generatorResults.GeneratedCode, generatorResults.DesignTimeLineMappings)
        {
            Compiled = compiled;
        }
    }
}
