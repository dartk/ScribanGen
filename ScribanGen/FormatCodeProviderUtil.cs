using Microsoft.CodeAnalysis;


namespace ScribanGen;


public static class FormatCodeProviderUtil
{
    public static IncrementalValueProvider<bool> FormatCodeProvider(
        this IncrementalGeneratorInitializationContext context)
    {
        return context.AnalyzerConfigOptionsProvider.Select(
            (optionsProvider, _) =>
            {
                var isFalse = optionsProvider.GlobalOptions.TryGetValue(
                        "build_property.ScribanGen_FormatCode",
                        out var value)
                    && value.Equals("false", StringComparison.InvariantCultureIgnoreCase);

                return !isFalse;
            });
    }
}