using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ScribanGen;


internal static class NameSyntaxExtensions
{
    public static string? ExtractName(this NameSyntax? name)
    {
        while (name != null)
        {
            switch (name)
            {
                case IdentifierNameSyntax ins:
                    return ins.Identifier.Text;

                case QualifiedNameSyntax qns:
                    name = qns.Right;
                    break;

                default:
                    return null;
            }
        }

        return null;
    }
}