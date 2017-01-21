using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jal.CodeAnalyzer.Core.Interfaces
{
    public interface ISyntaxTreeItem
    {
        CompilationUnitSyntax Back(int pages);

        void Add(CompilationUnitSyntax compilationUnitSyntax);
    }
}