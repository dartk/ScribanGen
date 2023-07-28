using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;


namespace ScribanGen;


public class ScribanRenderer
{
    public ScribanRenderer()
    {
        this._templateLoader = new DirectoryTemplateLoader();
        this._templateContext = new TemplateContext { TemplateLoader = this._templateLoader };
    }


    public Template Parse(ScribanFile scribanFile, Action<Diagnostic> reportDiagnostic)
    {
        this._templateLoader.TemplateDirectory = Path.GetDirectoryName(scribanFile.FilePath);
        var template = Template.Parse(scribanFile.Text);
        foreach (var msg in template.Messages)
        {
            var diagnosticType = msg.Type switch
            {
                ParserMessageType.Error => TemplateError,
                ParserMessageType.Warning => TemplateWarning,
                _ => throw new ArgumentOutOfRangeException()
            };
            var location = ToLocation(scribanFile.FilePath, msg.Span, scribanFile.LineOffset);
            var diagnostic = Diagnostic.Create(diagnosticType, location, msg.Message);
            reportDiagnostic(diagnostic);
        }

        return template;
    }


    public string Render(ScribanFile scribanFile, Action<Diagnostic> reportDiagnostic)
    {
        try
        {
            var template = this.Parse(scribanFile, reportDiagnostic);
            return template.HasErrors ? string.Empty : template.Render(this._templateContext);
        }
        catch (ScriptRuntimeException ex)
        {
            var scribanSpan = ex.Span;
            var location = ToLocation(scribanFile.FilePath, scribanSpan, scribanFile.LineOffset);
            var diagnostic = Diagnostic.Create(RenderingError, location, ex.OriginalMessage);
            reportDiagnostic(diagnostic);

            return string.Empty;
        }
    }


    private readonly TemplateContext _templateContext;
    private readonly DirectoryTemplateLoader _templateLoader;


    private static readonly DiagnosticDescriptor TemplateWarning = new(
        id: $"{DiagnosticIdPrefix}01",
        title: "Scriban Template Warning",
        messageFormat: "Scriban template warning: {0}",
        category: DiagnosticCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    private static readonly DiagnosticDescriptor TemplateError = new(
        id: $"{DiagnosticIdPrefix}02",
        title: "Scriban Template Error",
        messageFormat: "{0}",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    private static readonly DiagnosticDescriptor RenderingError = new(
        id: $"{DiagnosticIdPrefix}03",
        title: "Scriban Template Rendering Error",
        messageFormat: "{0}",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    private const string DiagnosticIdPrefix = "SCRIBANGEN";
    private const string DiagnosticCategory = "ScribanGen";


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


    private static Location ToLocation(string fileName, SourceSpan scribanSpan,
        int lineOffset = 0)
    {
        var textSpan = TextSpan.FromBounds(
            scribanSpan.Start.Offset,
            scribanSpan.End.Offset);

        var linePositionSpan = new LinePositionSpan(
            ScribanTextPositionToLinePosition(scribanSpan.Start, lineOffset),
            ScribanTextPositionToLinePosition(scribanSpan.End, lineOffset));
        return Location.Create(fileName, textSpan, linePositionSpan);
    }


    private static LinePosition ScribanTextPositionToLinePosition(TextPosition textPosition,
        int lineOffset) => new(textPosition.Line + lineOffset, textPosition.Column);
}