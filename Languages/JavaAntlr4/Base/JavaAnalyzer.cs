using CodeBaseContextGenerator.Core;
using CodeBaseContextGenerator.JavaAntlr4.Visitors;

namespace CodeBaseContextGenerator.JavaAntlr4.Base;

public static class JavaAnalyzer
{
    public static List<TypeRepresentation> Analyze(string path)
    {
        var result = new List<TypeRepresentation>();
        var inspector = new JavaAstInspector(GetRootDirectory(path));

        if (File.Exists(path) && Path.GetExtension(path).Equals(".java", StringComparison.OrdinalIgnoreCase))
        {
            result.AddRange(inspector.Inspect(path));
        }
        else if (Directory.Exists(path))
        {
            var javaFiles = Directory.GetFiles(path, "*.java", SearchOption.AllDirectories);
            foreach (var file in javaFiles)
            {
                result.AddRange(inspector.Inspect(file));
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✖ Invalid path. Please choose a .java file or folder.");
            Console.ResetColor();
        }

       
        
        RefResolver.Resolve(result);
        
        return result;
    }

    private static string GetRootDirectory(string path)
    {
        return File.Exists(path)
            ? Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory()
            : path;
    }
}