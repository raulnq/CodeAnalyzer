using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Jal.CodeAnalyzer.Core.Impl;
using Jal.CodeAnalyzer.Core.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jal.CodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExtractLocalVariableDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JalCodeAnalyzer_ExtractLocalVariable";
        public static ISyntaxTreeHistorial SyntaxTreeHistorial = new SyntaxTreeHistorial(3);
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ExtractLocalVariable_AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ExtractLocalVariable_AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ExtractLocalVariable_AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Refactoring";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {

            context.RegisterSyntaxNodeAction(ReportDiagnostic, SyntaxKind.ExpressionStatement);

            context.RegisterSyntaxTreeAction(UpdateHistorial);
        }

        private void UpdateHistorial(SyntaxTreeAnalysisContext syntaxTreeAnalysisContext)
        {
            SyntaxTreeHistorial.Save(syntaxTreeAnalysisContext.Tree.GetRoot() as CompilationUnitSyntax, syntaxTreeAnalysisContext.Options);
        }

        private void ReportDiagnostic(SyntaxNodeAnalysisContext obj)
        {
            var incompleteMember = obj.Node as ExpressionStatementSyntax;

            if (new ExtractLocalVariable(SyntaxTreeHistorial,new IdentifierNameBuilder()).DetectRefactoring(incompleteMember))
            {
                var diagnostic = Diagnostic.Create(Rule, incompleteMember.GetLocation());

                obj.ReportDiagnostic(diagnostic);
            }
        }
    }
}
