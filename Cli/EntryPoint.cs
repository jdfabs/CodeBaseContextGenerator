using CodeBaseContextGenerator.Core;
using CodeBaseContextGenerator.JavaAntlr4.Base;
using CodeBaseContextGenerator.Json;
using CodeBaseContextGenerator.LLM;
using CodeBaseContextGenerator.Utils;

namespace CodeBaseContextGenerator.Cli;

public class EntryPoint
{
    private readonly OllamaClient _ollama = new();
    private readonly string _jsonPath = PathUtils.ProjectFile("project_context.json");
    private readonly string _summaryPath = PathUtils.ProjectFile("program_summary.txt");

    public async Task RunAsync()
    {
        var path = new FileExplorer("java").BrowseAndGetPath();
        
        while (true)
        {
            
            Console.WriteLine("\n🔎 Analyzing project...");
            var representations = JavaAnalyzer.Analyze(path);

            var diff = await ChangeDetector.DetectChangesAsync(
                _jsonPath, representations, _ollama
            );

            if (diff.HasChanges)
            {
                JsonWriter.SaveMerged(_jsonPath, diff.MergedJson);
                SummaryWriter.Write(_summaryPath, diff.MergedJson);
                Console.WriteLine("✅ Analysis complete. Press any key to rerun...");
            }
            else
            {
                Console.WriteLine("➖ No changes detected.");
            }

            Console.ReadKey(true);
        }
    }
}
