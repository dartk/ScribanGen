namespace ScribanGen;


/// <summary>
///
/// </summary>
/// <param name="FilePath"></param>
/// <param name="Text"></param>
/// <param name="LineOffset">Used for reporting diagnostic for templates embedded in comments</param>
public readonly record struct ScribanFile(string FilePath, string Text, int LineOffset = 0)
{
    public string FileName() => Path.GetFileName(this.FilePath);
    public bool IsNotEmpty() => !string.IsNullOrWhiteSpace(this.Text);
    public bool FileNameStartsWithUnderscore() => this.FileName()[0] == '_';
    public bool ShouldBeRendered() => !this.FileNameStartsWithUnderscore();
    public bool CanBeIncluded() => this.FileNameStartsWithUnderscore();
}