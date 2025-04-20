using CodeBaseContextGenerator.LLM;

namespace CodeBaseContextGenerator;

public static class SummaryGenerator
{
    public static async Task<string> SummarizeAsync(string code, OllamaClient ollama)
    {
        var prompt = $@"
You are generating structured summaries of Java code for automated analysis.

Instructions:
- Use this format (no deviations):
Purpose:[high-level intent], Behavior:[specific operations or responsibilities]

Rules:
- DO NOT include class or method names
- DO NOT include parameter names or types
- DO NOT quote or refer to source code
- Use plain English, lowercase, complete sentences
- Use general terms like “this class handles authentication logic”
- Keep Behavior concise but informative, describing what the code does

Java code:
{code}";

        var response = await ollama.SendPromptAsync("llama3", prompt);
        return Clean(response);
    }

    private static string Clean(string raw)
    {
        raw = raw.Replace("\"", "").Replace("'", "").Replace("\r", "").Replace("\n", " ").Trim();
        var idx = raw.IndexOf("Purpose:", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? raw[idx..].Trim() : raw;
    }
}