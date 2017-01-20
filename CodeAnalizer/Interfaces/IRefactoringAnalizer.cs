using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CodeAnalizer.Interfaces
{
    public interface IRefactoringAnalizer<in T> where T:SyntaxNode
    {
        bool DetectRefactoring(T syntaxnode);

        Task<Document> ApplyRefactoring(T syntaxnode, Document document, CancellationToken cancellationToken);
    }
}