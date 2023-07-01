namespace ScribanGen;


public readonly record struct ScribanFile(string FilePath, string Text)
{
    public string FileName() => Path.GetFileName(this.FilePath);
    public bool IsNotEmpty() => !string.IsNullOrWhiteSpace(this.Text);
    public bool FileNameStartsWithUnderscore() => this.FileName()[0] == '_';
    public bool ShouldBeRendered() => !this.FileNameStartsWithUnderscore();
    public bool CanBeIncluded() => this.FileNameStartsWithUnderscore();
}