using System.Security.Cryptography;
using System.Text;

namespace CodeBaseContextGenerator;

public class TypeRepresentation
{
    public string Name { get; set; }
    public string Type { get; set; } // class, interface, enum, record
    public string FilePath { get; set; }
    public string Body { get; set; }
    public string Hash => ComputeHash();
    
    
        
    private string ComputeHash()
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(Body);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}