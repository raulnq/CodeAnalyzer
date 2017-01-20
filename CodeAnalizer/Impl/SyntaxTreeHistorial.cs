using System.Collections.Generic;
using System.Reflection;
using CodeAnalizer.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalizer.Impl
{
    public class SyntaxTreeHistorial : ISyntaxTreeHistorial
    {
        private readonly IDictionary<string, ISyntaxTreeItem> _syntaxTrees;

        private readonly int _maxHistorialAge;

        public SyntaxTreeHistorial(int maxHistorialAge)
        {
            _maxHistorialAge = maxHistorialAge;
            _syntaxTrees = new Dictionary<string, ISyntaxTreeItem>();
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

                    if (_syntaxTrees.ContainsKey(file))
                    {
                        var syntaxTree = _syntaxTrees[file];

                        syntaxTree.Add(compilationUnitSyntax);
                    }
                    else
                    {
                        var syntaxTree = new SyntaxTreeItem(_maxHistorialAge);

                        syntaxTree.Add(compilationUnitSyntax);

                        _syntaxTrees.Add(file, syntaxTree);
                    }
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