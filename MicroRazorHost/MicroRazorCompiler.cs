using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Razor;

namespace MicroRazorHost
{
    public class MicroRazorCompiler
    {
        private RazorCodeLanguage _language;

        public RazorEngineHost Host { get; private set; }

        public MicroRazorCompiler(RazorCodeLanguage language)
        {
            _language = language;
            Host = ConfigureHost();
        }

        protected virtual RazorEngineHost ConfigureHost()
        {
            RazorEngineHost host = new RazorEngineHost(_language);
            host.DefaultBaseClass = typeof(TemplateBase).FullName;
            host.NamespaceImports.Add("System");
            host.NamespaceImports.Add("System.Collections");
            host.NamespaceImports.Add("System.Collections.Generic");
            host.NamespaceImports.Add("System.Dynamic");
            host.NamespaceImports.Add("System.Linq");
            return host;
        }

        public CompilationResults Compile(string template)
        {
            Host.DefaultClassName = "Template";
            Host.DefaultNamespace = "__CompiledTemplates";
            RazorTemplateEngine engine = new RazorTemplateEngine(Host);
            GeneratorResults generatorResults;
            using (TextReader reader = new StringReader(template))
            {
                generatorResults = engine.GenerateCode(reader);
            }

            if (!generatorResults.Success)
            {
                return new CompilationResults(generatorResults, new List<CompilerError>());
            }

            var referencedAssemblies = new[]
            {
                "mscorlib.dll",
                "system.dll",
                "system.core.dll",
                "microsoft.csharp.dll", 
                typeof(TemplateBase).Assembly.Location,
            };
            var compilerParameters = new CompilerParameters(referencedAssemblies)
            {
                GenerateInMemory = true,
                CompilerOptions = "/optimize"
            };


            // Compile the code to a temporary assembly
            CodeDomProvider provider = Activator.CreateInstance(_language.CodeDomProviderType) as CodeDomProvider;
            if (provider == null)
            {
                throw new InvalidCastException(String.Format("Unable to convert '{0}' to a CodeDomProvider", _language.CodeDomProviderType.FullName));
            }
            var compilerResults = provider.CompileAssemblyFromDom(compilerParameters, generatorResults.GeneratedCode);
            if (compilerResults.Errors.HasErrors)
            {
                return new CompilationResults(generatorResults, compilerResults.Errors.Cast<CompilerError>().ToList());
            }
            var type = compilerResults.CompiledAssembly.GetType("__CompiledTemplates.Template");
            if (type == null)
            {
                throw new MissingMemberException("Unable to find compiled template in assembly");
            }
            TemplateBase compiled = Activator.CreateInstance(type) as TemplateBase;
            if (compiled == null)
            {
                throw new InvalidCastException("Unable to convert template to TemplateBase");
            }
            return new CompilationResults(generatorResults, compiled);
        }

        public static MicroRazorCompiler CreateCSharp()
        {
            return new MicroRazorCompiler(new CSharpRazorCodeLanguage());
        }

        public static MicroRazorCompiler CreateVB()
        {
            return new MicroRazorCompiler(new VBRazorCodeLanguage());
        }
    }
}
