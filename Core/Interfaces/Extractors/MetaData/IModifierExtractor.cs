using System.Collections.Immutable;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IModifierExtractor<in TNode> {
    protected ImmutableHashSet<CodeModifier> ExtractModifiers(TNode node);
}