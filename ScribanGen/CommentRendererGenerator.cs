using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Parsing;


namespace ScribanGen;


[Generator]
public class CommentRendererGenerator : IIncrementalGenerator
{
    private const string FirstLine = "/* ScribanGen render";


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node.IsKind(SyntaxKind.CompilationUnit),
                transform: static (context, _) =>
                {
                    var node = context.Node;
                    var triviaArray = node.DescendantTrivia().Where(static t =>
                            t.IsKind(SyntaxKind.MultiLineCommentTrivia) &&
                            t.ToString().StartsWith(FirstLine,
                                StringComparison.InvariantCultureIgnoreCase))
                        .ToImmutableArray();

                    return triviaArray;
                })
            .Where(static array => !array.IsEmpty);

        context.RegisterSourceOutput(provider, static (context, triviaArray) =>
        {
            var token = context.CancellationToken;

            var changes = new List<TextChange>();
            AddRemoveUnrelatedNodesChanges(changes, triviaArray);

            var tree = triviaArray[0].SyntaxTree!;
            var filePath = tree.FilePath;
            var source = tree.GetText();

            foreach (var trivia in triviaArray)
            {
                token.ThrowIfCancellationRequested();

                var lineOffset = source.Lines.GetLinePosition(trivia.SpanStart).Line + 1;
                var templateText = RemoveFirstAndLastLines(trivia.ToString());
                var scribanFile = new ScribanFile(filePath, templateText, lineOffset);
                var renderedText = ScribanRenderer.Render(scribanFile, context.ReportDiagnostic);
                var change = new TextChange(trivia.Span, renderedText);
                changes.Add(change);
            }

            token.ThrowIfCancellationRequested();
            var newSource = source.WithChanges(changes);
            var fileName = Path.GetFileNameWithoutExtension(filePath) + ".g.cs";
            context.AddSource(fileName, newSource);


            static string RemoveFirstAndLastLines(string str)
            {
                var firstLineBreakIndex = str.IndexOf('\n');
                str = str.Remove(0, firstLineBreakIndex + 1);
                var lastLineBreakIndex = str.LastIndexOf('\n');
                str = str.Remove(lastLineBreakIndex, str.Length - lastLineBreakIndex);
                return str;
            }
        });


        context.RegisterSourceOutput(provider, static (context, arg) =>
        {
        });
    }


    [ThreadStatic] private static ScribanRenderer? _scribanRenderer;


    private static ScribanRenderer ScribanRenderer
    {
        get
        {
            _scribanRenderer ??= new ScribanRenderer();
            return _scribanRenderer;
        }
    }


    static void AddRemoveUnrelatedNodesChanges(List<TextChange> textChanges,
        ImmutableArray<SyntaxTrivia> triviaArray)
    {
        void AddRemoveChange(TextSpan span)
        {
            textChanges.Add(RemoveChange(span));
        }

        // nodes that should not be deleted
        var protectedNodes = new HashSet<SyntaxNode>();

        foreach (var trivia in triviaArray)
        {
            var node = trivia.Token.Parent!;
            protectedNodes.Add(node);

            var parent = node.Parent;
            for (; parent is not null; node = parent, parent = node.Parent)
            {
                foreach (var otherNode in parent.ChildNodes())
                {
                    if (ReferenceEquals(otherNode, node))
                    {
                        protectedNodes.Add(otherNode);
                    }
                }
            }
        }

        var tree = triviaArray[0].SyntaxTree!;
        var root = tree.GetRoot();
        protectedNodes.Add(root);
        AddRemoveUnprotectedNodesChanges(root);


        void AddRemoveUnprotectedNodesChanges(SyntaxNode node)
        {
            // keep nodes of this types
            if (node.Kind() is SyntaxKind.UsingDirective
                or SyntaxKind.IdentifierName
                or SyntaxKind.QualifiedName)
            {
                return;
            }

            // FileScopedNamespaceDeclaration is checked for a case when a template is
            // placed just in a file scoped namespace outside of any type
            if (!protectedNodes.Contains(node)
                && node.Kind() is not SyntaxKind.FileScopedNamespaceDeclaration)
            {
                AddRemoveChange(node.Span);
                return;
            }

            // do not remove children for nodes of this type
            if (node.Kind() is SyntaxKind.MethodDeclaration)
            {
                return;
            }

            foreach (var child in node.ChildNodes())
            {
                AddRemoveUnprotectedNodesChanges(child);
            }
        }
    }


    static TextChange RemoveChange(TextSpan span)
    {
        return new TextChange(span, string.Empty);
    }
}