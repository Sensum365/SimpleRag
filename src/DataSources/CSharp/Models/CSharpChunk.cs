using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace SimpleRag.DataSources.CSharp.Models;

/// <summary>
/// Represents a chunk of C# code.
/// </summary>
/// <param name="kind">The kind of C# code element.</param>
/// <param name="namespace">The namespace of the code element.</param>
/// <param name="parent">The parent of the code element.</param>
/// <param name="parentKind">The kind of the parent code element.</param>
/// <param name="name">The name of the code element.</param>
/// <param name="xmlSummary">The XML documentation summary for the code element.</param>
/// <param name="value">The string representation of the code element.</param>
/// <param name="dependencies">A list of dependencies for the code element.</param>
/// <param name="node">The syntax node from Roslyn.</param>
public class CSharpChunk(CSharpKind kind, string @namespace, string? parent, CSharpKind? parentKind, string name, string xmlSummary, string value, List<string> dependencies, SyntaxNode node)
{
    /// <summary>
    /// Gets the kind of C# code element.
    /// </summary>
    public CSharpKind Kind { get; } = kind;

    /// <summary>
    /// Gets the namespace of the code element.
    /// </summary>
    public string Namespace { get; } = @namespace;

    /// <summary>
    /// Gets the parent of the code element.
    /// </summary>
    public string? Parent { get; } = parent;

    /// <summary>
    /// Gets the kind of the parent code element.
    /// </summary>
    public CSharpKind? ParentKind { get; } = parentKind;

    /// <summary>
    /// Gets the name of the code element.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets or sets the XML documentation summary for the code element.
    /// </summary>
    public string XmlSummary { get; set; } = xmlSummary;

    /// <summary>
    /// Gets the string representation of the code element.
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Gets a list of dependencies for the code element.
    /// </summary>
    public List<string> Dependencies { get; } = dependencies;

    /// <summary>
    /// Gets the syntax node from Roslyn.
    /// </summary>
    public SyntaxNode Node { get; } = node;

    /// <summary>
    /// Gets the kind of the code element as a string.
    /// </summary>
    public string KindAsString => Kind.ToString();

    /// <summary>
    /// Gets the kind of the parent code element as a string.
    /// </summary>
    public string? ParentKindAsString => ParentKind?.ToString();

    /// <summary>
    /// Gets the full path to the code element.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a list of other chunks that this chunk references.
    /// </summary>
    public List<CSharpChunk>? References { get; set; }

    /// <summary>
    /// Gets or sets the file path of the source code.
    /// </summary>
    public string SourcePath { get; set; } = "";

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{Kind}: {Path}";
    }

    /// <summary>
    /// Gets the display string for the code element.
    /// </summary>
    /// <returns>The formatted display string.</returns>
    public string GetDisplayString()
    {
        return Formatter.Format(Node, new AdhocWorkspace()).ToString();
    }
}