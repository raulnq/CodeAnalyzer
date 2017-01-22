using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jal.CodeAnalyzer.Core.Impl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Jal.CodeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExtractLocalVariableCodeFixProvider)), Shared]
    public class ExtractLocalVariableCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Make an extract local variable refactoring";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ExtractLocalVariableDiagnosticAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var expressionStatementSyntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ExpressionStatementSyntax>().First();
            //var incompleteMemberSyntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IncompleteMemberSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => new ExtractLocalVariable(ExtractLocalVariableDiagnosticAnalyzer.SyntaxTreeHistorial, new IdentifierNameBuilder()).ApplyRefactoring(expressionStatementSyntax, context.Document, c),
                    equivalenceKey: Title),
                diagnostic);
        }
    }
}