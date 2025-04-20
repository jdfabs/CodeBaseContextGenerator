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
    private string _projectPath = "";

    public async Task RunAsync()
    {
        // 1️⃣ Select project root
        _projectPath = new FileExplorer("java").BrowseAndGetPath();

        // 2️⃣ Start background analysis loop
        var backgroundTask = Task.Run(BackgroundAnalyzerLoop);

        // 3️⃣ Launch interactive project explorer (in main thread)
        var explorer = new Project.ProjectExplorer(_jsonPath);
        Console.ReadKey();
        explorer.Browse();
    }

    private async Task BackgroundAnalyzerLoop()
    {
        while (true)
        {
            Console.Beep();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n⏳ Checking for updates at {DateTime.Now:HH:mm:ss}...");
            Console.ResetColor();

            var representations = JavaAnalyzer.Analyze(_projectPath);

            var diff = await ChangeDetector.DetectChangesAsync(
                _jsonPath, representations, _ollama
            );

            if (diff.HasChanges)
            {
                JsonWriter.SaveMerged(_jsonPath, diff.MergedJson);
                SummaryWriter.Write(_summaryPath, diff.MergedJson);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Changes detected and saved at {DateTime.Now:HH:mm:ss}");
                Console.ResetColor();
            }

            await Task.Delay(TimeSpan.FromSeconds(60)); // ⏲ Configurable interval
        }
    }
}
