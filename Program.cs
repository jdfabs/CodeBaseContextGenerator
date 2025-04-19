namespace CodeBaseContextGenerator;

class Program
{
    private static string javaFilePath;

    static async Task Main(string[] args)
    {
        SelectTarget();
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

    private static void SelectTarget()
    {
        javaFilePath = FileExplorer.Browse();

        if (!File.Exists(javaFilePath))
        {
            Console.WriteLine($"File not found: {javaFilePath}");
            return;
        }
    }

    private static void CheckForChanges()
    {
        var source = File.ReadAllText(javaFilePath);
        var types = TypeUtils.ExtractJavaTypes(source);

        var previousHashes = TypeUtils.LoadPreviousHashes();
        Console.WriteLine($"Loading previous hashes: {previousHashes.Count}");

        var currentHashes = types.ToDictionary(t => $"{t.Type}:{t.Name}", t => t.Hash);
        var changes = currentHashes
            .Where(kvp => !previousHashes.TryGetValue(kvp.Key, out var oldHash) || oldHash != kvp.Value)
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
                Console.WriteLine($"- {change.Key} (new or modified)");
            }

            TypeUtils.SaveHashes(currentHashes);
        }
    }
}