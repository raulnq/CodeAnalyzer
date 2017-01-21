using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jal.CodeAnalyzer.Core.Interfaces
{
    public interface ISyntaxTreeHistorial
    {
        void Save(CompilationUnitSyntax compilationUnitSyntax, AnalyzerOptions analyzerOptions);

        ISyntaxTreeItem Get(CompilationUnitSyntax compilationUnitSyntax);
    }
}