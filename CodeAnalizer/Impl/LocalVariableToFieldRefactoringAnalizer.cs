using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalizer.Extensions;
using CodeAnalizer.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalizer.Impl
{
    public class LocalVariableToFieldRefactoringAnalizer : IRefactoringAnalizer<IncompleteMemberSyntax>
    {
        private readonly ISyntaxTreeHistorial _syntaxTreeHistorial;

        public LocalVariableToFieldRefactoringAnalizer(ISyntaxTreeHistorial syntaxTreeHistorial)
        {
            _syntaxTreeHistorial = syntaxTreeHistorial;
        }

        private bool ValidIncompleteMember(IncompleteMemberSyntax incompleteMemberSyntax, IdentifierNameSyntax identifiername)
        {
            return incompleteMemberSyntax?.Parent is ClassDeclarationSyntax && !string.IsNullOrWhiteSpace(identifiername?.Identifier.ValueText);
        }

        private bool ValidLocalDeclaration(LocalDeclarationStatementSyntax localDeclarationStatementSyntax)
        {
            return localDeclarationStatementSyntax.Declaration.Variables.Count == 1 &&
                   !localDeclarationStatementSyntax.SemicolonToken.IsMissing;
        }

        public bool DetectRefactoring(IncompleteMemberSyntax incompleteMemberSyntax)
        {
            var identifiername = incompleteMemberSyntax.SearchFirstDescedantAndSelf<IdentifierNameSyntax>();

            if (ValidIncompleteMember(incompleteMemberSyntax, identifiername))
            {
                var compilationUnitSyntax = incompleteMemberSyntax.SearchFirstAncestorsAndSelf<CompilationUnitSyntax>();

                var historial = _syntaxTreeHistorial.Get(compilationUnitSyntax);

                var span = historial?.Back(2)
                    .SearchDescedantAndSelf<LocalDeclarationStatementSyntax>()
                    .FirstOrDefault(x=> ValidLocalDeclaration(x) && x.Declaration.Variables[0].Identifier.ValueText == identifiername.Identifier.ValueText)?
                    .SourceSpan();

                if (span != null)
                {
                    var secondlocaldeclaration = historial?.Back(1)
                        .FindNode(span.Value)
                        .SearchDescedantAndSelf<LocalDeclarationStatementSyntax>()
                        .FirstOrDefault(x=> ValidLocalDeclaration(x) && string.IsNullOrWhiteSpace(x.Declaration.Variables[0].Identifier.ValueText));

                    if (secondlocaldeclaration != null)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public async Task<Document> ApplyRefactoring(IncompleteMemberSyntax incompleteMemberSyntax, Document document, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var identifiername = incompleteMemberSyntax.SearchFirstDescedantAndSelf<IdentifierNameSyntax>();

            var currentcompilationunit = incompleteMemberSyntax.SearchFirstAncestorsAndSelf<CompilationUnitSyntax>();

            if (currentcompilationunit != null)
            {

                var historial = _syntaxTreeHistorial.Get(currentcompilationunit);

                if (historial!=null)
                {
                    var firstcompilationunit = historial.Back(2);

                    var firstlocaldeclaration = firstcompilationunit.SearchDescedantAndSelf<LocalDeclarationStatementSyntax>()
                        .FirstOrDefault(x => x.Declaration.Variables.Count == 1 && !x.SemicolonToken.IsMissing && x.Declaration.Variables[0].Identifier.ValueText == identifiername.Identifier.ValueText);

                    if (firstlocaldeclaration != null)
                    {
                        var localdeclarationspan = firstlocaldeclaration.GetLocation().SourceSpan;

                        var secondcompilationunit = historial.Back(1);

                        var secondlocaldeclaration = secondcompilationunit.FindNode(localdeclarationspan).SearchDescedantAndSelf<LocalDeclarationStatementSyntax>()
                            .FirstOrDefault(x => x.Declaration.Variables.Count == 1 && !x.SemicolonToken.IsMissing && string.IsNullOrWhiteSpace(x.Declaration.Variables[0].Identifier.ValueText));

                        var localdeclarationtodeletespan = secondlocaldeclaration.GetLocation().SourceSpan;

                        var localdeclarationtodelete = root.FindNode(localdeclarationtodeletespan).SearchFirstDescedantAndAncestorsAndSelf<LocalDeclarationStatementSyntax>();

                        var fielddeclarationtoinsert = SyntaxFactory.FieldDeclaration(new SyntaxList<AttributeListSyntax>() { }, SyntaxFactory.TokenList(new SyntaxToken[] { SyntaxFactory.Token(SyntaxKind.PrivateKeyword) }), firstlocaldeclaration.Declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                        var rootwithoutlocaldeclaration = root.RemoveNode(localdeclarationtodelete, SyntaxRemoveOptions.KeepNoTrivia);

                        var incompletemember = rootwithoutlocaldeclaration.SearchDescedantAndSelf<IncompleteMemberSyntax>().FirstOrDefault(x => x.Parent is ClassDeclarationSyntax && x.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(y => y.Identifier.ValueText == identifiername.Identifier.ValueText));

                        var rootwithfielddeclaration = rootwithoutlocaldeclaration.InsertNodesAfter(incompletemember, new[] { fielddeclarationtoinsert });

                        var incompletemembertodelete = rootwithfielddeclaration.SearchDescedantAndSelf<IncompleteMemberSyntax>().FirstOrDefault(x => x.Parent is ClassDeclarationSyntax && x.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(y => y.Identifier.ValueText == identifiername.Identifier.ValueText));

                        var finalroot = rootwithfielddeclaration.RemoveNode(incompletemembertodelete, SyntaxRemoveOptions.KeepNoTrivia);

                        return document.WithSyntaxRoot(finalroot);
                    }
                }
            }

            return document;
        }
    }
}