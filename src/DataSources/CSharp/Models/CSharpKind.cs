namespace SimpleRag.DataSources.CSharp.Models;

/// <summary>
/// Represents the kind of C# code element.
/// </summary>
public enum CSharpKind
{
    /// <summary>No specific kind.</summary>
    None,

    /// <summary>An interface declaration.</summary>
    Interface,

    /// <summary>A delegate declaration.</summary>
    Delegate,

    /// <summary>An enum declaration.</summary>
    Enum,

    /// <summary>A method declaration.</summary>
    Method,

    /// <summary>A class declaration.</summary>
    Class,

    /// <summary>A struct declaration.</summary>
    Struct,

    /// <summary>A record declaration.</summary>
    Record,

    /// <summary>A constructor declaration.</summary>
    Constructor
}