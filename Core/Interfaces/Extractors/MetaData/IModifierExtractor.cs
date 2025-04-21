using System.Collections.Immutable;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IModifierExtractor<in TNode> : IExtractor<TNode> {
    ImmutableHashSet<CodeModifier> ExtractModifiers(TNode node);
}