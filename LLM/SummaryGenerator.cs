using CodeBaseContextGenerator.LLM;

namespace CodeBaseContextGenerator;

public static class SummaryGenerator
{
    private const string PromptTemplate = """
                                          You are a code analysis assistant that summarizes Java code for documentation and automated understanding.
                                          Your task:
                                          Generate **structured summaries** using this exact format:
                                          Purpose:[one sentence explaining the high-level intent], Behavior:[one sentence describing what the code does in practice]
                                          
                                          Guidelines:
                                          - Do NOT include class or method names
                                          - Do NOT mention parameter names or types
                                          - Do NOT refer to line numbers or quote the code
                                          - Write in **plain English**
                                          - Use **lowercase**, **concise**, and **complete sentences**
                                          - Use abstract phrasing like “this class manages X” or “this method performs Y”
                                          - If the code is simple or unclear, still write a complete sentence
                                          
                                          Example:
                                          (Purpose: handles user authentication logic, Behavior: verifies credentials and starts session on success)
                                          
                                          ---
                                          
                                          Here is the Java code to summarize:
                                          {0}
                                          """;
    public static async Task<string> SummarizeAsync(string code, OllamaClient ollama, string model = "llama3")
    {
        var prompt = string.Format(PromptTemplate, code);
        var response = await ollama.SendPromptAsync(model, prompt);
        return Clean(response);
    }

    private static string Clean(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Purpose: Unknown, Behavior: Not specified";

        raw = raw.Replace("\"", "")
            .Replace("'", "")
            .Replace("\r", "")
            .Replace("\n", " ")
            .Trim();

        // Ensure we start from Purpose
        var start = raw.IndexOf("Purpose:", StringComparison.OrdinalIgnoreCase);
        if (start < 0) return "Purpose: Unclear, Behavior: Not detected";

        var cleaned = raw[start..].Trim();

        // Basic fallback
        if (!cleaned.Contains("Behavior:"))
            cleaned += " Behavior: Not specified.";

        return cleaned;
    }
}