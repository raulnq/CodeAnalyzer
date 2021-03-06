using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Jal.CodeAnalyzer.Core.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jal.CodeAnalyzer.Core.Impl
{
    public class SyntaxTreeHistorial : ISyntaxTreeHistorial
    {
        private readonly ConcurrentDictionary<string, ISyntaxTreeItem> _syntaxTrees;

        private readonly int _maxHistorialAge;

        public SyntaxTreeHistorial(int maxHistorialAge)
        {
            _maxHistorialAge = maxHistorialAge;
            _syntaxTrees = new ConcurrentDictionary<string, ISyntaxTreeItem>();
        }

        private bool IsValid(CompilationUnitSyntax compilationUnitSyntax)
        {
            return !string.IsNullOrEmpty(compilationUnitSyntax?.SyntaxTree?.FilePath) && !compilationUnitSyntax.SyntaxTree.FilePath.Contains("TemporaryGeneratedFile");
        }

        public void Save(CompilationUnitSyntax compilationUnitSyntax, string workspaceKind)
        {
            if (workspaceKind == WorkspaceKind.Host)
            {
                if (IsValid(compilationUnitSyntax))
                {
                    var file = compilationUnitSyntax.SyntaxTree.FilePath;

                    var newSyntaxTreeItem = new SyntaxTreeItem(_maxHistorialAge);

                    newSyntaxTreeItem.Add(compilationUnitSyntax);

                    _syntaxTrees.AddOrUpdate(file, newSyntaxTreeItem, (key, syntaxTreeItem) =>
                    {
                        syntaxTreeItem.Add(compilationUnitSyntax);

                        return syntaxTreeItem;
                    });
                }
            }
        }

        public void Save(CompilationUnitSyntax compilationUnitSyntax, AnalyzerOptions analyzerOptions)
        {
            var analizeroptionstype = analyzerOptions.GetType();

            var workspaceproperty = analizeroptionstype.GetRuntimeProperty("Workspace");

            var workspace = workspaceproperty.GetValue(analyzerOptions) as Workspace;

            if (workspace != null)
            {
                Save(compilationUnitSyntax, workspace.Kind);
            }
        }

        public ISyntaxTreeItem Get(CompilationUnitSyntax compilationUnitSyntax)
        {
            var file = compilationUnitSyntax.SyntaxTree.FilePath;

            if (!string.IsNullOrWhiteSpace(file) && _syntaxTrees.ContainsKey(file))
            {
                return _syntaxTrees[file];
            }

            return null;
        }
    }
}