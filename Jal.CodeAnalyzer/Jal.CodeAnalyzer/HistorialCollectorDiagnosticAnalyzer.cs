using System.Collections.Immutable;
using Jal.CodeAnalyzer.Core.Impl;
using Jal.CodeAnalyzer.Core.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jal.CodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HistorialCollectorDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JalCodeAnalyzer_HistorialCollector";
        public static ISyntaxTreeHistorial SyntaxTreeHistorial = new SyntaxTreeHistorial(3);

        private const string Category = "Refactoring";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Syntax tree historial collector", "Syntax tree historial collector", Category, DiagnosticSeverity.Info, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {

            context.RegisterSyntaxTreeAction(UpdateHistorial);
        }

        private void UpdateHistorial(SyntaxTreeAnalysisContext syntaxTreeAnalysisContext)
        {
            SyntaxTreeHistorial.Save(syntaxTreeAnalysisContext.Tree.GetRoot() as CompilationUnitSyntax, syntaxTreeAnalysisContext.Options);
        }

    }
}
