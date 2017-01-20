using CodeAnalizer.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalizer.Impl
{
    public static class SimpleSyntaxFactory
    {
        public static LocalDeclarationStatementSyntax CreateLocalVariableDeclaration(string text, string name, string type)
        {
            var literal = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, text, text, SyntaxTriviaList.Empty)).WithLeadingTrivia(SyntaxFactory.Space);

            var typeName = SyntaxFactory.ParseTypeName(type);

            var equalsValueClause = SyntaxFactory.EqualsValueClause(literal).WithLeadingTrivia(SyntaxFactory.Space);

            var declarator = SyntaxFactory.VariableDeclarator(name).WithLeadingTrivia(SyntaxFactory.Space).WithInitializer(equalsValueClause);

            var declaration = SyntaxFactory.VariableDeclaration(typeName, SyntaxFactory.SeparatedList(new[] { declarator }));

            return SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.TokenList(), declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(SyntaxFactory.Whitespace("\r\n"));
        }
    }
}