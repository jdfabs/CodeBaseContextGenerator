using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace CodeBaseContextGenerator;

public class TypeRepresentation
{
    public string Name           { get; set; }
    public string Type           { get; set; }
    public string Privacy        { get; set; }
    public string ReturnType     { get; set; }
    public string Parameters     { get; set; }

    // Rename the old Content to Code
    [JsonPropertyName("Content")]
    public string Code           { get; set; }

    // New field for your LLM docstring
    public string Summary        { get; set; }

    public string SourcePath     { get; set; }
    public List<TypeReference> ReferencedTypes { get; set; }
    public bool IsAbstract            { get; set; }
    public IReadOnlyCollection<string> Modifiers { get; set; }
    public string? Javadoc            { get; set; }

    public List<FieldRepresentation> Fields          { get; set; }
    public List<ConstructorRepresentation> Constructors { get; set; }
    public List<TypeRepresentation> Methods     { get; set; }
    public List<TypeRepresentation> NestedTypes { get; set; } // NEW

    // Hash is computed only over the raw code
    public string Hash => ComputeHash();

    private string ComputeHash()
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(Code);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}
public class FieldRepresentation
{
    public string Name     { get; set; }
    public string Type     { get; set; }
    public string Privacy  { get; set; }
    public List<string> Modifiers { get; set; }
    public string? Javadoc { get; set; }
}

public class ConstructorRepresentation
{
    public string Name       { get; set; }
    public string Privacy    { get; set; }
    public List<string> Modifiers { get; set; }
    public string Parameters { get; set; }
    public List<string> ExceptionsThrown { get; set; }
    public string? Javadoc   { get; set; }
}   

public class TypeReference
{
    public string Name { get; set; }
    public string Source { get; set; } // Name@RelativePath
    public string Kind { get; set; }   // "extends", "implements", "uses", etc.
}
