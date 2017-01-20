using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalizer.Interfaces
{
    public interface ISyntaxTreeItem
    {
        CompilationUnitSyntax Back(int pages);

        void Add(CompilationUnitSyntax compilationUnitSyntax);
    }
}