using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace ScribanGen;


[Generator]
public class CommentRendererGenerator : IIncrementalGenerator
{
    private const string ScribanRenderMultilineComments = nameof(ScribanRenderMultilineComments);


    private const string ScribanRenderMultilineCommentsAttribute =
        nameof(ScribanRenderMultilineCommentsAttribute);


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource($"{ScribanRenderMultilineCommentsAttribute}.g.cs",
                $$"""
                /// <summary>
                /// Renders multiline comments as Scriban templates except first and last line.
                /// </summary>
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
                internal sealed class {{ScribanRenderMultilineCommentsAttribute}} : System.Attribute
                {
                }
                """);
        });

        var formatCodeProvider = context.FormatCodeProvider();

        var triviaProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is AttributeSyntax attribute
                    && attribute.Name.ExtractName() is ScribanRenderMultilineComments
                        or ScribanRenderMultilineCommentsAttribute,
                transform: static (context, _) =>
                {
                    var attributeNode = context.Node;
                    var typeNode = attributeNode.Parent?.Parent;
                    if (typeNode is null) return ImmutableArray<SyntaxTrivia>.Empty;

                    var triviaArray = typeNode.DescendantTrivia().Where(static t =>
                            t.IsKind(SyntaxKind.MultiLineCommentTrivia))
                        .ToImmutableArray();

                    return triviaArray;
                })
            .Where(static array => !array.IsEmpty);

        context.RegisterSourceOutput(triviaProvider.Combine(formatCodeProvider),
            static (context, arg) =>
            {
                var (triviaArray, formatCode) = arg;
                var token = context.CancellationToken;

                var changes = new List<TextChange>();
                AddRemoveUnrelatedNodesChanges(changes, triviaArray);

                var tree = triviaArray[0].SyntaxTree!;
                var filePath = tree.FilePath;
                var source = tree.GetText();

                foreach (var trivia in triviaArray)
                {
                    token.ThrowIfCancellationRequested();

                    var lineOffset = source.Lines.GetLinePosition(trivia.SpanStart).Line;
                    var templateText = RemoveCommentStartEndSymbols(trivia.ToString());
                    var scribanFile = new ScribanFile(filePath, templateText, lineOffset);
                    var renderedText =
                        ScribanRenderer.Render(scribanFile, context.ReportDiagnostic);
                    var change = new TextChange(trivia.Span, renderedText);
                    changes.Add(change);
                }

                token.ThrowIfCancellationRequested();
                var renderedSource = source.WithChanges(changes);
                var fileName = Path.GetFileNameWithoutExtension(filePath) + ".g.cs";

                if (formatCode)
                {
                    var renderedTree = CSharpSyntaxTree.ParseText(renderedSource);
                    var formattedSource = renderedTree.GetRoot().NormalizeWhitespace().ToString();
                    context.AddSource(fileName, formattedSource);
                }
                else
                {
                    context.AddSource(fileName, renderedSource);
                }


                static string RemoveCommentStartEndSymbols(string str)
                {
                    return str.Substring(2, str.Length - 4);
                }
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

            var parent = node.Parent;
            for (; parent is not null; node = parent, parent = node.Parent)
            {
                foreach (var otherNode in parent.ChildNodes())
                {
                    if (ReferenceEquals(otherNode, node) && node is TypeDeclarationSyntax
                        or NamespaceDeclarationSyntax or CompilationUnitSyntax)
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