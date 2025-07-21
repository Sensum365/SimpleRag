using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleRag.DataSources.CSharp.Chunker;

/// <summary>
/// Breaks C# code into smaller chunks for ingestion.
/// </summary>
[PublicAPI]
public class CSharpChunker : ICSharpChunker
{
    /// <summary>
    /// Parses the provided code and returns the discovered code entities.
    /// </summary>
    /// <param name="code">The code to analyze.</param>
    /// <returns>A list of discovered code chunks.</returns>
    public List<CSharpChunk> GetChunks(string code)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        List<CSharpChunk> entries = [];
        entries.AddRange(ProcessTypeDeclaration<ClassDeclarationSyntax>(root, CSharpChunkKind.Class));
        entries.AddRange(ProcessTypeDeclaration<StructDeclarationSyntax>(root, CSharpChunkKind.Struct));
        entries.AddRange(ProcessTypeDeclaration<RecordDeclarationSyntax>(root, CSharpChunkKind.Record));
        entries.AddRange(ProcessEnums(root));
        entries.AddRange(ProcessDelegates(root));
        entries.AddRange(ProcessInterfaces(root));
        return entries;
    }

    private List<CSharpChunk> ProcessInterfaces(SyntaxNode root)
    {
        List<CSharpChunk> result = [];
        InterfaceDeclarationSyntax[] nodes = root.DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .ToArray();
        foreach (InterfaceDeclarationSyntax node in nodes)
        {
            if (!IsPublic(node.Modifiers))
            {
                continue;
            }

            string ns = GetNamespace(node);
            string xmlSummary = GetXmlSummary(node);
            string name = node.Identifier.ValueText;
            var parent = GetParentFromNesting(node.Parent);
            CSharpChunkKind parentKind = GetParentType(node.Parent);
            result.Add(new CSharpChunk(CSharpChunkKind.Interface, ns, parent, parentKind, name, xmlSummary, node.ToString(), [], root));
        }

        return result;
    }

    private List<CSharpChunk> ProcessDelegates(SyntaxNode root)
    {
        List<CSharpChunk> result = [];
        DelegateDeclarationSyntax[] nodes = root.DescendantNodes()
            .OfType<DelegateDeclarationSyntax>()
            .ToArray();

        foreach (DelegateDeclarationSyntax node in nodes)
        {
            if (!IsPublic(node.Modifiers))
            {
                continue;
            }

            string ns = GetNamespace(node);
            string xmlSummary = GetXmlSummary(node);
            string name = node.Identifier.ValueText;
            var parent = GetParentFromNesting(node.Parent);
            CSharpChunkKind parentKind = GetParentType(node.Parent);
            result.Add(new CSharpChunk(CSharpChunkKind.Delegate, ns, parent, parentKind, name, xmlSummary, node.ToString(), [], node));
        }

        return result;
    }

    private List<CSharpChunk> ProcessEnums(SyntaxNode root)
    {
        List<CSharpChunk> result = [];
        EnumDeclarationSyntax[] nodes = root.DescendantNodes().OfType<EnumDeclarationSyntax>().ToArray();
        foreach (EnumDeclarationSyntax node in nodes)
        {
            if (!IsPublic(node.Modifiers))
            {
                continue;
            }

            string ns = GetNamespace(node);
            string xmlSummary = GetXmlSummary(node);
            string name = node.Identifier.ValueText;
            var parent = GetParentFromNesting(node.Parent);
            CSharpChunkKind parentKind = GetParentType(node.Parent);
            result.Add(new CSharpChunk(CSharpChunkKind.Enum, ns, parent, parentKind, name, xmlSummary, node.ToString(), [], node));
        }

        return result;
    }

    private List<CSharpChunk> ProcessTypeDeclaration<T>(SyntaxNode root, CSharpChunkKind kind) where T : TypeDeclarationSyntax
    {
        List<CSharpChunk> result = [];
        var nodes = root.DescendantNodes().OfType<T>().ToArray();
        foreach (T node in nodes)
        {
            if (!IsPublic(node.Modifiers))
            {
                continue;
            }

            PropertyDeclarationSyntax[] properties = GetPublicProperties(node.Members);
            MethodDeclarationSyntax[] methods = GetPublicMethods(node.Members);
            FieldDeclarationSyntax[] constants = GetPublicConstants(node.Members);
            ConversionOperatorDeclarationSyntax[] implicitOperators = GetPublicImplicitOperators(node.Members);
            ConstructorDeclarationSyntax[] constructors = GetPublicConstructors(node.Members);

            string ns = GetNamespace(node);

            //Store methods separately
            foreach (MethodDeclarationSyntax method in methods)
            {
                string name = method.Identifier.ValueText;
                string xmlSummary = GetXmlSummary(method);
                string content = method.ToString().Replace(method.Body?.ToString() ?? Guid.NewGuid().ToString(), "").Trim().Trim();
                string parent = node.Identifier.ValueText;
                CSharpChunkKind parentKind = kind;
                List<string> dependencies = GetMethodDependencies(method);
                result.Add(new CSharpChunk(CSharpChunkKind.Method, ns, parent, parentKind, name, xmlSummary, content, dependencies, method));
            }

            //Store constructors separately
            foreach (ConstructorDeclarationSyntax constructor in constructors)
            {
                string name = constructor.Identifier.ValueText;
                string xmlSummary = GetXmlSummary(constructor);
                ConstructorDeclarationSyntax content = constructor.WithBody(null);
                string parent = node.Identifier.ValueText;
                CSharpChunkKind parentKind = kind;
                var dependencies = constructor.ParameterList.Parameters.Select(x => x.Type?.ToString() ?? "unknown").ToList();
                dependencies = RemoveDuplicateAndTrivialDependencies(dependencies);
                result.Add(new CSharpChunk(CSharpChunkKind.Constructor, ns, parent, parentKind, name, xmlSummary, content.ToString(), dependencies, constructor));
            }

            //Entry itself
            {
                //Store the Type itself with everything but the Methods
                string name = node.Identifier.ValueText;
                List<string> dependencies = [];
                StringBuilder sb = new();

                sb.Append("public ");
                if (IsStatic(node.Modifiers))
                {
                    sb.Append("static ");
                }

                if (IsAbstract(node.Modifiers))
                {
                    sb.Append("abstract ");
                }

                sb.Append($"{kind.ToString().ToLowerInvariant()} {name}"); //Do this better (partial stuff support)!

                //Base Types and Interfaces
                if (node.BaseList != null)
                {
                    bool first = true;
                    foreach (var @base in node.BaseList.Types)
                    {
                        string separator = ", ";
                        if (first)
                        {
                            separator = ": ";
                            first = false;
                        }

                        sb.Append($"{separator}{@base.Type.ToString()} ");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("{");

                foreach (FieldDeclarationSyntax constant in constants)
                {
                    sb.Append(GetXmlSummary(constant));
                    sb.AppendLine(constant.ToString());
                    sb.AppendLine();
                    TypeSyntax type = constant.Declaration.Type;
                    dependencies.Add(type.ToString());
                }

                foreach (PropertyDeclarationSyntax property in properties)
                {
                    string xmlSummary = GetXmlSummary(property);
                    sb.Append(xmlSummary);
                    string value = RemoveAttributes(RemoveExpressionBody(property)).ToString();
                    sb.AppendLine(value);
                    sb.AppendLine();
                    TypeSyntax type = property.Type;
                    dependencies.Add(type.ToString());
                }

                foreach (ConversionOperatorDeclarationSyntax @operator in implicitOperators)
                {
                    string xmlSummary = GetXmlSummary(@operator);
                    sb.Append(xmlSummary);
                    string value = @operator.ToString().Replace(@operator.Body?.ToString() ?? Guid.NewGuid().ToString(), "").Trim().Trim();
                    sb.AppendLine(value);
                    sb.AppendLine();
                    dependencies.AddRange(@operator.ParameterList.Parameters.Select(x => x.Type?.ToString() ?? "unknown").ToList());
                }

                sb.AppendLine("}");
                var parent = GetParentFromNesting(node.Parent);
                CSharpChunkKind parentKind = GetParentType(node.Parent);
                dependencies = RemoveDuplicateAndTrivialDependencies(dependencies);
                result.Add(new CSharpChunk(kind, ns, parent, parentKind, name, GetXmlSummary(node), sb.ToString(), dependencies, node));
            }
        }

        return result;
    }

    private static PropertyDeclarationSyntax RemoveExpressionBody(PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody != null)
        {
            property = property.WithExpressionBody(null)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                .WithAccessorList(SyntaxFactory.AccessorList(
                    SyntaxFactory.List([
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    ])));
        }

        return property;
    }

    private static PropertyDeclarationSyntax RemoveAttributes(PropertyDeclarationSyntax property)
    {
        return property.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());
    }

    private static string GetXmlSummary(SyntaxNode node)
    {
        DocumentationCommentTriviaSyntax? trivia = node.GetLeadingTrivia().Select(t => t.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();
        if (trivia == null)
        {
            return string.Empty;
        }

        string xmlSummary = trivia.ToString();
        while (xmlSummary.Contains(" /"))
        {
            xmlSummary = xmlSummary.Replace(" /", "/");
        }

        return "///" + xmlSummary;
    }

    private static bool IsPublic(SyntaxTokenList modifiers)
    {
        return modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
    }

    private static bool IsConstant(SyntaxTokenList modifiers)
    {
        return modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
    }

    private static bool IsStatic(SyntaxTokenList modifiers)
    {
        return modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
    }

    private static bool IsAbstract(SyntaxTokenList modifiers)
    {
        return modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
    }

    private static PropertyDeclarationSyntax[] GetPublicProperties(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<PropertyDeclarationSyntax>().Where(x => IsPublic(x.Modifiers)).ToArray();
    }

    private static FieldDeclarationSyntax[] GetPublicConstants(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<FieldDeclarationSyntax>().Where(x => IsPublic(x.Modifiers) && IsConstant(x.Modifiers)).ToArray();
    }

    private static ConstructorDeclarationSyntax[] GetPublicConstructors(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<ConstructorDeclarationSyntax>().Where(x => IsPublic(x.Modifiers)).ToArray();
    }

    private static MethodDeclarationSyntax[] GetPublicMethods(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<MethodDeclarationSyntax>().Where(x => IsPublic(x.Modifiers)).ToArray();
    }

    private static string GetNamespace(SyntaxNode node)
    {
        SyntaxNode? current = node;
        while (current != null)
        {
            if (current is NamespaceDeclarationSyntax namespaceDeclaration)
                return namespaceDeclaration.Name.ToString();
            if (current is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
                return fileScopedNamespace.Name.ToString();

            current = current.Parent;
        }

        return string.Empty;
    }

    private static List<string> GetMethodDependencies(MethodDeclarationSyntax method)
    {
        List<string> result = [];
        result.AddRange(method.ParameterList.Parameters.Select(p => p.Type?.ToString() ?? "unknown"));
        result = RemoveDuplicateAndTrivialDependencies(result);
        return result;
    }

    private static List<string> RemoveDuplicateAndTrivialDependencies(List<string> dependencies)
    {
        dependencies = dependencies.Distinct().ToList();

        // Base types to remove
        string[] baseTypes =
        [
            "Stream", "Exception", "CancellationToken", "string", "int", "long", "short", "byte", "bool", "char", "float", "double", "decimal",
            "uint", "ulong", "ushort", "sbyte", "object", "dynamic", "void",
            "DateTime", "DateOnly", "TimeOnly", "DateTimeOffset"
        ];
        // Collection type templates
        string[] collectionTemplates =
        [
            "{0}[]",
            "List<{0}>",
            "ReadOnlyList<{0}>",
            "IEnumerable<{0}>",
            "IList<{0}>",
            "ICollection<{0}>"
        ];

        HashSet<string> trivialTypes = new(StringComparer.OrdinalIgnoreCase);

        // Add base types
        foreach (var type in baseTypes)
        {
            trivialTypes.Add(type);

            // Add nullable forms
            trivialTypes.Add($"{type}?");

            // Add collection forms
            foreach (var collection in collectionTemplates)
            {
                trivialTypes.Add(string.Format(collection, type));
            }
        }

        // Remove nullable, array, and generic collection forms as well
        return dependencies
            .Where(dep =>
            {
                var typeName = dep.Trim();
                // Remove nullable marker
                if (typeName.EndsWith("?"))
                    typeName = typeName.TrimEnd('?');
                // Remove array brackets
                typeName = typeName.TrimEnd('[', ']');
                // Remove whitespace in generics
                typeName = typeName.Replace(" ", "");
                return !trivialTypes.Contains(typeName);
            })
            .ToList();
    }


    private string? GetParentFromNesting(SyntaxNode? parent)
    {
        return parent switch
        {
            ClassDeclarationSyntax @class => @class.Identifier.Text,
            RecordDeclarationSyntax record => record.Identifier.Text,
            StructDeclarationSyntax @struct => @struct.Identifier.Text,
            InterfaceDeclarationSyntax @interface => @interface.Identifier.Text,
            _ => null
        };
    }

    private CSharpChunkKind GetParentType(SyntaxNode? parent)
    {
        return parent switch
        {
            ClassDeclarationSyntax => CSharpChunkKind.Class,
            RecordDeclarationSyntax => CSharpChunkKind.Record,
            StructDeclarationSyntax => CSharpChunkKind.Struct,
            InterfaceDeclarationSyntax => CSharpChunkKind.Interface,
            _ => CSharpChunkKind.None
        };
    }

    private static ConversionOperatorDeclarationSyntax[] GetPublicImplicitOperators(SyntaxList<MemberDeclarationSyntax> members)
    {
        return members.OfType<ConversionOperatorDeclarationSyntax>().Where(x => IsPublic(x.Modifiers)).ToArray();
    }
}