using System.Linq;
using Jal.CodeAnalyzer.Core.Impl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jal.CodeAnalyzer.Core.Extensions
{
    public static class SyntaxNodeExtension
    {
        public static T[] SearchDescedantAndSelf<T>(this SyntaxNode syntaxNode)
        {
            return syntaxNode.DescendantNodesAndSelf().OfType<T>().ToArray();
        }
        public static T SearchFirstDescedantAndSelf<T>(this SyntaxNode syntaxNode)
        {
            return syntaxNode.DescendantNodesAndSelf().OfType<T>().FirstOrDefault();
        }

        public static T SearchFirstDescedantAndAncestorsAndSelf<T>(this SyntaxNode syntaxNode) where T : class
        {
            return syntaxNode.DescendantNodesAndSelf().OfType<T>().FirstOrDefault() ?? syntaxNode.AncestorsAndSelf().OfType<T>().FirstOrDefault();
        }

        public static T SearchFirstAncestorsAndSelf<T>(this SyntaxNode syntaxNode)
        {
            return syntaxNode.AncestorsAndSelf().OfType<T>().FirstOrDefault();
        }

        public static TextSpan SourceSpan(this SyntaxNode syntaxNode)
        {
            return syntaxNode.GetLocation().SourceSpan;
        }

        public static MethodDeclarationSyntax SearchMethodDeclarationByName(this SyntaxNode syntaxNode, string methodname)
        {
            return syntaxNode.SearchDescedantAndSelf<MethodDeclarationSyntax>().FirstOrDefault(x => x.Identifier.ValueText == methodname);
        }

        public static LiteralExpressionSyntax SearchLiteralExpressionByTextAndKind(this SyntaxNode syntaxNode, string text, SyntaxKind kind)
        {
            return syntaxNode.SearchDescedantAndSelf<LiteralExpressionSyntax>().FirstOrDefault(x => x.Token.Text == text && x.Token.Kind() == kind);

        }

        public static IdentifierNameSyntax SearcMissingIdentifierNameByLine(this SyntaxNode syntaxNode, int line)
        {
            return syntaxNode.SearchDescedantAndSelf<IdentifierNameSyntax>()
                                .FirstOrDefault(x => x.Identifier.IsMissing && x.GetLocation().GetLineSpan().StartLinePosition.Line == line);
        }

        public static SyntaxNode RemoveNode(this SyntaxNode root, TextSpan span)
        {
            var node = root.FindNode(span);

            return root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
        }

        public static SyntaxNode InsertLocalDeclaration(this SyntaxNode root, TextSpan span, SyntaxKind kind, string text, string name)
        {
            var node = root.FindNode(span);

            var type = "string";

            if (kind == SyntaxKind.NumericLiteralToken)
            {
                type = "int";
            }

            var localvariable = SimpleSyntaxFactory.CreateLocalVariableDeclaration(text, name, type);

            return root.InsertNodesAfter(node, new[] { localvariable });
        }

        public static SyntaxNode ReplaceIdentifierNameSyntax(this SyntaxNode root, string name, IdentifierNameSyntax missingIdentifier)
        {
            var newidentifier = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(name));

            return root.ReplaceNode(missingIdentifier, newidentifier);
        }
    }
}