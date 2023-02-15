using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban.Parsing;
using Scriban.Syntax;


namespace CSharp.SourceGen.Scriban;


[Generator]
public class ScribanGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }


    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles)
        {
            if (Path.GetExtension(file.Path) != ".scriban" ||
                Path.GetFileName(file.Path).First() == '_')
            {
                continue;
            }

            try
            {
                var source = this._scribanRenderer.Render(Path.GetFullPath(file.Path));
                context.AddSource(Path.GetFileNameWithoutExtension(file.Path), source);
            }
            catch (ScriptRuntimeException ex)
            {
                var scribanSpan = ex.Span;

                var textSpan = TextSpan.FromBounds(
                    scribanSpan.Start.Offset,
                    scribanSpan.End.Offset);

                var linePositionSpan = new LinePositionSpan(
                    ScribanTextPositionToLinePosition(scribanSpan.Start),
                    ScribanTextPositionToLinePosition(scribanSpan.End));

                static LinePosition ScribanTextPositionToLinePosition(TextPosition textPosition) =>
                    new(textPosition.Line, textPosition.Column);

                var location = Location.Create(scribanSpan.FileName, textSpan, linePositionSpan);
                context.ReportDiagnostic(Diagnostic.Create(
                    ScribanError, location, ex.OriginalMessage));
            }
        }
    }


    private readonly ScribanRenderer _scribanRenderer = new();


    private static readonly DiagnosticDescriptor ScribanError = new(
        id: $"{DiagnosticIdPrefix}",
        title: "Scriban template rendering error",
        messageFormat: "{0}",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    private const string DiagnosticIdPrefix = "SourceGen.Scriban";
    private const string DiagnosticCategory = "CSharp.SourceGen.Scriban";
}