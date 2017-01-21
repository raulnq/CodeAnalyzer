using System.Collections.Generic;
using System.Linq;
using Jal.CodeAnalyzer.Core.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jal.CodeAnalyzer.Core.Impl
{
    public class SyntaxTreeItem : ISyntaxTreeItem
    {
        private readonly IList<CompilationUnitSyntax> _historial;

        private readonly int _maxHistorialAge;
        public SyntaxTreeItem(int maxHistorialAge)
        {
            _maxHistorialAge = maxHistorialAge;

            _historial = new List<CompilationUnitSyntax>();
        }

        public CompilationUnitSyntax Back(int pages)
        {
            var index = _historial.Count - pages - 1;

            return index >= 0 ? _historial[index] : null;
        }

        public void Add(CompilationUnitSyntax compilationUnitSyntax)
        {
            var code = compilationUnitSyntax.ToFullString();

            if (_historial.Count > 0)
            {
                var lastcode = _historial.Last().ToFullString();

                if (code != lastcode)
                {
                    if (_historial.Count >= _maxHistorialAge)
                    {
                        _historial.Remove(_historial.First());
                    }

                    _historial.Add(compilationUnitSyntax);
                }
            }
            else
            {
                _historial.Add(compilationUnitSyntax);
            }

        }
    }
}