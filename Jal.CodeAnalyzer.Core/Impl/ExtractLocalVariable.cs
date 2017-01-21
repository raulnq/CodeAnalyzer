using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jal.CodeAnalyzer.Core.Extensions;
using Jal.CodeAnalyzer.Core.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jal.CodeAnalyzer.Core.Impl
{
    public class ExtractLocalVariable : IRefactoringAnalizer<ExpressionStatementSyntax>
    {
        private readonly ISyntaxTreeHistorial _syntaxTreeHistorial;

        private readonly IIdentifierNameBuilder _identifierNameBuilder;

        public ExtractLocalVariable(ISyntaxTreeHistorial syntaxTreeHistorial, IIdentifierNameBuilder identifierNameBuilder)
        {
            _syntaxTreeHistorial = syntaxTreeHistorial;
            _identifierNameBuilder = identifierNameBuilder;
        }

        public bool DetectRefactoring(ExpressionStatementSyntax expressionStatementSyntax)
        {

            var literals = expressionStatementSyntax.SearchDescedantAndSelf<LiteralExpressionSyntax>();

            var assigments = expressionStatementSyntax.SearchDescedantAndSelf<AssignmentExpressionSyntax>();

            var methoddeclaration = expressionStatementSyntax.SearchFirstAncestorsAndSelf<MethodDeclarationSyntax>();

            if (!string.IsNullOrWhiteSpace(methoddeclaration?.Identifier.ValueText) && expressionStatementSyntax.SemicolonToken.IsMissing && literals.Length ==1 && assigments.Length==0)
            {
                var literal = literals.First();

                var kind = literal.Token.Kind();

                if (kind == SyntaxKind.StringLiteralToken || kind == SyntaxKind.NumericLiteralToken)
                {
                    var value = literal.Token.Text;

                    var methodname = methoddeclaration.Identifier.ValueText;

                    var compilationUnitSyntax = expressionStatementSyntax.SearchFirstAncestorsAndSelf<CompilationUnitSyntax>();

                    var historial = _syntaxTreeHistorial.Get(compilationUnitSyntax);

                    var firstliteral = historial?
                        .Back(2)?
                        .SearchMethodDeclarationByName(methodname)?
                        .SearchLiteralExpressionByTextAndKind(value, kind);

                    if (firstliteral != null)
                    {
                        var line = firstliteral.GetLocation().GetLineSpan().StartLinePosition.Line;

                        var secondtliteral = historial?
                            .Back(1)?
                            .SearchMethodDeclarationByName(methodname)
                            .SearcMissingIdentifierNameByLine(line);

                        if (secondtliteral != null)
                        {
                            return true;
                        }
                    }
                    
                }
            }

            return false;
        }

        public SyntaxNode ApplyRefactoring(SyntaxNode root, ExpressionStatementSyntax expressionStatementSyntax)
        {
            var compilationunit = expressionStatementSyntax.SearchFirstAncestorsAndSelf<CompilationUnitSyntax>();

            if (compilationunit != null)
            {
                var method = expressionStatementSyntax.SearchFirstAncestorsAndSelf<MethodDeclarationSyntax>();

                var literal = expressionStatementSyntax.SearchFirstDescedantAndSelf<LiteralExpressionSyntax>();

                var methodname = method.Identifier.ValueText;

                var span = expressionStatementSyntax.SourceSpan();

                var text = literal.Token.Text;

                var kind = literal.Token.Kind();

                var line = _syntaxTreeHistorial
                    .Get(compilationunit)?
                    .Back(2)?
                    .SearchMethodDeclarationByName(methodname)?
                    .SearchLiteralExpressionByTextAndKind(text, kind)?
                    .GetLocation()?
                    .GetLineSpan()
                    .StartLinePosition.Line;

                if (line != null)
                {
                    var missingIdentifier = root
                        .SearchMethodDeclarationByName(methodname)?
                        .SearcMissingIdentifierNameByLine(line.Value);

                    if (missingIdentifier != null)
                    {
                        var name = _identifierNameBuilder.Build();

                        return root
                            .ReplaceIdentifierNameSyntax(name, missingIdentifier)
                            .InsertLocalDeclaration(span, kind, text, name)
                            .RemoveNode(span);
                    }
                }
            }

            return root;
        }

        public async Task<Document> ApplyRefactoring(ExpressionStatementSyntax expressionStatementSyntax,Document document, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            root = ApplyRefactoring(root, expressionStatementSyntax);

            return document.WithSyntaxRoot(root);
        }
    }
}