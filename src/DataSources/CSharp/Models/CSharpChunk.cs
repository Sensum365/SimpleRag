using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace SimpleRag.DataSources.CSharp.Models;

public class CSharpChunk(CSharpKind kind, string @namespace, string? parent, CSharpKind? parentKind, string name, string xmlSummary, string value, List<string> dependencies, SyntaxNode node)
{
    public CSharpKind Kind { get; } = kind;

    public string Namespace { get; } = @namespace;

    public string? Parent { get; } = parent;

    public CSharpKind? ParentKind { get; } = parentKind;

    public string Name { get; } = name;

    public string XmlSummary { get; set; } = xmlSummary;

    public string Value { get; } = value;

    public List<string> Dependencies { get; } = dependencies;

    public SyntaxNode Node { get; } = node;

    public string KindAsString => Kind.ToString();

    public string? ParentKindAsString => ParentKind?.ToString();

    public string Path
    {
        get
        {
            StringBuilder sb = new();
            sb.Append(Namespace);
            if (!string.IsNullOrWhiteSpace(Parent))
            {
                sb.Append("." + Parent);
            }

            sb.Append("." + Name);
            return sb.ToString();
        }
    }

    public List<CSharpChunk>? References { get; set; }
    public string SourcePath { get; set; } = "";

    public override string ToString()
    {
        return $"{Kind}: {Path}";
    }

    public string GetDisplayString()
    {
        return Formatter.Format(Node, new AdhocWorkspace()).ToString();
    }
}