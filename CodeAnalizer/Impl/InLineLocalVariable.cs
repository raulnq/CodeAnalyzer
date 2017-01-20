using System;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalizer.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalizer.Impl
{
    public class InLineLocalVariable : IRefactoringAnalizer<LocalDeclarationStatementSyntax>
    {
        public bool DetectRefactoring(LocalDeclarationStatementSyntax syntaxnode)
        {
            throw new NotImplementedException();
        }

        public Task<Document> ApplyRefactoring(LocalDeclarationStatementSyntax syntaxnode, Document document, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}