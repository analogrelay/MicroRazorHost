using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MicroRazorHost.Test
{
    public class MicroRazorCompilerTests
    {
        [Fact]
        public void SimpleTemplate()
        {
            // The template
            const string templ = "<p>@Environment.Version</p>";
            string expected = "<p>" + Environment.Version.ToString() + "</p>";

            // Arrange
            MicroRazorCompiler compiler = MicroRazorCompiler.CreateCSharp();
            compiler.Host.NamespaceImports.Add("System");
            
            // Act
            var results = compiler.Compile(templ);
            Assert.True(results.Success);
            Assert.Equal(expected, results.Compiled.Run());
        }

        [Fact]
        public void HtmlEncoding()
        {
            // The template
            const string templ = "<p>@(\"<p>\")</p>";
            string expected = "<p>&lt;p&gt;</p>";

            // Arrange
            MicroRazorCompiler compiler = MicroRazorCompiler.CreateCSharp();
            compiler.Host.NamespaceImports.Add("System");

            // Act
            var results = compiler.Compile(templ);
            Assert.True(results.Success);
            Assert.Equal(expected, results.Compiled.Run());
        }

        [Fact]
        public void LiteralAttribute()
        {
            // The template
            const string templ = "<p class=\"foo\"/>";
            string expected = "<p class=\"foo\"/>";

            // Arrange
            MicroRazorCompiler compiler = MicroRazorCompiler.CreateCSharp();
            compiler.Host.NamespaceImports.Add("System");

            // Act
            var results = compiler.Compile(templ);
            Assert.True(results.Success);
            Assert.Equal(expected, results.Compiled.Run());
        }

        [Fact]
        public void NonLiteralAttribute()
        {
            // The template
            const string templ = "<p class=\"@Environment.Version\"/>";
            string expected = "<p class=\"" + Environment.Version.ToString() + "\"/>";

            // Arrange
            MicroRazorCompiler compiler = MicroRazorCompiler.CreateCSharp();
            compiler.Host.NamespaceImports.Add("System");

            // Act
            var results = compiler.Compile(templ);
            Assert.True(results.Success);
            Assert.Equal(expected, results.Compiled.Run());
        }

        [Fact]
        public void ConditionalAttribute()
        {
            // The template
            const string templ = "@{object f = null;}<p class=\"foo @f bar\" />";
            string expected = "<p class=\"foo bar\" />";

            // Arrange
            MicroRazorCompiler compiler = MicroRazorCompiler.CreateCSharp();
            compiler.Host.NamespaceImports.Add("System");

            // Act
            var results = compiler.Compile(templ);
            Assert.True(results.Success);
            Assert.Equal(expected, results.Compiled.Run());
        }

        [Fact]
        public void DynamicModels()
        {
            // The template
            const string templ = "<p>@Model.Foo:@Model.Bar</p>";
            string expected = "<p>Bim:Bop</p>";

            // The model
            var model = new { Foo = "Bim", Bar = "Bop" };

            // Arrange
            MicroRazorCompiler compiler = MicroRazorCompiler.CreateCSharp();
            compiler.Host.NamespaceImports.Add("System");

            // Act
            var results = compiler.Compile(templ);
            Assert.True(results.Success);
            Assert.Equal(expected, results.Compiled.Run(model));
        }
    }
}
