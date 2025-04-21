using System.Collections.Immutable;
using CodeBaseContextGenerator.Core.Interfaces.Assemblers;
using CodeBaseContextGenerator.Core.Interfaces.Extractors;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;
using CodeBaseContextGenerator.Core.Models.Representations;
using CodeBaseContextGenerator.Core.Shared;

namespace CodeBaseContextGenerator.Core.Base.Assemblers;

public abstract class TypeRepresentationAssemblerBase<TNode>(IExtractorSet<TNode> extractors)
    : ITypeRepresentationAssembler<TNode>
{
    public ITypeRepresentation Assemble(TNode node)
    {
        var name = extractors.Name.Extract(node);
        var kind = extractors.Metadata.Extract(node);
        var modifiers = extractors.Modifiers.Extract(node);
        var docs = extractors.Docs.Extract(node);
        var (path, firstLine, lastLine, firstCol, lastCol) = extractors.Anchor.Extract(node);
        var baseTypes = extractors.BaseTypes.Extract(node);
        var usages = extractors.Usages.Extract(node);
        var fields = extractors.Fields.Extract(node);
        var constructors = extractors.Constructors.Extract(node);
        var methods = extractors.Methods.Extract(node);
        var nested = extractors.NestedTypes.Assemble(node);
        var code = extractors.Code.Extract(node);
        var fullQuanlifiedName = extractors.FullQualifiedName.Extract(node);
        var summary = SummaryGenerator.Summarize(code);
        return CreateRepresentation(
            name, fullQuanlifiedName, kind, modifiers, docs,
            path, firstLine, lastLine, firstCol, lastCol, baseTypes, usages,
            fields, constructors, methods, nested, code, summary
        );
    }

    protected virtual ITypeRepresentation CreateRepresentation(
        string name,
        string fullQualifiedName,
        string kind,
        ImmutableHashSet<CodeModifier> modifiers,
        string? docs,
        string filePath,
        int? startLine,
        int? endLine,
        int? startCol,
        int? endCol,
        IReadOnlyCollection<ITypeReference> baseTypes,
        IReadOnlyCollection<ITypeReference>? usageTypes,
        IReadOnlyCollection<IFieldRepresentation>? fields,
        IReadOnlyCollection<IConstructorRepresentation>? constructors,
        IReadOnlyCollection<IMethodRepresentation>? methods,
        IReadOnlyCollection<ITypeRepresentation>? nestedTypes,
        string code,
        string summary
    )
    {
        return new TypeRepresentationBase()
        {
            Name = name,
            FullQualifiedName = fullQualifiedName,
            Modifiers = modifiers,
            Docs = docs,
            FilePath = filePath,
            Code = code,
            ReferenceTypes = usageTypes ?? Array.Empty<ITypeReference>(),
            StartLine = startLine ?? null,
            EndLine = endLine ?? null,
            StartColumn = startCol ?? null,
            EndColumn = endCol ?? null,
            Type = kind,
            Summary = summary,

            BaseTypes = baseTypes,
            Fields = fields ?? Array.Empty<IFieldRepresentation>(),
            Methods = methods ?? Array.Empty<IMethodRepresentation>(),
            Constructors = constructors ?? Array.Empty<IConstructorRepresentation>(),
            NestedTypes = nestedTypes ?? Array.Empty<ITypeRepresentation>(),
        };
    }
}