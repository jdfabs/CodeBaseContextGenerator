namespace CodeBaseContextGenerator.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        var entry = new EntryPoint();
        await entry.RunAsync();
    }
}