using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;


namespace ScribanGenerator;


public class ScribanRenderer {

    public ScribanRenderer() {
        this._templateLoader = new DirectoryTemplateLoader();
        this._templateContext = new TemplateContext { TemplateLoader = this._templateLoader };
    }


    public string Render(string fullPath) {
        var template = Template.Parse(File.ReadAllText(fullPath), fullPath);
        this._templateLoader.TemplateDirectory = Path.GetDirectoryName(fullPath);
        return template.Render(this._templateContext);
    }
        

    private readonly TemplateContext _templateContext;
    private readonly DirectoryTemplateLoader _templateLoader;
    

    /// <summary>
    /// Loads templates from specified folder.
    /// </summary>
    private class DirectoryTemplateLoader : ITemplateLoader {

        public string? TemplateDirectory;


        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) {
            if (this.TemplateDirectory == null) {
                return string.Empty;
            }
            
            return Path.Combine(this.TemplateDirectory, templateName);
        }


        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath) {
            return File.ReadAllText(templatePath);
        }


        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath) {
            return new ValueTask<string>(File.ReadAllText(templatePath));
        }

    }
    
}