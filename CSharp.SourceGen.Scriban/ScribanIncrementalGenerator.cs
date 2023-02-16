using System.Text;
using Microsoft.CodeAnalysis;


namespace CSharp.SourceGen.Scriban;


[Generator(LanguageNames.CSharp)]
public class ScribanIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".scriban"))
            .Select(static (file, token) =>
            {
                var source = file.GetText(token)?.ToString() ?? string.Empty;
                return new ScribanFile(file.Path, source);
            })
            .Where(static x => x.IsNotEmpty());

        var filesToInclude = files.Where(static file => file.CanBeIncluded());
        var filesToRender = files.Where(static file => file.ShouldBeRendered());

        // Templates can include other templates that start with underscore
        var filesWithDependencies = filesToRender.Combine(filesToInclude.Collect()).Select(
            static (arg, _) =>
            {
                var (template, includedTemplates) = arg;
                var builder = new StringBuilder();
                foreach (var includedTemplate in includedTemplates)
                {
                    if (!template.Text.Contains(includedTemplate.FileName())) continue;
                    builder.AppendLine(includedTemplate.FileName());
                    builder.AppendLine(includedTemplate.Text);
                }

                return (template, builder.ToString());
            });

        context.RegisterSourceOutput(filesWithDependencies, static (productionContext, scriban) =>
            RenderScribanTemplate(productionContext, scriban.template));
    }


    private static void RenderScribanTemplate(SourceProductionContext context, ScribanFile template)
    {
        var token = context.CancellationToken;

        if (token.IsCancellationRequested) return;
        var renderer = new ScribanRenderer();
        var source = renderer.Render(template, context.ReportDiagnostic);

        if (token.IsCancellationRequested) return;
        context.AddSource(template.FileName() + ".out", source);
    }
}