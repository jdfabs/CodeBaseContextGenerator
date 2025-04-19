namespace CodeBaseContextGenerator;

class Program
{
    private static string path;

    static async Task Main(string[] args)
    {
        path = FileExplorer.Browse();
        CheckForChanges();

        while (true)
        {
            Console.WriteLine("Press any key to reload files");
            Console.ReadKey();
            CheckForChanges();
        }


        /*                                        aass
        var ollama = new OllamaClient();

        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) continue;

            Console.Write("LLM: ");
            await ollama.StreamPromptAsync("gemma3:4b", input);
        }
        while (true)
        {
            Console.Write("You: ");
            var userInput = Console.ReadLine();

            var reply = await ollama.SendPromptAsync("llama3", userInput);

            Console.WriteLine($"LLM: {reply}");
        }*/
    }

    private static void CheckForChanges()
    {
        var javaFiles = new List<string>();

        if (File.Exists(path))
        {
            if (Path.GetExtension(path) == ".java")
            {
                javaFiles.Add(path);
            }
            else
            {
                Console.WriteLine("⚠️ Only .java files are supported.");
                return;
            }
        }
        else if (Directory.Exists(path))
        {
            javaFiles = Directory.GetFiles(path, "*.java", SearchOption.AllDirectories).ToList();
        }
        else
        {
            Console.WriteLine($"❌ Path not found: {path}");
            return;
        }

        Console.WriteLine($"📂 Found {javaFiles.Count} Java file(s).");

        var allTypes = new List<TypeRepresentation>();
        foreach (var file in javaFiles)
        {
            var source = File.ReadAllText(file);
            var types = TypeUtils.ExtractJavaTypes(source, file);

            foreach (var type in types)
            {
                // Prefix with filename to make hashes unique per-file
                type.SourcePath = Path.GetRelativePath(path, file);
            }

            allTypes.AddRange(types);
        }

        var previousHashes = TypeUtils.LoadPreviousHashes();
        
        TypeUtils.PopulateReferencedTypes(allTypes);

        var currentHashes = allTypes.ToDictionary(
            t => $"{t.Type}:{t.Name}@{t.SourcePath}",
            t => new TypeHashEntry
            {
                Hash = t.Hash,
                Type = t.Type,
                Privacy = t.Privacy,
                ReturnType = t.ReturnType,
                Name = t.Name,
                Parameters = t.Parameters,
                Content = t.Content,
                ReferencedTypes = t.ReferencedTypes
            }
        );


        var changes = currentHashes
            .Where(kvp => !previousHashes.TryGetValue(kvp.Key, out var oldEntry) || oldEntry.Hash != kvp.Value.Hash)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


        if (changes.Count == 0)
        {
            Console.WriteLine("✅ No changes detected.");
        }
        else
        {
            Console.WriteLine("🔄 Changes detected in:");
            foreach (var change in changes)
            {
                Console.WriteLine($"- {change.Key}");
            }

            TypeUtils.SaveHashes(currentHashes);
        }
    }
}