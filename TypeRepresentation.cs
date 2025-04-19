using System.Security.Cryptography;
using System.Text;

namespace CodeBaseContextGenerator;

public class TypeRepresentation
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Privacy { get; set; } // public, private, protected
    public string ReturnType { get; set; } // void, int, String, etc.
    public string Parameters { get; set; } // (int a, String b)
    public string Content { get; set; } 
    public string SourcePath { get; set; }
    public List<TypeReference> ReferencedTypes { get; set; } // List of types referenced in the method, e.g. another class or interface
    public string Hash => ComputeHash();

    private string ComputeHash()
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(Content);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}
public class TypeHashEntry
{
    public string Hash { get; set; }
    public string Type { get; set; }
    public string Privacy { get; set; } // public, private, protected
    public string ReturnType { get; set; } // void, int, String, etc.
    public string Name { get; set; } // method name
    public string Parameters { get; set; } // (int a, String b)
    public string Content { get; set; } 
    public List<TypeReference> ReferencedTypes { get; set; } // List of types referenced in the method, e.g. another class or interface
}

public class TypeReference
{
    public string Name { get; set; }
    public string Source { get; set; } // Name@RelativePath
    public string Kind { get; set; }   // "extends", "implements", "uses", etc.
}
