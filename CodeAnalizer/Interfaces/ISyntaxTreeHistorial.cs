using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalizer.Interfaces
{
    public interface ISyntaxTreeHistorial
    {
        void Save(CompilationUnitSyntax compilationUnitSyntax, AnalyzerOptions analyzerOptions);

        ISyntaxTreeItem Get(CompilationUnitSyntax compilationUnitSyntax);
    }
}