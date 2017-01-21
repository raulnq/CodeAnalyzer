using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Jal.CodeAnalyzer.Core.Impl;
using Jal.CodeAnalyzer.Core.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CodeAnalyzer.Tests
{
    public class Class1
    {
        [Test]
        public void ExtractLocalVariable_()
        {
            var historial = new SyntaxTreeHistorial(3);

            historial.Save(LoadStep("Step1"), WorkspaceKind.Host);

            historial.Save(LoadStep("Step2"), WorkspaceKind.Host);

            var current = LoadStep("Step3");

            historial.Save(current, WorkspaceKind.Host);

            var builder = new Mock<IIdentifierNameBuilder>();

            builder.Setup(x => x.Build()).Returns("name");

            var sut = new ExtractLocalVariable(historial, builder.Object);

            var expressionsyntax = current.FindNode(new TextSpan(142,8)) as ExpressionStatementSyntax;

            var result = sut.DetectRefactoring(expressionsyntax);

            result.ShouldBeTrue();

            var targetcode = LoadResult();

            var code = sut.ApplyRefactoring(current, expressionsyntax).ToFullString();

            code.ShouldBe(targetcode);
        }

        private static CompilationUnitSyntax LoadStep(string step)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + @"\ExtractLocalVariable\" + $"{step}.txt";

            var content = File.ReadAllText(path);

            return CSharpSyntaxTree.ParseText(content, null, "path").GetRoot() as CompilationUnitSyntax;
        }

        private static string LoadResult()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + @"\ExtractLocalVariable\" + $"Result.txt";

            return File.ReadAllText(path);
        }
    }
}
