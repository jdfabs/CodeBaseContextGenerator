using System.Text.Json;

namespace CodeBaseContextGenerator.Core.Utils.Json;

public class JsonLoader
{
    public static T? Load<T>(string path, bool verbose = true)
    {
        try
        {
            if (!File.Exists(path))
            {
                if (verbose)
                    Console.WriteLine($"⚠ JSON file not found: {Path.GetFullPath(path)}");
                return default;
            }

            var raw = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(raw))
            {
                if (verbose)
                    Console.WriteLine($"⚠ JSON file is empty: {Path.GetFullPath(path)}");
                return default;
            }

            var parsed = JsonSerializer.Deserialize<T>(raw);
            return parsed;
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.WriteLine($"❌ Failed to parse JSON: {Path.GetFullPath(path)}");
                Console.WriteLine(ex.Message);
            }

            return default;
        }
    } 
}