namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface ISourceAnchor {
    // ISourceAnchor is used to track exact source location (file + line/column) of key elements for UI navigation, diffing, and file-watching use cases. Optional but recommended for types, methods, and constructors.
    string FilePath { get; }
    int? Line { get; }
    int? Column { get; }
}