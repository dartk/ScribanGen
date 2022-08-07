using Microsoft.CodeAnalysis;


namespace ScribanCSharpGenerator;


[Generator]
public class ScribanGenerator : ISourceGenerator {

    public void Initialize(GeneratorInitializationContext context) {
    }


    public void Execute(GeneratorExecutionContext context) {
        foreach (var file in context.AdditionalFiles) {
            if (Path.GetExtension(file.Path) != ".scriban" || Path.GetFileName(file.Path).First() == '_') {
                continue;
            }

            var source = this._scribanRenderer.Render(Path.GetFullPath(file.Path));
            context.AddSource(Path.GetFileNameWithoutExtension(file.Path), source);
        }
    }


    private readonly ScribanRenderer _scribanRenderer = new ();

}