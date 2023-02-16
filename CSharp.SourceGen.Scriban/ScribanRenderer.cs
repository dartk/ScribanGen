using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;


namespace CSharp.SourceGen.Scriban;


public class ScribanRenderer
{
    public ScribanRenderer()
    {
        this._templateLoader = new DirectoryTemplateLoader();
        this._templateContext = new TemplateContext { TemplateLoader = this._templateLoader };
    }


    public string Render(ScribanFile scribanFile, Action<Diagnostic> reportDiagnostic)
    {
        try
        {
            this._templateLoader.TemplateDirectory = Path.GetDirectoryName(scribanFile.FilePath);
            return Template.Parse(scribanFile.Text).Render(this._templateContext);
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
            reportDiagnostic(Diagnostic.Create(
                ScribanError, location, ex.OriginalMessage));

            return string.Empty;
        }
    }


    private readonly TemplateContext _templateContext;
    private readonly DirectoryTemplateLoader _templateLoader;


    private static readonly DiagnosticDescriptor ScribanError = new(
        id: $"{DiagnosticIdPrefix}",
        title: "Scriban template rendering error",
        messageFormat: "{0}",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    private const string DiagnosticIdPrefix = "SourceGen.Scriban";
    private const string DiagnosticCategory = "CSharp.SourceGen.Scriban";


    /// <summary>
    /// Loads templates from specified folder.
    /// </summary>
    private class DirectoryTemplateLoader : ITemplateLoader
    {
        public string? TemplateDirectory;


        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            if (this.TemplateDirectory == null)
            {
                return string.Empty;
            }

            return Path.Combine(this.TemplateDirectory, templateName);
        }


        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return File.ReadAllText(templatePath);
        }


        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan,
            string templatePath)
        {
            return new ValueTask<string>(File.ReadAllText(templatePath));
        }
    }
}