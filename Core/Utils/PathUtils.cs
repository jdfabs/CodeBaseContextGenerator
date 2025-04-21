namespace CodeBaseContextGenerator.Core.Utils;

public static class PathUtils
{
    /// <summary>
    /// Returns the project root (3 levels up from current execution dir).
    /// </summary>
    public static string ProjectRoot =>
        Directory.GetParent(Directory.GetCurrentDirectory())?
            .Parent?
            .Parent?
            .FullName ?? Directory.GetCurrentDirectory();

    /// <summary>
    /// Combines the project root with a relative file or folder path.
    /// </summary>
    public static string ProjectFile(string relativePath) =>
        Path.Combine(ProjectRoot, relativePath);

    /// <summary>
    /// Converts a full path to a path relative to the given base path.
    /// </summary>
    public static string GetRelativePath(string basePath, string fullPath) =>
        Path.GetRelativePath(basePath, fullPath)
            .Replace('\\', '/'); // normalize for consistency

    /// <summary>
    /// Normalizes slashes and trims quotes.
    /// </summary>
    public static string CleanPath(string path) =>
        path.Replace("\\", "/").Replace("\"", "").Trim();
}