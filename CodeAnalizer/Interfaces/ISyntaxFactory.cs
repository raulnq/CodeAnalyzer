using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalizer.Interfaces
{
    public interface ISyntaxFactory
    {
        LocalDeclarationStatementSyntax CreateLocalVariableDeclaration(string text, string name, string type);
    }
}